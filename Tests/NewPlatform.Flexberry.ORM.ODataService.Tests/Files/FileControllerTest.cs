namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Files
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    //using System.Fakes;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web;
    //using System.Web.Fakes;
    using System.Web.Http;
    using System.Web.Http.Cors;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    //using Microsoft.QualityTools.Testing.Fakes;
    //using NUnit.Framework;

    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Extensions;


    using File = ICSSoft.STORMNET.FileType.File;
    using WebFile = ICSSoft.STORMNET.UserDataTypes.WebFile;

    /// <summary>
    /// Тесты файлового контроллера <see cref="FileController"/>, отвечающего за загрузку файлов на сервер и их скачивание.
    /// </summary>
    //[TestFixture]
    public class FileControllerTest
    {
        /// <summary>
        /// Класс-обертка для работы с Flexberry ORM.
        /// </summary>
        public class DataServiceWrapper : BaseIntegratedTest
        {
            /// <summary>
            /// Инициализирует класс-обертку для работы с Flexberry ORM.
            /// </summary>
            public DataServiceWrapper()
                : base("FileCtrl")
            {
            }

            /// <summary>
            /// Получает коллекцию поддерживаемых сервисов данных.
            /// </summary>
            public IEnumerable<IDataService> AllowedDataServices => DataServices;
                }

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
        /// <param name="context">Контекст выполнения тестов.</param>
        //[TestFixtureSetUp]
        public static void InitializeClass()
        {
            _filesDirectoryPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Files";

            _uploadsDirectoryPath = $"{_filesDirectoryPath}\\Uploads_{Guid.NewGuid().ToString("D")}";
            if (!Directory.Exists(_uploadsDirectoryPath))
            {
                Directory.CreateDirectory(_uploadsDirectoryPath);
            }

            _downloadsDirectoryPath = $"{_filesDirectoryPath}\\Downloads_{Guid.NewGuid().ToString("D")}";
            if (!Directory.Exists(_downloadsDirectoryPath))
            {
                Directory.CreateDirectory(_downloadsDirectoryPath);
            }

            _srcImageFilePath = $"{_filesDirectoryPath}\\delorean.png";

            _srcTextFilePath = $"{_filesDirectoryPath}\\readme.txt";
        }

        /// <summary>
        /// Деинициализирует тестовый класс (деинициализация выполняется после того как все тесты завершат свою работу).
        /// </summary>
        //[TestFixtureTearDown]
        public static void CleanUpClass()
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

        /// <summary>
        /// Осуществляет создание подкаталога с заданным именем в каталоге <see cref="_uploadsDirectoryPath"/>.
        /// </summary>
        /// <param name="subDirectoryName">Имя создаваемого подкаталога.</param>
        /// <returns>Абсолютный путь к созданному подкаталогу.</returns>
        public string CreateUploadsSubDirectory(string subDirectoryName)
        {
            string directoryPath = $"{_uploadsDirectoryPath}\\{subDirectoryName}";
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
            string directoryPath = $"{_downloadsDirectoryPath}\\{subDirectoryName}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        /// <summary>
        /// Осуществляет проверку того, что файлы корректно загружаются на сервер.
        /// </summary>
        //[Test]
        public void TestUploadSuccess()
        {
            string fileBaseUrl = "http://localhost/api/File";

            // TODO: По какой-то причине этот тест виснет при асинхронном запросе к серверу если создаются временные БД.
            // Скорее всего проблема в контексте синхронизации ASP.NET и асинхронном методе контроллера.
            //using (DataServiceWrapper dataServiceWrapper = new DataServiceWrapper())
            //using (ShimsContext.Create())
            {
                // Подменяем HttpContext.
                //ShimHttpContext.CurrentGet = () =>
                {
                    //return new HttpContext(new HttpRequest(null, "http://localhost", null), new HttpResponse(null));
                };

                //foreach (IDataService dataService in dataServiceWrapper.AllowedDataServices)
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                    {
                    FileInfo srcFileInfo = new FileInfo(srcFilePath);

                    // Ключ загрузки файла (этим ключем будет именоваться подкаталог в каталоге UploadsDirectoryPath).
                    Guid fileUploadKeyGuid = Guid.NewGuid();
                    string fileUploadKey = fileUploadKeyGuid.ToString("D");
                    string fileName = srcFileInfo.Name;
                    string fileMimeType = MimeMapping.GetMimeMapping(fileName);
                    long fileSize = srcFileInfo.Length;

                        string uploadedFilePath = $"{_uploadsDirectoryPath}\\{fileUploadKey}\\{srcFileInfo.Name}";

                        using (var config = new HttpConfiguration())
                    {
                        config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                            config.MapODataServiceFileRoute("File", "api/File", _uploadsDirectoryPath, new MSSQLDataService());

                            using (var server = new HttpServer(config))
                        {
                            using (var client = new HttpClient(server, false))
                            {
                                    using (var uploadingImageFileContent = new StreamContent(srcFileInfo.Open(FileMode.Open, FileAccess.Read)))
                                {
                                    uploadingImageFileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { FileName = srcFileInfo.Name };
                                    uploadingImageFileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(srcFileInfo.Name));

                                        var formDataContent = new MultipartFormDataContent { uploadingImageFileContent };

                                    // Подменяем NewGuid, чтобы имя подкаталога, в который будет загружен файл, было заранее известно.
                                        //ShimGuid.NewGuid = () => fileUploadKeyGuid;
                                    using (HttpResponseMessage response = client.PostAsync(fileBaseUrl, formDataContent).Result)
                                    {
                                        // Проверяем, что запрос завершился успешно.
                                        //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                                        // Проверяем, что загруженный на сервер файл существует, и совпадает с тем файлом, из которого производилась загрузка.
                                        //Assert.AreEqual(true, System.IO.File.Exists(uploadedFilePath));
                                        //Assert.AreEqual(true, FilesComparer.FilesAreEqual(srcFilePath, uploadedFilePath));

                                        // Получаем описание загруженного файла, из ответа, полученного от сервера.
                                        FileDescription receivedFileDescription = FileDescription.FromJson(response.Content.ReadAsStringAsync().Result);

                                        Uri receivedFileUri = new Uri(receivedFileDescription.FileUrl);
                                        Uri receivedPreviewUri = new Uri(receivedFileDescription.PreviewUrl);

                                        string receivedFileBaseUrl = receivedFileUri.GetLeftPart(UriPartial.Path);
                                        string receivedPreviewBaseUrl = receivedPreviewUri.GetLeftPart(UriPartial.Path);

                                        NameValueCollection receivedFileQueryParameters = HttpUtility.ParseQueryString(receivedFileUri.Query);
                                        NameValueCollection receivedPreviewQueryParameters = HttpUtility.ParseQueryString(receivedPreviewUri.Query);

                                            // Проверяем, что полученное описание загруженного файла совпадает с ожидаемым.
                                            /*
                                        Assert.AreEqual(fileUploadKey, receivedFileDescription.FileUploadKey);
                                        Assert.AreEqual(fileName, receivedFileDescription.FileName);
                                        Assert.AreEqual(fileSize, receivedFileDescription.FileSize);
                                        Assert.AreEqual(fileMimeType, receivedFileDescription.FileMimeType);
                                        Assert.AreEqual(null, receivedFileDescription.EntityTypeName);
                                        Assert.AreEqual(null, receivedFileDescription.EntityPropertyName);
                                        Assert.AreEqual(null, receivedFileDescription.EntityPrimaryKey);

                                        Assert.AreEqual(fileBaseUrl, receivedFileBaseUrl);
                                        Assert.AreEqual(fileBaseUrl, receivedPreviewBaseUrl);

                                        Assert.AreEqual(2, receivedFileQueryParameters.Count);
                                        Assert.AreEqual(fileUploadKey, receivedFileQueryParameters[nameof(FileDescription.FileUploadKey)]);
                                        Assert.AreEqual(fileName, receivedFileQueryParameters[nameof(FileDescription.FileName)]);

                                        Assert.AreEqual(3, receivedPreviewQueryParameters.Count);
                                        Assert.AreEqual(fileUploadKey, receivedPreviewQueryParameters[nameof(FileDescription.FileUploadKey)]);
                                        Assert.AreEqual(fileName, receivedPreviewQueryParameters[nameof(FileDescription.FileName)]);
                                        Assert.AreEqual(true, bool.Parse(receivedPreviewQueryParameters["GetPreview"]));
                                        */
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        }

        /// <summary>
        /// Осуществляет проверку того, что файлы корректно скачиваются с сервера.
        /// </summary>
        //[Test]
        public void TestFileDownloadSuccess()
        {
            string fileBaseUrl = "http://localhost/api/File";

            using (DataServiceWrapper dataServiceWrapper = new DataServiceWrapper())
            //using (ShimsContext.Create())
            {
                //ShimHttpContext.CurrentGet = () => new HttpContext(new HttpRequest(null, "http://localhost", null), new HttpResponse(null));

                foreach (IDataService dataService in dataServiceWrapper.AllowedDataServices)
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                    {
                    FileInfo fileInfo = new FileInfo(srcFilePath);

                    string fileUploadKey = Guid.NewGuid().ToString("D");
                    string fileName = fileInfo.Name;
                    string fileMimeType = MimeMapping.GetMimeMapping(fileName);
                    string fileSize = fileInfo.Length.ToString();
                    string fileContentDisposition = string.Format("attachment; filename=\"{0}\"; filename*=UTF-8''{1}; size={2}", fileName, Uri.EscapeDataString(fileName), fileSize);

                    // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
                    // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
                    string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
                        string uploadedFilePath = $"{uploadedFileDirectoryPath}\\{fileName}";
                    System.IO.File.Copy(srcFilePath, uploadedFilePath, true);

                    // Описание загруженного файла, содержащее URL для скачивания файла с сервера.
                    FileDescription uploadedFileDescription = new FileDescription(fileBaseUrl, uploadedFilePath);

                    // Конфигурируем HttpServer, файловый контроллер, и обращаемся к серверу для скачивания файла.
                        using (var config = new HttpConfiguration())
                    {
                        config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                            config.MapODataServiceFileRoute("File", "api/File", _uploadsDirectoryPath, dataService);

                            using (var server = new HttpServer(config))
                            using (var client = new HttpClient(server, false))
                                using (HttpResponseMessage response = client.GetAsync(uploadedFileDescription.FileUrl).Result)
                                {
                                    //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                                    // Проверяем, что в ответе правильно указаны: длина, тип файла, и content-disposition.
                                    //Assert.AreEqual(fileSize, response.Content.Headers.GetValues("Content-Length").FirstOrDefault());
                                    //Assert.AreEqual(fileMimeType, response.Content.Headers.GetValues("Content-Type").FirstOrDefault());
                                    //Assert.AreEqual(fileContentDisposition, response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault());

                                    // Получаем поток байтов, скачиваемого файла.
                                    using (Stream stream = response.Content.ReadAsStreamAsync().Result)
                                    {
                                    // Вычитываем поток в буфер.
                                        byte[] bytes = new byte[stream.Length];
                                        stream.Read(bytes, 0, (int)stream.Length);

                                        // Сохраняем поток в виде файла (в каталог, предназначенный для скачанных с сервера файлов).
                                        // Тем самым имитируем ситуацию как будто файл был скачан.
                                        string downloadedFileDirectoryPath = CreateDownloadsSubDirectory(fileUploadKey);
                                    string downloadedFilePath = $"{downloadedFileDirectoryPath}\\{fileName}";
                                        using (FileStream fileStream = new FileStream(downloadedFilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fileStream.Write(bytes, 0, (int)stream.Length);
                                            fileStream.Close();

                                            // Проверяем, что загруженный на сервер, и скачанный с сервера файлы - одинаковы.
                                            //Assert.AreEqual(true, FilesComparer.FilesAreEqual(uploadedFilePath, downloadedFilePath));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        /// <summary>
        /// Осуществляет проверку того, что preview-файлов корректно скачиваются с сервера.
        /// </summary>
        //[Test]
        public void TestPreviewDownloadSuccess()
        {
            string fileBaseUrl = "http://localhost/api/File";

            using (DataServiceWrapper dataServiceWrapper = new DataServiceWrapper())
            //using (ShimsContext.Create())
            {
                //ShimHttpContext.CurrentGet = () => new HttpContext(new HttpRequest(null, "http://localhost", null), new HttpResponse(null));

                foreach (IDataService dataService in dataServiceWrapper.AllowedDataServices)
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                {
                    FileInfo fileInfo = new FileInfo(srcFilePath);

                    string fileUploadKey = Guid.NewGuid().ToString("D");
                    string fileName = fileInfo.Name;
                    string fileMimeType = MimeMapping.GetMimeMapping(fileName);

                    // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
                    // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
                    string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
                        string uploadedFilePath = $"{uploadedFileDirectoryPath}\\{fileName}";
                    System.IO.File.Copy(srcFilePath, uploadedFilePath, true);

                    // Загруженный файл в виде Base64String.
                    string uploadedFileBase64String = FileController.GetBase64StringFileData(uploadedFilePath);

                    // Описание загруженного файла, содержащее URL для скачивания preview-файла с сервера.
                    FileDescription uploadedFileDescription = new FileDescription(fileBaseUrl, uploadedFilePath);

                    // Конфигурируем HttpServer, файловый контроллер, и обращаемся к серверу для скачивания preview-файла.
                        using (var config = new HttpConfiguration())
                    {
                        config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                            config.MapODataServiceFileRoute("File", "api/File", _uploadsDirectoryPath, dataService);

                            using (var server = new HttpServer(config))
                            using (var client = new HttpClient(server, false))
                                using (HttpResponseMessage response = client.GetAsync(uploadedFileDescription.PreviewUrl).Result)
                                {
                                    //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                                    // Получаем Base64String, содержащую preview скачанного файла.
                                    string downloadedFileBase64String = response.Content.ReadAsStringAsync().Result;

                                    if (fileMimeType.StartsWith("image"))
                                    {
                                        // Для файлов являющихся изображениями, должна возвращаться Base64String с указанием MIME-типа.
                                        //Assert.AreEqual(uploadedFileBase64String, downloadedFileBase64String);
                                    //Assert.AreEqual(true, downloadedFileBase64String.ToLower().StartsWith($"data:{fileMimeType}"));
                                    }
                                    else
                                    {
                                        // Для файлов не являющихся изображениями, должна возвращаться пустая строка.
                                        //Assert.AreEqual(true, downloadedFileBase64String == string.Empty);
                                        //Assert.AreEqual(0.ToString(), response.Content.Headers.GetValues("Content-Length").FirstOrDefault());
                                    }
                                }
                            }
                        }
                    }
                }
            }

        /// <summary>
        /// Осуществляет проверку того, что файлы, связанные с объектами данных, корректно скачиваются с сервера.
        /// </summary>
        //[Test]
        public void TestFileDownloadFromDataObjectSuccess()
        {
            string fileBaseUrl = "http://localhost/api/File";

            using (DataServiceWrapper dataServiceWrapper = new DataServiceWrapper())
            {
                foreach (IDataService dataService in dataServiceWrapper.AllowedDataServices)
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                        {
                            FileInfo fileInfo = new FileInfo(srcFilePath);

                            string fileUploadKey = Guid.NewGuid().ToString("D");
                            string fileName = fileInfo.Name;
                            string fileMimeType = MimeMapping.GetMimeMapping(fileName);
                            long fileSize = fileInfo.Length;
                            string fileContentDisposition = string.Format("attachment; filename=\"{0}\"; filename*=UTF-8''{1}; size={2}", fileName, Uri.EscapeDataString(fileName), fileSize);

                            // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
                            // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
                            string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
                            string uploadedFilePath = string.Concat(uploadedFileDirectoryPath, "\\", fileName);
                            System.IO.File.Copy(srcFilePath, uploadedFilePath, true);

                            // Описание загруженного файла, содержащее URL для скачивания файла с сервера.
                            FileDescription uploadedFileDescription = new FileDescription(fileBaseUrl, uploadedFilePath);

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
                                                        PropertyDateTime = new DateTimeOffset(DateTime.Now).UtcDateTime
                        };

                            // Свойства необходимые для описания файла, связанного с объектом данных.
                            string entityTypeName = entity.GetType().AssemblyQualifiedName;
                            string entityPrimaryKey = entity.__PrimaryKey.ToString();
                            List<string> entityPropertiesnames = new List<string> { entityFilePropertyName, entityWebFilePropertyName };

                        // Сохраняем объект данных в БД (используя подменный сервис данных).
                        dataService.UpdateObject(entity);

                            // Перебираем разнотипные файловые свойства объекта данных.
                            foreach (string entityPropertyName in entityPropertiesnames)
                            {
                                // Описание файла, связанного с объектом данных, которое содержит ссылку на скачивание файла.
                            var entityRelatedFileDescription = new FileDescription(fileBaseUrl)
                                                                                   {
                                                                                       FileName = fileName,
                                                                                       FileSize = fileSize,
                                                                                       FileMimeType = fileMimeType,
                                                                                       EntityTypeName = entityTypeName,
                                                                                       EntityPrimaryKey = entityPrimaryKey,
                                                                                       EntityPropertyName = entityPropertyName
                                                                                   };

                                // Конфигурируем HttpServer, файловый контроллер, и обращаемся к серверу для скачивания файла.
                            using (var config = new HttpConfiguration())
                                {
                                    config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                                config.MapODataServiceFileRoute("File", "api/File", _uploadsDirectoryPath, dataService);

                                using (var server = new HttpServer(config))
                                        using (var client = new HttpClient(server, false))
                                            using (HttpResponseMessage response = client.GetAsync(entityRelatedFileDescription.FileUrl).Result)
                                            {
                                                //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                                                // Проверяем, что в ответе правильно указаны: длина, тип файла, и content-disposition.
                                                //Assert.AreEqual(fileSize.ToString(), response.Content.Headers.GetValues("Content-Length").FirstOrDefault());
                                                //Assert.AreEqual(fileMimeType, response.Content.Headers.GetValues("Content-Type").FirstOrDefault());
                                                //Assert.AreEqual(fileContentDisposition, response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault());

                                                // Получаем поток байтов, скачиваемого файла.
                                                using (Stream stream = response.Content.ReadAsStreamAsync().Result)
                                                {
                                        // Вычитываем поток в буфер.
                                                    byte[] bytes = new byte[stream.Length];
                                                    stream.Read(bytes, 0, (int)stream.Length);

                                                    // Сохраняем поток в виде файла (в каталог, предназначенный для скачанных с сервера файлов).
                                                    // Тем самым имитируем ситуацию как будто файл был скачан.
                                                    string downloadedFileDirectoryPath = CreateDownloadsSubDirectory(fileUploadKey);
                                        string downloadedFilePath = $"{downloadedFileDirectoryPath}\\{fileName}";
                                        using (var fileStream = new FileStream(downloadedFilePath, FileMode.Create, FileAccess.Write))
                                                    {
                                                        fileStream.Write(bytes, 0, (int)stream.Length);
                                                        fileStream.Close();

                                                        // Проверяем, что загруженный на сервер, и скачанный с сервера файлы - одинаковы.
                                                        //Assert.AreEqual(true, FilesComparer.FilesAreEqual(uploadedFilePath, downloadedFilePath));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

        /// <summary>
        /// Осуществляет проверку того, что для файлов, связанных с объектами данных, корректно скачиваются их preview-изображения.
        /// </summary>
        //[Test]
        public void TestPreviewDownloadFromDataObjectSuccess()
        {
            string fileBaseUrl = "http://localhost/api/File";

            using (DataServiceWrapper dataServiceWrapper = new DataServiceWrapper())
            {
                foreach (IDataService dataService in dataServiceWrapper.AllowedDataServices)
                {
                    foreach (string srcFilePath in new List<string> { _srcImageFilePath, _srcTextFilePath })
                        {
                            FileInfo fileInfo = new FileInfo(srcFilePath);

                            string fileUploadKey = Guid.NewGuid().ToString("D");
                            string fileName = fileInfo.Name;
                            string fileMimeType = MimeMapping.GetMimeMapping(fileName);
                            long fileSize = fileInfo.Length;

                            // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
                            // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
                            string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
                            string uploadedFilePath = string.Concat(uploadedFileDirectoryPath, "\\", fileName);
                            System.IO.File.Copy(srcFilePath, uploadedFilePath, true);

                            // Описание загруженного файла, содержащее URL для скачивания файла с сервера.
                            FileDescription uploadedFileDescription = new FileDescription(fileBaseUrl, uploadedFilePath);

                            // Загруженный файл в виде Base64String.
                            string uploadedFileBase64String = FileController.GetBase64StringFileData(uploadedFilePath);

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
                                PropertyDateTime = new DateTimeOffset(DateTime.Now).UtcDateTime
                            };

                            // Свойства необходимые для описания файла, связанного с объектом данных.
                            string entityTypeName = entity.GetType().AssemblyQualifiedName;
                            string entityPrimaryKey = entity.__PrimaryKey.ToString();
                            List<string> entityPropertiesnames = new List<string> { entityFilePropertyName, entityWebFilePropertyName };

                        // Сохраняем объект данных в БД (используя подменный сервис данных).
                        dataService.UpdateObject(entity);

                            // Перебираем разнотипные файловые свойства объекта данных.
                            foreach (string entityPropertyName in entityPropertiesnames)
                            {
                                // Описание файла, связанного с объектом данных, которое содержит ссылку на скачивание файла.
                                FileDescription entityRelatedFileDescription = new FileDescription(fileBaseUrl)
                                {
                                    FileName = fileName,
                                    FileSize = fileSize,
                                    FileMimeType = fileMimeType,
                                    EntityTypeName = entityTypeName,
                                    EntityPrimaryKey = entityPrimaryKey,
                                    EntityPropertyName = entityPropertyName
                                };

                                // Конфигурируем HttpServer, файловый контроллер, и обращаемся к серверу для скачивания файла.
                            using (var config = new HttpConfiguration())
                                {
                                    config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                                config.MapODataServiceFileRoute("File", "api/File", _uploadsDirectoryPath, dataService);

                                using (var server = new HttpServer(config))
                                        using (var client = new HttpClient(server, false))
                                            using (HttpResponseMessage response = client.GetAsync(entityRelatedFileDescription.PreviewUrl).Result)
                                            {
                                                //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                                                // Получаем Base64String, содержащую preview скачанного файла.
                                                string downloadedFileBase64String = response.Content.ReadAsStringAsync().Result;

                                                if (fileMimeType.StartsWith("image"))
                                                {
                                                    // Для файлов являющихся изображениями, должна возвращаться Base64String с указанием MIME-типа.
                                                    //Assert.AreEqual(uploadedFileBase64String, downloadedFileBase64String);
                                        //Assert.AreEqual(true, downloadedFileBase64String.ToLower().StartsWith($"data:{fileMimeType}"));
                                                }
                                                else
                                                {
                                                    // Для файлов не являющихся изображениями, должна возвращаться пустая строка.
                                                    //Assert.AreEqual(true, downloadedFileBase64String == string.Empty);
                                                    //Assert.AreEqual(0.ToString(), response.Content.Headers.GetValues("Content-Length").FirstOrDefault());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
