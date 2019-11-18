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
    /// Класс тестов для тестирования $skip, $top, $orderby.
    /// </summary>
    public class SkipTopOrderByTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Осуществляет проверку поиска с использованием $skip, $top, $orderby.
        /// </summary>
        [Fact]
        public void TestSkipTopOrderBy()
        {
            ActODataService(args =>
            {
                // Создаем объекты и кладем их в базу данных.
                DataObject[] countries = new DataObject[15];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = string.Format("Страна №{0}", i) };
                }

                args.DataService.UpdateObjects(ref countries);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}?{1}", args.Token.Model.GetEdmEntitySet(typeof(Страна)).Name, "$count=true&$skip=0&$top=5&$orderby=Название desc");

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedJsonCountries = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedCountries = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonCountries);

                    // Убедимся, что объекты получены и их нужное количество.
                    Assert.True(receivedCountries.ContainsKey("value"));
                    Assert.Equal(((JArray)receivedCountries["value"]).Count, 5);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку поиска с $orderby с использованием полей мастера.
        /// </summary>
        [Fact]
        public void MasterFieldsOrderBy()
        {
            ActODataService(args =>
            {
                ExternalLangDef.LanguageDef.DataService = args.DataService;
                Страна страна1 = new Страна { Название = "Россия" };
                Страна страна2 = new Страна { Название = "Белоруссия" };
                Лес лес1 = new Лес { Название = "Бор" };
                Лес лес2 = new Лес { Название = "Березовая роща" };
                Медведь медвПапа1 = new Медведь { СтранаРождения = страна1, ЛесОбитания = лес1, ПорядковыйНомер = 1, Вес = 200, Пол = tПол.Мужской };
                медвПапа1.СтранаРождения = страна1;
                Медведь медвПапа2 = new Медведь { СтранаРождения = страна1, ЛесОбитания = лес1, ПорядковыйНомер = 2, Вес = 150, Пол = tПол.Мужской };
                Медведь медвМама1 = new Медведь { СтранаРождения = страна1, ЛесОбитания = лес2, ПорядковыйНомер = 3, Вес = 120, Пол = tПол.Женский };
                Медведь медвМама2 = new Медведь { СтранаРождения = страна1, ЛесОбитания = лес2, ПорядковыйНомер = 3, Вес = 110, Пол = tПол.Женский };
                Медведь медвежонок1 = new Медведь { СтранаРождения = страна2, ЛесОбитания = лес2, Папа = медвПапа1, Мама = медвМама1, Вес = 48, Пол = tПол.Мужской };
                Медведь медвежонок2 = new Медведь { СтранаРождения = страна2, ЛесОбитания = лес2, Папа = медвПапа2, Мама = медвМама1, Вес = 22, Пол = tПол.Мужской };
                Медведь медвежонок3 = new Медведь { СтранаРождения = страна2, ЛесОбитания = лес2, Папа = медвПапа1, Мама = медвМама2, Вес = 58, Пол = tПол.Мужской };
                Медведь медвежонок4 = new Медведь { СтранаРождения = страна2, ЛесОбитания = лес2, Папа = медвПапа2, Мама = медвМама2, Вес = 62, Пол = tПол.Мужской };
                var objs = new DataObject[] { страна1, страна2, медвМама1, медвМама2, медвПапа1, медвПапа2, лес1, лес2 };
                args.DataService.UpdateObjects(ref objs);
                objs = new DataObject[] { медвежонок1, медвежонок2, медвежонок3, медвежонок4 };
                args.DataService.UpdateObjects(ref objs);

                // Проверка использования в фильтрации перечислений.
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}&$expand={2}",
                    args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                    "Папа ne null",
                    "Папа,Мама&$orderby=Мама/Вес desc,Папа/Вес desc");

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(4, ((JArray)receivedDict["value"]).Count);

                    int[] expectedValues = { 48, 22, 58, 62 };

                    for (int i = 0; i < expectedValues.Length; i++)
                    {
                        var медведь = ((JArray)receivedDict["value"])[i];
                        Assert.Equal(expectedValues[i], (int)(long)медведь.ToObject<Dictionary<string, object>>()["Вес"]);
                    }
                }

                requestUrl = "http://localhost/odata/Медведьs?$filter=Папа ne null&$expand=Папа,Мама&$orderby=Мама/Вес desc,Папа/Вес";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(4, ((JArray)receivedDict["value"]).Count);

                    int[] expectedValues = { 22, 48, 62, 58 };

                    for (int i = 0; i < expectedValues.Length; i++)
                    {
                        var медведь = ((JArray)receivedDict["value"])[i];
                        Assert.Equal(expectedValues[i], (int)(long)медведь.ToObject<Dictionary<string, object>>()["Вес"]);
                    }
                }

            });
        }

        /// <summary>
        /// Осуществляет проверку поиска с $orderby для поля NullableDateTime.
        /// </summary>
        [Fact]
        public void NullableDateTimeOrderBy()
        {
            ActODataService(args =>
            {
                ExternalLangDef.LanguageDef.DataService = args.DataService;
                NullableDateTime dt1 = (NullableDateTime)new DateTime(2017, 11, 21);
                NullableDateTime dt2 = (NullableDateTime)new DateTime(2017, 10, 21);
                NullableDateTime dt3 = (NullableDateTime)new DateTime(2017, 09, 21);
                NullableDateTime dt4 = (NullableDateTime)new DateTime(2017, 08, 21);
                Лес лес1 = new Лес { Название = "Еловый", ДатаПоследнегоОсмотра = dt1 };
                Лес лес2 = new Лес { Название = "Сосновый", ДатаПоследнегоОсмотра = dt2 };
                Лес лес3 = new Лес { Название = "Смешанный", ДатаПоследнегоОсмотра = dt3 };
                Лес лес4 = new Лес { Название = "Березовый", ДатаПоследнегоОсмотра = dt4 };

                var objs = new DataObject[] { лес1, лес2, лес3, лес4 };
                args.DataService.UpdateObjects(ref objs);

                // Проверка использования в фильтрации перечислений.
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$orderby={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Лес)).Name,
                    "ДатаПоследнегоОсмотра desc");

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(4, ((JArray)receivedDict["value"]).Count);

                    NullableDateTime[] expectedValues = { dt1, dt2, dt3, dt4 };

                    for (int i = 0; i < expectedValues.Length; i++)
                    {
                        var лес = ((JArray)receivedDict["value"])[i];
                        Assert.Equal(expectedValues[i], (NullableDateTime)new DateTimeOffset((DateTime)лес.ToObject<Dictionary<string, object>>()["ДатаПоследнегоОсмотра"]).UtcDateTime);
                    }
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку поиска с $orderby для поля NullableDateTime для мастера.
        /// </summary>
        [Fact]
        public void NullableDateTimeOrderByMaster()
        {
            ActODataService(args =>
            {
                ExternalLangDef.LanguageDef.DataService = args.DataService;
                NullableDateTime dt1 = (NullableDateTime)new DateTime(2017, 11, 21);
                NullableDateTime dt2 = (NullableDateTime)new DateTime(2017, 10, 21);
                NullableDateTime dt3 = (NullableDateTime)new DateTime(2017, 09, 21);
                NullableDateTime dt4 = (NullableDateTime)new DateTime(2017, 08, 21);
                Лес лес1 = new Лес { Название = "Еловый", ДатаПоследнегоОсмотра = dt1 };
                Лес лес2 = new Лес { Название = "Сосновый", ДатаПоследнегоОсмотра = dt2 };
                Лес лес3 = new Лес { Название = "Смешанный", ДатаПоследнегоОсмотра = dt3 };
                Лес лес4 = new Лес { Название = "Березовый", ДатаПоследнегоОсмотра = dt4 };
                Медведь медведь1 = new Медведь { Пол = tПол.Мужской, ЛесОбитания = лес1 };
                Медведь медведь2 = new Медведь { Пол = tПол.Женский, ЛесОбитания = лес2 };
                Медведь медведь3 = new Медведь { Пол = tПол.Мужской, ЛесОбитания = лес3 };
                Медведь медведь4 = new Медведь { Пол = tПол.Женский, ЛесОбитания = лес4 };

                var objs = new DataObject[] { медведь1, медведь2, медведь3, медведь4 };
                args.DataService.UpdateObjects(ref objs);

                // Проверка использования в фильтрации перечислений.
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$expand={1}&$orderby={2}",
                    args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                    "ЛесОбитания($select=ДатаПоследнегоОсмотра)",
                    "ЛесОбитания/ДатаПоследнегоОсмотра desc");

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(4, ((JArray)receivedDict["value"]).Count);

                    NullableDateTime[] expectedValues = { dt1, dt2, dt3, dt4 };

                    for (int i = 0; i < expectedValues.Length; i++)
                    {
                        var медведь = ((JArray)receivedDict["value"])[i];
                        var лес = медведь.ToObject<Dictionary<string, JToken>>()["ЛесОбитания"];
                        Assert.Equal(expectedValues[i], (NullableDateTime)new DateTimeOffset((DateTime)лес.ToObject<Dictionary<string, object>>()["ДатаПоследнегоОсмотра"]).UtcDateTime);
                    }
                }
            });
        }
    }
}
