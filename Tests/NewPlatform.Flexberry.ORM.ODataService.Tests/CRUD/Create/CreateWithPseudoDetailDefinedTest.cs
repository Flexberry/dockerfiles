namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Create
{
    using System.Net;
    using System.Net.Http;
    using ICSSoft.STORMNET;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Xunit;

    /// <summary>
    /// Unit-test class for creation entity instance with pseudodetail field defined through OData service.
    /// </summary>
    public class CreateWithPseudoDetailDefinedTest : BaseODataServiceIntegratedTest
    {
        private static PseudoDetailDefinitions GetPseudoDetailDefinitions()
        {
            var pseudoDetailDefinitions = new PseudoDetailDefinitions();

            pseudoDetailDefinitions.Add(new DefaultPseudoDetailDefinition<Медведь, Блоха>(
                Блоха.Views.PseudoDetailView,
                Information.ExtractPropertyPath<Блоха>(x => x.МедведьОбитания),
                "Блохи"));

            return pseudoDetailDefinitions;
        }

#if NETFRAMEWORK
        public CreateWithPseudoDetailDefinedTest() : base(pseudoDetailDefinitions: GetPseudoDetailDefinitions())
        {
        }
#endif
#if NETCOREAPP
        public CreateWithPseudoDetailDefinedTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output) : base(factory, output, pseudoDetailDefinitions: GetPseudoDetailDefinitions())
        {
        }
#endif

        /// <summary>
        /// Tests the creation of entity instance with pseudodetail field defined.
        /// </summary>
        [Fact]
        public void TestCreate()
        {
            ActODataService(args =>
            {
                // Arrange.
                Медведь медведь = new Медведь() { ПорядковыйНомер = 1 };

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name);

                string bearJson = медведь.ToJson(Медведь.Views.МедведьE, args.Token.Model);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, bearJson).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                }
            });
        }

        /// <summary>
        /// Tests the batch creation of entity instance with pseudodetail field defined.
        /// </summary>
        [Fact]
        public void TestBatchCreate()
        {
            ActODataService(args =>
            {
                // Arrange.
                Медведь медведь = new Медведь() { ПорядковыйНомер = 1 };

                const string baseUrl = "http://localhost/odata";

                string[] changesets = new[]
                {
                    CreateChangeset(
                        $"{baseUrl}/{args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name}",
                        "{}",
                        медведь),
                };
                HttpRequestMessage batchRequest = CreateBatchRequest(baseUrl, changesets);

                using (HttpResponseMessage response = args.HttpClient.SendAsync(batchRequest).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    CheckODataBatchResponseStatusCode(response, new[] { HttpStatusCode.Created });
                }
            });
        }

        /// <summary>
        /// Tests the batch creation of entity instance with pseudodetail field defined.
        /// </summary>
        [Fact]
        public void TestBatchCreateWithDetails()
        {
            ActODataService(args =>
            {
                // Arrange.
                Медведь медведь = new Медведь() { ПорядковыйНомер = 1 };
                var берлога1 = new Берлога { Наименование = "Для хорошего настроения", Медведь = медведь };

                const string baseUrl = "http://localhost/odata";

                string[] changesets = new[]
                {
                    CreateChangeset(
                        $"{baseUrl}/{args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name}",
                        медведь.ToJson(Медведь.Views.МедведьE, args.Token.Model),
                        медведь),
                    CreateChangeset(
                        $"{baseUrl}/{args.Token.Model.GetEdmEntitySet(typeof(Берлога)).Name}",
                        берлога1.ToJson(Берлога.Views.БерлогаE, args.Token.Model),
                        берлога1),
                };
                HttpRequestMessage batchRequest = CreateBatchRequest(baseUrl, changesets);

                using (HttpResponseMessage response = args.HttpClient.SendAsync(batchRequest).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    CheckODataBatchResponseStatusCode(response, new[] { HttpStatusCode.Created, HttpStatusCode.Created });
                }
            });
        }
    }
}
