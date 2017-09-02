namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Script.Serialization;
    using ICSSoft.STORMNET;
    using Xunit;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    /// <summary>
    /// Unit-test class for read data with reference to master through OData service.
    /// </summary>

    public class ReferenceToMasterTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Read aggregator with Detaille with reference to one master.
        /// </summary>
        [Fact]
        public void TestReadAggregatorWithOneMaster()
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

                string requestUrl = string.Format("http://localhost/odata/{0}?{1}", args.Token.Model.GetEdmEntitySet(typeof(Библиотека)).Name, viewUrl);

                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedDict =
                        new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedStr);
                    Assert.Equal(1, ((ArrayList) receivedDict["value"]).Count);

                    var value = ((ArrayList) receivedDict["value"])[0];
                    var журнал = ((ArrayList) ((Dictionary<string, object>) value)["Журнал"])[0];
                    var авторЖурнала = ((Dictionary<string, object>) журнал)["Автор2"];
                    var авторЖурналаPK = (string) ((Dictionary<string, object>) авторЖурнала)["__PrimaryKey"];

                    var книга = ((ArrayList) ((Dictionary<string, object>) value)["Книга"])[0];
                    var авторКниги = ((Dictionary<string, object>) книга)["Автор1"];
                    var авторКнигиPK = (string) ((Dictionary<string, object>) авторКниги)["__PrimaryKey"];

                    Assert.Equal(авторЖурналаPK, авторКнигиPK);
                }
            });
        }

        /// <summary>
        /// Read object with not stored master.
        /// </summary>
        [Fact(Skip = "Нужно разобраться почему возникает ошибка.")]
        public void TestNotStoredMaster()
        {
            ActODataService(args =>
            {
                var library = new Библиотека { Адрес = "TestStreet", __PrimaryKey = new Guid("8dcd3aa3-11c2-456d-902c-03323e1ae635") };
                var booksSupplier = new ПоставщикКниг { Ссылка = new Guid("8dcd3aa3-11c2-456d-902c-03323e1ae635") };
                var objs = new DataObject[]
                {
                    library, booksSupplier
                };
                args.DataService.UpdateObjects(ref objs);

                var view = ПоставщикКниг.Views.ViewFull;
                string viewUrl = ViewExtensions.ToODataQuery(view);

                string requestUrl = string.Format("http://localhost/odata/{0}?{1}", args.Token.Model.GetEdmEntitySet(typeof(ПоставщикКниг)).Name, viewUrl);

                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedDict =
                        new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(receivedStr);
                    Assert.Equal(1, ((ArrayList)receivedDict["value"]).Count);
                }
            });
        }
    }
}
