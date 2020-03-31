namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.OData.Batch;
    using System.Web.OData.Extensions;
    using System.Web.OData.Formatter;
    using ICSSoft.STORMNET.Business;
    using NewPlatform.Flexberry.ORM.ODataService.Batch;
    using NewPlatform.Flexberry.ORM.ODataService.Formatter;
    using NewPlatform.Flexberry.ORM.ODataService.Handlers;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Routing;

    /// <summary>
    /// Класс, содержащий расширения для сервиса данных.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Maps the OData service.
        /// </summary>
        /// <param name="config">The current HTTP configuration.</param>
        /// <param name="builder">The EDM model builder.</param>
        /// <param name="httpServer">HttpServer instance (GlobalConfiguration.DefaultServer).</param>
        /// <param name="routeName">The name of the route (<see cref="DataObjectRoutingConventions.DefaultRouteName"/> be default).</param>
        /// <param name="routePrefix">The route prefix (<see cref="DataObjectRoutingConventions.DefaultRoutePrefix"/> be default).</param>
        /// <param name="isSyncBatchUpdate">Use synchronous mode for call subrequests in batch query.</param>
        /// <returns>OData service registration token.</returns>
        public static ManagementToken MapODataServiceDataObjectRoute(
            this HttpConfiguration config,
            IDataObjectEdmModelBuilder builder,
            HttpServer httpServer,
            string routeName = DataObjectRoutingConventions.DefaultRouteName,
            string routePrefix = DataObjectRoutingConventions.DefaultRoutePrefix,
            bool? isSyncBatchUpdate = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Contract assertion not met: config != null");
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder), "Contract assertion not met: builder != null");
            }

            if (routeName == null)
            {
                throw new ArgumentNullException(nameof(routeName), "Contract assertion not met: routeName != null");
            }

            if (routeName == string.Empty)
            {
                throw new ArgumentException("Contract assertion not met: routeName != string.Empty", nameof(routeName));
            }

            if (routePrefix == null)
            {
                throw new ArgumentNullException(nameof(routePrefix), "Contract assertion not met: routePrefix != null");
            }

            if (routePrefix == string.Empty)
            {
                throw new ArgumentException("Contract assertion not met: routePrefix != string.Empty", nameof(routePrefix));
            }

            // Model.
            var model = builder.Build();

            // Support batch requests.
            IDataService dataService = (IDataService)config.DependencyResolver.GetService(typeof(IDataService));

            if (dataService == null)
            {
                throw new InvalidOperationException("IDataService is not registered in the dependency scope.");
            }

            ODataBatchHandler batchHandler = new DataObjectODataBatchHandler(dataService, httpServer, isSyncBatchUpdate);
            batchHandler.ODataRouteName = routeName;
            config.Routes.MapHttpBatchRoute(routeName + "Batch", routePrefix + "/$batch", batchHandler);

            // Routing for DataObjects.
            var pathHandler = new ExtendedODataPathHandler();
            var routingConventions = DataObjectRoutingConventions.CreateDefault();
            var route = config.MapODataServiceRoute(routeName, routePrefix, model, pathHandler, routingConventions);

            // Controllers.
            var registeredActivator = (IHttpControllerActivator)config.Services.GetService(typeof(IHttpControllerActivator));
            var fallbackActivator = registeredActivator ?? new DefaultHttpControllerActivator();
            config.Services.Replace(typeof(IHttpControllerActivator), new DataObjectControllerActivator(fallbackActivator));

            // Formatters.
            var customODataSerializerProvider = new CustomODataSerializerProvider();
            var extendedODataDeserializerProvider = new ExtendedODataDeserializerProvider();
            var odataFormatters = ODataMediaTypeFormatters.Create(customODataSerializerProvider, extendedODataDeserializerProvider);
            config.Formatters.InsertRange(0, odataFormatters);
            config.Properties[typeof(CustomODataSerializerProvider)] = customODataSerializerProvider;

            // Token.
            var token = new ManagementToken(route, model);
            config.SetODataServiceToken(token);

            // Handlers.
            if (config.MessageHandlers.FirstOrDefault(h => h is PostPatchHandler) == null)
            {
                config.MessageHandlers.Add(new PostPatchHandler());
            }

            return token;
        }

        /// <summary>
        /// Gets the OData Service token for current request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Stored OData Service token.</returns>
        /// <exception cref="InvalidOperationException">Thrown on errors in loading token from configuration.</exception>
        public static ManagementToken GetODataServiceToken(this HttpRequestMessage request)
        {
            object savedToken;
            if (!request.GetConfiguration().Properties.TryGetValue(request.GetRouteData().Route, out savedToken))
            {
                throw new InvalidOperationException("OData Service management token hasn't been set in the appropriate handler.");
            }

            var result = savedToken as ManagementToken;
            if (result == null)
            {
                throw new InvalidOperationException("Something different has been saved instead of OData Service management token.");
            }

            return result;
        }

        /// <summary>
        /// Sets the OData Service token to the configuration for internal purposes.
        /// </summary>
        /// <param name="config">The current HTTP configuration.</param>
        /// <param name="token">The OData Service token.</param>
        private static void SetODataServiceToken(this HttpConfiguration config, ManagementToken token)
        {
            config.Properties[token.Route] = token;
        }
    }
}
