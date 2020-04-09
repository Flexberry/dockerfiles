namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using System;
    using System.Web.OData.Routing;
    using NewPlatform.Flexberry.ORM.ODataService.Model;

    /// <summary>
    /// Helper for <see cref="ODataRoute"/> extensions.
    /// </summary>
    internal static class ODataRouteHelper
    {
        /// <summary>
        /// Creates a <see cref="ManagementToken"/> instance and registers it as the OData route management token.
        /// </summary>
        /// <param name="route">The <see cref="ODataRoute"/> instance.</param>
        /// <param name="model">The <see cref="DataObjectEdmModel"/> instance.</param>
        /// <returns>An <see cref="ManagementToken"/> instance.</returns>
        internal static ManagementToken CreateManagementToken(ODataRoute route, DataObjectEdmModel model)
        {
            var token = new ManagementToken(route, model);
            route.DataTokens.Add(ManagementToken.DataTokenKey, token);

            return token;
        }

        /// <summary>
        /// Gets a <see cref="ManagementToken"/> instance previously registered as the OData route management token.
        /// </summary>
        /// <param name="route">The <see cref="ODataRoute"/> instance.</param>
        /// <param name="isRequired">The flag indicating that the OData route management token is required.</param>
        /// <returns>A <see cref="ManagementToken"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown on errors at the OData route management token retrieving.</exception>
        internal static ManagementToken GetManagementToken(ODataRoute route, bool isRequired)
        {
            object o;
            bool ok = route.DataTokens.TryGetValue(ManagementToken.DataTokenKey, out o);

            var result = o as ManagementToken;

            if (ok)
            {
                // A management token type check.
                if (result == null)
                {
                    throw new InvalidOperationException("Something different has been saved instead of a management token.");
                }

                // The management token route value check.
                if (result.Route != route)
                {
                    throw new InvalidOperationException("The management token route value not matches the 'route' parameter value.");
                }
            }

            // A management token instance is required but not exists check.
            if (isRequired && result == null)
            {
                throw new InvalidOperationException("A management token hasn't been set in the appropriate handler.");
            }

            return result;
        }
    }
}
