namespace NewPlatform.Flexberry.ORM.ODataService.Files
{
#if NETFRAMEWORK
    using System.Web;
#elif NETSTANDARD
    using Microsoft.AspNetCore.StaticFiles;
#endif

    /// <summary>
    /// MIME type utilities.
    /// </summary>
    public static class MimeTypeUtils
    {
#if NETFRAMEWORK
        /// <summary>
        /// Given a file path, determine the MIME type.
        /// </summary>
        /// <param name="subpath">A file path.</param>
        /// <returns>The MIME type.</returns>
        public static string GetFileMimeType(string subpath)
        {
            return MimeMapping.GetMimeMapping(subpath);
        }
#elif NETSTANDARD
        private static readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

        /// <summary>
        /// Given a file path, determine the MIME type.
        /// </summary>
        /// <param name="subpath">A file path.</param>
        /// <returns>The MIME type.</returns>
        public static string GetFileMimeType(string subpath)
        {
            fileExtensionContentTypeProvider.TryGetContentType(subpath, out string contentType);

            return contentType ?? "application/octet-stream";
        }
#endif
    }
}
