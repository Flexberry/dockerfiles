namespace NewPlatform.Flexberry.ORM.ODataServiceCore.Common.Exceptions
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// The <see cref="IExceptionFilter"/> implementation for unhandled exceptions.
    /// </summary>
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly bool exceptionDataAllowed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomExceptionFilter"/> class.
        /// </summary>
        /// <param name="hostingEnvironment">The current hosting environment.</param>
        public CustomExceptionFilter(IHostingEnvironment hostingEnvironment)
        {
            exceptionDataAllowed = hostingEnvironment.IsDevelopment();
        }

        /// <inheritdoc/>
        public void OnException(ExceptionContext context)
        {
            Exception exception = context.Exception;
            int statusCode = StatusCodes.Status500InternalServerError;

            if (exception is CustomException customException)
            {
                statusCode = customException.StatusCode;
                exception = customException.InnerException;
            }

            object error = GetError(statusCode, exception);

            var result = new ObjectResult(error);
            result.StatusCode = statusCode;

            context.Result = result;
        }

        private void FillExceptionData(int statusCode, Exception exception, List<object> details, List<object> trace)
        {
            trace.Add(new { message = exception.Message, stack = exception.StackTrace });

            if (exception.InnerException != null)
            {
                details.Add(new { code = statusCode.ToString(), message = exception.InnerException.Message });

                FillExceptionData(statusCode, exception.InnerException, details, trace);
            }
        }

        /// <summary>
        /// Creates an <see cref="object"/> instance with error data.
        /// </summary>
        /// <param name="statusCode">The http status code.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>An <see cref="object"/> instance with error data.</returns>
        private object GetError(int statusCode, Exception exception)
        {
            if (!exceptionDataAllowed)
            {
                return new { Error = new { code = statusCode.ToString(), message = "An error has occured." } };
            }

            var details = new List<object>();
            var trace = new List<object>();
            FillExceptionData(statusCode, exception, details, trace);

            return new
            {
                Error = new
                {
                    code = statusCode.ToString(),
                    message = exception.Message,
                    details,
                    innererror = new
                    {
                        trace
                    }
                }
            };
        }
    }
}
