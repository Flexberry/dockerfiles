namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Script.Serialization;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Windows.Forms;

    using Xunit;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

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
                    Dictionary<string, object> receivedCountries = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedJsonCountries);

                    // Убедимся, что объекты получены и их нужное количество.
                    Assert.True(receivedCountries.ContainsKey("value"));
                    Assert.Equal(((ArrayList)receivedCountries["value"]).Count, 5);
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

                string requestUrl;
                // Проверка использования в фильтрации перечислений.
                requestUrl = "http://localhost/odata/Медведьs?$filter=Папа ne null&$expand=Папа,Мама&$orderby=Мама/Вес desc,Папа/Вес desc";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(4, ((ArrayList)receivedDict["value"]).Count);

                    int[] expectedValues = { 48, 22, 58, 62 };

                    for (int i = 0; i < expectedValues.Length; i++)
                    {
                        var медведь = ((ArrayList)receivedDict["value"])[i];
                        Assert.Equal(expectedValues[i], (int)((Dictionary<string, object>)медведь)["Вес"]);
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
                    Dictionary<string, object> receivedDict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(4, ((ArrayList)receivedDict["value"]).Count);

                    int[] expectedValues = { 22, 48, 62, 58 };

                    for (int i = 0; i < expectedValues.Length; i++)
                    {
                        var медведь = ((ArrayList)receivedDict["value"])[i];
                        Assert.Equal(expectedValues[i], (int)((Dictionary<string, object>)медведь)["Вес"]);
                    }
                }

            });
        }

    }
}
