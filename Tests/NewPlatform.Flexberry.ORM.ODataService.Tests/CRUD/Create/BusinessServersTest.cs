namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Create
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET.Business;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

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
            // Объекты для тестирования создания.
            Медведь медв = new Медведь { Вес = 48 };
            Лес лес1 = new Лес { Название = "Бор" };
            Лес лес2 = new Лес { Название = "Березовая роща" };
            медв.ЛесОбитания = лес1;
            var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
            var берлога2 = new Берлога { Наименование = "Для плохого настроения", ЛесРасположения = лес2 };
            медв.Берлога.Add(берлога1);
            медв.Берлога.Add(берлога2);

            // Объекты для тестирования создания с обновлением.
            Медведь медвежонок = new Медведь { Вес = 12 };
            var берлога3 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
            медвежонок.Берлога.Add(берлога3);

            ActODataService(args =>
            {
                // ------------------ Только создания объектов ------------------
                // Подготовка тестовых данных в формате OData.
                var controller = new Controllers.DataObjectController(args.DataService, null, args.Token.Model, args.Token.Events, args.Token.Functions);
                Microsoft.AspNet.OData.EdmEntityObject edmObj = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Медведь)), медв, 1, null);
                var edmЛес1 = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Лес)), лес1, 1, null);
                var edmЛес2 = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Лес)), лес2, 1, null);
                edmObj.TrySetPropertyValue("ЛесОбитания", edmЛес1);
                var coll = controller.GetEdmCollection(медв.Берлога, typeof(Берлога), 1, null);
                edmObj.TrySetPropertyValue("Берлога", coll);
                Microsoft.AspNet.OData.EdmEntityObject edmБерлога1 = (Microsoft.AspNet.OData.EdmEntityObject)coll[0];
                Microsoft.AspNet.OData.EdmEntityObject edmБерлога2 = (Microsoft.AspNet.OData.EdmEntityObject)coll[1];
                edmБерлога1.TrySetPropertyValue("ЛесРасположения", edmЛес1);
                edmБерлога2.TrySetPropertyValue("ЛесРасположения", edmЛес2);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                HttpResponseMessage response = args.HttpClient.PostAsJsonAsync(requestUrl, edmObj).Result;

                // Убедимся, что запрос завершился успешно.
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                // Получим строку с ответом.
                string receivedJsonObjs = response.Content.ReadAsStringAsync().Result.Beautify();

                // В ответе приходит объект с созданной сущностью.
                // Преобразуем полученный объект в словарь.
                Dictionary<string, object> receivedObjs = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonObjs);
                Assert.Equal("Object created.", receivedObjs["ПолеБС"]);

                // Проверяем что созданы зависимые объекты, вычитав с помощью args.DataService
                var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Берлога), "БерлогаE");
                lcs.LoadingTypes = new[] { typeof(Берлога) };

                var dobjs = args.DataService.LoadObjects(lcs);
                Assert.Equal(2, dobjs.Length);

                foreach (var obj in dobjs)
                {
                    var berloga = (Берлога)obj;
                    Assert.Equal("Object created.", berloga.ПолеБС);
                }
            });
        }
    }
}
