namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Events
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.OData;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Exceptions;

    using NewPlatform.Flexberry.ORM.ODataService.Controllers;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования логики после операций модификации данных OData-сервисом (вставка, обновление, удаление).
    /// </summary>
    public class AfterSaveTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Содержит DataObject, который является параметром в методах AfterCreate, AfterUpdate и AfterDelete.
        /// </summary>
        private DataObject ParamObj { get; set; }

        /// <summary>
        /// Метод вызываемый после создания объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        public void AfterCreate(DataObject obj)
        {
            ParamObj = obj;
        }

        /// <summary>
        /// Метод вызываемый после обновления объекта.
        /// </summary>
        /// <param name="obj">Объект после обновления.</param>
        public void AfterUpdate(DataObject obj)
        {
            ParamObj = obj;
        }

        /// <summary>
        /// Метод вызываемый после удаления объекта.
        /// </summary>
        /// <param name="obj">Объект перед удалением.</param>
        public void AfterDelete(DataObject obj)
        {
            ParamObj = obj;
        }

        /// <summary>
        /// Осуществляет проверку того, что при POST запросах происходит вставка объекта,
        /// зависимые объекты (мастера, детейлы) обрабатываются в зависимости от наличия в БД - вставляются или обновляются.
        /// </summary>
        [Fact]
        public void AfterSavePostComplexObjectTest()
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
                args.Token.Events.CallbackAfterCreate = AfterCreate;
                args.Token.Events.CallbackAfterUpdate = AfterUpdate;
                args.Token.Events.CallbackAfterDelete = AfterDelete;

                // ------------------ Только создания объектов ------------------
                // Подготовка тестовых данных в формате OData.
                var controller = new DataObjectController(args.DataService, null, args.Token.Model, args.Token.Events, args.Token.Functions);
                EdmEntityObject edmObj = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Медведь)), медв, 1, null);
                var edmЛес1 = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Лес)), лес1, 1, null);
                var edmЛес2 = controller.GetEdmObject(args.Token.Model.GetEdmEntityType(typeof(Лес)), лес2, 1, null);
                edmObj.TrySetPropertyValue("ЛесОбитания", edmЛес1);
                var coll = controller.GetEdmCollection(медв.Берлога, typeof(Берлога), 1, null);
                edmObj.TrySetPropertyValue("Берлога", coll);
                EdmEntityObject edmБерлога1 = (EdmEntityObject)coll[0]; // controller.GetEdmObject(args.ODataService.Model.GetEdmEntityType(typeof(Берлога)), берлога1, 1, null);
                EdmEntityObject edmБерлога2 = (EdmEntityObject)coll[1]; // controller.GetEdmObject(args.ODataService.Model.GetEdmEntityType(typeof(Берлога)), берлога2, 1, null);
                edmБерлога1.TrySetPropertyValue("ЛесРасположения", edmЛес1);
                edmБерлога2.TrySetPropertyValue("ЛесРасположения", edmЛес2);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name);

                ParamObj = null;

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                HttpResponseMessage response = args.HttpClient.PostAsJsonAsync(requestUrl, edmObj).Result;
                Assert.NotNull(ParamObj);

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
                var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Лес), "ЛесE");
                lcs.LoadingTypes = new[] { typeof(Лес) };
                var dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(2, dobjs.Length);

                lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Берлога), "БерлогаE");
                lcs.LoadingTypes = new[] { typeof(Берлога) };

                // lcs.LimitFunction = ldef.GetFunction(ldef.funcEQ, new VariableDef(ldef.GuidType, SQLWhereLanguageDef.StormMainObjectKey), keyValue);
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
                edmБерлога1 = (EdmEntityObject)coll[0];
                edmБерлога2 = (EdmEntityObject)coll[1];
                edmБерлога1.TrySetPropertyValue("ЛесРасположения", edmЛес2);
                edmБерлога2.TrySetPropertyValue("ЛесРасположения", edmЛес1);

                ParamObj = null;

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                response = args.HttpClient.PostAsJsonAsync(requestUrl, edmObj).Result;
                Assert.NotNull(ParamObj);

                // Убедимся, что запрос завершился успешно.
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

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
        /// Осуществляет проверку частичного обновления данных (передаются только значения модифицированных атрибутов)
        /// для простейшего объекта, т.е. мастера и детейлы не заданы и не модифицируются.
        /// Объект с изменениями передается JSON-строкой.
        /// </summary>
        [Fact]
        public void AfterSavePatchSimpleObjectTest()
        {
            ActODataService(args =>
            {
                args.Token.Events.CallbackAfterCreate = AfterCreate;
                args.Token.Events.CallbackAfterUpdate = AfterUpdate;
                args.Token.Events.CallbackAfterDelete = AfterDelete;

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

                ParamObj = null;

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем обновляемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    Assert.NotNull(ParamObj);

                    // Убедимся, что запрос завершился успешно (тело ответа д.б. пустым при отсутствии ошибок обновления).
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                    // Проверяем что объект данных был обновлен в базе, причем только по переданным атрибутам.
                    Лес updatedЛес = new Лес { __PrimaryKey = лес.__PrimaryKey };
                    args.DataService.LoadObject(updatedЛес);

                    Assert.Equal(150, updatedЛес.Площадь);
                    Assert.Equal("Чаща", updatedЛес.Название);
                }
            });
        }

        /// <summary>
        /// Осуществляет проверку удаления данных.
        /// </summary>
        [Fact]
        public void AfterSaveDeleteObjectTest()
        {
            ActODataService(args =>
            {
                args.Token.Events.CallbackAfterCreate = AfterCreate;
                args.Token.Events.CallbackAfterUpdate = AfterUpdate;
                args.Token.Events.CallbackAfterDelete = AfterDelete;

                // ------------------ Удаление простого объекта -----------------------------
                // Создаем объект данных, который потом будем удалять, и добавляем в базу обычным сервисом данных.
                Медведь медв = new Медведь { Пол = tПол.Мужской, Вес = 80, ПорядковыйНомер = 1 };
                args.DataService.UpdateObject(медв);

                // Формируем URL запроса к OData-сервису (с идентификатором удаляемой сущности).
                string requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name, ((ICSSoft.STORMNET.KeyGen.KeyGuid)медв.__PrimaryKey).Guid.ToString());

                ParamObj = null;

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.DeleteAsync(requestUrl).Result)
                {
                    Assert.NotNull(ParamObj);

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
    }
}
