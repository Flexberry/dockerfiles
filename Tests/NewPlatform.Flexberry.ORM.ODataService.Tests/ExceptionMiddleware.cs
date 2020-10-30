#if NETCOREAPP
namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System;
    using System.Net;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Middleware for exception handling in tests.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initialize new instance of ExceptionMiddleware.
        /// </summary>
        /// <param name="next">Request delegate.</param>
        public ExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invoke pipe.
        /// </summary>
        /// <param name="httpContext">Http context for current pipe.</param>
        /// <returns>Task for execute.</returns>
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await this.next(httpContext);
            }
            catch (Exception ex)
            {
                httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpContext.Response.WriteAsync("Internal server error");
            }
        }
    }
}
#endif
