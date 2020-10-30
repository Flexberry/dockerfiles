namespace NewPlatform.Flexberry.ORM.ODataService.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using NewPlatform.Flexberry.ORM.ODataService.Controllers;
    using NewPlatform.Flexberry.ORM.ODataService.Events;

#if NETFRAMEWORK
    using System.Web.Http;
    using System.Web.Http.Batch;
#endif

#if NETSTANDARD
    using ICSSoft.Services;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Adapters;
    using Microsoft.AspNet.OData.Common;
    using Microsoft.AspNetCore.Http;
#endif

    /// <summary>
    /// Batch handler for DataService.
    /// </summary>
    internal class DataObjectODataBatchHandler : DefaultODataBatchHandler
    {
        /// <summary>
        /// Request Properties collection key for DataObjectsToUpdate list.
        /// </summary>
        public const string DataObjectsToUpdatePropertyKey = "DataObjectsToUpdate";

        /// <summary>
        /// Request Properties collection key for DataObjectCache instance.
        /// </summary>
        public const string DataObjectCachePropertyKey = "DataObjectCache";

        /// <summary>
        /// The container with registered events.
        /// </summary>
        private IEventHandlerContainer _events;

        /// <summary>
        /// Initializes the container with registered events.
        /// </summary>
        /// <param name="events">The container with registered events.</param>
        public void InitializeEvents(IEventHandlerContainer events)
        {
            _events = events;
        }

#if NETFRAMEWORK
        /// <summary>
        /// if set to true then use synchronous mode for call subrequests.
        /// </summary>
        private readonly bool isSyncMode;


        /// <summary>
        /// DataService instance for execute queries.
        /// </summary>
        private readonly IDataService dataService;

        /// <summary>
        /// Initializes a new instance of the NewPlatform.Flexberry.ORM.ODataService.Batch.DataObjectODataBatchHandler class.
        /// </summary>
        /// <param name="dataService">DataService instance for execute queries.</param>
        /// <param name="httpServer">The System.Web.Http.HttpServer for handling the individual batch requests.</param>
        /// <param name="isSyncMode">Use synchronous mode for call subrequests.</param>
        public DataObjectODataBatchHandler(IDataService dataService, HttpServer httpServer, bool? isSyncMode = null)
            : base(httpServer)
        {
            this.dataService = dataService;

            this.isSyncMode = isSyncMode ?? Type.GetType("Mono.Runtime") != null;
        }

#endif

#if NETSTANDARD

        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectODataBatchHandler"/> class.
        /// </summary>
        public DataObjectODataBatchHandler()
            : base()
        {
        }

#endif

#if NETFRAMEWORK

        /// <inheritdoc />
        public override async Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateRequest(request);

            IList<ODataBatchRequestItem> subRequests = isSyncMode
                ? ParseBatchRequestsAsync(request, cancellationToken).Result
                : await ParseBatchRequestsAsync(request, cancellationToken);

            try
            {
                if (isSyncMode)
                {
                    IList<ODataBatchResponseItem> responses = ExecuteRequestMessagesAsync(subRequests, cancellationToken).Result;
                    return CreateResponseMessageAsync(responses, request, cancellationToken).Result;
                }
                else
                {
                    IList<ODataBatchResponseItem> responses = await ExecuteRequestMessagesAsync(subRequests, cancellationToken);
                    return await CreateResponseMessageAsync(responses, request, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                return DataObjectController.InternalServerErrorMessage(ex, _events, request);
            }
            finally
            {
                foreach (ODataBatchRequestItem subRequest in subRequests)
                {
                    request.RegisterForDispose(subRequest.GetResourcesForDisposal());
                    request.RegisterForDispose(subRequest);
                }
            }
        }
#endif

#if NETSTANDARD
        /// <inheritdoc />
        public override async Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (nextHandler == null)
            {
                throw Error.ArgumentNull(nameof(nextHandler));
            }

            if (!await ValidateRequest(context.Request))
            {
                return;
            }

            try
            {
                IList<ODataBatchRequestItem> subRequests = await ParseBatchRequestsAsync(context);

                ODataOptions options = context.RequestServices.GetRequiredService<ODataOptions>();
                bool enableContinueOnErrorHeader = (options != null)
                    ? options.EnableContinueOnErrorHeader
                    : false;

                SetContinueOnError(new WebApiRequestHeaders(context.Request.Headers), enableContinueOnErrorHeader);

                IList<ODataBatchResponseItem> responses = await ExecuteRequestMessagesAsync(new ODataBatchRequestsWrapper(context, subRequests), nextHandler);
                await CreateResponseMessageAsync(responses, context.Request);
            }
            catch (Exception ex)
            {
                if (_events?.CallbackAfterInternalServerError != null)
                {
                    var statusCode = System.Net.HttpStatusCode.InternalServerError;
                    _events.CallbackAfterInternalServerError(ex, ref statusCode);
                }

                throw;
            }
        }

#endif

#if NETFRAMEWORK
        /// <inheritdoc />
        public override async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IServiceProvider requestContainer = request.CreateRequestContainer(ODataRouteName);
            ODataMessageReaderSettings oDataReaderSettings = requestContainer.GetRequiredService<ODataMessageReaderSettings>();

            oDataReaderSettings.MessageQuotas = MessageQuotas;
            oDataReaderSettings.BaseUri = GetBaseUri(request);

            ODataMessageReader reader = isSyncMode
                ? request.Content.GetODataMessageReaderAsync(requestContainer, cancellationToken).Result
                : await request.Content.GetODataMessageReaderAsync(requestContainer, cancellationToken);

            request.RegisterForDispose(reader);

            List<ODataBatchRequestItem> requests = new List<ODataBatchRequestItem>();
            ODataBatchReader batchReader = reader.CreateODataBatchReader();
            Guid batchId = Guid.NewGuid();
            while (batchReader.Read())
            {
                switch (batchReader.State)
                {
                    case ODataBatchReaderState.ChangesetStart:
                        IList<HttpRequestMessage> changeSetRequests = isSyncMode
                            ? batchReader.ReadChangeSetRequestAsync(batchId, cancellationToken).Result
                            : await batchReader.ReadChangeSetRequestAsync(batchId, cancellationToken);

                        foreach (HttpRequestMessage changeSetRequest in changeSetRequests)
                        {
                            changeSetRequest.CopyBatchRequestProperties(request);
                            changeSetRequest.DeleteRequestContainer(false);
                        }

                        requests.Add(new ChangeSetRequestItem(changeSetRequests));
                        break;
                    case ODataBatchReaderState.Operation:
                        HttpRequestMessage operationRequest = isSyncMode
                            ? batchReader.ReadOperationRequestAsync(batchId, true, cancellationToken).Result
                            : await batchReader.ReadOperationRequestAsync(batchId, true, cancellationToken);

                        operationRequest.CopyBatchRequestProperties(request);
                        operationRequest.DeleteRequestContainer(false);
                        requests.Add(new OperationRequestItem(operationRequest));
                        break;
                }
            }

            return requests;
        }
#endif

#if NETFRAMEWORK
        /// <inheritdoc />
        public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, CancellationToken cancellation)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            IList<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            try
            {
                foreach (ODataBatchRequestItem request in requests)
                {
                    ODataBatchResponseItem response;
                    switch (request)
                    {
                        case OperationRequestItem operation:
                            response = isSyncMode
                                ? request.SendRequestAsync(Invoker, cancellation).Result
                                : await request.SendRequestAsync(Invoker, cancellation);
                            break;
                        case ChangeSetRequestItem change:
                            response = isSyncMode
                                ? ExecuteChangeSet(change, cancellation).Result
                                : await ExecuteChangeSet(change, cancellation);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unsupported request of type `{request.GetType()}`");
                    }

                    responses.Add(response);
                }
            }
            catch
            {
                foreach (ODataBatchResponseItem response in responses)
                {
                    response?.Dispose();
                }

                throw;
            }

            return responses;
        }
#endif

#if NETSTANDARD
        /// <inheritdoc />
        public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
        {
            if (requests == null)
            {
                throw Error.ArgumentNull(nameof(requests));
            }

            if (handler == null)
            {
                throw Error.ArgumentNull(nameof(handler));
            }

            IList<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();

            HttpContext batchContext = (requests as ODataBatchRequestsWrapper).BatchContext;
            foreach (ODataBatchRequestItem request in requests)
            {
                ODataBatchResponseItem responseItem;
                switch (request)
                {
                    case OperationRequestItem operation:
                        responseItem = await request.SendRequestAsync(handler);
                        break;
                    case ChangeSetRequestItem changeSet:
                        responseItem = await ExecuteChangeSet(batchContext, changeSet, handler);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unsupported request of type `{request.GetType()}`");
                }

                responses.Add(responseItem);

                if (responseItem != null && responseItem.IsResponseSuccessful() == false && ContinueOnError == false)
                {
                    break;
                }
            }

            return responses;
        }
#endif

#if NETFRAMEWORK
        /// <summary>
        /// Execute changeset processing.
        /// </summary>
        /// <param name="changeSet">Changeset for processing.</param>
        /// <param name="cancellation">Cancelation token.</param>
        /// <returns>Task for changeset processing.</returns>
        private async Task<ODataBatchResponseItem> ExecuteChangeSet(ChangeSetRequestItem changeSet, CancellationToken cancellation)
        {
            if (changeSet == null)
            {
                throw new ArgumentNullException(nameof(changeSet));
            }

            List<DataObject> dataObjectsToUpdate = new List<DataObject>();
            DataObjectCache dataObjectCache = new DataObjectCache();
            dataObjectCache.StartCaching(false);

            foreach (HttpRequestMessage request in changeSet.Requests)
            {
                if (!request.Properties.ContainsKey(DataObjectsToUpdatePropertyKey))
                {
                    request.Properties.Add(DataObjectsToUpdatePropertyKey, dataObjectsToUpdate);
                }

                if (!request.Properties.ContainsKey(DataObjectCachePropertyKey))
                {
                    request.Properties.Add(DataObjectCachePropertyKey, dataObjectCache);
                }
            }

            ChangeSetResponseItem changeSetResponse = isSyncMode
                ? (ChangeSetResponseItem)changeSet.SendRequestAsync(Invoker, cancellation).Result
                : (ChangeSetResponseItem)await changeSet.SendRequestAsync(Invoker, cancellation);

            if (changeSetResponse.Responses.All(r => r.IsSuccessStatusCode))
            {
                try
                {
                    DataObject[] dataObjects = dataObjectsToUpdate.ToArray();
                    dataService.UpdateObjects(ref dataObjects);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return changeSetResponse;
        }
#endif

#if NETSTANDARD
        /// <summary>
        /// Execute changeset processing.
        /// </summary>
        /// <param name="batchContext">The http context of a batch request.</param>
        /// <param name="changeSet">The changeset for processing.</param>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>Task for changeset processing.</returns>
        private async Task<ODataBatchResponseItem> ExecuteChangeSet(HttpContext batchContext, ChangeSetRequestItem changeSet, RequestDelegate handler)
        {
            if (changeSet == null)
            {
                throw new ArgumentNullException(nameof(changeSet));
            }

            List<DataObject> dataObjectsToUpdate = new List<DataObject>();
            DataObjectCache dataObjectCache = new DataObjectCache();
            dataObjectCache.StartCaching(false);

            foreach (HttpContext context in changeSet.Contexts)
            {
                if (!context.Items.ContainsKey(DataObjectsToUpdatePropertyKey))
                {
                    context.Items.Add(DataObjectsToUpdatePropertyKey, dataObjectsToUpdate);
                }

                if (!context.Items.ContainsKey(DataObjectCachePropertyKey))
                {
                    context.Items.Add(DataObjectCachePropertyKey, dataObjectCache);
                }
            }

            ChangeSetResponseItem changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(handler);

            if (changeSetResponse.Contexts.All(x => x.Response.IsSuccessStatusCode()))
            {
                try
                {
                    IDataService dataService = UnityFactoryHelper.ResolveRequiredIfNull(batchContext.RequestServices.GetService<IDataService>());
                    DataObject[] dataObjects = dataObjectsToUpdate.ToArray();
                    dataService.UpdateObjects(ref dataObjects);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return changeSetResponse;
        }
#endif
    }
}
