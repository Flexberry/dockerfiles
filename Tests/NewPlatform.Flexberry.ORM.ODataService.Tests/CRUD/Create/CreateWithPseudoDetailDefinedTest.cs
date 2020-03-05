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

        public CreateWithPseudoDetailDefinedTest() : base(pseudoDetailDefinitions: GetPseudoDetailDefinitions())
        {
        }

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
            ActODataService(async (args) =>
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

                using (HttpResponseMessage response = await args.HttpClient.SendAsync(batchRequest))
                {
                    // Убедимся, что запрос завершился успешно.
                    CheckODataBatchResponseStatusCode(response, new HttpStatusCode[] { HttpStatusCode.Created });
                }
            });
        }
    }
}
