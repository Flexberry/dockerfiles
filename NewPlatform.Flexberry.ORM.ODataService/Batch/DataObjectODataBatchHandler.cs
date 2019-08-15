namespace NewPlatform.Flexberry.ORM.ODataService.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
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
        /// DataService instance for execute queries.
        /// </summary>
        private IDataService dataService;

        /// <summary>
        /// Request Properties collection key for DataObjectsToUpdate list.
        /// </summary>
        public const string DataObjectsToUpdatePropertyKey = "DataObjectsToUpdate";

        /// <summary>
        /// Flag, indicates that runtime is mono 5.*.
        /// </summary>
        private static bool? isMono5Runtime;

        /// <summary>
        /// Static constructor for hack with mono 5.
        /// </summary>
        static DataObjectODataBatchHandler()
        {
            // Mono 5 has problems with async-await calls and correct save HttpContext.Current instance throught tasks threads. This hack need to disable multithreading in batch requests for mono 5.*.
            if (isMono5Runtime == null)
            {
                isMono5Runtime = IsMono5Runtime();
            }
        }

        /// <summary>
        /// Initializes a new instance of the NewPlatform.Flexberry.ORM.ODataService.Batch.DataObjectODataBatchHandler class.
        /// </summary>
        /// <param name="dataService">DataService instance for execute queries.</param>
        /// <param name="httpServer">The System.Web.Http.HttpServer for handling the individual batch requests.</param>
        public DataObjectODataBatchHandler(IDataService dataService, HttpServer httpServer)
            : base(httpServer)
        {
            this.dataService = dataService;
        }

        /// <summary>
        /// Is mono 5.* Runtime.
        /// </summary>
        /// <returns></returns>
        private static bool IsMono5Runtime()
        {
            Type monoRuntimeType = Type.GetType("Mono.Runtime");

            if (monoRuntimeType != null)
            {
                MethodInfo displayName = monoRuntimeType.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);

                if (displayName != null)
                {
                    string monoVersion = displayName.Invoke(null, null).ToString();

                    if (monoVersion.StartsWith("5."))
                    {
                        return true;
                    }
                }
            }

            return false;
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

            if (isMono5Runtime == true)
            {
                subRequests = ParseBatchRequestsAsync(request, cancellationToken).Result;
            }
            else
            {
                subRequests = await ParseBatchRequestsAsync(request, cancellationToken);
            }

            try
            {
                if (isMono5Runtime == true)
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

            if (isMono5Runtime == true)
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

                    if (isMono5Runtime == true)
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

                    if (isMono5Runtime == true)
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
                        if (isMono5Runtime == true)
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
                        if (isMono5Runtime == true)
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

            foreach (HttpRequestMessage request in changeSet.Requests)
            {
                if (!request.Properties.ContainsKey(DataObjectsToUpdatePropertyKey))
                {
                    request.Properties.Add(DataObjectsToUpdatePropertyKey, dataObjectsToUpdate);
                }
            }

            ChangeSetResponseItem changeSetResponse;
            if (isMono5Runtime == true)
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
