namespace NewPlatform.Flexberry.ORM.ODataServiceCore.Batch
{
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A wrapper for the batch requests collection, which provides access to the batch http context.
    /// </summary>
    internal class ODataBatchRequestsWrapper : IEnumerable<ODataBatchRequestItem>
    {
        private readonly IEnumerable<ODataBatchRequestItem> requests;

        /// <summary>
        /// The batch http context.
        /// </summary>
        public HttpContext BatchContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchRequestsWrapper"/> class.
        /// </summary>
        /// <param name="batchContext">The batch http context.</param>
        /// <param name="requests">The batch requests collection.</param>
        public ODataBatchRequestsWrapper(HttpContext batchContext, IEnumerable<ODataBatchRequestItem> requests)
        {
            BatchContext = batchContext;          
            this.requests = requests;
        }

        ///<inherirdoc/>
        public IEnumerator<ODataBatchRequestItem> GetEnumerator()
        {
            return requests.GetEnumerator();
        }

        ///<inherirdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return requests.GetEnumerator();
        }
    }
}
