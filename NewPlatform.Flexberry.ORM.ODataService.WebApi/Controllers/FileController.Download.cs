namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web;
    using System.Web.Http;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.Files.Providers;

    using File = ICSSoft.STORMNET.FileType.File;
    using WebFile = ICSSoft.STORMNET.UserDataTypes.WebFile;

    /// <summary>
    /// WebApi-контроллер, предназначенный для загрузки файлов на сервер и скачивания ранее загруженных файлов.
    /// </summary>
    public partial class FileController
    {
        /// <summary>
        /// Осуществляет получение данных файла в виде Base64String.
        /// </summary>
        /// <param name="contentType">MIME-тип данных.</param>
        /// <param name="stream">Поток байтов файла.</param>
        /// <returns>Данные файла в виде  Base64String.</returns>
        public static string GetBase64StringFileData(string contentType, Stream stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            return string.Format("data:{0};base64,{1}", contentType, Convert.ToBase64String(buffer));
        }

        /// <summary>
        /// Осуществляет получение данных файла в виде Base64String.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Данные файла в виде  Base64String.</returns>
        public static string GetBase64StringFileData(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return string.Empty;
            }

            string result;

            FileInfo fileInfo = new FileInfo(filePath);
            using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read))
            {
                result = GetBase64StringFileData(MimeMapping.GetMimeMapping(fileInfo.Name), fileStream);
            }

            return result;
        }

        /// <summary>
        /// Осуществляет скачивание файлов с сервера.
        /// В зависимости от значения флага <paramref name="getPreview"/> возвращается либо содержимое файла, либо файл в виде приложения.
        /// </summary>
        /// <param name="fileDescription">Описание запрашиваемого файла.</param>
        /// <param name="getPreview">Параметр, определяющий, требуется ли файл просто для предпросмотра (если значение <c>true</c>), либо требуется его скачать и сохранить.</param>
        /// <returns>Описание загруженного файла.</returns>
        [HttpGet]
        public HttpResponseMessage Get([FromUri] FileDescription fileDescription = null, [FromUri] bool getPreview = false)
        {
            HttpResponseMessage response = null;

            Stream fileStream = null;
            string fileName = null;
            string fileMimeType = null;
            long fileSize = 0;

            try
            {
                fileStream = GetFileStream(fileDescription, out fileName, out fileMimeType, out fileSize);

                if (getPreview)
                {
                    response = GetFilePreviewResponse(fileStream, fileName, fileMimeType, fileSize);
                    fileStream.Close();
                }
                else
                {
                    response = GetFileAttachmentResponse(fileStream, fileName, fileMimeType, fileSize);
                }
            }
            catch (Exception ex)
            {
                fileStream?.Close();

                response = Request.CreateResponse(ex is FileNotFoundException ? HttpStatusCode.NotFound : HttpStatusCode.InternalServerError, ex.Message);
                throw new HttpResponseException(response);
            }

            return response;
        }

        /// <summary>
        /// Осуществляет получение потока данных для запрашиваемого файла (а также имя файла, MIME-тип, и размер в байтах).
        /// </summary>
        /// <param name="fileDescription">Описание файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="fileMimeType">MIME-тип файла.</param>
        /// <param name="fileSize">Размер файла в байтах.</param>
        /// <returns>Поток данных для запрашиваемого файла.</returns>
        private Stream GetFileStream(
            FileDescription fileDescription,
            out string fileName,
            out string fileMimeType,
            out long fileSize)
        {
            if (fileDescription == null)
            {
                throw new ArgumentNullException(nameof(fileDescription));
            }

            Stream fileStream = null;
            Type dataObjectType = null;
            Type filePropertyType = null;

            if (!string.IsNullOrEmpty(fileDescription.EntityPrimaryKey))
            {
                // Запрашиваемый файл уже был связан с объектом данных, и нужно вычитать из него файловое свойство.
                dataObjectType = Type.GetType(fileDescription.EntityTypeName, true);
                filePropertyType = Information.GetPropertyType(dataObjectType, fileDescription.EntityPropertyName);
            }
            else
            {
                // Запрашиваемый файл еще не был связан с объектом даных, а значит находится в каталоге загрузок,
                // в подкаталоге с именем fileDescription.FileUplodKey.
                // Получение файлов по ключу загрузки реализовано в DataObjectWebFileProvider.
                filePropertyType = typeof(WebFile);
            }

            if (!HasDataObjectFileProvider(filePropertyType))
            {
                throw new Exception(string.Format("DataObjectFileProvider for \"{0}\" property type not found.", filePropertyType.AssemblyQualifiedName));
            }

            IDataObjectFileProvider dataObjectFileProvider = GetDataObjectFileProvider(filePropertyType);
            object fileProperty = dataObjectFileProvider.GetFileProperty(fileDescription);

            fileStream = dataObjectFileProvider.GetFileStream(fileProperty);
            fileName = dataObjectFileProvider.GetFileName(fileProperty);
            fileMimeType = dataObjectFileProvider.GetFileMimeType(fileProperty);
            fileSize = fileStream.Length;

            return fileStream;
        }

        /// <summary>
        /// Осуществляет получение ответа севера на запрос о получении preview-изображения файла.
        /// </summary>
        /// <param name="fileStream">Поток данных файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="fileMimeType">MIME-тип файла.</param>
        /// <param name="fileSize">Размер файла в байтах.</param>
        /// <returns>Поток данных для запрашиваемого файла.</returns>
        private HttpResponseMessage GetFilePreviewResponse(Stream fileStream, string fileName, string fileMimeType, long fileSize)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            // Преобразуем поток данных файла в строку base64.
            string base64FileData = fileMimeType.ToLower().StartsWith("image")
                       ? GetBase64StringFileData(fileMimeType, fileStream)
                       : string.Empty;

            response.Content = new StringContent(base64FileData);
            response.Content.Headers.ContentLength = base64FileData.Length;

            return response;
        }

        /// <summary>
        /// Осуществляет получение ответа севера на запрос о получении файла.
        /// </summary>
        /// <param name="fileStream">Поток данных файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="fileMimeType">MIME-тип файла.</param>
        /// <param name="fileSize">Размер файла в байтах.</param>
        /// <returns>Поток данных для запрашиваемого файла.</returns>
        private HttpResponseMessage GetFileAttachmentResponse(Stream fileStream, string fileName, string fileMimeType, long fileSize)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StreamContent(fileStream);
            response.Content.Headers.ContentLength = fileStream.Length;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(fileMimeType);

            // C именами файлов, которые заданы не в латинице могут быть проблемы при скачивании: http://stackoverflow.com/questions/18050718/utf-8-encoding-name-in-downloaded-file,
            // поэтому Content-Disposition следует задавать следующим образом.
            string contentDisposition = string.Format("attachment; filename=\"{0}\"; filename*=UTF-8''{1}; size={2}", fileName, Uri.EscapeDataString(fileName), fileSize);
            response.Content.Headers.Add("Content-Disposition", contentDisposition);

            return response;
        }
    }
}