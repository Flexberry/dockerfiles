namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Windows.Forms;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования применения $filter в OData-сервисе.
    /// </summary>
    public class BuiltinQueryFunctionsTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Осуществляет проверку применения функций any и all в запросах OData.
        /// Тест проверяет следующие факты:
        /// <list type="number">
        /// <item><description>Использование в фильтрации функции any.</description></item>
        /// <item><description>Использование в фильтрации функции all.</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public void TestFilterAnyAll()
        {
            ActODataService(args =>
            {
                ExternalLangDef.LanguageDef.DataService = args.DataService;

                DateTime date = new DateTimeOffset(DateTime.Now).UtcDateTime;
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertyEnum = Цифра.Семь, PropertyDateTime = date };
                Медведь медв = new Медведь { Вес = 48, Пол = tПол.Мужской };
                Медведь медв2 = new Медведь { Вес = 148, Пол = tПол.Мужской };
                Лес лес1 = new Лес { Название = "Бор" };
                Лес лес2 = new Лес { Название = "Березовая роща" };
                медв.ЛесОбитания = лес1;
                var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                var берлога2 = new Берлога { Наименование = "Для плохого настроения", ЛесРасположения = лес2 };
                var берлога3 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                медв.Берлога.Add(берлога1);
                медв.Берлога.Add(берлога2);
                медв2.Берлога.Add(берлога3);
                Блоха блоха = new Блоха() { Кличка = "1", МедведьОбитания = медв };
                var objs = new DataObject[] { класс, медв, медв2, берлога2, берлога1, берлога3, лес1, лес2, блоха };
                args.DataService.UpdateObjects(ref objs);
                string requestUrl;

                // Проверка использования в фильтрации функции any.
                requestUrl = "http://localhost/odata/Медведьs?$expand=Берлога&$filter=Берлога/any(f:f/Наименование eq 'Для хорошего настроения')";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(2, ((JArray)receivedDict["value"]).Count);
                }

                // Проверка использования в фильтрации функции all.
                requestUrl = $"http://localhost/odata/Медведьs?$filter=Берлога/all(f:f/Наименование eq 'Для хорошего настроения')";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку применения функций $select, $expand, $select  $expand, а так же запроса беза опций.
        /// </summary>
        [Fact]
        public void TestGuid()
        {
            ActODataService(args =>
            {
                ExternalLangDef.LanguageDef.DataService = args.DataService;

                DateTime date = new DateTimeOffset(DateTime.Now).UtcDateTime;
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertyEnum = Цифра.Семь, PropertyDateTime = date };
                Медведь медв = new Медведь { Вес = 48, Пол = tПол.Мужской, __PrimaryKey = new Guid("3f5cc1ca-6b2c-4c38-ba02-4b3fd5f1726c") };
                Медведь медв2 = new Медведь { Вес = 148, Пол = tПол.Мужской };
                Лес лес1 = new Лес { Название = "Бор" };
                Лес лес2 = new Лес { Название = "Березовая роща" };
                медв.ЛесОбитания = лес1;
                var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                var берлога2 = new Берлога { Наименование = "Для плохого настроения", ЛесРасположения = лес2 };
                var берлога3 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                медв.Берлога.Add(берлога1);
                медв.Берлога.Add(берлога2);
                медв2.Берлога.Add(берлога3);
                Блоха блоха = new Блоха() { Кличка = "1", МедведьОбитания = медв };
                var objs = new DataObject[] { класс, медв, медв2, берлога2, берлога1, берлога3, лес1, лес2, блоха };
                args.DataService.UpdateObjects(ref objs);
                string requestUrl;

                // Проверка запроса без опций
                requestUrl = "http://localhost/odata/Медведьs(3f5cc1ca-6b2c-4c38-ba02-4b3fd5f1726c)";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    // Должны возвращаться собственные свойства + @odata.context
                    Assert.Equal(14, receivedDict.Count);
                    Assert.Equal(null, receivedDict["Creator"]);
                    Assert.Equal(48, (int)(long)receivedDict["Вес"]);
                }

                // Проверка запроса с $select
                requestUrl = "http://localhost/odata/Медведьs(3f5cc1ca-6b2c-4c38-ba02-4b3fd5f1726c)?$select=Вес,Пол";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    // Должны возвращаться свойства, перечисленные в select + @odata.context
                    Assert.Equal(3, receivedDict.Count);
                    Assert.Equal("http://localhost/odata/$metadata#Медведьs(Вес,Пол)/$entity", receivedDict["@odata.context"]);
                    Assert.Equal("Мужской", receivedDict["Пол"]);
                    Assert.Equal(48, (int)(long)receivedDict["Вес"]);
                }

                // Проверка запроса с $expand
                requestUrl = "http://localhost/odata/Медведьs(3f5cc1ca-6b2c-4c38-ba02-4b3fd5f1726c)?$expand=Берлога";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    // Должны возвращаться собственные свойства + свойства, перечисленные в Expand + @odata.context
                    Assert.Equal(15, receivedDict.Count);

                    // У медведя с таким первичным ключом две берлоги
                    Assert.Equal(2, ((JArray)receivedDict["Берлога"]).Count);
                    Assert.Equal(48, (int)(long)receivedDict["Вес"]);
                }

                // Проверка запроса с $expand и $select
                requestUrl = "http://localhost/odata/Медведьs(3f5cc1ca-6b2c-4c38-ba02-4b3fd5f1726c)?$expand=Берлога($select=Наименование)";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    // Должны возвращаться собственные свойства + свойства, перечисленные в Expand + @odata.context
                    Assert.Equal(15, receivedDict.Count);

                    // У медведя с таким первичным ключом две берлоги
                    Assert.Equal(2, ((JArray)receivedDict["Берлога"]).Count);

                    // Для каждой берлоги должно вернуться название
                    var берлоги = new List<Dictionary<string, object>>();
                    берлоги.Add(((JArray)receivedDict["Берлога"])[0].ToObject<Dictionary<string, object>>());
                    берлоги.Add(((JArray)receivedDict["Берлога"])[1].ToObject<Dictionary<string, object>>());
                    берлоги = берлоги.OrderBy(x => x["Наименование"]).ToList();

                    Assert.Equal("Для плохого настроения", берлоги.First()["Наименование"]);

                    Assert.Equal("Для хорошего настроения", берлоги.Last()["Наименование"]);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку применения $filter с функцией isof.
        /// Тест проверяет следующие факты:
        /// <list type="number">
        /// <item><description>Использование $filter с функцией isof.</description></item>
        /// <item><description>Без использования $filter. Должны вернуться также сущности, имеющие дочерний тип.</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public void TestFilterIsOf()
        {
            ActODataService(args =>
            {
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertyString = "parent", PropertyDateTime = new DateTimeOffset(DateTime.Now).UtcDateTime };
                ДочернийКласс класс2 = new ДочернийКласс() { ChildProperty = "child01", PropertyString = "child1", PropertyDateTime = new DateTimeOffset(DateTime.Now).UtcDateTime };
                ДочернийКласс класс3 = new ДочернийКласс() { ChildProperty = "child02", PropertyString = "child2", PropertyDateTime = new DateTimeOffset(DateTime.Now).UtcDateTime };

                // КлассNotStored классNotStored = new КлассNotStored() { StrAttr = "notStored" };
                КлассStoredDerived классStoredDerived = new КлассStoredDerived() { StrAttr = "Stored", StrAttr2 = "str2" };
                var objs = new DataObject[] { класс, класс2, класс3, классStoredDerived };
                args.DataService.UpdateObjects(ref objs);
                string requestUrl;

                // Использование $filter с функцией isof. Должны вернуться сущности всех типов, родительские и дочерние.
                requestUrl = $"http://localhost/odata/КлассСМножествомТиповs?$filter=isof('NewPlatform.Flexberry.ORM.ODataService.Tests.КлассСМножествомТипов') or isof('NewPlatform.Flexberry.ORM.ODataService.Tests.ДочернийКласс')";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(3, ((JArray)receivedDict["value"]).Count);
                }

                // Использование $filter с функцией isof. Должны вернуться сущности только дочернего типа.
                requestUrl = $"http://localhost/odata/КлассСМножествомТиповs?$filter=isof('NewPlatform.Flexberry.ORM.ODataService.Tests.ДочернийКласс')";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(2, ((JArray)receivedDict["value"]).Count);
                }

                // Без использования $filter. Должны вернуться также сущности, имеющие дочерний тип.
                requestUrl = $"http://localhost/odata/КлассСМножествомТиповs";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(3, ((JArray)receivedDict["value"]).Count);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку применения функции now() в запросах OData.
        /// </summary>
        [Fact(Skip = "Sometimes this test work incorrect")]
        public void TestFilterNow()
        {
            ActODataService(args =>
            {
                ExternalLangDef.LanguageDef.DataService = args.DataService;

                DateTime date = DateTime.UtcNow;
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertyEnum = Цифра.Семь, PropertyDateTime = date };
                var objs = new DataObject[] { класс };
                args.DataService.UpdateObjects(ref objs);
                string requestUrl;

                requestUrl = "http://localhost/odata/КлассСМножествомТиповs?$filter=PropertyDateTime ge now()";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(0, ((JArray)receivedDict["value"]).Count);
                }

                requestUrl = $"http://localhost/odata/КлассСМножествомТиповs?$filter=PropertyDateTime le now()";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
                }

                requestUrl = $"http://localhost/odata/КлассСМножествомТиповs?$filter=PropertyDateTime eq now()";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(0, ((JArray)receivedDict["value"]).Count);
                }
            });
        }
    }
}
