namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System.Collections.Generic;
    using System.Net;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Unit-test class for filtering data through OData service by master details fields.
    /// </summary>
    public class FilterByMasterDetailFieldTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        /// <param name="output">Вывод отладочной информации.</param>
        public FilterByMasterDetailFieldTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output)
            : base(factory, output)
        {
        }
#endif

        /// <summary>
        /// Tests filtering data by master field.
        /// </summary>
        [Fact]
        public void TestFilterByMasterDetailField()
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

                string лес1Pk = ((KeyGuid)лес1.__PrimaryKey).Guid.ToString("D");

                string requestUrl = string.Format(
                "http://localhost/odata/{0}?$filter={1}",
                args.Token.Model.GetEdmEntitySet(typeof(Блоха)).Name,
                "МедведьОбитания/Берлога/any(f:(f/Наименование eq 'Берлога 1') and ( ( not(f/ЛесРасположения/Площадь eq 123)) or (f/ЛесРасположения/__PrimaryKey eq " + лес1Pk + ") ))");

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
        /// Tests filtering data by master field with complex predicate.
        /// </summary>
        [Fact]
        public void TestFilterByMasterDetailComplexPredicate()
        {
            ActODataService(args =>
            {
                // Arrange.
                Медведь медведь1 = new Медведь() { ПорядковыйНомер = 1, };
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

                string лес1Pk = ((KeyGuid)лес1.__PrimaryKey).Guid.ToString("D");
                string медведь1Pk = ((KeyGuid)медведь1.__PrimaryKey).Guid.ToString("D");

                string requestUrl = string.Format(
                "http://localhost/odata/{0}?$filter={1}",
                args.Token.Model.GetEdmEntitySet(typeof(Блоха)).Name,
                "(МедведьОбитания/Берлога/any(f:(f/Наименование eq 'Берлога 1') and ( ( not(f/ЛесРасположения/Площадь eq 123)) or (f/ЛесРасположения/__PrimaryKey eq " + лес1Pk + ") )) ) and (МедведьОбитания/__PrimaryKey eq " + медведь1Pk + ")");

                using (var response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);
                    Assert.Equal(2, ((JArray)receivedDict["value"]).Count);
                }
            });
        }
    }
}
