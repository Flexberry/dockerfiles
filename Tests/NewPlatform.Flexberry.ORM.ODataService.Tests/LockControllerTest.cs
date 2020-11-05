#if NETFRAMEWORK
namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using System.Web.Http.SelfHost;
    using System.Web.Http.SelfHost.Channels;

    using NewPlatform.Flexberry.Services;
    using Unity;
    using Unity.AspNet.WebApi;
    using Xunit;
    using LockService = NewPlatform.Flexberry.Services.LockService;

    /// <summary>
    /// Unit-test class for <see cref="LockController"/>.
    /// </summary>
    public class LockControllerTest : BaseIntegratedTest
    {
        /// <summary>
        /// Configuration for WebAPI self host for NTLM authentication.
        ///
        /// TODO: This class may be useful.
        /// </summary>
        /// <seealso cref="HttpSelfHostConfiguration" />
        public class NtlmSelfHostConfiguration : HttpSelfHostConfiguration
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NtlmSelfHostConfiguration"/> class.
            /// </summary>
            /// <param name="baseAddress">The base address in a form of string.</param>
            public NtlmSelfHostConfiguration(string baseAddress)
                : base(baseAddress)
            {
            }

            /// <summary>
            /// Called to apply the configuration on the endpoint level.
            /// </summary>
            /// <param name="httpBinding">The HTTP binding.</param>
            /// <returns>Used from base method.</returns>
            protected override BindingParameterCollection OnConfigureBinding(HttpBinding httpBinding)
            {
                httpBinding.Security.Mode = HttpBindingSecurityMode.TransportCredentialOnly;
                httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                return base.OnConfigureBinding(httpBinding);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockControllerTest"/> class.
        /// </summary>
        public LockControllerTest()
            : base("ODataLck")
        {
        }

        /// <summary>
        /// Integrated unit test for <see cref="LockController"/>.
        /// Starts and stops self-hosted WebAPI server with locks.
        /// </summary>
        [Fact(Skip = "Разобраться что это за тест и насколько он нужен.")]
        public void TestStartStop()
        {
            foreach (var dataService in DataServices)
            {
                var container = new UnityContainer();
                container.RegisterInstance(typeof(ILockService), new LockService(dataService));

                using (HttpConfiguration config = new HttpConfiguration())
                {
                    config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                    config.Routes.MapHttpRoute("Flexberry - Lock", "lock/{dataObjectId}", new { controller = "Lock", action = "Lock" });
                    config.Routes.MapHttpRoute("Flexberry - Unlock", "unlock/{dataObjectId}", new { controller = "Lock", action = "Unlock" });
                    config.DependencyResolver = new UnityDependencyResolver(container);

                    using (HttpServer server = new HttpServer(config))
                    {
                        using (HttpClientHandler handler = new HttpClientHandler { UseDefaultCredentials = true })
                        {
                            server.InnerHandler = handler;

                            // TODO: check 401 without (403 with wrong) credentials
                            using (var client = new HttpClient(server, false))
                            {
                                client.BaseAddress = new Uri("http://localhost:8050");
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                using (HttpResponseMessage response = client.GetAsync("lock/1").Result)
                                {
                                    Assert.True(response.IsSuccessStatusCode);
                                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                                }

                                using (HttpResponseMessage response = client.GetAsync("unlock/1").Result)
                                {
                                    Assert.True(response.IsSuccessStatusCode);
                                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                                }

                                using (HttpResponseMessage response = client.GetAsync("unlock/2").Result)
                                {
                                    Assert.False(response.IsSuccessStatusCode);
                                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif
