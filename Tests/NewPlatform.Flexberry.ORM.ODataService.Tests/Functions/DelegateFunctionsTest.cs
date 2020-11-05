namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    using NewPlatform.Flexberry.ORM.ODataService.Functions;

    using Newtonsoft.Json;

    using Xunit;

    /// <summary>
    /// Unit test class for OData Service user-defined functions
    /// </summary>
    public class DelegateFunctionsTest : BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        /// <param name="output">Вывод отладочной информации.</param>
        public DelegateFunctionsTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output)
            : base(factory, output)
        {
        }
#endif

        /// <summary>
        /// Unit test for <see cref="IFunctionContainer.Register(Delegate)"/>.
        /// Tests the function call without query parameters.
        /// </summary>
        [Fact]
        public void TestFunctionCallWithoutQueryParameters()
        {
            ActODataService(args =>
            {
                args.Token.Functions.Register(new Func<int, int, int>(AddWithoutQueryParameters));

                string url = "http://localhost/odata/AddWithoutQueryParameters(a=2,b=2)";
                using (HttpResponseMessage response = args.HttpClient.GetAsync(url).Result)
                {
                    var resultText = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultText);

                    Assert.Equal(4, (int)(long)result["value"]);
                }
            });
        }

        /// <summary>
        /// Unit test for <see cref="IFunctionContainer.Register(Delegate)"/>.
        /// Tests the function call with query parameters.
        /// </summary>
        [Fact]
        public void TestFunctionCallWithQueryParameters()
        {
            ActODataService(args =>
            {
                args.Token.Functions.Register(new Func<QueryParameters, int, int, int>(AddWithQueryParameters));

                string url = "http://localhost/odata/AddWithQueryParameters(a=2,b=2)";
                using (HttpResponseMessage response = args.HttpClient.GetAsync(url).Result)
                {
                    var resultText = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultText);

                    Assert.Equal(4, (int)(long)result["value"]);
                }
            });
        }

        private static int AddWithoutQueryParameters(int a, int b)
        {
            return a + b;
        }

        private static int AddWithQueryParameters(QueryParameters queryParameters, int a, int b)
        {
            Assert.NotNull(queryParameters);

            return a + b;
        }
    }
}
