namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using ICSSoft.STORMNET.Business;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
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
        /// Maps the OData Service DataObject route.
        /// </summary>
        /// <param name="config">The current HTTP configuration.</param>
        /// <param name="builder">The EDM model builder.</param>
        /// <param name="httpServer">HttpServer instance (GlobalConfiguration.DefaultServer).</param>
        /// <param name="routeName">The name of the route (<see cref="DataObjectRoutingConventions.DefaultRouteName"/> to be default).</param>
        /// <param name="routePrefix">The route prefix (<see cref="DataObjectRoutingConventions.DefaultRoutePrefix"/> to be default).</param>
        /// <param name="isSyncBatchUpdate">Use synchronous mode for call subrequests in batch query.</param>
        /// <returns>A <see cref="ManagementToken"/> instance.</returns>
        public static ManagementToken MapDataObjectRoute(
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
            DataObjectEdmModel model = builder.Build();

            // DataService for batch requests support.
            IDataService dataService = (IDataService)config.DependencyResolver.GetService(typeof(IDataService));
            if (dataService == null)
            {
                throw new InvalidOperationException("IDataService is not registered in the dependency scope.");
            }

            // Routing for DataObjects with $batch endpoint and custom formatters.
            var pathHandler = new ExtendedODataPathHandler();
            List<IODataRoutingConvention> routingConventions = DataObjectRoutingConventions.CreateDefault();
            var batchHandler = new DataObjectODataBatchHandler(dataService, httpServer, isSyncBatchUpdate);
            ODataRoute route = config.MapODataServiceRoute(routeName, routePrefix, cb => cb
                .AddService(ServiceLifetime.Singleton, typeof(IEdmModel), sp => model)
                .AddService(ServiceLifetime.Singleton, typeof(IODataPathHandler), sp => pathHandler)
                .AddService(ServiceLifetime.Singleton, typeof(IEnumerable<IODataRoutingConvention>), sp => routingConventions.AsEnumerable())
                .AddService(ServiceLifetime.Singleton, typeof(ODataBatchHandler), sp => batchHandler)
                .AddService(ServiceLifetime.Singleton, typeof(ODataSerializerProvider), sp => new CustomODataSerializerProvider(sp))
                .AddService(ServiceLifetime.Singleton, typeof(ODataDeserializerProvider), sp => new ExtendedODataDeserializerProvider(sp)));

            // Token.
            ManagementToken token = route.CreateManagementToken(model);

            // Controllers.
            var registeredActivator = (IHttpControllerActivator)config.Services.GetService(typeof(IHttpControllerActivator));
            var fallbackActivator = registeredActivator ?? new DefaultHttpControllerActivator();
            config.Services.Replace(typeof(IHttpControllerActivator), new DataObjectControllerActivator(fallbackActivator));

            // Handlers.
            if (config.MessageHandlers.FirstOrDefault(h => h is PostPatchHandler) == null)
            {
                config.MessageHandlers.Add(new PostPatchHandler());
            }

            return token;
        }

        /// <summary>
        /// Maps the OData Service DataObject route.
        /// </summary>
        /// <param name="config">The current HTTP configuration.</param>
        /// <param name="builder">The EDM model builder.</param>
        /// <param name="httpServer">HttpServer instance (GlobalConfiguration.DefaultServer).</param>
        /// <param name="routeName">The name of the route (<see cref="DataObjectRoutingConventions.DefaultRouteName"/> to be default).</param>
        /// <param name="routePrefix">The route prefix (<see cref="DataObjectRoutingConventions.DefaultRoutePrefix"/> to be default).</param>
        /// <param name="isSyncBatchUpdate">Use synchronous mode for call subrequests in batch query.</param>
        /// <returns>A <see cref="ManagementToken"/> instance.</returns>
        [Obsolete("Use MapDataObjectRoute() method instead.")]
        public static ManagementToken MapODataServiceDataObjectRoute(
            this HttpConfiguration config,
            IDataObjectEdmModelBuilder builder,
            HttpServer httpServer,
            string routeName = DataObjectRoutingConventions.DefaultRouteName,
            string routePrefix = DataObjectRoutingConventions.DefaultRoutePrefix,
            bool? isSyncBatchUpdate = null)
        {
            return MapDataObjectRoute(config, builder, httpServer, routeName, routePrefix, isSyncBatchUpdate);
        }
    }
}
