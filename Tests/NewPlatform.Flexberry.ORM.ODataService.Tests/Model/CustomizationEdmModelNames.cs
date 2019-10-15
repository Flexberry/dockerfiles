namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Model
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Windows.Forms;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers;

    using Newtonsoft.Json;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования метаданных, получаемых от OData-сервиса.
    /// </summary>
    public class CustomizationEdmModelNames : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Осуществляет проверку того, что при запросах с параметром <text>$count=true</text>, возвращаются метаданные с количеством присланных объектов.
        /// </summary>
        [Fact]
        public void CustomizationEdmModelReadTest()
        {
            ActODataService(args =>
            {
                var наследник = new Наследник() { Свойство = 1234.5, Свойство1 = "str", Свойство2 = 22 };
                var детейл = new Детейл() { prop1 = 1 };
                var детейл2 = new Детейл2() { prop2 = "str2" };
                var мастер = new Мастер() { prop = "str3" };
                var мастер2 = new Мастер2() { свойство2 = -1 };
                var master = new Master() { property = "str4" };
                наследник.Master = master;
                наследник.Мастер = мастер;
                мастер.Мастер2 = мастер2;
                наследник.Детейл.Add(детейл);
                детейл.Детейл2.Add(детейл2);

                var objs = new DataObject[] { наследник };
                args.DataService.UpdateObjects(ref objs);
            });
        }

        /// <summary>
        /// Осуществляет проверку того, что при запросах с параметром <text>$count=true</text>, возвращаются метаданные с количеством присланных объектов.
        /// </summary>
        [Fact]
        public void CustomizationEdmModelCreateTest()
        {
            ActODataService(args =>
            {
                var наследник = new Наследник() { Свойство = 1234.5, Свойство1 = "str", Свойство2 = 22 };
                var детейл = new Детейл() { prop1 = 1 };
                var детейл2 = new Детейл2() { prop2 = "str2" };
                var мастер = new Мастер() { prop = "str3" };
                var мастер2 = new Мастер2() { свойство2 = -1 };
                var master = new Master() { property = "str4" };
                наследник.Master = master;
                наследник.Мастер = мастер;
                мастер.Мастер2 = мастер2;
                наследник.Детейл.Add(детейл);
                детейл.Детейл2.Add(детейл2);

                ExternalLangDef.LanguageDef.DataService = args.DataService;

                // ------------------ Только создания объектов ------------------
                // Подготовка тестовых данных в формате OData.
                var controller = new Controllers.DataObjectController(args.DataService, null, args.Token.Model, args.Token.Events, args.Token.Functions);
                System.Web.OData.EdmEntityObject edmObj = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Наследник)), наследник, 1, null);
                var edm_master = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Master)), master, 1, null);
                var edm_мастер = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Мастер)), мастер, 1, null);
                var edm_мастер2 = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Мастер2)), мастер2, 1, null);
                edmObj.TrySetPropertyValue("MasterAlias", edm_мастер);
                edmObj.TrySetPropertyValue("Master", edm_master);
                edm_мастер.TrySetPropertyValue("Master2Alias", edm_мастер2);
                var coll = controller.GetEdmCollection(наследник.Детейл, typeof(Детейл), 1, null);
                edmObj.TrySetPropertyValue("DetailAlias", coll);
                System.Web.OData.EdmEntityObject edmДетейл = (System.Web.OData.EdmEntityObject)coll[0];

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Наследник)).Name);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                HttpResponseMessage response = args.HttpClient.PostAsJsonAsync(requestUrl, edmObj).Result;

                // Убедимся, что запрос завершился успешно.
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            });
        }

        /// <summary>
        /// Осуществляет проверку того, что при POST запросах происходит вставка и удаление связей объекта.
        /// Зависимые объекты (мастера, детейлы) представлены в виде - Имя_Связи@odata.bind: Имя_Набора_Сущностей(ключ) или Имя_Связи@odata.bind: [ Имя_Набора_Сущностей(ключ) ]   .
        /// Тест проверяет следующие факты:
        /// <list type="number">
        /// <item><description>Вставка связи мастерового объекта.</description></item>
        /// <item><description>Удаление связи мастеровго объекта путём присвоения null свойству.</description></item>
        /// <item><description>Удаление связи мастеровго объекта путём присвоения null для Имя_Связи@odata.bind.</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public void PostNavigationPropertiesTest()
        {
            string[] детейлPropertiesNames =
            {
                    Information.ExtractPropertyPath<Детейл>(x => x.prop1)
            };
            string[] masterPropertiesNames =
            {
                    Information.ExtractPropertyPath<Master>(x => x.property)
            };
            string[] мастерPropertiesNames =
            {
                    Information.ExtractPropertyPath<Мастер>(x => x.prop)
            };
            string[] мастер2PropertiesNames =
            {
                    Information.ExtractPropertyPath<Мастер2>(x => x.свойство2)
            };
            string[] наследникPropertiesNames =
            {
                    Information.ExtractPropertyPath<Наследник>(x => x.Свойство),
                    Information.ExtractPropertyPath<Наследник>(x => x.Свойство1),
                    Information.ExtractPropertyPath<Наследник>(x => x.Свойство2)
            };
            var детейлDynamicView = new View(new ViewAttribute("детейлDynamicView", детейлPropertiesNames), typeof(Детейл));
            var masterDynamicView = new View(new ViewAttribute("masterDynamicView", masterPropertiesNames), typeof(Master));
            var мастерDynamicView = new View(new ViewAttribute("мастерDynamicView", мастерPropertiesNames), typeof(Мастер));
            var мастер2DynamicView = new View(new ViewAttribute("мастер2DynamicView", мастер2PropertiesNames), typeof(Мастер2));
            var наследникDynamicView = new View(new ViewAttribute("наследникDynamicView", наследникPropertiesNames), typeof(Наследник));

            // Объекты для тестирования создания.
            var наследник = new Наследник() { Свойство = 1234.5, Свойство1 = "str", Свойство2 = 22 };
            var детейл = new Детейл() { prop1 = 1 };
            var мастер = new Мастер() { prop = "str3" };
            var мастер2 = new Мастер2() { свойство2 = -1 };
            var master = new Master() { property = "str4" };
            наследник.Master = master;
            наследник.Мастер = мастер;
            мастер.Мастер2 = мастер2;
            наследник.Детейл.Add(детейл);
            ActODataService(args =>
            {
                string requestUrl;
                string receivedJsonMaster, receivedJsonМастер, receivedJsonНаследник;
                string requestJsonData = master.ToJson(masterDynamicView, args.Token.Model);
                requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Master)).Name);
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                    // Получим строку с ответом (в ней должна вернуться созданная сущность).
                    receivedJsonMaster = response.Content.ReadAsStringAsync().Result.Beautify();
                }

                requestJsonData = мастер.ToJson(мастерDynamicView, args.Token.Model);
                requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Мастер)).Name);
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                    // Получим строку с ответом (в ней должна вернуться созданная сущность).
                    receivedJsonМастер = response.Content.ReadAsStringAsync().Result.Beautify();
                }

                requestJsonData = наследник.ToJson(наследникDynamicView, args.Token.Model);
                DataObjectDictionary objJsonНаследник = DataObjectDictionary.Parse(requestJsonData, наследникDynamicView, args.Token.Model);
                Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonMaster);
                objJsonНаследник.Add("Master@odata.bind", string.Format(
                    "{0}({1})",
                    args.Token.Model.GetEdmEntitySet(typeof(Master)).Name,
                    receivedDict["__PrimaryKey"]));
                receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonМастер);
                objJsonНаследник.Add("MasterAlias@odata.bind", string.Format(
                    "{0}({1})",
                    args.Token.Model.GetEdmEntitySet(typeof(Мастер)).Name,
                    receivedDict["__PrimaryKey"]));
                objJsonНаследник.Add("DetailAlias@odata.bind", null);
                requestJsonData = objJsonНаследник.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Наследник)).Name);
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                    // Получим строку с ответом (в ней должна вернуться созданная сущность).
                    receivedJsonНаследник = response.Content.ReadAsStringAsync().Result.Beautify();
                }
            });
        }


    }
}
