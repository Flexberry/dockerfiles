namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers
{
    using System;
    using System.IO;
    using ICSSoft.STORMNET;
    using Microsoft.AspNetCore.Mvc;
    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.Files.Helpers;
    using NewPlatform.Flexberry.ORM.ODataService.Files.Providers;

    using WebFile = ICSSoft.STORMNET.UserDataTypes.WebFile;

    /// <summary>
    /// WebApi-контроллер, предназначенный для загрузки файлов на сервер и скачивания ранее загруженных файлов.
    /// </summary>
    public partial class FileController
    {
        /// <summary>
        /// Осуществляет скачивание файлов с сервера.
        /// В зависимости от значения флага <paramref name="getPreview"/> возвращается либо содержимое файла, либо файл в виде приложения.
        /// </summary>
        /// <param name="fileDescription">Описание запрашиваемого файла.</param>
        /// <param name="getPreview">Параметр, определяющий, требуется ли файл просто для предпросмотра (если значение <c>true</c>), либо требуется его скачать и сохранить.</param>
        /// <returns>Описание загруженного файла.</returns>
        [HttpGet]
        [ActionName("FileAction")]
        public IActionResult Download([FromQuery] FileDescription fileDescription = null, [FromQuery] bool getPreview = false)
        {
            IActionResult result;
            try
            {
                Stream fileStream = GetFileStream(fileDescription, out string fileName, out string fileMimeType, out long fileSize);

                if (getPreview)
                {
                    result = GetFilePreviewResponse(fileStream, fileMimeType);
                }
                else
                {
                    result = File(fileStream, fileMimeType, fileName);
                }
            }
            catch (FileNotFoundException)
            {
                string nodeName = fileDescription.FileUploadKey;
                string fileName = fileDescription.FileName.Replace("\"", string.Empty);

                var message = string.Format(
                    "Узел {0} файлового хранилища не содержит файл {1} или отсутствует.",
                    string.IsNullOrWhiteSpace(nodeName) ? string.Empty : $"\"{nodeName}\"",
                    string.IsNullOrWhiteSpace(fileName) ? string.Empty : $"\"{fileName}\"",
                    fileDescription.FileName);

                result = NotFound(message);
            }

            return result;
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

            Type filePropertyType;
            if (!string.IsNullOrEmpty(fileDescription.EntityPrimaryKey))
            {
                // Запрашиваемый файл уже был связан с объектом данных, и нужно вычитать из него файловое свойство.
                Type dataObjectType = Type.GetType(fileDescription.EntityTypeName, true);
                filePropertyType = Information.GetPropertyType(dataObjectType, fileDescription.EntityPropertyName);
            }
            else
            {
                // Запрашиваемый файл еще не был связан с объектом даных, а значит находится в каталоге загрузок,
                // в подкаталоге с именем fileDescription.FileUplodKey.
                // Получение файлов по ключу загрузки реализовано в DataObjectWebFileProvider.
                filePropertyType = typeof(WebFile);
            }

            if (!dataObjectFileAccessor.HasDataObjectFileProvider(filePropertyType))
            {
                throw new Exception(string.Format("DataObjectFileProvider for \"{0}\" property type not found.", filePropertyType.AssemblyQualifiedName));
            }

            IDataObjectFileProvider dataObjectFileProvider = dataObjectFileAccessor.GetDataObjectFileProvider(filePropertyType);
            object fileProperty = dataObjectFileProvider.GetFileProperty(dataService, fileDescription);

            Stream fileStream = dataObjectFileProvider.GetFileStream(fileProperty);
            fileName = dataObjectFileProvider.GetFileName(fileProperty);
            fileMimeType = dataObjectFileProvider.GetFileMimeType(fileProperty);
            fileSize = fileStream.Length;

            return fileStream;
        }

        /// <summary>
        /// Осуществляет получение ответа севера на запрос о получении preview-изображения файла.
        /// </summary>
        /// <param name="fileStream">Поток данных файла.</param>
        /// <param name="fileMimeType">MIME-тип файла.</param>
        /// <returns>Запрашиваемый файл в виде Base64String.</returns>
        private ContentResult GetFilePreviewResponse(Stream fileStream, string fileMimeType)
        {
            // Преобразуем поток данных файла в строку base64.
            using (fileStream)
            {
                string base64FileData = fileMimeType.ToLower().StartsWith("image")
                           ? Base64Helper.GetBase64StringFileData(fileMimeType, fileStream)
                           : string.Empty;

                return Content(base64FileData);
            }
        }
    }
}
