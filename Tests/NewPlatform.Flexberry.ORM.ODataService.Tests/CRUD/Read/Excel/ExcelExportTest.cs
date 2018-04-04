namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System.Net;
    using System.Net.Http;
    using ICSSoft.STORMNET;
    using Xunit;

    /// <summary>
    /// A class for testing exports from Excel.
    /// </summary>
    public class ExcelExportTest : SelfHostBaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Performs export testing from Excel.
        /// </summary>
        [Fact]
        public void ExportTest()
        {
            ActODataService(args =>
            {
                // Create objects and put them in the database.
                DataObject[] countries = new DataObject[5];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = string.Format("Страна №{0}", i) };
                }

                args.DataService.UpdateObjects(ref countries);
                this.ShowPauseDialog();
                // The request URL to the OData service is generated.
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?{1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Страна)).Name,
                    "exportExcel=true&colsOrder=Название/Название&detSeparateCols=false&detSeparateRows=false&$filter=contains(Название,'1')");

                // A request is made to the OData service and the response is processed.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    byte[] contentExcel = response.Content.ReadAsByteArrayAsync().Result;
                }
            });
        }
    }
}
