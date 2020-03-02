namespace NewPlatform.Flexberry.ORM.ODataService
{
    using System;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Routing;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using NewPlatform.Flexberry.ORM.ODataService.Batch;
    using NewPlatform.Flexberry.ORM.ODataService.Controllers;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Handlers;
    using NewPlatform.Flexberry.ORM.ODataService.Offline;

    /// <summary>
    /// Controller activator class for instantiating <see cref="DataObjectController"/> with parameters.
    /// </summary>
    /// <seealso cref="System.Web.Http.Dispatcher.IHttpControllerActivator" />
    public class DataObjectControllerActivator : IHttpControllerActivator
    {
        /// <summary>
        /// Activator for all controllers except <see cref="DataObjectController"/>.
        /// </summary>
        private readonly IHttpControllerActivator _fallbackActivator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectControllerActivator"/> class.
        /// </summary>
        /// <param name="fallbackActivator">Activator for all controllers except <see cref="DataObjectController"/>.</param>
        public DataObjectControllerActivator(IHttpControllerActivator fallbackActivator)
        {
            _fallbackActivator = fallbackActivator ?? throw new ArgumentNullException(nameof(fallbackActivator), "Contract assertion not met: fallbackActivator != null");
        }

        /// <remarks>Creates <see cref="DataObjectController"/> using current <see cref="IDependencyScope"/> and <see cref="IHttpRoute"/>.</remarks>
        /// <inheritdoc />
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return controllerDescriptor.ControllerType == typeof(DataObjectController)
                ? CreateDataObjectController(request, controllerDescriptor, controllerType)
                : _fallbackActivator.Create(request, controllerDescriptor, controllerType);
        }

        /// <summary>
        /// Creates <see cref="DataObjectController"/> using current <see cref="IDependencyScope"/> and <see cref="IHttpRoute"/>.
        /// </summary>
        /// <param name="request">The message request.</param>
        /// <param name="controllerDescriptor">The HTTP controller descriptor.</param>
        /// <param name="controllerType">The type of the controller.</param>
        /// <returns>An <see cref="DataObjectController" /> object for specified arguments.</returns>
        protected virtual DataObjectController CreateDataObjectController(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            IDataService dataService = GetDataService(request, controllerDescriptor, controllerType);

            DataObjectCache dataObjectCache = GetDataObjectCache(request, controllerDescriptor, controllerType);

            ManagementToken token = request.GetODataServiceToken();

            DataObjectController controller = new DataObjectController(dataService, dataObjectCache, token.Model, token.Events, token.Functions);
            controller.OfflineManager = GetOfflineManager(request, controllerDescriptor, controllerType) ?? controller.OfflineManager;

            return controller;
        }

        /// <summary>
        /// Gets the instance of <see cref="IDataService" /> using current <see cref="IDependencyScope"/> and <see cref="IHttpRoute"/>.
        /// </summary>
        /// <param name="request">The message request.</param>
        /// <param name="controllerDescriptor">The HTTP controller descriptor.</param>
        /// <param name="controllerType">The type of the controller.</param>
        /// <returns>Gets the instance of <see cref="IDataService" /> for specified arguments.</returns>
        /// <remarks>Extracts object from configurated <see cref="IDependencyResolver" />.</remarks>
        protected virtual IDataService GetDataService(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            IDependencyResolver dependencyResolver = request.GetConfiguration().DependencyResolver;
            IDataService dataService = (IDataService)dependencyResolver.GetService(typeof(IDataService));

            if (dataService == null)
            {
                throw new InvalidOperationException("IDataService is not registered in the dependency scope.");
            }

            return dataService;
        }

        /// <summary>
        /// Gets the instance of <see cref="DataObjectCache" /> using current <see cref="IDependencyScope"/> and <see cref="IHttpRoute"/>.
        /// </summary>
        /// <param name="request">The message request.</param>
        /// <param name="controllerDescriptor">The HTTP controller descriptor.</param>
        /// <param name="controllerType">The type of the controller.</param>
        /// <returns>Gets the instance of <see cref="DataObjectCache" /> for specified arguments.</returns>
        /// <remarks>Extracts object from request properties for batch requests.</remarks>
        protected virtual DataObjectCache GetDataObjectCache(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            DataObjectCache dataObjectCache = null;
            if (request.Properties.ContainsKey(PostPatchHandler.PropertyKeyBatchRequest) && request.Properties.ContainsKey(DataObjectODataBatchHandler.DataObjectCachePropertyKey))
            {
                dataObjectCache = request.Properties[DataObjectODataBatchHandler.DataObjectCachePropertyKey] as DataObjectCache;
            }

            return dataObjectCache;
        }

        /// <summary>
        /// Gets the instance of <see cref="BaseOfflineManager" /> using current <see cref="IDependencyScope"/> and <see cref="IHttpRoute"/>.
        /// </summary>
        /// <param name="request">The message request.</param>
        /// <param name="controllerDescriptor">The HTTP controller descriptor.</param>
        /// <param name="controllerType">The type of the controller.</param>
        /// <returns>Gets the instance of <see cref="BaseOfflineManager" /> for specified arguments.</returns>
        /// <remarks>Extracts object from configurated <see cref="IDependencyResolver" />.</remarks>
        protected virtual BaseOfflineManager GetOfflineManager(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            IDependencyResolver dependencyResolver = request.GetConfiguration().DependencyResolver;
            BaseOfflineManager offlineManager = (BaseOfflineManager)dependencyResolver.GetService(typeof(BaseOfflineManager));
            return offlineManager;
        }
    }
}
