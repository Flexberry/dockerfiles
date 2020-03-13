namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Update
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Exceptions;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers;

    using Newtonsoft.Json;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования операций модификации данных OData-сервисом (вставка, обновление, удаление).
    /// </summary>
    public class ModifyDataTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Осуществляет проверку того, что при PATCH запросах происходит вставка и удаление связей объекта.
        /// Зависимые объекты (мастера, детейлы) представлены в виде - Имя_Связи@odata.bind: Имя_Набора_Сущностей(ключ) или Имя_Связи@odata.bind: [ Имя_Набора_Сущностей(ключ) ]   .
        /// Тест проверяет следующие факты:
        /// <list type="number">
        /// <item><description>Вставка связи мастерового объекта.</description></item>
        /// <item><description>Удаление связи мастеровго объекта путём присвоения null свойству.</description></item>
        /// <item><description>Удаление связи мастеровго объекта путём присвоения null для Имя_Связи@odata.bind.</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public void PatchNavigationPropertiesTest()
        {
            ActODataService(args =>
            {
                ExternalLangDef.LanguageDef.DataService = args.DataService;
                string[] берлогаPropertiesNames =
                {
                    Information.ExtractPropertyPath<Берлога>(x => x.ПолеБС),
                    Information.ExtractPropertyPath<Берлога>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Берлога>(x => x.Наименование),
                    Information.ExtractPropertyPath<Берлога>(x => x.Заброшена)
                };
                string[] лесPropertiesNames =
                {
                    Information.ExtractPropertyPath<Лес>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Лес>(x => x.Площадь),
                    Information.ExtractPropertyPath<Лес>(x => x.Название),
                    Information.ExtractPropertyPath<Лес>(x => x.ДатаПоследнегоОсмотра)
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
                var лесDynamicView = new View(new ViewAttribute("лесDynamicView", лесPropertiesNames), typeof(Лес));
                var медвDynamicView = new View(new ViewAttribute("медвDynamicView", медвPropertiesNames), typeof(Медведь));

                // Объекты для тестирования создания.
                Медведь медв = new Медведь { Вес = 48 };
                Лес лес1 = new Лес { Название = "Бор" };
                Лес лес2 = new Лес { Название = "Березовая роща" };
                var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                var берлога2 = new Берлога { Наименование = "Для плохого настроения", ЛесРасположения = лес2 };
                медв.Берлога.Add(берлога1);
                var objs = new DataObject[] { медв, лес1, лес2, берлога1 };
                args.DataService.UpdateObjects(ref objs);
                string requestUrl;

                string requestJsonDataМедв = медв.ToJson(медвDynamicView, args.Token.Model);
                DataObjectDictionary objJson = DataObjectDictionary.Parse(requestJsonDataМедв, медвDynamicView, args.Token.Model);

                objJson.Add("ЛесОбитания@odata.bind", string.Format(
                    "{0}({1})",
                    args.Token.Model.GetEdmEntitySet(typeof(Лес)).Name,
                    ((KeyGuid)лес1.__PrimaryKey).Guid.ToString("D")));

                requestJsonDataМедв = objJson.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name, ((KeyGuid)медв.__PrimaryKey).Guid.ToString());
                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonDataМедв).Result)
                {
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                }

                var requestJsonDataБерлога = берлога1.ToJson(берлогаDynamicView, args.Token.Model);
                objJson = DataObjectDictionary.Parse(requestJsonDataБерлога, берлогаDynamicView, args.Token.Model);
                objJson.Add("Медведь", null);
                requestJsonDataБерлога = objJson.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Берлога)).Name, ((KeyGuid)медв.__PrimaryKey).Guid.ToString());
                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonDataБерлога).Result)
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                }

                requestJsonDataМедв = медв.ToJson(медвDynamicView, args.Token.Model);
                objJson = DataObjectDictionary.Parse(requestJsonDataМедв, медвDynamicView, args.Token.Model);
                objJson.Add("ЛесОбитания@odata.bind", null);
                requestJsonDataМедв = objJson.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name, ((KeyGuid)медв.__PrimaryKey).Guid.ToString());

                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonDataМедв).Result)
                {
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                }

                requestJsonDataМедв = медв.ToJson(медвDynamicView, args.Token.Model);
                objJson = DataObjectDictionary.Parse(requestJsonDataМедв, медвDynamicView, args.Token.Model);
                objJson.Add("ЛесОбитания", null);
                requestJsonDataМедв = objJson.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name, ((KeyGuid)медв.__PrimaryKey).Guid.ToString());

                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonDataМедв).Result)
                {
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                }

                requestJsonDataМедв = медв.ToJson(медвDynamicView, args.Token.Model);
                objJson = DataObjectDictionary.Parse(requestJsonDataМедв, медвDynamicView, args.Token.Model);
                objJson.Add("Берлога@odata.bind", null);
                requestJsonDataМедв = objJson.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name, ((KeyGuid)медв.__PrimaryKey).Guid.ToString());

                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonDataМедв).Result)
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                }
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
            string[] берлогаPropertiesNames =
            {
                    // Information.ExtractPropertyPath<Берлога>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Берлога>(x => x.Наименование),
                    Information.ExtractPropertyPath<Берлога>(x => x.Заброшена)
                };
            string[] лесPropertiesNames =
            {
                    // Information.ExtractPropertyPath<Лес>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Лес>(x => x.Площадь),
                    Information.ExtractPropertyPath<Лес>(x => x.Название),
                    Information.ExtractPropertyPath<Лес>(x => x.ДатаПоследнегоОсмотра)
            };
            string[] медвPropertiesNames =
            {
                    // Information.ExtractPropertyPath<Медведь>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Медведь>(x => x.Вес),

                    // Information.ExtractPropertyPath<Медведь>(x => x.Пол),
                    Information.ExtractPropertyPath<Медведь>(x => x.ДатаРождения),
                    Information.ExtractPropertyPath<Медведь>(x => x.ПорядковыйНомер)
            };
            var берлогаDynamicView = new View(new ViewAttribute("берлогаDynamicView", берлогаPropertiesNames), typeof(Берлога));
            var лесDynamicView = new View(new ViewAttribute("лесDynamicView", лесPropertiesNames), typeof(Лес));
            var медвDynamicView = new View(new ViewAttribute("медвDynamicView", медвPropertiesNames), typeof(Медведь));

            // Объекты для тестирования создания.
            Медведь медв = new Медведь { Вес = 48 };
            Лес лес1 = new Лес { Название = "Бор" };
            var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
            ActODataService(args =>
            {
                string requestUrl;
                string receivedJsonЛес1, receivedJsonМедв;
                string requestJsonDataЛес1 = лес1.ToJson(лесDynamicView, args.Token.Model);
                requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Лес)).Name);
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonDataЛес1).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                    // Получим строку с ответом (в ней должна вернуться созданная сущность).
                    receivedJsonЛес1 = response.Content.ReadAsStringAsync().Result.Beautify();
                }

                string requestJsonDataМедв = медв.ToJson(медвDynamicView, args.Token.Model);
                DataObjectDictionary objJsonМедв = DataObjectDictionary.Parse(requestJsonDataМедв, медвDynamicView, args.Token.Model);
                Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonЛес1);

                objJsonМедв.Add("ЛесОбитания@odata.bind", string.Format(
                    "{0}({1})",
                    args.Token.Model.GetEdmEntitySet(typeof(Лес)).Name,
                    receivedDict["__PrimaryKey"]));
                objJsonМедв.Add("Берлога@odata.bind", null);

                requestJsonDataМедв = objJsonМедв.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name);
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonDataМедв).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                    // Получим строку с ответом (в ней должна вернуться созданная сущность).
                    receivedJsonМедв = response.Content.ReadAsStringAsync().Result.Beautify();
                }

                var requestJsonDataБерлога = берлога1.ToJson(берлогаDynamicView, args.Token.Model);
                var objJson = DataObjectDictionary.Parse(requestJsonDataБерлога, берлогаDynamicView, args.Token.Model);
                receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedJsonМедв);
                objJson.Add("Медведь@odata.bind", string.Format(
                    "{0}({1})",
                    args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name,
                    receivedDict["__PrimaryKey"]));
                requestJsonDataБерлога = objJson.Serialize();
                requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Берлога)).Name);
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonDataБерлога).Result)
                {
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку того, что при POST запросах происходит вставка объекта,
        /// зависимые объекты (мастера, детейлы) обрабатываются в зависимости от наличия в БД - вставляются или обновляются.
        /// </summary>
        [Fact]
        public void PostComplexObjectTest()
        {
            // TODO: переписать тест с корректным формированием параметра - передаваемой сущности - для Post.
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
                ExternalLangDef.LanguageDef.DataService = args.DataService;
                // ------------------ Только создания объектов ------------------
                // Подготовка тестовых данных в формате OData.
                var controller = new Controllers.DataObjectController(args.DataService, null, args.Token.Model, args.Token.Events, args.Token.Functions);
                System.Web.OData.EdmEntityObject edmObj = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Медведь)), медв, 1, null);
                var edmЛес1 = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Лес)), лес1, 1, null);
                var edmЛес2 = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Лес)), лес2, 1, null);
                edmObj.TrySetPropertyValue("ЛесОбитания", edmЛес1);
                var coll = controller.GetEdmCollection(медв.Берлога, typeof(Берлога), 1, null);
                edmObj.TrySetPropertyValue("Берлога", coll);
                System.Web.OData.EdmEntityObject edmБерлога1 = (System.Web.OData.EdmEntityObject)coll[0];
                System.Web.OData.EdmEntityObject edmБерлога2 = (System.Web.OData.EdmEntityObject)coll[1];
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

                // Проверяем созданный объект, вычитав с помощью DataService
                DataObject createdObj = new Медведь { __PrimaryKey = медв.__PrimaryKey };
                args.DataService.LoadObject(createdObj);

                Assert.Equal(ObjectStatus.UnAltered, createdObj.GetStatus());
                Assert.Equal(((Медведь)createdObj).Вес, (int)(long)receivedObjs["Вес"]);

                // Проверяем что созданы все зависимые объекты, вычитав с помощью DataService
                var ldef = ICSSoft.STORMNET.FunctionalLanguage.SQLWhere.SQLWhereLanguageDef.LanguageDef;
                LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Лес), "ЛесE");
                lcs.LoadingTypes = new[] { typeof(Лес) };
                ICSSoft.STORMNET.DataObject[] dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(2, dobjs.Length);

                lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Берлога), "БерлогаE");
                lcs.LoadingTypes = new[] { typeof(Берлога) };
                dobjs = args.DataService.LoadObjects(lcs);
                Assert.Equal(2, dobjs.Length);

                // ------------------ Создание объекта и обновление связанных ------------------
                // Создаем нового медведя: в его мастере ЛесОбитания - лес1, но в нём изменим Название; в детейлы заберем от первого медведя  детейл2, изменив Название в мастере детейла.
                // Подготовка тестовых данных в формате OData.
                edmObj = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Медведь)), медвежонок, 1, null);
                edmObj.TrySetPropertyValue("ЛесОбитания", edmЛес1);
                edmЛес1.TrySetPropertyValue("Название", лес1.Название + "(обновл)");
                edmЛес2.TrySetPropertyValue("Название", лес2.Название + "(обновл)");
                медв.Берлога.Remove(берлога2);
                медвежонок.Берлога.Add(берлога2);
                coll = controller.GetEdmCollection(медвежонок.Берлога, typeof(Берлога), 1, null);
                edmObj.TrySetPropertyValue("Берлога", coll);
                edmБерлога1 = (System.Web.OData.EdmEntityObject)coll[0];
                edmБерлога2 = (System.Web.OData.EdmEntityObject)coll[1];
                edmБерлога1.TrySetPropertyValue("ЛесРасположения", edmЛес2);
                edmБерлога2.TrySetPropertyValue("ЛесРасположения", edmЛес1);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                response = args.HttpClient.PostAsJsonAsync(requestUrl, edmObj).Result;

                // Убедимся, что запрос завершился успешно.
                Assert.Equal(response.StatusCode, HttpStatusCode.Created);

                // Проверяем созданный объект, вычитав с помощью DataService
                createdObj = new Медведь { __PrimaryKey = медвежонок.__PrimaryKey };
                args.DataService.LoadObject(createdObj);

                Assert.Equal(ObjectStatus.UnAltered, createdObj.GetStatus());
                Assert.Equal(12, ((Медведь)createdObj).Вес);

                // Проверяем что созданы все зависимые объекты, вычитав с помощью DataService
                ldef = ICSSoft.STORMNET.FunctionalLanguage.SQLWhere.SQLWhereLanguageDef.LanguageDef;
                lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Лес), "ЛесE");
                lcs.LoadingTypes = new[] { typeof(Лес) };
                lcs.LimitFunction = ldef.GetFunction(
                    ldef.funcEQ,
                    new ICSSoft.STORMNET.FunctionalLanguage.VariableDef(ldef.GuidType, ICSSoft.STORMNET.FunctionalLanguage.SQLWhere.SQLWhereLanguageDef.StormMainObjectKey),
                    лес1.__PrimaryKey);
                dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(1, dobjs.Length);
                Assert.True(((Лес)dobjs[0]).Название.EndsWith("(обновл)"));

                lcs.LimitFunction = ldef.GetFunction(
                    ldef.funcEQ,
                    new ICSSoft.STORMNET.FunctionalLanguage.VariableDef(ldef.GuidType, ICSSoft.STORMNET.FunctionalLanguage.SQLWhere.SQLWhereLanguageDef.StormMainObjectKey),
                    лес2.__PrimaryKey);
                dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(1, dobjs.Length);
                Assert.True(((Лес)dobjs[0]).Название.EndsWith("(обновл)"));

                lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Берлога), "БерлогаE");
                lcs.LoadingTypes = new[] { typeof(Берлога) };
                lcs.LimitFunction = ldef.GetFunction(
                    ldef.funcEQ,
                    new ICSSoft.STORMNET.FunctionalLanguage.VariableDef(ldef.GuidType, "Медведь"),
                    медв.__PrimaryKey);
                dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(1, dobjs.Length);

                lcs.LimitFunction = ldef.GetFunction(
                    ldef.funcEQ,
                    new ICSSoft.STORMNET.FunctionalLanguage.VariableDef(ldef.GuidType, "Медведь"),
                    медвежонок.__PrimaryKey);
                dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(2, dobjs.Length);

                // Вернем детейл для того, чтобы тест работал со следующими СУБД.
                медвежонок.Берлога.Remove(берлога2);
                медв.Берлога.Add(берлога2);
            });
        }

        /// <summary>
        /// Осуществляет проверку создания сущности с датой и незаданным первичным ключом.
        /// </summary>
        [Fact]
        public void PostObjDateTimeNoPKTest()
        {
            ActODataService(args =>
            {
                // Создаем объект данных.
                Лес country = new Лес { Площадь = 10, Название = "Бор", ДатаПоследнегоОсмотра = (ICSSoft.STORMNET.UserDataTypes.NullableDateTime)DateTime.Now };

                // Преобразуем объект данных в JSON-строку.
                string[] contryPropertiesNames =
                {
                    Information.ExtractPropertyPath<Лес>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Лес>(x => x.Площадь),
                    Information.ExtractPropertyPath<Лес>(x => x.Название),
                    Information.ExtractPropertyPath<Лес>(x => x.ДатаПоследнегоОсмотра)
                };
                var contryDynamicView = new View(new ViewAttribute("ContryDynamicView", contryPropertiesNames), typeof(Лес));

                string requestJsonData = country.ToJson(contryDynamicView, args.Token.Model);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Лес)).Name);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку того, что при POST запросах, отправляющих простейшие объекты JSON-строкой, происходит корректная вставка.
        /// </summary>
        [Fact]
        public void PostDataTimeValueTest()
        {
            ActODataService(args =>
            {
                // Создаем объект данных.
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertyDateTime = DateTime.Now };

                // Преобразуем объект данных в JSON-строку.
                string[] classPropertiesNames =
                {
                    Information.ExtractPropertyPath<КлассСМножествомТипов>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<КлассСМножествомТипов>(x => x.PropertyDateTime)
                };

                var classDynamicView = new View(new ViewAttribute("classDynamicView", classPropertiesNames), typeof(КлассСМножествомТипов));

                string requestJsonData = класс.ToJson(classDynamicView, args.Token.Model);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(КлассСМножествомТипов)).Name);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку того, что при POST запросах, отправляющих простейшие объекты JSON-строкой, происходит корректная вставка.
        /// </summary>
        [Fact]
        public void PostSimpleObjectTest()
        {
            ActODataService(args =>
            {
                // Создаем объект данных.
                Страна country = new Страна { Название = "Russia" };

                // Преобразуем объект данных в JSON-строку.
                string[] contryPropertiesNames =
                {
                    Information.ExtractPropertyPath<Страна>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Страна>(x => x.Название)
                };
                var contryDynamicView = new View(new ViewAttribute("ContryDynamicView", contryPropertiesNames), typeof(Страна));

                string requestJsonData = country.ToJson(contryDynamicView, args.Token.Model);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Страна)).Name);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                    // Получим строку с ответом (в ней должна вернуться созданная сущность).
                    string receivedJsonCountry = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь (c приведением типов значений к типам свойств объекта данных).
                    DataObjectDictionary receivedDictionaryCountry = DataObjectDictionary.Parse(receivedJsonCountry, contryDynamicView, args.Token.Model);

                    // Сравним значения полученного и исходного объектов.
                    Assert.True(receivedDictionaryCountry.HasProperty(Information.ExtractPropertyPath<Страна>(x => x.__PrimaryKey)));
                    Assert.Equal(country.__PrimaryKey, receivedDictionaryCountry.GetPropertyValue(Information.ExtractPropertyPath<Страна>(x => x.__PrimaryKey)));

                    Assert.True(receivedDictionaryCountry.HasProperty(Information.ExtractPropertyPath<Страна>(x => x.Название)));
                    Assert.Equal(country.Название, receivedDictionaryCountry.GetPropertyValue(Information.ExtractPropertyPath<Страна>(x => x.Название)));

                    // Проверяем что объект данных был корректно создан в базе.
                    Страна createdCountry = new Страна { __PrimaryKey = country.__PrimaryKey };
                    args.DataService.LoadObject(contryDynamicView, createdCountry);

                    Assert.Equal(country.Название, createdCountry.Название);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку частичного обновления данных (передаются только значения модифицированных атрибутов)
        /// для простейшего объекта, т.е. мастера и детейлы не заданы и не модифицируются.
        /// Объект с изменениями передается JSON-строкой.
        /// </summary>
        [Fact]
        public void PatchSimpleObjectTest()
        {
            ActODataService(args =>
            {
                // Создаем объект данных, который потом будем обновлять, и добавляем в базу обычным сервисом данных.
                Лес лес = new Лес { Название = "Чаща", Площадь = 100 };
                args.DataService.UpdateObject(лес);

                // Обновляем часть атрибутов.
                лес.Площадь = 150;

                // Представление, по которому будем обновлять.
                string[] медвPropertiesNames =
                {
                    Information.ExtractPropertyPath<Лес>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Лес>(x => x.Площадь)
                };
                var лесDynamicView = new View(new ViewAttribute("лесDynamicView", медвPropertiesNames), typeof(Лес));

                // Преобразуем объект данных в JSON-строку.
                string requestJsonData = лес.ToJson(лесDynamicView, args.Token.Model);

                // Формируем URL запроса к OData-сервису (с идентификатором изменяемой сущности).
                string requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Лес)).Name, ((ICSSoft.STORMNET.KeyGen.KeyGuid)лес.__PrimaryKey).Guid.ToString());

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем обновляемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно (тело ответа д.б. пустым при отсутствии ошибок обновления).
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                    // Проверяем что объект данных был обновлен в базе, причем только по переданным атрибутам.
                    Лес updatedЛес = new Лес { __PrimaryKey = лес.__PrimaryKey };
                    args.DataService.LoadObject(updatedЛес);

                    Assert.Equal(лес.Площадь, updatedЛес.Площадь);
                    Assert.Equal(лес.Название, updatedЛес.Название);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку частичного обновления данных (передаются только значения модифицированных атрибутов)
        /// для мастера в детейле.
        /// По стандарту сервер OData не должен обрабатывать такой запрос и поэтому вернёт HTTP Код 400.
        /// Объект с изменениями передается JSON-строкой.
        /// </summary>
        [Fact]
        public void PatchComplexObjectTest()
        {
            ActODataService(args =>
            {
                // Объекты для тестирования обновления.
                Медведь медв = new Медведь { Вес = 48 };
                Лес лес1 = new Лес { Название = "Бор" };
                Лес лес2 = new Лес { Название = "Березовая роща" };
                медв.ЛесОбитания = лес1;
                var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                var берлога2 = new Берлога { Наименование = "Для плохого настроения", ЛесРасположения = лес2 };
                медв.Берлога.Add(берлога1);
                медв.Берлога.Add(берлога2);

                var objs = new DataObject[] { медв, лес1, лес2, берлога1, берлога2 };

                args.DataService.UpdateObjects(ref objs);

                // Преобразуем объект данных в JSON-строку.
                string[] медвPropertiesNames =
                {
                    Information.ExtractPropertyPath<Медведь>(x => x.__PrimaryKey),
                };
                var медвDynamicView = new View(new ViewAttribute("медвDynamicView", медвPropertiesNames), typeof(Медведь));

                string[] берлогаPropertiesNames =
                {
                    Information.ExtractPropertyPath<Берлога>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<Берлога>(x => x.ЛесРасположения),
                };
                var берлогаDynamicView = new View(new ViewAttribute("берлогаDynamicView", берлогаPropertiesNames), typeof(Берлога));

                медвDynamicView.AddDetailInView(Information.ExtractPropertyPath<Медведь>(x => x.Берлога), берлогаDynamicView, true);

                Медведь медвДляЗапроса = new Медведь { __PrimaryKey = медв.__PrimaryKey };
                Берлога берлогаДляЗапроса = new Берлога { __PrimaryKey = берлога1.__PrimaryKey, ЛесРасположения = лес2 };
                медвДляЗапроса.Берлога.Add(берлогаДляЗапроса);

                string requestJsonData = медвДляЗапроса.ToJson(медвDynamicView, args.Token.Model);

                // Формируем URL запроса к OData-сервису (с идентификатором изменяемой сущности).
                string requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name, ((ICSSoft.STORMNET.KeyGen.KeyGuid)медв.__PrimaryKey).Guid.ToString());

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем обновляемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку удаления данных.
        /// </summary>
        [Fact]
        public void DeleteObjectTest()
        {
            ActODataService(args =>
            {
                // ------------------ Удаление простого объекта с ключом __PrimaryKey в виде строки -----------------------------
                // Создаем объект данных, который потом будем удалять, и добавляем в базу обычным сервисом данных.
                var класс = new КлассСоСтроковымКлючом();
                args.DataService.UpdateObject(класс);
                // Формируем URL запроса к OData-сервису (с идентификатором удаляемой сущности).
                string requestUrl = string.Format("http://localhost/odata/{0}('{1}')", args.Token.Model.GetEdmEntitySet(typeof(КлассСоСтроковымКлючом)).Name, класс.__PrimaryKey);

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.DeleteAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно (тело ответа д.б. пустым при отсутствии ошибок удаления).
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                    // Проверяем что объект данных был удален из базы.
                    bool exists = true;
                    КлассСоСтроковымКлючом deletedКлассСоСтроковымКлючом = new КлассСоСтроковымКлючом { __PrimaryKey = класс.__PrimaryKey };
                    try
                    {
                        args.DataService.LoadObject(deletedКлассСоСтроковымКлючом);
                    }
                    catch (Exception ex)
                    {
                        if (ex is CantFindDataObjectException)
                            exists = false;
                    }

                    Assert.False(exists);
                }

                // ------------------ Удаление простого объекта -----------------------------
                // Создаем объект данных, который потом будем удалять, и добавляем в базу обычным сервисом данных.
                Медведь медв = new Медведь { Пол = tПол.Мужской, Вес = 80, ПорядковыйНомер = 1 };
                args.DataService.UpdateObject(медв);

                // Формируем URL запроса к OData-сервису (с идентификатором удаляемой сущности).
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name, ((ICSSoft.STORMNET.KeyGen.KeyGuid)медв.__PrimaryKey).Guid.ToString());

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.DeleteAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно (тело ответа д.б. пустым при отсутствии ошибок удаления).
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                    // Проверяем что объект данных был удален из базы.
                    bool exists = true;
                    Медведь deletedМедв = new Медведь { __PrimaryKey = медв.__PrimaryKey };
                    try
                    {
                        args.DataService.LoadObject(deletedМедв);
                    }
                    catch (Exception ex)
                    {
                        if (ex is CantFindDataObjectException)
                            exists = false;
                    }

                    Assert.False(exists);
                }

                // ------------------ Удаление детейла и объекта с детейлами -----------------------------
                // Создаем объект данных, который потом будем удалять, и добавляем в базу обычным сервисом данных.
                медв = new Медведь { Пол = tПол.Мужской, Вес = 80, ПорядковыйНомер = 1 };
                медв.Берлога.Add(new Берлога { Наименование = "Берлога для хорошего настроения" });
                медв.Берлога.Add(new Берлога { Наименование = "Берлога для плохого настроения" });
                Берлога delБерлога = new Берлога { Наименование = "Отдельно удаляемая берлога" };
                медв.Берлога.Add(delБерлога);
                args.DataService.UpdateObject(медв);

                // Проверяем что до вызова удалений в базе есть все детейлы.
                var ldef = ICSSoft.STORMNET.FunctionalLanguage.SQLWhere.SQLWhereLanguageDef.LanguageDef;
                LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Берлога), Берлога.Views.БерлогаE);
                lcs.LoadingTypes = new[] { typeof(Берлога) };
                ICSSoft.STORMNET.DataObject[] dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(3, dobjs.Length);

                // Формируем URL запроса к OData-сервису для удаления объекта-детейла (с идентификатором удаляемой сущности).
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Берлога)).Name, ((ICSSoft.STORMNET.KeyGen.KeyGuid)delБерлога.__PrimaryKey).Guid.ToString());

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.DeleteAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно (тело ответа д.б. пустым при отсутствии ошибок удаления).
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                    // Проверяем что объект-детейл был удален из базы.
                    bool exists = true;
                    Берлога deletedБерлога = new Берлога { __PrimaryKey = delБерлога.__PrimaryKey };
                    try
                    {
                        args.DataService.LoadObject(deletedБерлога);
                    }
                    catch (Exception ex)
                    {
                        if (ex is CantFindDataObjectException)
                            exists = false;
                    }

                    Assert.False(exists);

                    // Проверяем что объект-агрегатор остался в базе.
                    exists = true;
                    Медведь deletedМедв = new Медведь { __PrimaryKey = медв.__PrimaryKey };
                    try
                    {
                        args.DataService.LoadObject(deletedМедв);
                    }
                    catch (Exception ex)
                    {
                        if (ex is CantFindDataObjectException)
                            exists = false;
                    }

                    Assert.True(exists);

                    // Проверяем что детейлов объекта в базе осталось на 1 меньше, чем создавали.
                    dobjs = args.DataService.LoadObjects(lcs);

                    Assert.Equal(2, dobjs.Length);
                }

                // Формируем URL запроса к OData-сервису (с идентификатором удаляемой сущности).
                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name, ((ICSSoft.STORMNET.KeyGen.KeyGuid)медв.__PrimaryKey).Guid.ToString());

                // Обращаемся к OData-сервису для удаления объекта с детейлами и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.DeleteAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно (тело ответа д.б. пустым при отсутствии ошибок удаления).
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                    // Проверяем что объект данных был удален из базы.
                    bool exists = true;
                    Медведь deletedМедв = new Медведь { __PrimaryKey = медв.__PrimaryKey };
                    try
                    {
                        args.DataService.LoadObject(deletedМедв);
                    }
                    catch (Exception ex)
                    {
                        if (ex is CantFindDataObjectException)
                            exists = false;
                    }

                    Assert.False(exists);

                    // Проверяем что детейлов объекта в базе не осталось.
                    dobjs = args.DataService.LoadObjects(lcs);

                    Assert.Equal(0, dobjs.Length);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку обновления мастера с иерархическими детейлами.
        /// Мастер и детейлы заданы и модифицируются.
        /// Объект с изменениями передается JSON-строкой.
        /// </summary>
        [Fact]
        public void UpdateCicleDeteilTest()
        {
            ActODataService(args =>
            {
                // Мастер тестирования обновления.
                TestMaster testMaster1 = new TestMaster { TestMasterName = "TestMasterName" };
                var objs = new DataObject[] { testMaster1 };
                args.DataService.UpdateObjects(ref objs);

                // Колличество создаваемых детейлов.
                int deteilCount = 20;

                // Детейлы тестирования обновления.
                TestDetailWithCicle[] testDetailWithCicleArray = new TestDetailWithCicle[deteilCount];
                TestDetailWithCicle testDetailWithCicle = null;

                for (int i = 0; i < deteilCount; i++)
                {
                    if (i == 0)
                    {
                        testDetailWithCicle = new TestDetailWithCicle { TestDetailName = "TestDeteilName0", TestMaster = testMaster1 };
                    }
                    else
                    {
                        testDetailWithCicle = new TestDetailWithCicle { TestDetailName = "TestDeteilName" + i.ToString(), TestMaster = testMaster1, Parent = testDetailWithCicle };
                    }

                    testDetailWithCicleArray[i] = testDetailWithCicle;
                    objs = new DataObject[] { testDetailWithCicle };
                    args.DataService.UpdateObjects(ref objs);
                }

                // Обновляем атрибут мастера.
                testMaster1.TestMasterName = "TestMasterNameUpdate";

                // Представление, по которому будем обновлять.
                string[] testMasterPropertiesNames =
                {
                    Information.ExtractPropertyPath<TestMaster>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<TestMaster>(x => x.TestMasterName)
                };
                var testMasterDynamicView = new View(new ViewAttribute("testMasterDynamicView", testMasterPropertiesNames), typeof(TestMaster));

                // Преобразуем объект данных в JSON-строку.
                string requestJsonData = testMaster1.ToJson(testMasterDynamicView, args.Token.Model);

                // Формируем URL запроса к OData-сервису (с идентификатором изменяемой сущности).
                string requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(TestMaster)).Name, ((ICSSoft.STORMNET.KeyGen.KeyGuid)testMaster1.__PrimaryKey).Guid.ToString());

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем обновляемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно (тело ответа д.б. пустым при отсутствии ошибок обновления).
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                    // Проверяем что объект данных был обновлен в базе, причем только по переданным атрибутам.
                    TestMaster updatedTestMaster = new TestMaster { __PrimaryKey = testMaster1.__PrimaryKey };
                    args.DataService.LoadObject(updatedTestMaster);

                    Assert.Equal(testMaster1.TestMasterName, updatedTestMaster.TestMasterName);
                }

                // Обновление атрибутов Детейлов.
                for (int i = 0; i < deteilCount; i++)
                {
                    testDetailWithCicleArray[i].TestDetailName += "Update";
                }

                // Представление, по которому будем обновлять.
                string[] testDetailWithCiclePropertiesNames =
                {
                    Information.ExtractPropertyPath<TestDetailWithCicle>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<TestDetailWithCicle>(x => x.TestDetailName)
                };

                var testDetailWithCicleDynamicView = new View(new ViewAttribute("testDetailWithCicleDynamicView", testDetailWithCiclePropertiesNames), typeof(TestDetailWithCicle));

                for (int i = 0; i < deteilCount; i++)
                {
                    // Преобразуем объект данных в JSON-строку.
                    string requestJsonDatatestDetailWithCicle = testDetailWithCicleArray[i].ToJson(testDetailWithCicleDynamicView, args.Token.Model);
                    DataObjectDictionary objJson = DataObjectDictionary.Parse(requestJsonDatatestDetailWithCicle, testDetailWithCicleDynamicView, args.Token.Model);

                    objJson.Add("TestMaster@odata.bind", string.Format(
                        "{0}({1})",
                        args.Token.Model.GetEdmEntitySet(typeof(TestMaster)).Name,
                        ((KeyGuid)testMaster1.__PrimaryKey).Guid.ToString("D")));

                    if (i != 0)
                    {
                        objJson.Add("Parent@odata.bind", string.Format(
                            "{0}({1})",
                            args.Token.Model.GetEdmEntitySet(typeof(TestDetailWithCicle)).Name,
                            ((KeyGuid)testDetailWithCicleArray[i - 1].__PrimaryKey).Guid.ToString("D")));
                    }

                    requestJsonDatatestDetailWithCicle = objJson.Serialize();

                    // Формируем URL запроса к OData-сервису.
                    requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(TestDetailWithCicle)).Name, ((KeyGuid)testDetailWithCicleArray[i].__PrimaryKey).Guid.ToString());

                    using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonDatatestDetailWithCicle).Result)
                    {
                        // Убедимся, что запрос завершился успешно (тело ответа д.б. пустым при отсутствии ошибок обновления).
                        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                        // Проверяем что объект данных был обновлен в базе, причем только по переданным атрибутам.
                        TestDetailWithCicle updatedTestDetailWithCicle = new TestDetailWithCicle { __PrimaryKey = testDetailWithCicleArray[i].__PrimaryKey };
                        args.DataService.LoadObject(updatedTestDetailWithCicle);

                        Assert.Equal(testDetailWithCicleArray[i].TestDetailName, updatedTestDetailWithCicle.TestDetailName);
                    }
                }
            });
        }

        /// <summary>
        /// Test save details with inheritance.
        /// </summary>
        [Fact]
        public void SaveDetailWithInheritanceTest()
        {
            ActODataService(async (args) =>
            {
                var базовыйКласс = new БазовыйКласс() { Свойство1 = "sv1" };
                var детейл = new ДетейлНаследник() { prop1 = 1};
                базовыйКласс.Детейл.Add(детейл);

                args.DataService.UpdateObject(базовыйКласс);
                int newValue = 2;
                детейл.prop1 = newValue;

                const string baseUrl = "http://localhost/odata";

                string detJson = детейл.ToJson(ДетейлНаследник.Views.ДетейлНаследникE, args.Token.Model);
                detJson = detJson.Replace(nameof(ДетейлНаследник.БазовыйКласс), $"{nameof(ДетейлНаследник.БазовыйКласс)}@odata.bind");
                detJson = detJson.Replace("{\"__PrimaryKey\":\"", $"\"{args.Token.Model.GetEdmEntitySet(typeof(БазовыйКласс)).Name}(");
                detJson = detJson.Replace("\"}", ")\"");
                string[] changesets = new[]
                {
                    CreateChangeset(
                        $"{baseUrl}/{args.Token.Model.GetEdmEntitySet(typeof(БазовыйКласс)).Name}",
                        "{}",
                        базовыйКласс),
                    CreateChangeset(
                        $"{baseUrl}/{args.Token.Model.GetEdmEntitySet(typeof(ДетейлНаследник)).Name}",
                        detJson,
                        детейл),
                };
                HttpRequestMessage batchRequest = CreateBatchRequest(baseUrl, changesets);
                using (HttpResponseMessage response = await args.HttpClient.SendAsync(batchRequest))
                {
                    CheckODataBatchResponseStatusCode(response, new HttpStatusCode[] { HttpStatusCode.OK, HttpStatusCode.OK });

                    args.DataService.LoadObject(БазовыйКласс.Views.БазовыйКлассE, базовыйКласс);

                    var детейлы = базовыйКласс.Детейл.Cast<ДетейлНаследник>();

                    Assert.Equal(1, детейлы.Count());
                    Assert.Equal(newValue, детейлы.First().prop1);
                }
            });
        }
    }
}
