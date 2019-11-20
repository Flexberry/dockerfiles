namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Update
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers;

    using Newtonsoft.Json;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования бизнес-серверов.
    /// </summary>
    public class BusinessServersTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Осуществляет проверку того, что при POST запросах происходит вызов бизнес-сервера.
        /// </summary>
        [Fact]
        public void BSTest()
        {
            ActODataService(args =>
            {
                args.HttpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
                ExternalLangDef.LanguageDef.DataService = args.DataService;

                string[] берлогаPropertiesNames =
                {
                    Information.ExtractPropertyPath<Берлога>(x => x.ПолеБС),
                    Information.ExtractPropertyPath<Берлога>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Берлога>(x => x.Наименование),
                    Information.ExtractPropertyPath<Берлога>(x => x.Заброшена)
                };
                string[] медвPropertiesNames =
                {
                    Information.ExtractPropertyPath<Медведь>(x => x.ПолеБС),
                    Information.ExtractPropertyPath<Медведь>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Медведь>(x => x.Вес),

                    // Information.ExtractPropertyPath<Медведь>(x => x.Пол),
                    Information.ExtractPropertyPath<Медведь>(x => x.ДатаРождения),
                    Information.ExtractPropertyPath<Медведь>(x => x.ПорядковыйНомер)
                };
                var берлогаDynamicView = new View(new ViewAttribute("берлогаDynamicView", берлогаPropertiesNames), typeof(Берлога));
                var медвDynamicView = new View(new ViewAttribute("медвDynamicView", медвPropertiesNames), typeof(Медведь));

                // Объекты для тестирования создания.
                Медведь медв = new Медведь { Вес = 48 };
                Лес лес1 = new Лес { Название = "Бор" };
                Лес лес2 = new Лес { Название = "Березовая роща" };
                медв.ЛесОбитания = лес1;
                var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                var берлога2 = new Берлога { Наименование = "Для плохого настроения", ЛесРасположения = лес2 };
                медв.Берлога.Add(берлога1);
                медв.Берлога.Add(берлога2);

                var objs = new DataObject[] { медв };
                args.DataService.UpdateObjects(ref objs);

                // Проверим, что через сервис данных БС отрабатывает корректно.
                медв.ЛесОбитания = лес2;
                args.DataService.UpdateObject(медв);
                Assert.Equal($"Медведь обитает в {лес2.Название}", медв.ПолеБС);

                // Сделаем тоже самое, но через OData.
                string requestUrl;

                string requestJsonDataМедв = медв.ToJson(медвDynamicView, args.Token.Model);
                DataObjectDictionary objJsonМедв = DataObjectDictionary.Parse(requestJsonDataМедв, медвDynamicView, args.Token.Model);

                objJsonМедв.Add(
                    $"{nameof(Медведь.ЛесОбитания)}@odata.bind",
                    string.Format(
                        "{0}({1})",
                        args.Token.Model.GetEdmEntitySet(typeof(Лес)).Name,
                        ((KeyGuid)лес1.__PrimaryKey).Guid.ToString("D")));

                requestJsonDataМедв = objJsonМедв.Serialize();
                requestUrl = string.Format(
                    "http://localhost/odata/{0}({1})",
                    args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                    ((KeyGuid)медв.__PrimaryKey).Guid.ToString());

                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonDataМедв).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    string receivedJsonObjs = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedObjs = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonObjs);

                    Assert.Equal($"Медведь обитает в {лес1.Название}", receivedObjs[nameof(Медведь.ПолеБС)]);
                }

                // Проверим, что через сервис данных БС отрабатывает корректно.
                берлога1.ЛесРасположения = лес2;
                args.DataService.UpdateObject(берлога1);
                Assert.Equal($"Берлога расположена в {лес2.Название}", берлога1.ПолеБС);

                // Сделаем тоже самое, но через OData.
                string requestJsonDataБерлога = берлога1.ToJson(берлогаDynamicView, args.Token.Model);
                var objJsonБерлога = DataObjectDictionary.Parse(requestJsonDataБерлога, берлогаDynamicView, args.Token.Model);

                objJsonБерлога.Add(
                    $"{nameof(Берлога.ЛесРасположения)}@odata.bind",
                    string.Format(
                        "{0}({1})",
                        args.Token.Model.GetEdmEntitySet(typeof(Лес)).Name,
                        ((KeyGuid)лес1.__PrimaryKey).Guid.ToString("D")));

                objJsonБерлога.Add(
                    $"{nameof(Берлога.Медведь)}@odata.bind",
                    string.Format(
                        "{0}({1})",
                        args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                        ((KeyGuid)медв.__PrimaryKey).Guid.ToString("D")));

                requestJsonDataБерлога = objJsonБерлога.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Берлога)).Name, ((KeyGuid)берлога1.__PrimaryKey).Guid.ToString());

                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonDataБерлога).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    string receivedJsonObjs = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedObjs = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonObjs);

                    Assert.Equal($"Берлога расположена в {лес1.Название}", receivedObjs[nameof(Берлога.ПолеБС)]);
                }
            });
        }
    }
}
