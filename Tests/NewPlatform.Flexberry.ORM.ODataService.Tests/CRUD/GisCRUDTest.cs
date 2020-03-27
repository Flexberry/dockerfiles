namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business.LINQProvider.Extensions;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования работы с гео-данными.
    /// </summary>
    public class GisCRUDTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        public GisCRUDTest()
            : base("ODataGis", false, true)
        { }

        /// <summary>
        /// Осуществляет проверку фильтрации с использованием функции geo.intersects
        /// </summary>
        [Fact]
        public void TestFilterGis()
        {
            ActODataService(args =>
            {
                if (!GisIsAvailable(args.DataService))
                {
                    return;
                }

                ExternalLangDef.LanguageDef.DataService = args.DataService;

                DateTime date = new DateTimeOffset(DateTime.Now).UtcDateTime;
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertyEnum = Цифра.Семь, PropertyDateTime = date, PropertyGeography = "LINESTRING(0 0,1 1,1 2)".CreateGeography(), PropertyInt = 5};
                var objs = new DataObject[] { класс };
                args.DataService.UpdateObjects(ref objs);
                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(КлассСМножествомТипов)).Name,
                    "PropertyInt eq 5 and geo.intersects(geography1=PropertyGeography, geography2=geography'SRID=4326;LINESTRING(0 0,1 1,1 2)')");

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Получим строку с ответом.
                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();

                    // Преобразуем полученный объект в словарь.
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);

                    Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
                }
            });

        }

        /// <summary>
        /// Осуществляет проверку создания и изменения сущности.
        /// </summary>
        [Fact]
        public void ModifyTest()
        {
            ActODataService(args =>
            {
                if (!GisIsAvailable(args.DataService))
                {
                    return;
                }

                // Создаем объект данных.
                КлассСМножествомТипов класс = new КлассСМножествомТипов()
                {
                    PropertyEnum = Цифра.Семь, PropertyGeography = "POINT(3 3)".CreateGeography(), PropertyInt = 5,
                    PropertyDateTime = DateTime.Now
                };

                // Преобразуем объект данных в JSON-строку.
                string[] классСМножествомТиповPropertiesNames =
                {
                    Information.ExtractPropertyPath<КлассСМножествомТипов>(x => x.__PrimaryKey),
                    Information.ExtractPropertyPath<КлассСМножествомТипов>(x => x.PropertyEnum),
                    Information.ExtractPropertyPath<КлассСМножествомТипов>(x => x.PropertyGeography),
                    Information.ExtractPropertyPath<КлассСМножествомТипов>(x => x.PropertyInt),
                    Information.ExtractPropertyPath<КлассСМножествомТипов>(x => x.PropertyDateTime)
                };
                var dynamicView = new View(new ViewAttribute("dynamicView", классСМножествомТиповPropertiesNames), typeof(КлассСМножествомТипов));
                string requestJsonData = класс.ToJson(dynamicView, args.Token.Model);

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(КлассСМножествомТипов)).Name);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                using (HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                }

                requestUrl = string.Format("http://localhost/odata/{0}({1})", args.Token.Model.GetEdmEntitySet(typeof(КлассСМножествомТипов)).Name, ((KeyGuid)класс.__PrimaryKey).Guid.ToString());
                using (HttpResponseMessage response = args.HttpClient.PatchAsJsonStringAsync(requestUrl, requestJsonData).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                }
            });
        }
    }
}
