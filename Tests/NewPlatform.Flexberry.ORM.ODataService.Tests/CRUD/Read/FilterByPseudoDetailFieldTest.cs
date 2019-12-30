namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System.Collections.Generic;
    using System.Net;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Windows.Forms;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Unit-test class for filtering data through OData service by pseudodetail field.
    /// </summary>
    public class FilterByPseudoDetailFieldTest : BaseODataServiceIntegratedTest
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

        public FilterByPseudoDetailFieldTest()
            : base(
                  @"РТЦ Тестирование и документирование\Модели для юнит-тестов\Flexberry ORM\NewPlatform.Flexberry.ORM.ODataService.Tests\",
                  false,
                  false,
                  GetPseudoDetailDefinitions())
        {
        }

        /// <summary>
        /// Tests filtering data by pseudodetail field with EmptyAny.
        /// </summary>
        [Fact]
        public void TestFilterByPseudoDetailFieldEmptyAny()
        {
            ActODataService(args =>
            {
                // Arrange.
                Медведь медведь1 = new Медведь() { ПорядковыйНомер = 1 };
                Медведь медведь2 = new Медведь() { ПорядковыйНомер = 2 };

                Блоха блоха1 = new Блоха() { Кличка = "Блоха 1", МедведьОбитания = медведь1 };
                Блоха блоха2 = new Блоха() { Кличка = "Блоха 2", МедведьОбитания = медведь2 };
                Блоха блоха3 = new Блоха() { Кличка = "Блоха 3" };
                Блоха блоха4 = new Блоха() { Кличка = "Блоха 4", МедведьОбитания = медведь1 };

                DataObject[] newDataObjects = new DataObject[] { медведь1, медведь2, блоха1, блоха2, блоха3, блоха4 };

                args.DataService.UpdateObjects(ref newDataObjects);
                ExternalLangDef.LanguageDef.DataService = args.DataService;

                string requestUrl = string.Format(
                "http://localhost/odata/{0}?$filter={1}",
                args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                "Блохи/any()");

                using (var response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);
                    Assert.Equal(2, ((JArray)receivedDict["value"]).Count);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            });
        }

        /// <summary>
        /// Tests filtering data by pseudodetail field with NonEmptyAny.
        /// </summary>
        [Fact]
        public void TestFilterByPseudoDetailFieldNonEmptyAny()
        {
            ActODataService(args =>
            {
                // Arrange.
                Медведь медведь1 = new Медведь() { ПорядковыйНомер = 1 };
                Медведь медведь2 = new Медведь() { ПорядковыйНомер = 2 };

                Блоха блоха1 = new Блоха() { Кличка = "Блоха 1", МедведьОбитания = медведь1 };
                Блоха блоха2 = new Блоха() { Кличка = "Блоха 2", МедведьОбитания = медведь2 };
                Блоха блоха3 = new Блоха() { Кличка = "Блоха 3" };
                Блоха блоха4 = new Блоха() { Кличка = "Блоха 4", МедведьОбитания = медведь1 };

                DataObject[] newDataObjects = new DataObject[] { медведь1, медведь2, блоха1, блоха2, блоха3, блоха4 };

                args.DataService.UpdateObjects(ref newDataObjects);
                ExternalLangDef.LanguageDef.DataService = args.DataService;

                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                    "Блохи/any(f:(f/Кличка eq 'Блоха 1'))");

                using (var response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);
                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            });
        }

        /// <summary>
        /// Tests filtering data by pseudodetail field with All.
        /// </summary>
        [Fact]
        public void TestFilterByPseudoDetailFieldAll()
        {
            ActODataService(args =>
            {
                // Arrange.
                Медведь медведь1 = new Медведь() { ПорядковыйНомер = 1 };
                Медведь медведь2 = new Медведь() { ПорядковыйНомер = 2 };

                Блоха блоха1 = new Блоха() { Кличка = "Блоха 1", МедведьОбитания = медведь1 };
                Блоха блоха2 = new Блоха() { Кличка = "Блоха 2", МедведьОбитания = медведь2 };
                Блоха блоха3 = new Блоха() { Кличка = "Блоха 3" };
                Блоха блоха4 = new Блоха() { Кличка = "Блоха 4", МедведьОбитания = медведь1 };

                DataObject[] newDataObjects = new DataObject[] { медведь1, медведь2, блоха1, блоха2, блоха3, блоха4 };

                args.DataService.UpdateObjects(ref newDataObjects);
                ExternalLangDef.LanguageDef.DataService = args.DataService;

                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                    "Блохи/all(f:(f/Кличка eq 'Блоха 2'))");

                using (var response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);
                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            });
        }

        /// <summary>
        /// Tests filtering data by pseudodetail and detail fields.
        /// </summary>
        [Fact]
        public void TestFilterByPseudoDetailAndDetailFields()
        {
            ActODataService(args =>
            {
                // Arrange.
                Медведь медведь1 = new Медведь() { ПорядковыйНомер = 1 };
                Медведь медведь2 = new Медведь() { ПорядковыйНомер = 2 };

                Лес лес1 = new Лес() { Название = "Шишкин" };
                Лес лес2 = new Лес() { Название = "Ёжкин" };

                Берлога берлога1 = new Берлога() { Наименование = "Берлога 1", ЛесРасположения = лес1 };
                Берлога берлога2 = new Берлога() { Наименование = "Берлога 2", ЛесРасположения = лес1 };
                Берлога берлога3 = new Берлога() { Наименование = "Берлога 3", ЛесРасположения = лес2 };
                Берлога берлога4 = new Берлога() { Наименование = "Берлога 4", ЛесРасположения = лес2 };

                медведь1.Берлога.AddRange(берлога1, берлога2);
                медведь2.Берлога.AddRange(берлога3, берлога4);

                Блоха блоха1 = new Блоха() { Кличка = "Блоха 1", МедведьОбитания = медведь1 };
                Блоха блоха2 = new Блоха() { Кличка = "Блоха 2", МедведьОбитания = медведь2 };
                Блоха блоха3 = new Блоха() { Кличка = "Блоха 3" };
                Блоха блоха4 = new Блоха() { Кличка = "Блоха 4", МедведьОбитания = медведь1 };

                DataObject[] newDataObjects = new DataObject[] { лес1, лес2, медведь1, медведь2, берлога1, берлога2, берлога3, берлога4, блоха1, блоха2, блоха3, блоха4 };

                args.DataService.UpdateObjects(ref newDataObjects);
                ExternalLangDef.LanguageDef.DataService = args.DataService;

                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                    "Блохи/any(f:(f/Кличка eq 'Блоха 1')) and Берлога/any(f:(f/Наименование eq 'Берлога 1'))");

                using (var response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);
                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            });
        }
    }
}
