namespace NewPlatform.Flexberry.ORM.ODataService.Files
{
    using Microsoft.AspNetCore.StaticFiles;

    /// <summary>
    /// MIME type utilities.
    /// </summary>
    public static class MimeTypeUtils
    {
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
    }
}
