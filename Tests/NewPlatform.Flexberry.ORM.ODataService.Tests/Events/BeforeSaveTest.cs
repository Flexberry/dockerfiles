namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Events
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Exceptions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования логики перед операциями модификации данных OData-сервисом (вставка, обновление, удаление).
    /// </summary>
    public class BeforeSaveTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        public BeforeSaveTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory)
            : base(factory)
        {
        }
#endif

        /// <summary>
        /// Содержит DataObject, который является параметром в методах BeforeCreate, BeforeUpdate и BeforeDelete.
        /// </summary>
        private DataObject ParamObj { get; set; }

        /// <summary>
        /// Метод вызываемый перед созданием объекта.
        /// </summary>
        /// <param name="obj">Объект перед созданием.</param>
        public bool BeforeCreate(DataObject obj)
        {
            ParamObj = obj;
            return true;
        }

        /// <summary>
        /// Метод вызываемый перед обновлением объекта.
        /// </summary>
        /// <param name="obj">Объект перед обновлением.</param>
        public bool BeforeUpdate(DataObject obj)
        {
            ParamObj = obj;
            return true;
        }

        /// <summary>
        /// Метод вызываемый перед удалением объекта.
        /// </summary>
        /// <param name="obj">Объект перед удалением.</param>
        public bool BeforeDelete(DataObject obj)
        {
            ParamObj = obj;
            return true;
        }

        /// <summary>
        /// Осуществляет проверку того, что при POST запросах происходит вставка объекта,
        /// зависимые объекты (мастера, детейлы) обрабатываются в зависимости от наличия в БД - вставляются или обновляются.
        /// </summary>
        [Fact]
        public void BeforeSavePostComplexObjectTest()
        {
            ActODataService(args =>
            {
                args.Token.Events.CallbackBeforeCreate = BeforeCreate;
                args.Token.Events.CallbackBeforeUpdate = BeforeUpdate;
                args.Token.Events.CallbackBeforeDelete = BeforeDelete;

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

                // Только создание объектов.
                // Подготовка тестовых данных в формате OData.
                string json = медв.ToJson(медвDynamicView, args.Token.Model);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name);

                ParamObj = null;

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, json).Result;

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
                var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Лес), Лес.Views.ЛесE);
                lcs.LoadingTypes = new[] { typeof(Лес) };
                var dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(2, dobjs.Length);

                lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Берлога), Берлога.Views.БерлогаE);
                lcs.LoadingTypes = new[] { typeof(Берлога) };

                dobjs = args.DataService.LoadObjects(lcs);
                Assert.Equal(2, dobjs.Length);

                // Создание объекта и обновление связанных.
                // Создаем нового медведя: в его мастере ЛесОбитания - лес1, но в нём изменим Название; в детейлы заберем от первого медведя  детейл2, изменив Название в мастере детейла.
                // Подготовка тестовых данных в формате OData.
                // Объекты для тестирования создания с обновлением.
                Медведь медвежонок = new Медведь { Вес = 12 };
                var берлога3 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                медвежонок.Берлога.Add(берлога3);
                медв.Берлога.Remove(берлога2);
                медвежонок.Берлога.Add(берлога2);

                лес1.Название = лес1.Название + "(обновл)";
                лес2.Название = лес2.Название + "(обновл)";

                json = медвежонок.ToJson(медвDynamicView, args.Token.Model);

                ParamObj = null;

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                response = args.HttpClient.PostAsJsonStringAsync(requestUrl, json).Result;
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
                lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Лес), Лес.Views.ЛесE);
                lcs.LoadingTypes = new[] { typeof(Лес) };
                lcs.LimitFunction = ldef.GetFunction(
                    ldef.funcEQ,
                    new ICSSoft.STORMNET.FunctionalLanguage.VariableDef(ldef.GuidType, ICSSoft.STORMNET.FunctionalLanguage.SQLWhere.SQLWhereLanguageDef.StormMainObjectKey),
                    лес1.__PrimaryKey);
                dobjs = args.DataService.LoadObjects(lcs);

                Assert.Single(dobjs);
                Assert.EndsWith("(обновл)", ((Лес)dobjs[0]).Название);

                lcs.LimitFunction = ldef.GetFunction(
                    ldef.funcEQ,
                    new ICSSoft.STORMNET.FunctionalLanguage.VariableDef(ldef.GuidType, ICSSoft.STORMNET.FunctionalLanguage.SQLWhere.SQLWhereLanguageDef.StormMainObjectKey),
                    лес2.__PrimaryKey);
                dobjs = args.DataService.LoadObjects(lcs);

                Assert.Single(dobjs);
                Assert.EndsWith("(обновл)", ((Лес)dobjs[0]).Название);

                lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Берлога), Берлога.Views.БерлогаE);
                lcs.LoadingTypes = new[] { typeof(Берлога) };
                lcs.LimitFunction = ldef.GetFunction(
                    ldef.funcEQ,
                    new ICSSoft.STORMNET.FunctionalLanguage.VariableDef(ldef.GuidType, "Медведь"),
                    медв.__PrimaryKey);
                dobjs = args.DataService.LoadObjects(lcs);

                Assert.Single(dobjs);

                lcs.LimitFunction = ldef.GetFunction(
                    ldef.funcEQ,
                    new ICSSoft.STORMNET.FunctionalLanguage.VariableDef(ldef.GuidType, "Медведь"),
                    медвежонок.__PrimaryKey);
                dobjs = args.DataService.LoadObjects(lcs);

                Assert.Equal(2, dobjs.Length);
            });
        }

        /// <summary>
        /// Осуществляет проверку частичного обновления данных (передаются только значения модифицированных атрибутов)
        /// для простейшего объекта, т.е. мастера и детейлы не заданы и не модифицируются.
        /// Объект с изменениями передается JSON-строкой.
        /// </summary>
        [Fact]
        public void BeforeSavePatchSimpleObjectTest()
        {
            ActODataService(args =>
            {
                args.Token.Events.CallbackBeforeCreate = BeforeCreate;
                args.Token.Events.CallbackBeforeUpdate = BeforeUpdate;
                args.Token.Events.CallbackBeforeDelete = BeforeDelete;

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
        public void BeforeSaveDeleteObjectTest()
        {
            ActODataService(args =>
            {
                args.Token.Events.CallbackBeforeCreate = BeforeCreate;
                args.Token.Events.CallbackBeforeUpdate = BeforeUpdate;
                args.Token.Events.CallbackBeforeDelete = BeforeDelete;

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
