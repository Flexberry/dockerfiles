namespace NewPlatform.Flexberry.ORM.ODataServiceCore.Extensions
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides extension methods to add ODataService services.
    /// </summary>
    public static class ODataServiceCollectionExtensions
    {
        /// <summary>
        /// Adds essential ODataService services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>An <see cref="IODataBuilder"/> that can be used to further configure the OData services.</returns>
        public static IODataBuilder AddODataService(this IServiceCollection services)
        {
            return services.AddHttpContextAccessor()
                .AddOData();
        }
    }
}
