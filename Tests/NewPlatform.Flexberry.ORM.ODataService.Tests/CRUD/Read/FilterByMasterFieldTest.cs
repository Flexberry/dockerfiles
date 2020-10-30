namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read
{
    using System.Collections.Generic;
    using System.Net;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Unit-test class for filtering data through OData service by master fields.
    /// </summary>
    public class FilterByMasterFieldTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        public FilterByMasterFieldTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory)
            : base(factory)
        {
        }
#endif

        /// <summary>
        /// Tests filtering data by master field.
        /// </summary>
        [Fact]
        public void TestFilterByMasterField()
        {
            ActODataService(args =>
            {
                var driver1 = new Driver { CarCount = 3, Documents = true, Name = "Driver1" };
                var car1d1 = new Car { Driver = driver1, Model = "ВАЗ" };
                var car2d1 = new Car { Driver = driver1, Model = "ГАЗ" };
                var car3d1 = new Car { Driver = driver1, Model = "УАЗ" };

                var driver2 = new Driver { CarCount = 4, Documents = false, Name = "Driver2" };
                var car1d2 = new Car { Driver = driver2, Model = "BMW" };
                var car2d2 = new Car { Driver = driver2, Model = "Porsche" };
                var car3d2 = new Car { Driver = driver2, Model = "Lamborghini" };
                var car4d2 = new Car { Driver = driver2, Model = "Subaru" };

                var objs = new DataObject[]
                {
                    driver1, car1d1, car2d1, car3d1,
                    driver2, car1d2, car2d2, car3d2, car4d2
                };
                args.DataService.UpdateObjects(ref objs);

                string requestUrl = string.Format(
                    "http://localhost/odata/{0}?$filter={1}",
                    args.Token.Model.GetEdmEntitySet(typeof(Car)).Name,
                    "Driver/Name eq 'Driver2'");

                using (var response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    string receivedStr = response.Content.ReadAsStringAsync().Result.Beautify();
                    Dictionary<string, object> receivedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receivedStr);
                    Assert.Equal(4, ((JArray)receivedDict["value"]).Count);
                }
            });
        }
    }
}
