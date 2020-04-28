namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;
    using Microsoft.AspNet.OData.Batch;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Extensions;
    using Unity;
    using Unity.AspNet.WebApi;
    using Xunit;

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
                typeof(Car).Assembly
            };
            UseNamespaceInEntitySetName = useNamespaceInEntitySetName;

            _builder = new DefaultDataObjectEdmModelBuilder(DataObjectsAssembliesNames, UseNamespaceInEntitySetName, pseudoDetailDefinitions);
        }

        /// <summary>
        /// Метод вызываемый после возникновения исключения.
        /// </summary>
        /// <param name="e">Исключение, которое возникло внутри ODataService.</param>
        /// <param name="code">Возвращаемый код HTTP. По-умолчанияю 500.</param>
        /// <returns>Исключение, которое будет отправлено клиенту.</returns>
        public static Exception AfterInternalServerError(Exception e, ref HttpStatusCode code)
        {
            Assert.Null(e);
            code = HttpStatusCode.InternalServerError;
            return e;
        }

        /// <summary>
        /// Осуществляет перебор тестовых сервисов данных из <see cref="BaseIntegratedTest"/>, и вызывает переданный делегат
        /// для каждого сервиса данных, передав в него <see cref="HttpClient"/> для осуществления запросов к OData-сервису.
        /// </summary>
        /// <param name="action">Действие, выполняемое для каждого сервиса данных из <see cref="BaseIntegratedTest"/>.</param>
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

                    const string fileControllerPath = "api/File";
                    config.MapODataServiceFileRoute(
                        "File",
                        fileControllerPath,
                        Path.GetTempPath(),
                        dataService);

                    var token = config.MapDataObjectRoute(_builder, server, "odata", "odata", true);
                    token.Events.CallbackAfterInternalServerError = AfterInternalServerError;
                    var args = new TestArgs { UnityContainer = container, DataService = dataService, HttpClient = client, Token = token };
                    ExternalLangDef.LanguageDef.DataService = dataService;
                    action(args);
                }
            }
        }

        protected void CheckODataBatchResponseStatusCode(HttpResponseMessage response, HttpStatusCode[] statusCodes)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            int i = 0;
            ODataBatchContent content = response.Content as ODataBatchContent;
            foreach (ChangeSetResponseItem changeSetResponseItem in content.Responses)
            {
                foreach (HttpResponseMessage httpResponseMessage in changeSetResponseItem.Responses)
                {
                    Assert.Equal(statusCodes[i++], httpResponseMessage.StatusCode);
                }
            }
        }

        protected HttpRequestMessage CreateBatchRequest(string url, string[] changesets)
        {
            string boundary = $"batch_{Guid.NewGuid()}";
            string body = CreateBatchBody(boundary, changesets);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{url}/$batch"),
                Method = new HttpMethod("POST"),
                Content = new StringContent(body),
            };

            request.Content.Headers.ContentType.MediaType = "multipart/mixed";
            request.Content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));

            return request;
        }

        /// <summary>
        /// Проверка наличия поддержки Gis текущей реализацией <see cref="IDataService"/>.
        /// </summary>
        /// <param name="dataService">Сервис данных.</param>
        /// <returns>Значение true, если текущая реализация <see cref="IDataService"/> поддерживает Gis.</returns>
        protected bool GisIsAvailable(IDataService dataService)
        {
            return dataService is GisPostgresDataService || dataService is GisMSSQLDataService;
        }

        protected string CreateChangeset(string url, string body, DataObject dataObject)
        {
            var changeset = new StringBuilder();

            changeset.AppendLine($"{GetMethodAndUrl(dataObject, url)} HTTP/1.1");
            changeset.AppendLine($"Content-Type: application/json;type=entry");
            changeset.AppendLine($"Prefer: return=representation");
            changeset.AppendLine();

            changeset.AppendLine(body);

            return changeset.ToString();
        }

        private string CreateBatchBody(string boundary, string[] changesets)
        {
            var body = new StringBuilder($"--{boundary}");
            body.AppendLine();

            string changesetBoundary = $"changeset_{Guid.NewGuid()}";

            body.AppendLine($"Content-Type: multipart/mixed;boundary={changesetBoundary}");
            body.AppendLine();

            for (var i = 0; i < changesets.Length; i++)
            {
                body.AppendLine($"--{changesetBoundary}");
                body.AppendLine($"Content-Type: application/http");
                body.AppendLine($"Content-Transfer-Encoding: binary");
                body.AppendLine($"Content-ID: {i + 1}");
                body.AppendLine();

                body.AppendLine(changesets[i]);
            }

            body.AppendLine($"--{changesetBoundary}--");
            body.AppendLine($"--{boundary}--");
            body.AppendLine();

            return body.ToString();
        }

        private string GetMethodAndUrl(DataObject dataObject, string url)
        {
            switch (dataObject.GetStatus())
            {
                case ObjectStatus.Created:
                    return $"POST {url}";

                case ObjectStatus.Altered:
                case ObjectStatus.UnAltered:
                    return $"PATCH {url}({((KeyGuid)dataObject.__PrimaryKey).Guid})";

                case ObjectStatus.Deleted:
                    return $"DELETE {url}({((KeyGuid)dataObject.__PrimaryKey).Guid})";

                default:
                    throw new InvalidOperationException();
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
