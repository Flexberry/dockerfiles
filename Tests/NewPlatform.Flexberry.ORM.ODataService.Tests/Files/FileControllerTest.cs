namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Files
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.Files.Helpers;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers;

    using Unity;

    using Xunit;

    using File = ICSSoft.STORMNET.FileType.File;
    using WebFile = ICSSoft.STORMNET.UserDataTypes.WebFile;

    /// <summary>
    /// Тесты файлового контроллера <see cref="FileController"/>, отвечающего за загрузку файлов на сервер и их скачивание.
    /// </summary>
    public class FileControllerTest : BaseODataServiceIntegratedTest
    {
        private const string FileBaseUrl = "http://localhost/api/File";

        /// <summary>
        /// Путь к каталогу с тестовыми файлами.
        /// </summary>
        private static string _filesDirectoryPath;

        /// <summary>
        /// Путь к каталогу, предназначенному для загружаемых на сервер файлов.
        /// </summary>
        private static string _uploadsDirectoryPath;

        /// <summary>
        /// Путь к каталогу, предназначенному для загружаемых на сервер файлов.
        /// </summary>
        private static string _downloadsDirectoryPath;

        /// <summary>
        /// Путь к тестовому изображению.
        /// </summary>
        private static string _srcImageFilePath;

        /// <summary>
        /// Путь к тестовому текстовому файлу.
        /// </summary>
        private static string _srcTextFilePath;

        /// <summary>
        /// Инициализирует тестовый класс (инициализация выполняется перед запуском тестов).
        /// </summary>
#if NETFRAMEWORK
        public FileControllerTest()
#elif NETCOREAPP
        public FileControllerTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output)
            : base(factory, output)
#endif
        {
            _filesDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");

            _uploadsDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
            if (!Directory.Exists(_uploadsDirectoryPath))
            {
                Directory.CreateDirectory(_uploadsDirectoryPath);
            }

            _downloadsDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Downloads_{Guid.NewGuid().ToString("D")}");
            if (!Directory.Exists(_downloadsDirectoryPath))
            {
                Directory.CreateDirectory(_downloadsDirectoryPath);
            }

            _srcImageFilePath = Path.Combine(_filesDirectoryPath, "delorean.png");

            _srcTextFilePath = Path.Combine(_filesDirectoryPath, "readme.txt");
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Удаляем каталог с загруженными на сервер файлами.
                if (Directory.Exists(_uploadsDirectoryPath))
                {
                    Directory.Delete(_uploadsDirectoryPath, true);
                }

                // Удаляем каталог со скачанными с сервера файлами.
                if (Directory.Exists(_downloadsDirectoryPath))
                {
                    Directory.Delete(_downloadsDirectoryPath, true);
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Осуществляет создание подкаталога с заданным именем в каталоге <see cref="_uploadsDirectoryPath"/>.
        /// </summary>
        /// <param name="subDirectoryName">Имя создаваемого подкаталога.</param>
        /// <returns>Абсолютный путь к созданному подкаталогу.</returns>
        public string CreateUploadsSubDirectory(string subDirectoryName)
        {
            string directoryPath = Path.Combine(_uploadsDirectoryPath, subDirectoryName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        /// <summary>
        /// Осуществляет создание подкаталога с заданным именем в каталоге <see cref="_downloadsDirectoryPath"/>.
        /// </summary>
        /// <param name="subDirectoryName">Имя создаваемого подкаталога.</param>
        /// <returns>Абсолютный путь к созданному подкаталогу.</returns>
        public string CreateDownloadsSubDirectory(string subDirectoryName)
        {
            string directoryPath = Path.Combine(_downloadsDirectoryPath, subDirectoryName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        /// <summary>
        /// Тест проверяет, что все необходимые классы разрешаются через DI.
        /// </summary>
        [Fact]
        public void TestResolve()
        {
            ActODataService(
                args =>
                {
                    // Act.
                    var controller = args.UnityContainer.Resolve<FileController>();

                    // Assert.
                    Assert.NotNull(controller);
                });
        }

        /// <summary>
        /// Осуществляет проверку того, что файлы корректно загружаются на сервер.
        /// </summary>
        [Fact]
        public void TestUploadSuccess()
        {
            ActODataService(
                args =>
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                    {
                        // Arrange.
                        FileInfo srcFileInfo = new FileInfo(srcFilePath);

                        string fileName = srcFileInfo.Name;
                        string fileMimeType = MimeTypeUtils.GetFileMimeType(fileName);
                        long fileSize = srcFileInfo.Length;

                        using var uploadingImageFileContent = new StreamContent(srcFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read));
                        uploadingImageFileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { FileName = srcFileInfo.Name };
                        uploadingImageFileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeUtils.GetFileMimeType(srcFileInfo.Name));
                        var formDataContent = new MultipartFormDataContent { uploadingImageFileContent };

                        // Act.
                        using var response = args.HttpClient.PostAsync(FileBaseUrl, formDataContent).Result;
                        string rawContent = response.Content.ReadAsStringAsync().Result;

                        // Проверяем, что запрос завершился успешно.
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                        // Получаем описание загруженного файла, из ответа, полученного от сервера.
                        FileDescription receivedFileDescription = FileDescription.FromJson(rawContent);
                        string uploadedFilePath = Path.Combine(_uploadsDirectoryPath, receivedFileDescription.FileUploadKey, srcFileInfo.Name);

                        // Проверяем, что загруженный на сервер файл существует, и совпадает с тем файлом, из которого производилась загрузка.
                        Assert.True(System.IO.File.Exists(uploadedFilePath));
                        Assert.True(FilesComparer.FilesAreEqual(srcFilePath, uploadedFilePath));

                        Uri receivedFileUri = new Uri(receivedFileDescription.FileUrl);
                        Uri receivedPreviewUri = new Uri(receivedFileDescription.PreviewUrl);

                        string receivedFileBaseUrl = receivedFileUri.GetLeftPart(UriPartial.Path);
                        string receivedPreviewBaseUrl = receivedPreviewUri.GetLeftPart(UriPartial.Path);

                        NameValueCollection receivedFileQueryParameters = HttpUtility.ParseQueryString(receivedFileUri.Query);
                        NameValueCollection receivedPreviewQueryParameters = HttpUtility.ParseQueryString(receivedPreviewUri.Query);

                        // Проверяем, что полученное описание загруженного файла совпадает с ожидаемым.
                        // FileUploadKey не проверяем, т.к. генерируется на сервере через Guid.NewGuid.
                        Assert.Equal(fileName, receivedFileDescription.FileName);
                        Assert.Equal(fileSize, receivedFileDescription.FileSize);
                        Assert.Equal(fileMimeType, receivedFileDescription.FileMimeType);
                        Assert.Null(receivedFileDescription.EntityTypeName);
                        Assert.Null(receivedFileDescription.EntityPropertyName);
                        Assert.Null(receivedFileDescription.EntityPrimaryKey);

                        Assert.Equal(FileBaseUrl, receivedFileBaseUrl, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Equal(FileBaseUrl, receivedPreviewBaseUrl, StringComparer.InvariantCultureIgnoreCase);

                        Assert.Equal(2, receivedFileQueryParameters.Count);
                        Assert.Equal(fileName, receivedFileQueryParameters[nameof(FileDescription.FileName)]);

                        Assert.Equal(3, receivedPreviewQueryParameters.Count);
                        Assert.Equal(fileName, receivedPreviewQueryParameters[nameof(FileDescription.FileName)]);
                        Assert.True(bool.Parse(receivedPreviewQueryParameters["GetPreview"]));
                    }
                });
        }

        /// <summary>
        /// Осуществляет проверку того, что файлы корректно скачиваются с сервера.
        /// </summary>
        [Fact]
        public void TestFileDownloadSuccess()
        {
            ActODataService(
                args =>
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                    {
                        // Arrange.
                        FileInfo fileInfo = new FileInfo(srcFilePath);

                        string fileUploadKey = Guid.NewGuid().ToString("D");
                        string fileName = fileInfo.Name;
                        string fileMimeType = MimeTypeUtils.GetFileMimeType(fileName);
#if NETFRAMEWORK
                        string fileContentDisposition = string.Format("attachment; filename=\"{0}\"; filename*=UTF-8''{1}; size={2}", fileName, Uri.EscapeDataString(fileName), fileInfo.Length);
#elif NETCOREAPP
                        string fileContentDisposition = string.Format("attachment; filename={0}; filename*=UTF-8''{1}", fileName, Uri.EscapeDataString(fileName));
#endif

                        // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
                        // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
                        string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
                        string uploadedFilePath = Path.Combine(uploadedFileDirectoryPath, fileName);
                        System.IO.File.Copy(srcFilePath, uploadedFilePath, true);

                        // Описание загруженного файла, содержащее URL для скачивания файла с сервера.
                        FileDescription uploadedFileDescription = new FileDescription(FileBaseUrl, uploadedFilePath);

                        // Act.
                        using HttpResponseMessage response = args.HttpClient.GetAsync(uploadedFileDescription.FileUrl).Result;

                        // Проверяем, что запрос завершился успешно.
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                        // Проверяем, что в ответе правильно указаны: длина, тип файла, и content-disposition.
                        Assert.Equal(fileInfo.Length, response.Content.Headers.ContentLength);
                        Assert.Equal(fileMimeType, response.Content.Headers.ContentType.MediaType);
                        Assert.Equal(fileContentDisposition, response.Content.Headers.ContentDisposition.ToString());

                        // Получаем поток байтов, скачиваемого файла.
                        var bytes = response.Content.ReadAsByteArrayAsync().Result;

                        // Сохраняем поток в виде файла (в каталог, предназначенный для скачанных с сервера файлов).
                        // Тем самым имитируем ситуацию как будто файл был скачан.
                        string downloadedFileDirectoryPath = CreateDownloadsSubDirectory(fileUploadKey);
                        string downloadedFilePath = Path.Combine(downloadedFileDirectoryPath, fileName);
                        using FileStream fileStream = new FileStream(downloadedFilePath, FileMode.Create, FileAccess.Write);
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Close();

                        // Проверяем, что загруженный на сервер, и скачанный с сервера файлы - одинаковы.
                        Assert.True(FilesComparer.FilesAreEqual(uploadedFilePath, downloadedFilePath));
                    }
                });
        }

        /// <summary>
        /// Осуществляет проверку того, что preview-файлов корректно скачиваются с сервера.
        /// </summary>
        [Fact]
        public void TestPreviewDownloadSuccess()
        {
            ActODataService(
                args =>
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                    {
                        FileInfo fileInfo = new FileInfo(srcFilePath);

                        string fileUploadKey = Guid.NewGuid().ToString("D");
                        string fileName = fileInfo.Name;
                        string fileMimeType = MimeTypeUtils.GetFileMimeType(fileName);

                        // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
                        // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
                        string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
                        string uploadedFilePath = Path.Combine(uploadedFileDirectoryPath, fileName);
                        System.IO.File.Copy(srcFilePath, uploadedFilePath, true);

                        // Загруженный файл в виде Base64String.
                        string uploadedFileBase64String = Base64Helper.GetBase64StringFileData(uploadedFilePath);

                        // Описание загруженного файла, содержащее URL для скачивания preview-файла с сервера.
                        FileDescription uploadedFileDescription = new FileDescription(FileBaseUrl, uploadedFilePath);

                        // Act.
                        using HttpResponseMessage response = args.HttpClient.GetAsync(uploadedFileDescription.PreviewUrl).Result;
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                        // Получаем Base64String, содержащую preview скачанного файла.
                        string downloadedFileBase64String = response.Content.ReadAsStringAsync().Result;

                        if (fileMimeType.StartsWith("image"))
                        {
                            // Для файлов являющихся изображениями, должна возвращаться Base64String с указанием MIME-типа.
                            Assert.Equal(uploadedFileBase64String, downloadedFileBase64String);
                            Assert.StartsWith($"data:{fileMimeType}", downloadedFileBase64String.ToLower());
                        }
                        else
                        {
                            // Для файлов не являющихся изображениями, должна возвращаться пустая строка.
                            Assert.Empty(downloadedFileBase64String);
                            Assert.Equal(0, response.Content.Headers.ContentLength);
                        }
                    }
                });
        }

        /// <summary>
        /// Осуществляет проверку того, что файлы, связанные с объектами данных, корректно скачиваются с сервера.
        /// </summary>
        [Fact]
        public void TestFileDownloadFromDataObjectSuccess()
        {
            ActODataService(
                args =>
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                    {
                        FileInfo fileInfo = new FileInfo(srcFilePath);

                        string fileUploadKey = Guid.NewGuid().ToString("D");
                        string fileName = fileInfo.Name;
                        string fileMimeType = MimeTypeUtils.GetFileMimeType(fileName);
                        long fileSize = fileInfo.Length;
#if NETFRAMEWORK
                        string fileContentDisposition = string.Format("attachment; filename=\"{0}\"; filename*=UTF-8''{1}; size={2}", fileName, Uri.EscapeDataString(fileName), fileInfo.Length);
#elif NETCOREAPP
                        string fileContentDisposition = string.Format("attachment; filename={0}; filename*=UTF-8''{1}", fileName, Uri.EscapeDataString(fileName));
#endif

                        // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
                        // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
                        string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
                        string uploadedFilePath = Path.Combine(uploadedFileDirectoryPath, fileName);
                        System.IO.File.Copy(srcFilePath, uploadedFilePath, true);

                        // Описание загруженного файла, содержащее URL для скачивания файла с сервера.
                        FileDescription uploadedFileDescription = new FileDescription(FileBaseUrl, uploadedFilePath);

                        // Свойство объекта данных типа File, связывающее загруженный файл с объектом данных.
                        string entityFilePropertyName = nameof(КлассСМножествомТипов.PropertyStormnetFile);
                        File entityFileProperty = new File { Name = fileName, Size = fileSize };
                        entityFileProperty.FromNormalToZip(uploadedFilePath);

                        // Свойство объекта данных типа WebFile, связывающее загруженный файл с объектом данных.
                        string entityWebFilePropertyName = nameof(КлассСМножествомТипов.PropertyStormnetWebFile);
                        WebFile entityWebFileProperty = new WebFile { Name = fileName, Size = (int)fileSize, Url = uploadedFileDescription.FileUrl };

                        // Объект данных, с которым связан загруженный файл.
                        DataObject entity = new КлассСМножествомТипов
                        {
                            // Файловые свойства объекта данных.
                            PropertyStormnetFile = entityFileProperty,
                            PropertyStormnetWebFile = entityWebFileProperty,

                            // DateTime-свойство объекта данных.
                            // На работу с файловыми свойствами никак не влияет,
                            // но если оставить его незаполненным, сервис данных будет ругаться при сохранении.
                            PropertyDateTime = new DateTimeOffset(DateTime.Now).UtcDateTime,
                        };

                        // Свойства необходимые для описания файла, связанного с объектом данных.
                        string entityTypeName = entity.GetType().AssemblyQualifiedName;
                        string entityPrimaryKey = entity.__PrimaryKey.ToString();
                        List<string> entityPropertiesnames = new List<string> { entityFilePropertyName, entityWebFilePropertyName };

                        // Сохраняем объект данных в БД (используя подменный сервис данных).
                        args.DataService.UpdateObject(entity);

                        // Перебираем разнотипные файловые свойства объекта данных.
                        foreach (string entityPropertyName in entityPropertiesnames)
                        {
                            // Описание файла, связанного с объектом данных, которое содержит ссылку на скачивание файла.
                            var entityRelatedFileDescription = new FileDescription(FileBaseUrl)
                            {
                                FileName = fileName,
                                FileSize = fileSize,
                                FileMimeType = fileMimeType,
                                EntityTypeName = entityTypeName,
                                EntityPrimaryKey = entityPrimaryKey,
                                EntityPropertyName = entityPropertyName,
                            };

                            // Act.
                            using HttpResponseMessage response = args.HttpClient.GetAsync(entityRelatedFileDescription.FileUrl).Result;
                            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                            // Проверяем, что в ответе правильно указаны: длина, тип файла, и content-disposition.
                            Assert.Equal(fileInfo.Length, response.Content.Headers.ContentLength);
                            Assert.Equal(fileMimeType, response.Content.Headers.ContentType.MediaType);
                            Assert.Equal(fileContentDisposition, response.Content.Headers.ContentDisposition.ToString());

                                // Получаем поток байтов, скачиваемого файла.
                            var bytes = response.Content.ReadAsByteArrayAsync().Result;

                            // Сохраняем поток в виде файла (в каталог, предназначенный для скачанных с сервера файлов).
                            // Тем самым имитируем ситуацию как будто файл был скачан.
                            string downloadedFileDirectoryPath = CreateDownloadsSubDirectory(fileUploadKey);
                            string downloadedFilePath = Path.Combine(downloadedFileDirectoryPath, fileName);
                            using var fileStream = new FileStream(downloadedFilePath, FileMode.Create, FileAccess.Write);
                            fileStream.Write(bytes, 0, bytes.Length);
                            fileStream.Close();

                            // Проверяем, что загруженный на сервер, и скачанный с сервера файлы - одинаковы.
                            Assert.True(FilesComparer.FilesAreEqual(uploadedFilePath, downloadedFilePath));
                        }
                    }
                });
        }

        /// <summary>
        /// Осуществляет проверку того, что для файлов, связанных с объектами данных, корректно скачиваются их preview-изображения.
        /// </summary>
        [Fact]
        public void TestPreviewDownloadFromDataObjectSuccess()
        {
            ActODataService(
                args =>
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                    {
                        FileInfo fileInfo = new FileInfo(srcFilePath);

                        string fileUploadKey = Guid.NewGuid().ToString("D");
                        string fileName = fileInfo.Name;
                        string fileMimeType = MimeTypeUtils.GetFileMimeType(fileName);
                        long fileSize = fileInfo.Length;

                        // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
                        // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
                        string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
                        string uploadedFilePath = Path.Combine(uploadedFileDirectoryPath, fileName);
                        System.IO.File.Copy(srcFilePath, uploadedFilePath, true);

                        // Описание загруженного файла, содержащее URL для скачивания файла с сервера.
                        FileDescription uploadedFileDescription = new FileDescription(FileBaseUrl, uploadedFilePath);

                        // Загруженный файл в виде Base64String.
                        string uploadedFileBase64String = Base64Helper.GetBase64StringFileData(uploadedFilePath);

                        // Свойство объекта данных типа File, связывающее загруженный файл с объектом данных.
                        string entityFilePropertyName = nameof(КлассСМножествомТипов.PropertyStormnetFile);
                        File entityFileProperty = new File { Name = fileName, Size = fileSize };
                        entityFileProperty.FromNormalToZip(uploadedFilePath);

                        // Свойство объекта данных типа WebFile, связывающее загруженный файл с объектом данных.
                        string entityWebFilePropertyName = nameof(КлассСМножествомТипов.PropertyStormnetWebFile);
                        WebFile entityWebFileProperty = new WebFile { Name = fileName, Size = (int)fileSize, Url = uploadedFileDescription.FileUrl };

                        // Объект данных, с которым связан загруженный файл.
                        DataObject entity = new КлассСМножествомТипов
                        {
                            // Файловые свойства объекта данных.
                            PropertyStormnetFile = entityFileProperty,
                            PropertyStormnetWebFile = entityWebFileProperty,

                            // DateTime-свойство объекта данных.
                            // На работу с файловыми свойствами никак не влияет,
                            // но если оставить его незаполненным, сервис данных будет ругаться при сохранении.
                            PropertyDateTime = new DateTimeOffset(DateTime.Now).UtcDateTime,
                        };

                        // Свойства необходимые для описания файла, связанного с объектом данных.
                        string entityTypeName = entity.GetType().AssemblyQualifiedName;
                        string entityPrimaryKey = entity.__PrimaryKey.ToString();
                        List<string> entityPropertiesnames = new List<string> { entityFilePropertyName, entityWebFilePropertyName };

                        // Сохраняем объект данных в БД (используя подменный сервис данных).
                        args.DataService.UpdateObject(entity);

                        // Перебираем разнотипные файловые свойства объекта данных.
                        foreach (string entityPropertyName in entityPropertiesnames)
                        {
                            // Описание файла, связанного с объектом данных, которое содержит ссылку на скачивание файла.
                            FileDescription entityRelatedFileDescription = new FileDescription(FileBaseUrl)
                            {
                                FileName = fileName,
                                FileSize = fileSize,
                                FileMimeType = fileMimeType,
                                EntityTypeName = entityTypeName,
                                EntityPrimaryKey = entityPrimaryKey,
                                EntityPropertyName = entityPropertyName,
                            };

                            // Act.
                            using HttpResponseMessage response = args.HttpClient.GetAsync(entityRelatedFileDescription.PreviewUrl).Result;
                            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                            // Получаем Base64String, содержащую preview скачанного файла.
                            string downloadedFileBase64String = response.Content.ReadAsStringAsync().Result;

                            if (fileMimeType.StartsWith("image"))
                            {
                                // Для файлов являющихся изображениями, должна возвращаться Base64String с указанием MIME-типа.
                                Assert.Equal(uploadedFileBase64String, downloadedFileBase64String);
                                Assert.StartsWith($"data:{fileMimeType}", downloadedFileBase64String.ToLower());
                            }
                            else
                            {
                                // Для файлов не являющихся изображениями, должна возвращаться пустая строка.
                                Assert.Empty(downloadedFileBase64String);
                                Assert.Equal(0, response.Content.Headers.ContentLength);
                            }
                        }
                    }
                });
        }
    }
}
