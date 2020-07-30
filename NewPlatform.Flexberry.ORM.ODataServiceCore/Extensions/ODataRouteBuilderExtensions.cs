namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using System.Collections.Generic;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Common;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using NewPlatform.Flexberry.ORM.ODataService.Batch;
    using NewPlatform.Flexberry.ORM.ODataService.Controllers;
    using NewPlatform.Flexberry.ORM.ODataService.Formatter.Deserialization;
    using NewPlatform.Flexberry.ORM.ODataService.Formatter.Serialization;
    using NewPlatform.Flexberry.ORM.ODataService.Handlers;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Routing.Conventions;
    
    using ServiceLifetime = Microsoft.OData.ServiceLifetime;

    /// <summary>
    /// Provides extension methods for <see cref="IRouteBuilder"/> to add OData Service DataObject route.
    /// </summary>
    public static class ODataRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the specified OData Service DataObject route.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="modelBuilder">The Edm model builder.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        public static ManagementToken MapDataObjectRoute(
            this IRouteBuilder builder,
            IDataObjectEdmModelBuilder modelBuilder,
            string routeName = DataObjectRoutingConventions.DefaultRouteName,
            string routePrefix = DataObjectRoutingConventions.DefaultRoutePrefix)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            // Model.
            DataObjectEdmModel model = modelBuilder.Build();

            // Make sure the DataObjectController is registered with the ApplicationPartManager.
            var applicationPartManager = builder.ServiceProvider.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.ApplicationParts.Add(new AssemblyPart(typeof(DataObjectController).Assembly));

            ODataRoute route = builder.MapODataServiceRoute(routeName, routePrefix, cb => cb
                .AddService(ServiceLifetime.Singleton, typeof(IEdmModel), sp => model)
                .AddService(ServiceLifetime.Singleton, typeof(IODataPathHandler), sp => new ExtendedODataPathHandler())
                .AddService(ServiceLifetime.Singleton, typeof(IEnumerable<IODataRoutingConvention>), sp => DataObjectRoutingConventions.CreateDefault())
                .AddService(ServiceLifetime.Singleton, typeof(ODataBatchHandler), sp => new DataObjectODataBatchHandler())
                .AddService(ServiceLifetime.Singleton, typeof(ODataSerializerProvider), sp => new CustomODataSerializerProvider(sp))
                .AddService(ServiceLifetime.Singleton, typeof(ODataDeserializerProvider), sp => new ExtendedODataDeserializerProvider(sp)));

            // Token.
            ManagementToken token = route.CreateManagementToken(model);

            return token;
        }
    }
}
