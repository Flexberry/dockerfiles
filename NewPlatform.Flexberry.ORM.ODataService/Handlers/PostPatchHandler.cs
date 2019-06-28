using System.Linq;

namespace NewPlatform.Flexberry.ORM.ODataService.Handlers
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Определяет класс обработчика http-запроса (http request handler), который в случае, если данный запрос является
    /// POST или PATCH сохраняет тело запроса в свойствах данного запроса. В дальнейшем тело запроса будет использовано в методе
    /// DataObjectController.ReplaceOdataBindNull().
    /// </summary>
    public class PostPatchHandler : DelegatingHandler
    {
        /// <summary>
        /// Строковая константа, которая используется для доступа свойствам запроса.
        /// </summary>
        public const string RequestContent = "PostPatchHandler_RequestContent";

        /// <summary>
        /// Ключ к объекту в свойствах запроса, указывающий на то, что это часть batch-запроса.
        /// </summary>
        public const string PropertyKeyBatchRequest = "MS_BatchRequest";

        /// <summary>
        /// Ключ к объекту в свойствах запроса, указывающий на Id контента batch-запроса.
        /// </summary>
        public const string PropertyKeyContentId = "ContentId";

        /// <summary>
        /// Строковая константа, которая используется для доступа к оригинальному заголовку запроса Accept.
        /// </summary>
        public const string AcceptApplicationMsExcel = "PostPatchHandler_AcceptApplicationMsExcel";

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var val in request.Headers.Accept)
            {
                if (val.MediaType == "application/ms-excel")
                {
                    request.Properties.Add(AcceptApplicationMsExcel, true);
                    break;
                }
            }

            request.Headers.Accept.Clear();
            request.Headers.Remove("X-Requested-With");

            string key = RequestContent;
            if (request.Properties.ContainsKey(PropertyKeyBatchRequest) && (bool)request.Properties[PropertyKeyBatchRequest] == true)
            {
                key = RequestContent + $"_{PropertyKeyContentId}_{request.Properties[PropertyKeyContentId]}";
            }

            if (request.Method.Method == "POST" || request.Method.Method == "PATCH")
            {
                request.Properties.Add(key, request.Content.ReadAsStringAsync().Result);
            }

            /*
            /// Исправление для Mono, взято из https://github.com/OData/odata.net/issues/165
            */
            if (!request.Headers.Contains("Accept-Charset"))
                request.Headers.Add("Accept-Charset", new[] { "utf-8" });

            return base.SendAsync(request, cancellationToken);
        }
    }
}
