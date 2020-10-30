namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Create
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;
    using Newtonsoft.Json;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования бизнес-серверов.
    /// </summary>
    public class BusinessServersTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        public BusinessServersTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory)
            : base(factory)
        {
        }
#endif

        /// <summary>
        /// Осуществляет проверку того, что при POST запросах происходит вызов бизнес-сервера.
        /// </summary>
        [Fact]
        public void CallBSWhenCreateTest()
        {
            string[] берлогаPropertiesNames =
                {
                    Information.ExtractPropertyPath<Берлога>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Берлога>(x => x.Наименование),
                    Information.ExtractPropertyPath<Берлога>(x => x.ЛесРасположения),
                    Information.ExtractPropertyPath<Берлога>(x => x.ЛесРасположения.Название),
                };
            string[] медвPropertiesNames =
                {
                    Information.ExtractPropertyPath<Медведь>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Медведь>(x => x.Вес),
                    Information.ExtractPropertyPath<Медведь>(x => x.ЛесОбитания),
                    Information.ExtractPropertyPath<Медведь>(x => x.ЛесОбитания.Название),
                };
            var берлогаDynamicView = new View(new ViewAttribute("берлогаDynamicView", берлогаPropertiesNames), typeof(Берлога));
            var медвDynamicView = new View(new ViewAttribute("медвDynamicView", медвPropertiesNames), typeof(Медведь));
            медвDynamicView.AddDetailInView(Information.ExtractPropertyPath<Медведь>(x => x.Берлога), берлогаDynamicView, true);

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
                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name);

                string json = медв.ToJson(медвDynamicView, args.Token.Model);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, json).Result;

                // Убедимся, что запрос завершился успешно.
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                // Получим строку с ответом.
                string receivedJsonObjs = response.Content.ReadAsStringAsync().Result.Beautify();

                // В ответе приходит объект с созданной сущностью.
                // Преобразуем полученный объект в словарь.
                Dictionary<string, object> receivedObjs = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonObjs);
                Assert.Equal("Object created.", receivedObjs["ПолеБС"]);

                // Проверяем что созданы зависимые объекты, вычитав с помощью args.DataService
                var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Берлога), Берлога.Views.БерлогаE);
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
