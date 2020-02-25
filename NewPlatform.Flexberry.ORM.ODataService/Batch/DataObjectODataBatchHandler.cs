namespace NewPlatform.Flexberry.ORM.ODataService.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Batch;
    using System.Web.OData.Batch;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Microsoft.OData.Core;

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
        /// Flag, indicates that runtime is mono.
        /// </summary>
        private bool? isSyncMode;

        /// <summary>
        /// DataService instance for execute queries.
        /// </summary>
        private IDataService dataService;

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

            if (isSyncMode == null)
            {
                this.isSyncMode = Type.GetType("Mono.Runtime") != null;
            }
            else
            {
                this.isSyncMode = isSyncMode;
            }
        }

        /// <inheritdoc />
        public override async Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateRequest(request);

            IList<ODataBatchRequestItem> subRequests;

            if (isSyncMode == true)
            {
                subRequests = ParseBatchRequestsAsync(request, cancellationToken).Result;
            }
            else
            {
                subRequests = await ParseBatchRequestsAsync(request, cancellationToken);
            }

            try
            {
                if (isSyncMode == true)
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
            finally
            {
                foreach (ODataBatchRequestItem subRequest in subRequests)
                {
                    request.RegisterForDispose(subRequest.GetResourcesForDisposal());
                    request.RegisterForDispose(subRequest);
                }
            }
        }

        /// <inheritdoc />
        public override async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings
            {
                DisableMessageStreamDisposal = true,
                MessageQuotas = MessageQuotas,
                BaseUri = GetBaseUri(request)
            };

            ODataMessageReader reader;

            if (isSyncMode == true)
            {
                reader = request.Content.GetODataMessageReaderAsync(oDataReaderSettings, cancellationToken).Result;
            }
            else
            {
                reader = await request.Content.GetODataMessageReaderAsync(oDataReaderSettings, cancellationToken);
            }

            request.RegisterForDispose(reader);

            List<ODataBatchRequestItem> requests = new List<ODataBatchRequestItem>();
            ODataBatchReader batchReader = reader.CreateODataBatchReader();
            Guid batchId = Guid.NewGuid();
            while (batchReader.Read())
            {
                if (batchReader.State == ODataBatchReaderState.ChangesetStart)
                {
                    IList<HttpRequestMessage> changeSetRequests;

                    if (isSyncMode == true)
                    {
                        changeSetRequests = batchReader.ReadChangeSetRequestAsync(batchId, cancellationToken).Result;
                    }
                    else
                    {
                        changeSetRequests = await batchReader.ReadChangeSetRequestAsync(batchId, cancellationToken);
                    }

                    foreach (HttpRequestMessage changeSetRequest in changeSetRequests)
                    {
                        changeSetRequest.CopyBatchRequestProperties(request);
                    }

                    requests.Add(new ChangeSetRequestItem(changeSetRequests));
                }
                else if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    HttpRequestMessage operationRequest;

                    if (isSyncMode == true)
                    {
                        operationRequest = batchReader.ReadOperationRequestAsync(batchId, bufferContentStream: true, cancellationToken: cancellationToken).Result;
                    }
                    else
                    {
                        operationRequest = await batchReader.ReadOperationRequestAsync(batchId, bufferContentStream: true, cancellationToken: cancellationToken);
                    }

                    operationRequest.CopyBatchRequestProperties(request);
                    requests.Add(new OperationRequestItem(operationRequest));
                }
            }

            return requests;
        }

        /// <inheritdoc />
        public async override Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(
                   IEnumerable<ODataBatchRequestItem> requests,
                   CancellationToken cancellation)
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
                    var operation = request as OperationRequestItem;
                    if (operation != null)
                    {
                        ODataBatchResponseItem response;
                        if (isSyncMode == true)
                        {
                            response = request.SendRequestAsync(Invoker, cancellation).Result;
                        }
                        else
                        {
                            response = await request.SendRequestAsync(Invoker, cancellation);
                        }

                        responses.Add(response);
                    }
                    else
                    {
                        if (isSyncMode == true)
                        {
                            ExecuteChangeSet((ChangeSetRequestItem)request, responses, cancellation);
                        }
                        else
                        {
                            await ExecuteChangeSet((ChangeSetRequestItem)request, responses, cancellation);
                        }
                    }
                }
            }
            catch
            {
                foreach (ODataBatchResponseItem response in responses)
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }

                throw;
            }

            return responses;
        }

        /// <summary>
        /// Execute changeset processing.
        /// </summary>
        /// <param name="changeSet">Changeset for processing.</param>
        /// <param name="responses">Responses for each request.</param>
        /// <param name="cancellation">Cancelation token.</param>
        /// <returns>Task for changeset processing.</returns>
        private async Task ExecuteChangeSet(ChangeSetRequestItem changeSet, IList<ODataBatchResponseItem> responses, CancellationToken cancellation)
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

            ChangeSetResponseItem changeSetResponse;
            if (isSyncMode == true)
            {
                changeSetResponse = (ChangeSetResponseItem)changeSet.SendRequestAsync(Invoker, cancellation).Result;
            }
            else
            {
                changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(Invoker, cancellation);
            }

            responses.Add(changeSetResponse);

            if (changeSetResponse.Responses.All(r => r.IsSuccessStatusCode))
            {
                DataObject[] dataObjects = dataObjectsToUpdate.ToArray();
                dataService.UpdateObjects(ref dataObjects);
            }
        }
    }
}
