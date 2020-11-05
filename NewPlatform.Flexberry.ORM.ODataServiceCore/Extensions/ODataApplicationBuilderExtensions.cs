namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using System;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Common;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using NewPlatform.Flexberry.ORM.ODataServiceCore.Middleware;

    /// <summary>
    /// Provides extension methods for <see cref="IApplicationBuilder"/> to add ODataService routes.
    /// </summary>
    public static class ODataApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds ODataService to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <param name="configureRoutes">A callback to configure MVC routes.</param>
        /// <param name="maxTopValue">Sets the max value of $top that a client can request in route builder.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseODataService(this IApplicationBuilder app, Action<IRouteBuilder> configureRoutes, int? maxTopValue = int.MaxValue)
        {
            if (app == null)
            {
                throw Error.ArgumentNull("app");
            }

            VerifyODataServiceIsRegistered(app);

            return app
                .UseODataBatching()
                .UseMiddleware<RequestHeadersHookMiddleware>()
                .UseMvc(builder =>
                {
                    builder.Select().Expand().Filter().OrderBy().MaxTop(maxTopValue).Count();
                })
                .UseMvc(configureRoutes);
        }

        private static void VerifyODataServiceIsRegistered(IApplicationBuilder app)
        {
            // We use the IPerRouteContainer to verify if AddOData() was called before calling UseODataService
            if (app.ApplicationServices.GetService(typeof(IPerRouteContainer)) == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
            }
        }
    }
}
