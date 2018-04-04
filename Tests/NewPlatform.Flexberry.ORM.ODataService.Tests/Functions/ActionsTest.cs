namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Functions
{
    using System;
    using Action = NewPlatform.Flexberry.ORM.ODataService.Functions.Action;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Script.Serialization;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;

    using Xunit;

    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;
    using System.Text;

    /// <summary>
    /// Класс тестов для тестирования метаданных, получаемых от OData-сервиса.
    /// </summary>
    public class ActionsTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Осуществляет регистрацию пользовательских OData-actions.
        /// </summary>
        /// <param name="container">Container of user functions.</param>
        /// <param name="dataService">Сервис данных.</param>
        public void RegisterODataActions(IFunctionContainer container, IDataService dataService)
        {
            Dictionary<string, Type> parametersTypes = new Dictionary<string, Type>();
            if (!container.IsRegistered("ActionWithLcs"))
            {
                parametersTypes = new Dictionary<string, Type> { { "entitySet", typeof(string) }, { "query", typeof(string) } };
                container.Register(new Action(
                    "ActionWithLcs",
                    (queryParameters, parameters) =>
                    {
                        var type = queryParameters.GetDataObjectType(parameters["entitySet"] as string);
                        var uri = $"http://a/b/c?{parameters["query"]}";
                        var lcs = queryParameters.CreateLcs(type, uri);
                        var dobjs = dataService.LoadObjects(lcs);
                        return dobjs.AsEnumerable();
                    },
                    typeof(IEnumerable<DataObject>),
                    parametersTypes));
            }

            if (!container.IsRegistered("ActionVoid"))
            {
                parametersTypes = new Dictionary<string, Type> { { "entitySet", typeof(string) }, { "query", typeof(string) } };
                container.Register(new Action(
                    "ActionVoid",
                    (queryParameters, parameters) =>
                    {
                        var type = queryParameters.GetDataObjectType(parameters["entitySet"] as string);
                        var uri = $"http://a/b/c?{parameters["query"]}";
                        var lcs = queryParameters.CreateLcs(type, uri);
                        var dobjs = dataService.LoadObjects(lcs);
                        return null;
                    },
                    typeof(void),
                    parametersTypes));
            }

            if (!container.IsRegistered("ActionEnum"))
            {
                parametersTypes = new Dictionary<string, Type> { { "пол", typeof(tПол) } };
                container.Register(new Action(
                    "ActionEnum",
                    (queryParameters, parameters) =>
                    {
                        return (tПол)parameters["пол"];
                    },
                    typeof(tПол),
                    parametersTypes));
            }

            if (!container.IsRegistered("ActionEntity"))
            {
                parametersTypes = new Dictionary<string, Type> { { "entity", typeof(КлассСМножествомТипов) }, { "collection", typeof(IEnumerable<КлассСМножествомТипов>) } };
                container.Register(new Action(
                    "ActionEntity",
                    (queryParameters, parameters) =>
                    {
                        IEnumerable<КлассСМножествомТипов> collection = parameters["collection"] as IEnumerable<КлассСМножествомТипов>;
                        var item = collection.ToArray()[0];
                        return collection;
                    },
                    typeof(IEnumerable<КлассСМножествомТипов>),
                    parametersTypes));
            }
        }

        /// <summary>
        /// Осуществляет проверку возвращаемого значения сущности.
        /// </summary>
        [Fact]
        public void TestActionEntity()
        {
            ActODataService(args =>
            {
                RegisterODataActions(args.Token.Functions, args.DataService);
                var code = HttpStatusCode.InternalServerError;var s = $"{code.ToString()}";
                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/ActionEntity";
                string jsonClass = "{\"PropertyString\": \"свойство 1\", \"__PrimaryKey\":\"2b7afa44-2df7-4838-b489-18874435b0d0\"}";
                string json = $"{{\"entity\": {jsonClass}, \"collection\": [{jsonClass}] }}";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, json).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(1, (receivedDict["value"] as ArrayList).Count);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку возвращаемого значения перечисления.
        /// </summary>
        [Fact]
        public void TestActionEnum()
        {
            ActODataService(args =>
            {
                RegisterODataActions(args.Token.Functions, args.DataService);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/ActionEnum";
                string json = "{\"пол\": \"Мужской\"}";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, json).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.True(receivedDict["value"] as string == "Мужской");
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку возвращаемых значений функциями OData, которые используют LCS, созданный на основе запроса OData.
        /// </summary>
        [Fact]
        public void TestActionWithLcs()
        {
            ActODataService(args =>
            {
                args.Token.Functions.RegisterAction(new Func<QueryParameters, string, string, IEnumerable<DataObject>>(AddWithQueryParameters));
                RegisterODataActions(args.Token.Functions, args.DataService);

                // Создаем объекты и кладем их в базу данных.
                DataObject[] countries = new DataObject[5];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = $"Страна №{i}" };
                }

                args.DataService.UpdateObjects(ref countries);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/ActionWithLcs";
                string json = "{\"entitySet\": \"Странаs\", \"query\": \"$filter=Название eq 'Страна №1'\"}";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, json).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(1, (receivedDict["value"] as ArrayList).Count);
                }

                DataServiceProvider.DataService = args.DataService;
                requestUrl = $"http://localhost/odata/AddWithQueryParameters";
                json = "{\"entitySet\": \"Странаs\", \"query\": \"$filter=Название eq 'Страна №2'\"}";
                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, json).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(1, (receivedDict["value"] as ArrayList).Count);
                }

                requestUrl = $"http://localhost/odata/ActionVoid";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, json).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            });
        }

        private static IEnumerable<DataObject> AddWithQueryParameters(QueryParameters queryParameters, string entitySet, string query)
        {
            Assert.NotNull(queryParameters);
            SQLDataService dataService = DataServiceProvider.DataService as SQLDataService;
            var type = queryParameters.GetDataObjectType(entitySet);
            var uri = $"http://a/b/c?{query}";
            var lcs = queryParameters.CreateLcs(type, uri);
            var dobjs = dataService.LoadObjects(lcs);
            return dobjs.AsEnumerable();
        }

    }
}
