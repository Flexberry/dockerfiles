namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.UserDataTypes;
    using ICSSoft.STORMNET.Windows.Forms;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования применения $filter в OData-сервисе.
    /// </summary>
    public class FilterTest : BaseODataServiceIntegratedTest
    {
        [Fact]
        public void TestFilterStringKey()
        {
            ActODataService(args =>
            {
                var класс = new КлассСоСтроковымКлючом();
                Медведь медв = new Медведь { Вес = 48, Пол = tПол.Мужской };
                var objs = new DataObject[] { класс, медв };
                args.DataService.UpdateObjects(ref objs);

                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(КлассСоСтроковымКлючом)).Name,
                    $"__PrimaryKey eq '{класс.__PrimaryKey}'");

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
        /// Осуществляет проверку применения $filter в запросах OData для Nullable-типов из ICSSoft.STORMNET.UserDataTypes.
        /// Тест проверяет следующие факты:
        /// <list type="number">
        /// <item><description>Использование в фильтрации для NullableDateTime.</description></item>
        /// <item><description>Использование в фильтрации для NullableDecimal.</description></item>
        /// <item><description>Использование в фильтрации для NullableInt.</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public void TestFilterNullable()
        {
            ActODataService(args =>
            {
                NullableDateTime date = new NullableDateTime();
                date.Value = new DateTimeOffset(DateTime.Now).UtcDateTime;
                NullableInt i = new NullableInt();
                i.Value = 7;
                NullableDecimal d = new NullableDecimal();
                d.Value = new decimal(777.777);
                string prevDecimal = (d.Value - 1).ToString().Replace(",", ".");
                string nextDecimal = (d.Value + 1).ToString().Replace(",", ".");
                string prevDate = $"{date.Value.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss.fff")}%2B05:00";
                string nextDate = $"{date.Value.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss.fff")}%2B05:00";
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertyStormnetNullableInt = i, PropertyStormnetNullableDecimal = d, PropertyStormnetNullableDateTime = date, PropertyDateTime = date.Value };
                var objs = new DataObject[] { класс };
                args.DataService.UpdateObjects(ref objs);
                string requestUrl;
                /*
                // Проверка использования фильтрации для типа ICSSoft.STORMNET.UserDataTypes.NullableDateTime.
                requestUrl = $"http://localhost/odata/КлассСМножествомТиповs?$filter=(PropertyStormnetNullableDateTime gt {prevDate}) and (PropertyStormnetNullableDateTime lt {nextDate})";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(1, ((ArrayList)receivedDict["value"]).Count);
                }

                // Проверка использования фильтрации для типа ICSSoft.STORMNET.UserDataTypes.NullableDecimal.
                requestUrl = $"http://localhost/odata/КлассСМножествомТиповs?$filter=(PropertyStormnetNullableDecimal gt {prevDecimal}M) and (PropertyStormnetNullableDecimal lt {nextDecimal})";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(1, ((ArrayList)receivedDict["value"]).Count);
                }
                */

                // Проверка использования фильтрации для типа ICSSoft.STORMNET.UserDataTypes.NullableInt.
                requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(КлассСМножествомТипов)).Name,
                    $"PropertyStormnetNullableInt eq {i.Value.ToString()}");

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
        /// Осуществляет проверку применения $filter в запросах OData с использованием функций дат.
        /// </summary>
        [Fact]
        public void TestFilterNullableDateFunctions()
        {
            ActODataService(args =>
            {
                DateTime? date = new DateTime?(DateTime.UtcNow);
                ExternalLangDef.LanguageDef.DataService = args.DataService;
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertySystemNullableDateTime = date, PropertyDateTime = date.Value };
                var objs = new DataObject[] { класс };
                args.DataService.UpdateObjects(ref objs);

                // Проверка использования фильтрации для типа System.DateTime?.
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(КлассСМножествомТипов)).Name,
                    $"day(PropertySystemNullableDateTime) eq {date.Value.Day}");

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
        /// Осуществляет проверку применения $filter в запросах OData.
        /// Тест проверяет следующие факты:
        /// <list type="number">
        /// <item><description>Использование в фильтрации для перечислений.</description></item>
        /// <item><description>Использование в фильтрации для DateTime.</description></item>
        /// <item><description>Использование в фильтрации для поля __PrimaryKey.</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public void TestFilter()
        {
            ActODataService(args =>
            {
                DateTime date = new DateTimeOffset(DateTime.Now).UtcDateTime;
                string prevDate = $"{date.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss")}%2B05:00";
                string nextDate = $"{date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")}%2B05:00";
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
                var objs = new DataObject[] { класс, медв, медв2, берлога2, берлога1, берлога3, лес1, лес2 };
                args.DataService.UpdateObjects(ref objs);

                // Проверка использования в фильтрации перечислений.
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                    "Пол eq NewPlatform.Flexberry.ORM.ODataService.Tests.tПол'Мужской'");

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

                // Проверка использования в фильтрации DateTime.
                requestUrl = $"http://localhost/odata/КлассСМножествомТиповs?$filter=(PropertyDateTime gt {prevDate}) and (PropertyDateTime lt {nextDate})";

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

                // Проверка использования в фильтрации поля __PrimaryKey.
                string strGuid = ((ICSSoft.STORMNET.KeyGen.KeyGuid)медв.__PrimaryKey).Guid.ToString("D");
                requestUrl = $"http://localhost/odata/Медведьs?$filter=__PrimaryKey eq {strGuid}";

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
        /// Осуществляет проверку применения $filter в запросах OData для нехранимых полей.
        /// Тест проверяет следующие факты:
        /// <list type="number">
        /// <item><description>Использование в фильтрации для нехранимых полей.</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public void TestFilterNotStored()
        {
            ActODataService(args =>
            {
                DataObject класс = new КлассСМножествомТипов() { PropertyInt = 15, PropertyDateTime = DateTime.Now };
                args.DataService.UpdateObject(ref класс);

                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(КлассСМножествомТипов)).Name,
                    "NotStoredProperty eq 15");

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
    }
}
