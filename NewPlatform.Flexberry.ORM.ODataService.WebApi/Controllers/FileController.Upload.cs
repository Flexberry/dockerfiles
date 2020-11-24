namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using NewPlatform.Flexberry.ORM.ODataService.Files;

    /// <summary>
    /// WebApi-контроллер, предназначенный для загрузки файлов на сервер и скачивания ранее загруженных файлов.
    /// </summary>
    public partial class FileController
    {
        /// <summary>
        /// Осуществляет загрузку файлов на сервер.
        /// </summary>
        /// <remarks>
        /// Файлы загружаются в файловую систему, в каталог <see cref="UploadsDirectoryPath"/>/{UploadedFileKey},
        /// где UploadedFileGuid - <see cref="Guid"/>, идентифицирующий загруженный файл.
        /// </remarks>
        /// <returns>
        /// Описание загруженного файла.
        /// </returns>
        [HttpPost]
        public Task<FileDescription> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable, "Request content is not MIME multipart content."));
            }

            // Генерируем ключ, для идентификации загружаемого файла.
            string fileUploadKey = Guid.NewGuid().ToString("D");

            try
            {
                return UploadFile(fileUploadKey);
            }
            catch (Exception)
            {
                // Удаляем созданный каталог вместе с файлом, если при загрузке произошел какой-либо сбой.
                dataObjectFileAccessor.RemoveFileUploadDirectory(fileUploadKey);

                throw;
            }
        }

        /// <summary>
        /// Осуществляет загрузку файла на сервер в подкаталог с заданным именем внутри каталога <see cref="UploadsDirectoryPath"/>.
        /// </summary>
        /// <param name="fileUploadKey">
        /// Ключ загрузки файла (используется как имя подкаталога).
        /// </param>
        /// <returns>
        /// Асинхронная операция, которая в случае успешного выполнения вернет метаданные загруженного файла (<see cref="FileDescription"/>).
        /// </returns>
        private Task<FileDescription> UploadFile(string fileUploadKey)
        {
            // Создаём каталог для загружаемого файла, и провайдер для его вычитки и сохранения.
            string fileUploadPath = dataObjectFileAccessor.CreateFileUploadDirectory(fileUploadKey);
            FileUploadStreamProvider fileUploadProvider = new FileUploadStreamProvider(fileUploadPath);

            // Вычитываем загружаемый файл из запроса, и сохраняем в созданном каталоге.
            Task<FileDescription> uploadTask = Request.Content.ReadAsMultipartAsync(fileUploadProvider).ContinueWith((readRequestTask) =>
            {
                // Обрабатываем сбой/отмену при обработке файла.
                if (readRequestTask.IsFaulted || readRequestTask.IsCanceled)
                {
                    Exception exception = readRequestTask.Exception ?? new Exception("Read MIME multipart content task was faulted or cancelled.");

                    throw exception;
                }

                // Возвращаем описание загруженного файла.
                return new FileDescription(dataObjectFileAccessor.BaseUri.AbsoluteUri, fileUploadProvider.FileData.First().LocalFileName);
            }).ContinueWith((fileDescriptionTask) =>
            {
                // Если загрузка файла прошла успешно, нужно удалить ранее загруженнй файл,
                // который еще не ассоциированн с объектом данных.
                if (!(fileDescriptionTask.IsFaulted || fileDescriptionTask.IsCanceled))
                {
                    FileDescription previousFileDescription = FileDescription.FromJson(fileUploadProvider.FormData.Get("previousFileDescription"));
                    if (!string.IsNullOrEmpty(previousFileDescription?.FileUploadKey))
                    {
                        dataObjectFileAccessor.RemoveFileUploadDirectory(previousFileDescription.FileUploadKey);
                    }
                }

                return fileDescriptionTask.Result;
            });

            return uploadTask;
        }
    }
}
