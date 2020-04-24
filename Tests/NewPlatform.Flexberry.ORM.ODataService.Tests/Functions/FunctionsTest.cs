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
    using ICSSoft.STORMNET.Business.LINQProvider;

    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.OData;

    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования метаданных, получаемых от OData-сервиса.
    /// </summary>
    public class FunctionsTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Осуществляет регистрацию пользовательских OData-функций.
        /// </summary>
        /// <param name="container">Container of user functions.</param>
        /// <param name="dataService">Сервис данных.</param>
        public void RegisterODataUserFunctions(IFunctionContainer container, IDataService dataService)
        {
            Dictionary<string, Type> parametersTypes = new Dictionary<string, Type>();

            if (!container.IsRegistered("FunctionWithLcs1"))
            {
                parametersTypes = new Dictionary<string, Type> { { "entitySet", typeof(string) } };
                container.Register(new Function(
                    "FunctionWithLcs1",
                    (queryParameters, parameters) =>
                    {
                        var type = queryParameters.GetDataObjectType(parameters["entitySet"] as string);
                        var lcs = queryParameters.CreateLcs(type);
                        var dobjs = dataService.LoadObjects(lcs);
                        return dobjs.AsEnumerable();
                    },
                    typeof(IEnumerable<DataObject>),
                    parametersTypes));
            }

            if (!container.IsRegistered("FunctionWithLcs2"))
            {
                parametersTypes = new Dictionary<string, Type> { { "entitySet", typeof(string) }, { "query", typeof(string) } };
                container.Register(new Function(
                    "FunctionWithLcs2",
                    (queryParameters, parameters) =>
                    {
                        var type = queryParameters.GetDataObjectType(parameters["entitySet"] as string);
                        var uri = $"http://a/b/c?{parameters["query"]}";
                        var lcs = queryParameters.CreateLcs(type, uri);
                        var dobjs = dataService.LoadObjects(lcs);
                        return dobjs.Length;
                    },
                    typeof(int),
                    parametersTypes));
            }

            if (!container.IsRegistered("FunctionString"))
            {
                parametersTypes = new Dictionary<string, Type> { { "stringParam", typeof(string) } };
                container.Register(new Function(
                    "FunctionString",
                    (queryParameters, parameters) => parameters["stringParam"],
                    typeof(string),
                    parametersTypes));
            }

            if (!container.IsRegistered("FunctionInt"))
            {
                parametersTypes = new Dictionary<string, Type> { { "intParam", typeof(int) } };
                container.Register(new Function(
                    "FunctionInt",
                    (queryParameters, parameters) => parameters["intParam"],
                    typeof(int),
                    parametersTypes));
            }

            if (!container.IsRegistered("FunctionEntity"))
            {
                parametersTypes = new Dictionary<string, Type> { { "intParam", typeof(int) } };
                container.Register(new Function(
                    "FunctionEntity",
                    (queryParameters, parameters) =>
                    {
                        var result = (dataService as SQLDataService).Query<Страна>(Страна.Views.СтранаE).ToArray();
                        return result[(int)parameters["intParam"]];
                    },
                    typeof(Страна),
                    parametersTypes));
            }

            if (!container.IsRegistered("FunctionEntitiesCollection"))
            {
                parametersTypes = new Dictionary<string, Type> { { "intParam", typeof(int) } };
                container.Register(new Function(
                    "FunctionEntitiesCollection",
                    (queryParameters, parameters) =>
                    {
                        var top = (int)parameters["intParam"];
                        var result = (dataService as SQLDataService).Query<Страна>(Страна.Views.СтранаE).Take(top).ToArray();
                        queryParameters.Count = result.Length;
                        return result;
                    },
                    typeof(IEnumerable<Страна>),
                    parametersTypes));
            }

            if (!container.IsRegistered("FunctionSelectExpandEntity"))
            {
                parametersTypes = new Dictionary<string, Type> { { "intParam", typeof(int) } };
                container.Register(new Function(
                    "FunctionSelectExpandEntity",
                    (queryParameters, parameters) =>
                    {
                        var result = (dataService as SQLDataService).Query<Медведь>(Медведь.Views.МедведьE).ToArray();
                        return result[(int)parameters["intParam"]];
                    },
                    typeof(Медведь),
                    parametersTypes));
            }

            if (!container.IsRegistered("FunctionEnum"))
            {
                parametersTypes = new Dictionary<string, Type> { { "пол", typeof(tПол) } };
                container.Register(new Function(
                    "FunctionEnum",
                    (queryParameters, parameters) =>
                    {
                        return (tПол)parameters["пол"];
                    },
                    typeof(tПол),
                    parametersTypes));
            }

            if (!container.IsRegistered("FunctionHttpResponseException"))
            {
                parametersTypes = new Dictionary<string, Type> { /*{ "intParam", typeof(int) }*/ };
                container.Register(new Function(
                    "FunctionHttpResponseException",
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
        /// Осуществляет проверку возвращаемого значения перечисления.
        /// </summary>
        [Fact]
        public void TestFunctionEnum()
        {
            ActODataService(args =>
            {
                RegisterODataUserFunctions(args.Token.Functions, args.DataService);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/FunctionEnum(пол=NewPlatform.Flexberry.ORM.ODataService.Tests.tПол'Мужской')";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
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
        public void TestFunctionFunctionWithLcs()
        {
            ActODataService(args =>
            {
                RegisterODataUserFunctions(args.Token.Functions, args.DataService);

                // Создаем объекты и кладем их в базу данных.
                DataObject[] countries = new DataObject[5];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = $"Страна №{i}" };
                }

                args.DataService.UpdateObjects(ref countries);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/FunctionWithLcs2(entitySet='Странаs',query='$filter=Название eq ''Страна №1''')";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(1, (int)(long)receivedDict["value"]);
                }

                requestUrl = $"http://localhost/odata/FunctionWithLcs1(entitySet='Странаs')?$filter=Название eq 'Страна №1'";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
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
        /// Осуществляет проверку возвращаемого функцией OData значения, определяемого стандартом OData, как сущность.
        /// В запросе присутствуют операторы $select и $expand.
        /// </summary>
        [Fact]
        public void TestSelectExpandFunctionEntity()
        {
            ActODataService(args =>
            {
                RegisterODataUserFunctions(args.Token.Functions, args.DataService);

                Медведь медв = new Медведь { Вес = 48 };
                Лес лес1 = new Лес { Название = "Бор" };
                Лес лес2 = new Лес { Название = "Березовая роща" };
                медв.ЛесОбитания = лес1;
                var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                var берлога2 = new Берлога { Наименование = "Для плохого настроения", ЛесРасположения = лес2 };
                медв.Берлога.Add(берлога1);
                медв.Берлога.Add(берлога2);
                var objs = new DataObject[] { медв, берлога2, берлога1, лес1, лес2 };
                args.DataService.UpdateObjects(ref objs);
                var expectedResult = (args.DataService as SQLDataService).Query<Медведь>(Медведь.Views.МедведьE).ToArray();
                int intParam = 0;

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/FunctionSelectExpandEntity(intParam={intParam})?$expand=Берлога($select=Наименование)";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(expectedResult[intParam].Вес, (int)(long)receivedDict["Вес"]);

                    for (int i = 0; i < expectedResult[intParam].Берлога.Count; i++)
                    {
                        Assert.Equal(expectedResult[intParam].Берлога[i].Наименование, (string)((JArray)receivedDict["Берлога"])[i].ToObject<Dictionary<string, object>>()["Наименование"]);
                        Assert.Equal(1, ((JArray)receivedDict["Берлога"])[i].ToObject<Dictionary<string, object>>().Count);
                    }
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку возвращаемых функциями OData значений, определяемых стандартом OData, как сущности.
        /// </summary>
        [Fact]
        public void TestFunctionEntity()
        {
            ActODataService(args =>
            {
                RegisterODataUserFunctions(args.Token.Functions, args.DataService);

                // Создаем объекты и кладем их в базу данных.
                DataObject[] countries = new DataObject[5];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = $"Страна №{i}",   };
                }

                args.DataService.UpdateObjects(ref countries);

                var expectedResult = (args.DataService as SQLDataService).Query<Страна>(Страна.Views.СтранаE).ToArray();

                int intParam = 3;

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/FunctionEntity(intParam={intParam})";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(expectedResult[intParam].Название, receivedDict["Название"]);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку с параметром $count=true возвращаемых функциями OData значений, определяемых стандартом OData, как коллекции сущностей.
        /// </summary>
        [Fact]
        public void TestCountFunctionEntitiesCollection()
        {
            ActODataService(args =>
            {
                RegisterODataUserFunctions(args.Token.Functions, args.DataService);

                // Создаем объекты и кладем их в базу данных.
                DataObject[] countries = new DataObject[5];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = $"Страна №{i}" };
                }

                args.DataService.UpdateObjects(ref countries);

                int intParam = 3;

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/FunctionEntitiesCollection(intParam={intParam})?$count=true";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    // Убедимся, что объекты получены и их нужное количество.
                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(((JArray)receivedDict["value"]).Count, intParam);

                    // Убедимся, что метаданные о количестве объектов получены.
                    Assert.True(receivedDict.ContainsKey("@odata.count"));

                    // Убедимся, что количество объектов в метаданных совпадает, с ожидаемым количеством.
                    object receivedMetadataCount = receivedDict["@odata.count"];
                    Assert.Equal((int)(long)receivedMetadataCount, intParam);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку возвращаемых функциями OData значений, определяемых стандартом OData, как коллекции сущностей.
        /// </summary>
        [Fact]
        public void TestFunctionEntitiesCollection()
        {
            ActODataService(args =>
            {
                RegisterODataUserFunctions(args.Token.Functions, args.DataService);

                // Создаем объекты и кладем их в базу данных.
                DataObject[] countries = new DataObject[5];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = $"Страна №{i}" };
                }

                args.DataService.UpdateObjects(ref countries);

                int intParam = 3;

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/FunctionEntitiesCollection(intParam={intParam})";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(intParam, ((JArray)receivedDict["value"]).Count);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку возвращаемых функциями OData значений, определяемых стандартом OData, как примитивные.
        /// </summary>
        [Fact]
        public void TestFunctionsPrimitiveValuesTest()
        {
            ActODataService(args =>
            {
                RegisterODataUserFunctions(args.Token.Functions, args.DataService);

                string returnValueString = "123456фывап";

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/FunctionString(stringParam='{returnValueString}')";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(returnValueString, receivedDict["value"]);
                }

                int returnValueInt = 1234567;

                // Формируем URL запроса к OData-сервису.
                requestUrl = $"http://localhost/odata/FunctionInt(intParam={returnValueInt})";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.True(receivedDict.ContainsKey("value"));
                    Assert.Equal(returnValueInt, (int)(long)receivedDict["value"]);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку проброса исключения, содержащего ODataError.
        /// </summary>
        [Fact]
        public void TestFunctionHttpResponseException()
        {
            ActODataService(args =>
            {
                RegisterODataUserFunctions(args.Token.Functions, args.DataService);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = $"http://localhost/odata/FunctionHttpResponseException()";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Проверим, что возвращается код ошибки, указанный в функции.
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                    // Проверим сообщение об ошибке.
                    Assert.Equal("Сообщение об ошибке", ((ODataError)((ObjectContent)response.Content).Value).Message);
                }
            });
        }
    }
}
