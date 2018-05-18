namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using System.Web.Http.SelfHost;
    using System.Windows.Forms;

    using ICSSoft.STORMNET.Business;

    using NewPlatform.Flexberry.ORM.ODataService.Extensions;

    using Unity;
    using Unity.AspNet.WebApi;

    /// <summary>
    /// Базовый класс для тестирования работы с данными через ODataService.
    /// Реализует возможность подключения к WebApi-контроллеру из браузера при запуске под отладчиком.
    /// Этот механизм работает, если Visual Studio запущен с правами администратора.
    /// Необходимо перед чекином возвращать наследование тестов от BaseODataServiceIntegratedTest.
    /// </summary>
    public class SelfHostBaseODataServiceIntegratedTest : BaseODataServiceIntegratedTest
    {
        private IDataService currentDataService;

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public SelfHostBaseODataServiceIntegratedTest()
            : base("ODataDB", false)
        {

        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public SelfHostBaseODataServiceIntegratedTest(string stageCasePath = @"РТЦ Тестирование и документирование\Модели для юнит-тестов\Flexberry ORM\NewPlatform.Flexberry.ORM.ODataService.Tests\",
            bool useNamespaceInEntitySetName = false, bool useGisDataService = false)
            : base("ODataDB", useNamespaceInEntitySetName, useGisDataService)
        {

        }

        /// <summary>
        /// Этот метод работает только под отладчиком. Он предназначен для приостановки выполнения теста, чтобы можно было обратиться к WebApi-контроллеру из браузера. После нажатия на кнопку ОК в модальном диалоге, тест продолжит работу.
        /// </summary>
        public void ShowPauseDialog()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                MessageBox.Show("Нажмите ОК, чтобы продолжить работу теста.", $"Пауза ({currentDataService})", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Осуществляет перебор тестовых сервисов данных из <see cref="BaseOrmIntegratedTest"/>, и вызывает переданный делегат
        /// для каждого сервиса данных, передав в него <see cref="HttpClient"/> для осуществления запросов к OData-сервису.
        /// </summary>
        /// <param name="action">Действие, выполняемое для каждого сервиса данных из <see cref="BaseOrmIntegratedTest"/>.</param>
        public virtual void ActODataService(Action<TestArgs> action)
        {
            if (action == null)
                return;

            foreach (IDataService dataService in DataServices)
            {
                currentDataService = dataService;
                var container = new UnityContainer();
                container.RegisterInstance(dataService);
                var host = "localhost";

                // host = "ICS-KK12324";
                using (var config = new HttpSelfHostConfiguration(new Uri($"http://{host}/")))
                using (var server = new HttpSelfHostServer(config))
                using (var client = new HttpClient(server, false) { BaseAddress = new Uri($"http://{host}/odata/") })
                {
                    config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                    config.DependencyResolver = new UnityDependencyResolver(container);

                    var token = config.MapODataServiceDataObjectRoute(_builder);
                    var args = new TestArgs { UnityContainer = container, DataService = dataService, HttpClient = client, Token = token };

                    try
                    {
                        server.OpenAsync().Wait();

                        action(args);
                    }
                    finally
                    {
                        server.CloseAsync().Wait();
                    }
                }
            }
        }
    }
}
