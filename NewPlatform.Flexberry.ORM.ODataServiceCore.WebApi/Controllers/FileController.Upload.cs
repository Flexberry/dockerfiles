namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ICSSoft.STORMNET.UserDataTypes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using NewPlatform.Flexberry.ORM.ODataService.Files;

    /// <summary>
    /// WebApi-контроллер, предназначенный для загрузки файлов на сервер и скачивания ранее загруженных файлов.
    /// Часть, реализующая загрузку файлов на сервер.
    /// </summary>
    public partial class FileController
    {
        /// <summary>
        /// Осуществляет загрузку файлов на сервер.
        /// </summary>
        /// <remarks>
        /// Файлы загружаются в файловую систему, в подкаталог {UploadedFileKey} каталога файлового хранилища,
        /// где UploadedFileKey - <see cref="Guid"/>, идентифицирующий загруженный файл.
        /// </remarks>
        /// <returns>Описание загруженного файла.</returns>
        [HttpPost]
        [ActionName("FileAction")]
        public async Task<ObjectResult> Upload()
        {
            if (!Request.HasFormContentType)
            {
                var result = new ObjectResult("The request content is not MIME multipart content.");
                result.StatusCode = StatusCodes.Status406NotAcceptable;

                return result;
            }
 
            IFormFile formFile;
            try
            {
                if (Request.Form.Files == null)
                {
                    throw new InvalidOperationException();
                }

                formFile = Request.Form.Files.Single();
            }
            catch (InvalidOperationException)
            {
                var result = new ObjectResult("An exactly one file must be attached to the request content.");
                result.StatusCode = StatusCodes.Status406NotAcceptable;

                return result;
            }

            // Генерируем ключ, для идентификации загружаемого файла.
            string fileUploadKey = Guid.NewGuid().ToString("D");

            try
            {
                FileDescription fileDescription = await UploadFile(formFile, fileUploadKey);

                return Ok(fileDescription);
            }
            catch (Exception)
            {
                // Удаляем созданный каталог вместе с файлом, если при загрузке произошел какой-либо сбой.
                dataObjectFileAccessor.RemoveFileUploadDirectory(fileUploadKey);

                throw;
            }
        }

        /// <summary>
        /// Осуществляет загрузку файла на сервер в подкаталог с заданным именем внутри каталога файлового хранилища.
        /// </summary>
        /// <param name="formFile">Файл, переданный в http-запросе.</param>
        /// <param name="fileUploadKey">Ключ загрузки файла (используется как имя подкаталога).</param>
        /// <returns>
        /// Асинхронная операция, которая в случае успешного выполнения вернет метаданные загруженного файла (<see cref="FileDescription"/>).
        /// </returns>
        private async Task<FileDescription> UploadFile(IFormFile formFile, string fileUploadKey)
        {
            // Создаём каталог для загружаемого файла, и провайдер для его вычитки и сохранения.
            string fileUploadPath = dataObjectFileAccessor.CreateFileUploadDirectory(fileUploadKey);
            string fileName = string.IsNullOrWhiteSpace(formFile.FileName) ? "NoName" : formFile.FileName.Replace("\"", string.Empty);
            string localFileName = Path.Combine(fileUploadPath, fileName);

            using (FileStream fs = System.IO.File.Create(localFileName))
            {
                return await formFile
                    .CopyToAsync(fs)
                    .ContinueWith(previousTask => {
                        // Обрабатываем сбой/отмену при обработке файла.
                        if (previousTask.IsFaulted || previousTask.IsCanceled)
                        {
                            Exception exception = previousTask.Exception ?? new Exception("The task to save a file attached to the request content was faulted or cancelled.");

                            throw exception;
                        }

                        // Для корректного вычисления размера файла файловый поток должен быть закрыт до создания описания загруженного файла.
                        fs.Close();

                        // Возвращаем описание загруженного файла.
                        return new FileDescription(dataObjectFileAccessor.GetDataObjectFileProvider(typeof(WebFile)).FileBaseUrl, localFileName);
                    })
                    .ContinueWith(previousTask =>
                    {
                        // Если загрузка файла прошла успешно, нужно удалить ранее загруженнй файл,
                        // который еще не ассоциированн с объектом данных.
                        if (!(previousTask.IsFaulted || previousTask.IsCanceled))
                        {
                            Request.Form.TryGetValue("previousFileDescription", out StringValues previousFileDescriptionValue);

                            FileDescription previousFileDescription = FileDescription.FromJson(previousFileDescriptionValue.ToString());
                            if (!string.IsNullOrEmpty(previousFileDescription?.FileUploadKey))
                            {
                                dataObjectFileAccessor.RemoveFileUploadDirectory(previousFileDescription.FileUploadKey);
                            }
                        }

                        return previousTask.Result;
                    });
            }
        }
    }
}
