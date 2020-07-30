namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers;

    /// <summary>
    /// Provides extension methods for <see cref="IRouteBuilder"/> to add OData Service file storage route.
    /// </summary>
    public static class MapRouteRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the specified OData Service file storage route.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        public static IRouteBuilder MapFileRoute(
            this IRouteBuilder builder,
            string routeName = "file")
        {
            // Make sure the FileController is registered with the ApplicationPartManager.
            var applicationPartManager = builder.ServiceProvider.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.ApplicationParts.Add(new AssemblyPart(typeof(FileController).Assembly));
            
            var fileProviderAccessor = builder.ServiceProvider.GetRequiredService<IDataObjectFileAccessor>();

            return builder.MapRoute(routeName, fileProviderAccessor.RouteUrl, defaults: new { controller = "File", action = "FileAction" });
        }
    }
}
