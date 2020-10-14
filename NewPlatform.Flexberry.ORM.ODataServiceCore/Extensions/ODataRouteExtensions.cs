namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using Microsoft.AspNet.OData.Routing;
    using NewPlatform.Flexberry.ORM.ODataService.Model;

    /// <summary>
    /// Provides extension methods for the <see cref="ODataRoute"/> class.
    /// </summary>
    public static class ODataRouteExtensions
    {
        /// <summary>
        /// Creates a <see cref="ManagementToken"/> instance and registers it as the OData route management token.
        /// </summary>
        /// <param name="route">The <see cref="ODataRoute"/> instance.</param>
        /// <param name="model">The <see cref="DataObjectEdmModel"/> instance.</param>
        /// <returns>An <see cref="ManagementToken"/> instance.</returns>
        public static ManagementToken CreateManagementToken(this ODataRoute route, DataObjectEdmModel model)
        {
            return ODataRouteHelper.CreateManagementToken(route, model);
        }

        /// <summary>
        /// Gets a <see cref="ManagementToken"/> instance previously registered as the OData route management token.
        /// </summary>
        /// <param name="route">The <see cref="ODataRoute"/> instance.</param>
        /// <returns>A <see cref="ManagementToken"/> instance.</returns>
        public static ManagementToken GetManagementToken(this ODataRoute route)
        {
            return ODataRouteHelper.GetManagementToken(route, true);
        }
    }
}
