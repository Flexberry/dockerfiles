namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Create
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;
    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования изменения мастера при создании детейла.
    /// </summary>
    public class ChangeMasterInBSTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        /// <param name="output">Вывод отладочной информации.</param>
        public ChangeMasterInBSTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output)
            : base(factory, output)
        {
        }
#endif

        /// <summary>
        /// Тест на проверку сохранения изменений мастера,
        /// которые были внесены в BS детейла при его (детейла) создании.
        /// </summary>
        [Fact(Skip = "Skip until fix https://github.com/Flexberry/NewPlatform.Flexberry.ORM/issues/65")]
        public void ChangeAgregatorTest()
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

        /// <summary>
        /// Тест на проверку сохранения изменений мастера,
        /// которые были внесены в BS ссылающегося объекта при его создании.
        /// </summary>
        [Fact]
        public void ChangeMasterTest()
        {
            ActODataService(args =>
            {
                var лес = new Лес() { Название = "Таинственный" };
                var медведь = new Медведь() { ЛесОбитания = лес, ЦветГлаз = nameof(ChangeMasterTest) };

                var objectsArr = new DataObject[] { лес };
                args.DataService.UpdateObjects(ref objectsArr);

                // Преобразуем объект данных в JSON-строку.
                string[] bearProperties =
                {
                    Information.ExtractPropertyPath<Медведь>(x => x.ЛесОбитания),
                    Information.ExtractPropertyPath<Медведь>(x => x.ЦветГлаз)
                };
                var bearDynamicView = new View(new ViewAttribute("bearDynamicView", bearProperties), typeof(Медведь));

                // Формируем URL запроса к OData-сервису.
                string requestUrl = string.Format("http://localhost/odata/{0}", args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name);

                string bearJson = медведь.ToJson(bearDynamicView, args.Token.Model);

                // Убедимся, что поле мастера не изменялось.
                Assert.Equal(медведь.ЛесОбитания.GetStatus(), ObjectStatus.UnAltered);

                // Обращаемся к OData-сервису и обрабатываем ответ, в теле запроса передаем создаваемый объект в формате JSON.
                HttpResponseMessage response = args.HttpClient.PostAsJsonStringAsync(requestUrl, bearJson).Result;

                // Убедимся, что запрос завершился успешно.
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                // Проверяем, что поле мастера изменилось в БС.
                LoadingCustomizationStruct lcsForest = LoadingCustomizationStruct.GetSimpleStruct(typeof(Лес), Лес.Views.ЛесE);
                System.Collections.Generic.List<string> count =
                    args.DataService.LoadObjects(lcsForest).Cast<Лес>().Select(x => x.Название).ToList();
                Assert.Equal(nameof(ChangeMasterTest), count[0]);

                // Проверяем что объект сохранился.
                LoadingCustomizationStruct lcsBear = LoadingCustomizationStruct.GetSimpleStruct(typeof(Медведь), Медведь.Views.МедведьL);
                System.Collections.Generic.List<string> bearField = args.DataService.LoadObjects(lcsBear).Cast<Медведь>().Select(x => x.ЦветГлаз).ToList();
                Assert.Equal(nameof(ChangeMasterTest), bearField[0]);
            });
        }
    }
}
