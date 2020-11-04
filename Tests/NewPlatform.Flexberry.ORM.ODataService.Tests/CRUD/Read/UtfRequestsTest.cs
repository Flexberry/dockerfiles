namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Unit-test class for read data through OData service with using UTF8 requests.
    /// </summary>
    public class UtfRequestsTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        /// <param name="output">Вывод отладочной информации.</param>
        public UtfRequestsTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output)
            : base(factory, output)
        {
        }
#endif

        /// <summary>
        /// Using UTF-8 requests.
        /// </summary>
        [Fact]
        public void TestUsingUtfRequests()
        {
            ActODataService(args =>
            {
                var author = new Автор {Имя = "TestAuthor"};
                var magazine = new Журнал {Название = "TestMagazine", Номер = 30, Автор2 = author};
                var book = new Книга {Название = "TestBook", Автор1 = author};
                var library = new Библиотека {Адрес = "TestStreet"};

                var dMagazines = new DetailArrayOfЖурнал(library);
                dMagazines.Add(magazine);

                var dBooks = new DetailArrayOfКнига(library);
                dBooks.Add(book);

                library.Книга = dBooks;
                library.Журнал = dMagazines;

                var objs = new DataObject[]
                {
                    library, author,
                    magazine, book
                };
                args.DataService.UpdateObjects(ref objs);

                var view = Библиотека.Views.Eview;
                string viewUrl = ViewExtensions.ToODataQuery(view);

                var partUrlArr = new[]
                {
                    new {Model = args.Token.Model.GetEdmEntitySet(typeof (Библиотека)).Name, Query = viewUrl},
                    new
                    {
                        Model = args.Token.Model.GetEdmEntitySet(typeof (Библиотека)).Name.Unicodify(),
                        Query =
                            string.Join("&",
                                viewUrl.Split('&')
                                    .Select(i => string.Join("=", i.Split('=').Select(m => m.Unicodify())))
                                    .ToArray())
                    }
                };
                foreach (var item in partUrlArr)
                {
                    string requestUrl = string.Format("http://localhost/odata/{0}?{1}", item.Model, item.Query);

                    using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                        string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                        Dictionary<string, object> receivedDict =
                            JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);
                        Assert.Equal(1, ((JArray)receivedDict["value"]).Count);
                    }
                }
            });
        }
    }
}
