namespace NewPlatform.Flexberry.ORM.ODataService.Files
{
    using System.Net.Http;
    using System.Net.Http.Headers;

    /// <summary>
    /// Провайдер, для асинхронной загрузки файла в файловую систему.
    /// </summary>
    public class FileUploadStreamProvider : MultipartFormDataStreamProvider
    {
        /// <summary>
        /// Инициализирует провайдер для асинхронной загрузки файла в файловую систему.
        /// </summary>
        /// <param name="path">
        /// Путь, по которому будет осуществляться загрузка файла в файловую систему (включает имя файла).
        /// </param>
        public FileUploadStreamProvider(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Получает имя загружаемого файла.
        /// </summary>
        /// <param name="contentHeaders">
        /// Заголовки загружаемого контента.
        /// </param>
        /// <returns>Имя загружаемого файла.</returns>
        public override string GetLocalFileName(HttpContentHeaders contentHeaders)
        {
            return string.IsNullOrWhiteSpace(contentHeaders.ContentDisposition.FileName)
                ? "NoName"
                : contentHeaders.ContentDisposition.FileName.Replace("\"", string.Empty);
        }
    }
}