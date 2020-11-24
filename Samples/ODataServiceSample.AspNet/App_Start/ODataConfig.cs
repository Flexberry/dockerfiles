namespace ODataServiceSample.AspNet
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using ICSSoft.Services;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using IIS.Caseberry.Logging.Objects;
    using NewPlatform.Flexberry.AspNet.WebApi.Cors;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Extensions;
    using NewPlatform.Flexberry.Services;
    using Unity;
    using Unity.AspNet.WebApi;

    /// <summary>
    /// Configure OData Service.
    /// </summary>
    internal static class ODataConfig
    {
        /// <summary>
        /// Configure OData by DataObjects assembly.
        /// </summary>
        /// <param name="config">Http configuration object.</param>
        /// <param name="container">Unity container.</param>
        public static void Configure(HttpConfiguration config, IUnityContainer container, HttpServer httpServer)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            // To support CORS uncomment the line below.
            config.EnableCors(new DynamicCorsPolicyProvider(true));

            // Use constructor with true first parameter for enable SupportsCredentials.

            // Use Unity as WebAPI dependency resolver
            config.DependencyResolver = new UnityDependencyResolver(container);

            // Config file upload.
            const string fileControllerPath = "api/File";
            config.MapODataServiceFileRoute("File", fileControllerPath);
            var fileAccessor = new DefaultDataObjectFileAccessor(new Uri("http://localhost:44324/"), fileControllerPath, "Uploads");
            container.RegisterInstance<IDataObjectFileAccessor>(fileAccessor);

            // Create EDM model builder
            var assemblies = new[]
            {
                Assembly.Load("NewPlatform.Flexberry.ORM.ODataService.Tests.Objects"),
                typeof(ApplicationLog).Assembly,
                typeof(UserSetting).Assembly,
                typeof(Lock).Assembly,
            };
            var builder = new DefaultDataObjectEdmModelBuilder(assemblies);

            // Map OData Service
            var token = config.MapDataObjectRoute(builder, httpServer);

            // User functions
            token.Functions.Register(new Func<QueryParameters, string>(Test));
            token.Functions.Register(new Func<string, bool>(ClearLogRecords));
            token.Functions.RegisterAction(new Func<QueryParameters, string, string, object>(DeleteAllSelect));

            // Event handlers
            token.Events.CallbackAfterCreate = CallbackAfterCreate;
        }

        private static void CallbackAfterCreate(DataObject dataObject)
        {
            // TODO: implement handler
        }

        private static string Test(QueryParameters queryParameters)
        {
            return "Hello world!";
        }

        /// <summary>
        /// OData function for clearing log records.
        /// </summary>
        /// <param name="dateTime">Stringed <see cref="DateTimeOffset"/> date. Delete all records older that date.</param>
        /// <returns>Success operation return <c>true</c>.</returns>
        private static bool ClearLogRecords(string dateTime)
        {
            DateTimeOffset date;
            if (!DateTimeOffset.TryParse(dateTime, out date))
            {
                throw new ArgumentException("Invalid date format", nameof(dateTime));
            }

            SQLDataService ds = (SQLDataService)DataServiceProvider.DataService;
            IDbConnection connection = ds.GetConnection();
            connection.Open();
            try
            {
                //IDbCommand command = connection.CreateCommand();
                //string tableName = Information.GetClassStorageName(typeof(ApplicationLog));
                //string timestampColumnName = Information.GetPropertyStorageName(
                //    typeof(ApplicationLog),
                //    Information.ExtractPropertyName<ApplicationLog>(a => a.Timestamp));
                //command.CommandText = $"DELETE FROM {tableName} WHERE {timestampColumnName} <= @timestamp";
                //SqlParameter timestampParameter = new SqlParameter("@timestamp", SqlDbType.DateTimeOffset) { Value = date };
                //command.Parameters.Add(timestampParameter);
                //command.ExecuteScalar();
                return true;
            }
            finally
            {
                // Close the connection if that's how we got it.
                connection.Close();
            }
        }

        /// <summary>
        /// OData function for delete all select records.
        /// </summary>
        /// <param name="queryParameters">Request OData Parameters.</param>
        /// <param name="pathName">Type name.</param>
        /// <param name="filterQuery">Query for filter.</param>
        /// <returns>Number of deleted records.</returns>
        private static object DeleteAllSelect(QueryParameters queryParameters, string pathName, string filterQuery)
        {
            try
            {
                SQLDataService dataService = DataServiceProvider.DataService as SQLDataService;

                var uri = $"http://a/b/c?{filterQuery}";
                Type type = queryParameters.GetDataObjectType(pathName);
                LoadingCustomizationStruct lcs = queryParameters.CreateLcs(type, uri);
                DataObject[] updateObjects = dataService.LoadObjects(lcs);
                int deletedCount = updateObjects.Length;

                for (var i = 0; i < updateObjects.Length; i++)
                {
                    updateObjects[i].SetStatus(ObjectStatus.Deleted);
                }

                DataObject[] updateObjectsArray = updateObjects.ToArray();
                dataService.UpdateObjects(ref updateObjectsArray);

                return new
                {
                    deletedCount,
                    message = string.Empty
                };
            }
            catch (Exception e)
            {
                Exception ex = e;
                var msg = ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    msg += Environment.NewLine + ex.Message;
                }

                return new
                {
                    deletedCount = -1,
                    message = msg
                };
            }
        }
    }
}
