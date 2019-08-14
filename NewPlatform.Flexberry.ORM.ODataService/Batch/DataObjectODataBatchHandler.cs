namespace NewPlatform.Flexberry.ORM.ODataService.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
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

        private bool isMono5Runtime = true;

        SynchronizationContext synchronizationContext;

        /// <summary>
        /// Initializes a new instance of the NewPlatform.Flexberry.ORM.ODataService.Batch.DataObjectODataBatchHandler class.
        /// </summary>
        /// <param name="dataService">DataService instance for execute queries.</param>
        /// <param name="httpServer">The System.Web.Http.HttpServer for handling the individual batch requests.</param>
        public DataObjectODataBatchHandler(IDataService dataService, HttpServer httpServer)
            : base(httpServer)
        {
            //if (isMonoRuntime && SynchronizationContext.Current == null)
            //{
            //    SynchronizationContext synchronizationContext = new SynchronizationContext();
            //    SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            //}

            string str = "constructor-> " + "|" + SynchronizationContext.Current + "|" + TaskScheduler.Current + "|" + Thread.CurrentThread.ManagedThreadId + "|" + HttpContext.Current;
            LogService.LogError(str);

            this.dataService = dataService;

            // Mono 5 has problems with async-await calls and correct save HttpContext.Current instance throught tasks threads. This hack need to disable multithreading in batch requests for mono 5.*.
            isMono5Runtime = IsMono5Runtime();

            if (isMono5Runtime)
            {
                synchronizationContext = new SynchronizationContext();
            }
        }

        private bool IsMono5Runtime()
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
                        LogService.LogError(monoVersion); // TODO: remove it.
                        return true;
                    }
                }
            }

            return false;
        }

        public override async Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string newsc = "";
            if (isMono5Runtime && SynchronizationContext.Current == null)
            {
                synchronizationContext = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                newsc = "*";
            }

            string str = "ProcessBatchAsync->" + SynchronizationContext.Current + newsc + "|" + TaskScheduler.Current + "|" + Thread.CurrentThread.ManagedThreadId + "|" + HttpContext.Current;
            LogService.LogError(str);

            if (HttpContext.Current == null)
            {
                throw new Exception("Empty HttpContext in DataObjectODataBatchHandler ProcessBatchAsync");
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateRequest(request);

            IList<ODataBatchRequestItem> subRequests;

            if (isMono5Runtime)
            {
                subRequests = ParseBatchRequestsAsync(request, cancellationToken).Result;
            }
            else
            {
                subRequests = await ParseBatchRequestsAsync(request, cancellationToken);
            }

            if (isMono5Runtime && SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                newsc = "*";
            }

            try
            {
                if (isMono5Runtime)
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

        public override async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string newsc = "";
            if (isMono5Runtime && SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                newsc = "*";
            }

            string str = "ParseBatchRequestsAsync->" + SynchronizationContext.Current + newsc + "|" + TaskScheduler.Current + "|" + Thread.CurrentThread.ManagedThreadId + "|" + HttpContext.Current;
            LogService.LogError(str);

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

            if (isMono5Runtime)
            {
                reader = request.Content.GetODataMessageReaderAsync(oDataReaderSettings, cancellationToken).Result;
            }
            else
            {
                reader = await request.Content.GetODataMessageReaderAsync(oDataReaderSettings, cancellationToken);
            }

            if (isMono5Runtime && SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                newsc = "*";
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

                    if (isMono5Runtime)
                    {
                        changeSetRequests = batchReader.ReadChangeSetRequestAsync(batchId, cancellationToken).Result;

                        if (isMono5Runtime && SynchronizationContext.Current == null)
                        {
                            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                            newsc = "*";
                        }

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

                    if (isMono5Runtime)
                    {
                        operationRequest = batchReader.ReadOperationRequestAsync(batchId, bufferContentStream: true, cancellationToken: cancellationToken).Result;

                        if (isMono5Runtime && SynchronizationContext.Current == null)
                        {
                            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                            newsc = "*";
                        }

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
            string newsc = "";

            if (isMono5Runtime && SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                newsc = "*";
            }

            string str = "ExecuteRequestMessagesAsync->" + SynchronizationContext.Current + newsc + "|" + TaskScheduler.Current + "|" + Thread.CurrentThread.ManagedThreadId + "|" + HttpContext.Current;
            LogService.LogError(str);
            //if (HttpContext.Current == null)
            //{
            //    throw new Exception("Empty HttpContext in DataObjectODataBatchHandler ExecuteRequestMessagesAsync");
            //}

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
                        if (isMono5Runtime)
                        {
                            response = request.SendRequestAsync(Invoker, cancellation).Result;
                            if (isMono5Runtime && SynchronizationContext.Current == null)
                            {
                                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                                newsc = "*";
                            }
                        }
                        else
                        {
                            response = await request.SendRequestAsync(Invoker, cancellation);
                        }

                        responses.Add(response);
                    }
                    else
                    {
                        if (isMono5Runtime)
                        {
                            ExecuteChangeSet((ChangeSetRequestItem)request, responses, cancellation);
                            if (isMono5Runtime && SynchronizationContext.Current == null)
                            {
                                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                                newsc = "*";
                            }

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
            string newsc = "";

            if (isMono5Runtime && SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                newsc = "*";
            }

            string str = "ExecuteChangeSet->" + SynchronizationContext.Current + newsc + "|" + TaskScheduler.Current + "|" + Thread.CurrentThread.ManagedThreadId + "|" + HttpContext.Current;
            LogService.LogError(str);

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
            if (isMono5Runtime)
            {
                changeSetResponse = (ChangeSetResponseItem)changeSet.SendRequestAsync(Invoker, cancellation).Result;
                if (isMono5Runtime && SynchronizationContext.Current == null)
                {
                    SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                    newsc = "*";
                }
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
