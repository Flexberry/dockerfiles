#if NETCOREAPP
namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using ODataServiceSample.AspNetCore;
    using Xunit;

    /// <summary>
    /// Тесты, специфичные для .NET Core.
    /// </summary>
    public class DotNetCoreTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public DotNetCoreTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Base metadata test.
        /// </summary>
        /// <returns>Test task.</returns>
        [Fact]
        public async Task GetMetadataTest()
        {
            HttpClient client = _factory.CreateClient();

            // Arrange & Act
            var response = await client.GetAsync("/odata/$metadata");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Load BS type test.
        /// </summary>
        [Fact]
        public void LoadTypeTest()
        {
            string typeName = "NewPlatform.Flexberry.ORM.ODataService.Tests.BearBS, NewPlatform.Flexberry.ORM.ODataService.Tests.BusinessServers";

            Type type = Type.GetType(typeName);

            Assert.NotNull(type);
        }
    }
}
#endif
