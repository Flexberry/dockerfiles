namespace NewPlatform.Flexberry.ORM.ODataServiceCore.Common.Exceptions
{
    using System;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// The wrapper for an overridden unhandled exception.
    /// </summary>
    public class CustomException : Exception
    {
        /// <summary>
        /// The http status code to return.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomException"/> class.
        /// </summary>
        /// <param name="innerException">The exception to return.</param>
        /// <param name="statusCode">The http status code to return.</param>
        public CustomException(Exception innerException, int statusCode = StatusCodes.Status500InternalServerError)
            : base(null, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
