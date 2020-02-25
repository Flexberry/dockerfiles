namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Create
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    using Xunit;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    /// <summary>
    /// Класс тестов для тестирования изменения мастера при создании детейла.
    /// </summary>
    
    public class ChangeMasterInBSTest : BaseODataServiceIntegratedTest
    {
        /// <summary>
        /// Тест на проверку сохранения изменений мастера,
        /// которые были внесены в BS детейла при его(детейла) создании.
        /// </summary>
        [Fact(Skip = "Skip until fix https://github.com/Flexberry/NewPlatform.Flexberry.ORM/issues/65")]
        public void ChangeMasterTest()
        {
            ActODataService(args =>
            {
                var driverTest = new Driver { CarCount = 2 };
                var carTest = new Car { Driver = driverTest };

                var drvArr = new DataObject[] { driverTest };
                args.DataService.UpdateObjects(ref drvArr);

                // Преобразуем объект данных в JSON-строку.
                string[] carPropertiesDriver =
                {
                    Information.ExtractPropertyPath<Car>(x => x.Driver)
                };
                var carDynamicView = new View(new ViewAttribute("carDynamicView", carPropertiesDriver), typeof(Car));

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Car)).Name);

                string carJson = carTest.ToJson(carDynamicView, args.Token.Model);

                // Убедимся, что поле мастера не изменялось.
                Assert.Equal(carTest.Driver.GetStatus(), ObjectStatus.UnAltered);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, carJson).Result;

                // Убедимся, что запрос завершился успешно.
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                // Проверяем, что поле мастера изменилось в БС.
                var viewDriver = Driver.Views.AllData;
                var lcsDriver = LoadingCustomizationStruct.GetSimpleStruct(typeof(Driver), viewDriver);
                var poleCarCount =
                    args.DataService.LoadObjects(lcsDriver).Cast<Driver>().Select(x => x.CarCount).ToList();
                Assert.Equal(3, poleCarCount[0]);

                // Проверяем что детейл сохранился.
                var viewCar = Car.Views.AllData;
                var lcsCar = LoadingCustomizationStruct.GetSimpleStruct(typeof(Car), viewCar);
                var poleNumber = args.DataService.LoadObjects(lcsCar).Cast<Car>().Select(x => x.Number).ToList();
                Assert.Equal("TECT/3", poleNumber[0]);
            });
        }
    }
}
