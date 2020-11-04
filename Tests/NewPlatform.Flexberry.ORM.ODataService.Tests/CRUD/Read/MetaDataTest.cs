namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования метаданных, получаемых от OData-сервиса.
    /// </summary>
    public class MetaDataTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        /// <param name="output">Вывод отладочной информации.</param>
        public MetaDataTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output)
            : base(factory, output)
        {
        }
#endif

        /// <summary>
        /// Осуществляет проверку того, что при запросах с параметром $count=true, возвращаются метаданные с количеством присланных объектов.
        /// </summary>
        [Fact]
        public void ObjectsWithCountTest()
        {
            ActODataService(args =>
            {
                // Создаем объекты и кладем их в базу данных.
                DataObject[] countries = new DataObject[5];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = string.Format("Страна №{0}", i) };
                }

                args.DataService.UpdateObjects(ref countries);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}?{1}", args.Token.Model.GetEdmEntitySet(typeof(Страна)).Name, "$count=true");

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
                    Assert.Equal(((JArray)receivedCountries["value"]).Count, countriesCount);

                    // Убедимся, что метаданные о количестве объектов получены.
                    Assert.True(receivedCountries.ContainsKey("@odata.count"));

                    // Убедимся, что количество объектов в метаданных совпадает, с ожидаемым количеством.
                    object receivedMetadataCount = receivedCountries["@odata.count"];
                    Assert.Equal((int)(long)receivedMetadataCount, countriesCount);
                }
            });
        }
    }
}
