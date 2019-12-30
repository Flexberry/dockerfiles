namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Model;

    using Unity;
    using Unity.AspNet.WebApi;

    /// <summary>
    /// Базовый класс для тестирования работы с данными через ODataService.
    /// </summary>
    public class BaseODataServiceIntegratedTest : BaseIntegratedTest
    {
        protected readonly IDataObjectEdmModelBuilder _builder;

        public class TestArgs
        {
            public IUnityContainer UnityContainer { get; set; }

            public ManagementToken Token { get; set; }

            public IDataService DataService { get; set; }

            public HttpClient HttpClient { get; set; }
        }

        /// <summary>
        /// Имена сборок с объектами данных.
        /// </summary>
        public Assembly[] DataObjectsAssembliesNames { get; protected set; }

        /// <summary>
        /// Флаг, показывающий нужно ли добавлять пространства имен типов, к именам соответствующих им наборов сущностей.
        /// </summary>
        public bool UseNamespaceInEntitySetName { get; protected set; }

        public BaseODataServiceIntegratedTest(
            string stageCasePath = @"РТЦ Тестирование и документирование\Модели для юнит-тестов\Flexberry ORM\NewPlatform.Flexberry.ORM.ODataService.Tests\",
            bool useNamespaceInEntitySetName = false,
            bool useGisDataService = false,
            PseudoDetailDefinitions pseudoDetailDefinitions = null)
            : base("ODataDB", useGisDataService)
        {
            DataObjectsAssembliesNames = new[]
            {
                typeof(Car).Assembly,
                //typeof(Agent).Assembly
            };
            UseNamespaceInEntitySetName = useNamespaceInEntitySetName;

            _builder = new DefaultDataObjectEdmModelBuilder(DataObjectsAssembliesNames, UseNamespaceInEntitySetName, pseudoDetailDefinitions);
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
                var container = new UnityContainer();
                container.RegisterInstance(dataService);

                using (var config = new HttpConfiguration())
                using (var server = new HttpServer(config))
                using (var client = new HttpClient(server, false) { BaseAddress = new Uri("http://localhost/odata/") })
                {
                    server.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                    config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                    config.DependencyResolver = new UnityDependencyResolver(container);

                    var token = config.MapODataServiceDataObjectRoute(_builder, new HttpServer());
                    var args = new TestArgs { UnityContainer = container, DataService = dataService, HttpClient = client, Token = token };
                    action(args);
                }
            }
        }

        /*
        private bool PropertyFilter(PropertyInfo propertyInfo)
        {
            return Information.ExtractPropertyInfo<Agent>(x => x.Pwd) != propertyInfo;
        }
        */
    }
}
