#if NETCOREAPP
namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System.IO;
    using ICSSoft.Services;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Unity.Microsoft.DependencyInjection;

    /// <summary>
    /// Custom web application factory for tests.
    /// </summary>
    /// <typeparam name="TStartup">Startup type.</typeparam>
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        /// <inheritdoc/>
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            string contentRootDirectory = Directory.GetCurrentDirectory();
            var container = UnityFactory.GetContainer();
            var webHostBuilder = new WebHostBuilder()
                            .UseUnityServiceProvider(container)
                            .UseContentRoot(contentRootDirectory)
                            .UseStartup<TestStartup>();
            return webHostBuilder;
        }
    }
}
#endif
