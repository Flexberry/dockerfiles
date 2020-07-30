namespace NewPlatform.Flexberry.ORM.ODataServiceCore.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Определяет компонент middleware, встраиваемый в конвейер обработки http-запросов для модификации заголовков и разделяемых данных запроса.
    /// Если запрос является POST или PATCH, в разделяемых данных такого запроса также сохраняется тело запроса, в дальнейшем используемое
    /// в методе DataObjectController.ReplaceOdataBindNull().
    /// </summary>
    public class RequestHeadersHookMiddleware
    {
        /// <summary>
        /// Строковая константа, которая используется для доступа к телу запроса в разделяемых данных запроса.
        /// </summary>
        public const string PropertyKeyRequestContent = "PostPatchHandler_RequestContent";

        /// <summary>
        /// Строковая константа, которая используется для доступа к оригинальному заголовку запроса Accept в разделяемых данных запроса.
        /// </summary>
        public const string AcceptApplicationMsExcel = "PostPatchHandler_AcceptApplicationMsExcel";

        private readonly RequestDelegate _next;

        /// <summary>
        /// Instantiates a new instance of <see cref="RequestHeadersHookMiddleware"/>.
        /// </summary>
        public RequestHeadersHookMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke the middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            
            IEnumerable<string> mimeTypeDirectives = AcceptHeaderParser.MimeTypeDirectivesFromAcceptHeader(request.Headers[HeaderNames.Accept]);
            if (mimeTypeDirectives.Any(x => x.Equals("application/ms-excel", StringComparison.OrdinalIgnoreCase)))
            {
                context.Items.Add(AcceptApplicationMsExcel, true);
            }

            request.Headers[HeaderNames.Accept] = string.Empty; // Clear Accept header.
            request.Headers.Remove("X-Requested-With");

            if (request.Method == "POST" || request.Method == "PATCH")
            {
                request.EnableBuffering();

                // Leave the body open so the next middleware can read it.
                string bodyString;
                using (var reader = new System.IO.StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8, 
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true))
                {
                    bodyString = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                }

                context.Items.Add(PropertyKeyRequestContent, bodyString);
            }

            /*
            /// Исправление для Mono, взято из https://github.com/OData/odata.net/issues/165
            */
            if (!request.Headers.ContainsKey("Accept-Charset"))
                request.Headers.Add("Accept-Charset", new[] { "utf-8" });

            await _next.Invoke(context);
        }
    }
}
