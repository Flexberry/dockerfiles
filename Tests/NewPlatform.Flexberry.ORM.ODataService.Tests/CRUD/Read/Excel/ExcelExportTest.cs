namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System.Net;
    using System.Net.Http;
    using ICSSoft.STORMNET;
    using Xunit;

    /// <summary>
    /// Класс для тестирования экспорта из Excel.
    /// </summary>
    public class ExcelExportTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Осуществляет тестирование экспорта из Excel.
        /// </summary>
        [Fact]
        public void ExportTest()
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
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?{1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Страна)).Name,
                    "exportExcel=true&colsOrder=Название/Название&detSeparateCols=false&detSeparateRows=false&$filter=contains(Название,'1')");

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    byte[] contentExcel = response.Content.ReadAsByteArrayAsync().Result;
                }
            });
        }
    }
}
