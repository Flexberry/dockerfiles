namespace NewPlatform.Flexberry.ORM.ODataService.Handlers
{
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.OData.Routing;

    using NewPlatform.Flexberry.ORM.ODataService.Model;

    /// <summary>
    /// Delegate WebAPI handler class that updates current EDM model at each request.
    /// </summary>
    /// <seealso cref="DelegatingHandler" />
    internal class PerRequestUpdateEdmModelHandler : DelegatingHandler
    {
        /// <summary>
        /// The OData Service management token.
        /// </summary>
        private readonly ManagementToken _token;

        /// <summary>
        /// The model builder for updating model.
        /// </summary>
        private readonly IDataObjectEdmModelBuilder _modelBuilder;

        /// <summary>
        /// The property for changing EDM model using reflection.
        /// </summary>
        private readonly PropertyInfo _edmModelProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerRequestUpdateEdmModelHandler"/> class.
        /// </summary>
        /// <param name="token">The OData Service management token.</param>
        /// <param name="modelBuilder">
        /// The model builder for updating model.
        /// Each request EDM model associated with <paramref name="token"/> will be updated.
        /// </param>
        public PerRequestUpdateEdmModelHandler(ManagementToken token, IDataObjectEdmModelBuilder modelBuilder)
        {
            // In current Microsoft implementation of OData for WebAPI, EDM model is stored
            // in ODataPathRouteConstraint, but doesn't have public setter.
            // In order to rebuild EDM model for different requests (e.g. authorized users),
            // this handler uses reflection for private setter calling.
            _edmModelProperty = typeof(ODataPathRouteConstraint).GetProperty("EdmModel");
            if (_edmModelProperty == null)
            {
                throw new InvalidOperationException("Current OData API cannot be used for editing EDM model.");
            }

            _token = token ?? throw new ArgumentNullException(nameof(token), "Contract assertion not met: token != null");
            _modelBuilder = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder), "Contract assertion not met: modelBuilder != null");
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
        /// </returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var model = _modelBuilder.Build();
            _token.Model = model;
            _edmModelProperty.SetValue(_token.Route.PathRouteConstraint, model, null);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
