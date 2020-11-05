namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using Xunit;

    /// <summary>
    /// A class for testing exports from Excel.
    /// </summary>
    public class ExcelExportTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        /// <param name="output">Вывод отладочной информации.</param>
        public ExcelExportTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output)
            : base(factory, output)
        {
        }
#endif

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

        /// <summary>
        /// Performs export testing from Excel.
        /// </summary>
        [Fact]
        public void ExportInvalidColumnNameTest()
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

                // The request URL to the OData service is generated.
                const string propertyName = "Название";
                const string caption = "Название, понятное/// название";
                string encodeInvalidColsOrder = string.Format(
                    "{0}/{1}",
                    HttpUtility.UrlEncode(propertyName),
                    HttpUtility.UrlEncode(caption));
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?{1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Страна)).Name,
                    $"exportExcel=true&colsOrder={WebUtility.UrlEncode(encodeInvalidColsOrder)}&detSeparateCols=false&detSeparateRows=false&$filter=contains(Название,'1')");
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    byte[] contentExcel = response.Content.ReadAsByteArrayAsync().Result;
                }
            });
        }

        /// <summary>
        /// Performs export testing from Excel for odata functions.
        /// </summary>
        [Fact]
        public void TestFunctionExportTest()
        {
            ActODataService(args =>
            {
                DataService = args.DataService as SQLDataService;
                args.Token.Functions.Register(new Func<QueryParameters, string, Страна[]>(FunctionExportExcel));

                // Create objects and put them in the database.
                DataObject[] countries = new DataObject[5];
                int countriesCount = countries.Length;
                for (int i = 0; i < countriesCount; i++)
                {
                    countries[i] = new Страна { Название = string.Format("Страна №{0}", i) };
                }

                DataService.UpdateObjects(ref countries);
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?{1}",
                    "FunctionExportExcel(entitySet='Странаs')",
                    "exportExcel=true&colsOrder=Название/Название&detSeparateCols=false&detSeparateRows=false&$filter=contains(Название,'1')");
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    byte[] contentExcel = response.Content.ReadAsByteArrayAsync().Result;
                }
            });
        }

        private SQLDataService DataService { get; set; }

        /// <summary>
        /// Функция подготавливающая данные для экспорта в Excel. Для правильной работы необходимо, чтобы в декларации был указан реальный тип возвращаемых значений.
        /// Не подходит указание типа DataObject.
        /// </summary>
        /// <param name="queryParameters"></param>
        /// <param name="entitySet"></param>
        /// <returns></returns>
        private Страна[] FunctionExportExcel(QueryParameters queryParameters, string entitySet)
        {
            Type type = queryParameters.GetDataObjectType(entitySet);
            LoadingCustomizationStruct lcs = queryParameters.CreateLcs(type);
            Страна[] dobjs = DataService.LoadObjects(lcs).Cast<Страна>().ToArray();
            return dobjs;
        }

    }
}
