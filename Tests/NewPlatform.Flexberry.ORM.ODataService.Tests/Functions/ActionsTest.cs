namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.OData;

    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    using Action = ODataService.Functions.Action;

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

            if (!container.IsRegistered("ActionODataHttpResponseException"))
            {
                parametersTypes = new Dictionary<string, Type> { };
                container.Register(new Action(
                    "ActionODataHttpResponseException",
                    (queryParameters, parameters) =>
                    {
                        throw new HttpResponseException(queryParameters.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            new ODataError() { ErrorCode = "400", Message = "Сообщение об ошибке" }));
                    },
                    typeof(IEnumerable<DataObject>),
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
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
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
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

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
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
                }

                DataService = args.DataService as SQLDataService;
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
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
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

        /// <summary>
        /// Осуществляет проверку проброса исключения, содержащего ODataError.
        /// </summary>
        [Fact]
        public void TestActionODataHttpResponseException()
        {
            ActODataService(args =>
            {
                RegisterODataActions(args.Token.Functions, args.DataService);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/ActionODataHttpResponseException";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, string.Empty).Result)
                {
                    // Проверим, что возвращается код ошибки, указанный в функции.
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                    // Проверим сообщение об ошибке.
                    Assert.Equal("Сообщение об ошибке", ((ODataError)((ObjectContent)response.Content).Value).Message);
                }
            });
        }

        private SQLDataService DataService { get; set; }

        private IEnumerable<DataObject> AddWithQueryParameters(QueryParameters queryParameters, string entitySet, string query)
        {
            Assert.NotNull(queryParameters);
            var type = queryParameters.GetDataObjectType(entitySet);
            var uri = $"http://a/b/c?{query}";
            var lcs = queryParameters.CreateLcs(type, uri);
            var dobjs = DataService.LoadObjects(lcs);
            return dobjs.AsEnumerable();
        }

    }
}
