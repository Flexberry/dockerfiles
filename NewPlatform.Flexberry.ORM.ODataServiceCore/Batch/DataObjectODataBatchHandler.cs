namespace NewPlatform.Flexberry.ORM.ODataService.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ICSSoft.Services;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Microsoft.AspNet.OData.Adapters;
    using Microsoft.AspNet.OData.Common;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.AspNet.OData;
    using NewPlatform.Flexberry.ORM.ODataServiceCore.Batch;

    /// <summary>
    /// The batch handler for <see cref="IDataService"/>.
    /// </summary>
    internal class DataObjectODataBatchHandler : DefaultODataBatchHandler
    {
        /// <summary>
        /// Context Items collection key for DataObjectsToUpdate list.
        /// </summary>
        public const string DataObjectsToUpdatePropertyKey = "DataObjectsToUpdate";

        /// <summary>
        /// Context Items collection key for DataObjectCache instance.
        /// </summary>
        public const string DataObjectCachePropertyKey = "DataObjectCache";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectODataBatchHandler"/> class.
        /// </summary>
        public DataObjectODataBatchHandler()
            : base()
        {
        }

        /// <inheritdoc />
        public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
        {
            if (requests == null)
            {
                throw Error.ArgumentNull("requests");
            }
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
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

        /// <inheritdoc/>
        public override async Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }
            if (nextHandler == null)
            {
                throw Error.ArgumentNull("nextHandler");
            }

            if (!await ValidateRequest(context.Request))
            {
                return;
            }

            IList<ODataBatchRequestItem> subRequests = await ParseBatchRequestsAsync(context);

            ODataOptions options = context.RequestServices.GetRequiredService<ODataOptions>();
            bool enableContinueOnErrorHeader = (options != null)
                ? options.EnableContinueOnErrorHeader
                : false;

            SetContinueOnError(new WebApiRequestHeaders(context.Request.Headers), enableContinueOnErrorHeader);

            IList<ODataBatchResponseItem> responses = await ExecuteRequestMessagesAsync(new ODataBatchRequestsWrapper(context, subRequests), nextHandler);
            await CreateResponseMessageAsync(responses, context.Request);
        }

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
                    throw ex;
                }
            }

            return changeSetResponse;
        }
    }
}
