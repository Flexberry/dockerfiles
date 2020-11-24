#if NETFRAMEWORK
namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Files
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Cors;

    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;

    using Newtonsoft.Json;

    using Xunit;

    /// <summary>
    /// Класс, содержащий модульные тесты для метаданных, описывающих файловые свойства объектов данных.
    /// </summary>
    public class FileDescriptionTest
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
        /// Путь к тестовому текстовому файлу.
        /// </summary>
        private static string _srcTextFilePath;

        /// <summary>
        /// Инициализирует тестовый класс (инициализация выполняется перед запуском тестов).
        /// </summary>
        public FileDescriptionTest()
        {
            _filesDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");

            _uploadsDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
            if (!Directory.Exists(_uploadsDirectoryPath))
            {
                Directory.CreateDirectory(_uploadsDirectoryPath);
            }

            _srcTextFilePath = Path.Combine(_filesDirectoryPath, "readme.txt");
        }

        /// <summary>
        /// Осуществляет очистку результатов работы тестов.
        /// </summary>
        ~FileDescriptionTest()
        {
            FileDescriptionTestController.FileDescriptionGet = null;
            FileDescriptionTestController.FileDescriptionPost = null;
            FileDescriptionTestController.FileDescriptionPut = null;
        }

        /// <summary>
        /// Тестовый WebApi контроллер для проверки того, как <see cref="FileDescription"/>
        /// сериализуется в JSON при возвращении в качестве ответа на запрос к серверу,
        /// десериализуется из JSON-строки, приходящей в теле запроса к серверу,
        /// и десериализуется из параметров запроса, указанных в URL, по которому происходит обращение к серверу.
        /// </summary>
        public class FileDescriptionTestController : ApiController
        {
            /// <summary>
            /// Описание файла, которое должно возвращаться обработчиком GET-запроса.
            /// </summary>
            public static FileDescription FileDescriptionGet { get; set; }

            /// <summary>
            /// Описание файла, которое было получено обработчиком POST-запроса.
            /// </summary>
            public static FileDescription FileDescriptionPost { get; set; }

            /// <summary>
            /// Описание файла, которое было получено обработчиком PUT-запроса.
            /// </summary>
            public static FileDescription FileDescriptionPut { get; set; }

            /// <summary>
            /// Возвращает описание файла заданное в свойстве <see cref="FileDescriptionGet"/>.
            /// </summary>
            /// <returns>Описание файла обернутое в <see cref="HttpResponseMessage"/>.</returns>
            [HttpGet]
            public HttpResponseMessage Get()
            {
                HttpResponseMessage response = Request.CreateResponse(FileDescriptionGet);
                response.StatusCode = HttpStatusCode.OK;

                return response;
            }

            /// <summary>
            /// Получает описание файла, и сохраняет его в свойстве <see cref="FileDescriptionPost"/>.
            /// </summary>
            /// <param name="fileDescription">Описание файла, полученное от клиента в теле запроса.</param>
            /// <returns>Пустой <see cref="HttpResponseMessage"/>.</returns>
            [HttpPost]
            public HttpResponseMessage Post([FromBody] FileDescription fileDescription)
            {
                fileDescription.FileBaseUrl = Request.RequestUri.GetLeftPart(UriPartial.Path);
                FileDescriptionPost = fileDescription;

                HttpResponseMessage response = Request.CreateResponse();
                response.StatusCode = HttpStatusCode.OK;

                return response;
            }

            /// <summary>
            /// Получает описание файла, и сохраняет его в свойстве <see cref="FileDescriptionPut"/>.
            /// </summary>
            /// <param name="fileDescription">Описание файла, полученное от клиента в параметрах запроса.</param>
            /// <returns>Пустой <see cref="HttpResponseMessage"/>.</returns>
            [HttpPut]
            public HttpResponseMessage Put([FromUri] FileDescription fileDescription)
            {
                fileDescription.FileBaseUrl = Request.RequestUri.GetLeftPart(UriPartial.Path);
                FileDescriptionPut = fileDescription;

                HttpResponseMessage response = Request.CreateResponse();
                response.StatusCode = HttpStatusCode.OK;

                return response;
            }
        }

        /// <summary>
        /// Осуществляет создание подкаталога с заданным именем в каталоге <see cref="_uploadsDirectoryPath"/>.
        /// </summary>
        /// <param name="subDirectoryName">Имя создаваемого подкаталога.</param>
        /// <returns>Абсолютный путь к созданному подкаталогу.</returns>
        public static string CreateUploadsSubDirectory(string subDirectoryName)
        {
            string directoryPath = Path.Combine(_uploadsDirectoryPath, subDirectoryName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/> не формируются
        /// если не задано свойтсво <see cref="FileDescription.FileBaseUrl"/>.
        /// </summary>
        [Fact]
        public void TestUrlsByEmptyFileBaseUrl()
        {
            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";

            // Свойства достаточные для описания файла, который привязан к объекту данных
            // (тип и первичный ключ объекта данных, а так же имя файлового свойства в объекте данных).
            КлассСМножествомТипов entity = new КлассСМножествомТипов();
            string entityTypeName = entity.GetType().AssemblyQualifiedName;
            string entityPropertyName = nameof(entity.PropertyStormnetFile);
            string entityPrimaryKey = entity.__PrimaryKey.ToString();

            // Варианты описаний файлов с достаточными наборами свойств. но с пустыми базовыми URL-адресами.
            List<FileDescription> fileDescriptions = new List<FileDescription>
            {
                new FileDescription(null) { FileUploadKey = fileUploadKey, FileName = fileName },
                new FileDescription(null) { EntityTypeName = entityTypeName, EntityPropertyName = entityPropertyName, EntityPrimaryKey = entityPrimaryKey },
                new FileDescription(string.Empty) { FileUploadKey = fileUploadKey, FileName = fileName },
                new FileDescription(string.Empty) { EntityTypeName = entityTypeName, EntityPropertyName = entityPropertyName, EntityPrimaryKey = entityPrimaryKey },
            };

            foreach (var fileDescription in fileDescriptions)
            {
                Assert.Null(fileDescription.FileUrl);
                Assert.Null(fileDescription.PreviewUrl);
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/> не формируются,
        /// если заданный набор свойств недостаточен для описания файла.
        /// </summary>
        [Fact]
        public void TestUrlsByIncompleteProperties()
        {
            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";

            // Свойства достаточные для описания файла, который привязан к объекту данных
            // (тип и первичный ключ объекта данных, а так же имя файлового свойства в объекте данных).
            КлассСМножествомТипов entity = new КлассСМножествомТипов();
            string entityTypeName = entity.GetType().AssemblyQualifiedName;
            string entityPropertyName = nameof(entity.PropertyStormnetFile);
            string entityPrimaryKey = entity.__PrimaryKey.ToString();

            // Описания файлов с недостаточными наборами свойств.
            List<FileDescription> incompleteFileDescriptions = new List<FileDescription>
            {
                // Свойства не заданы вообще.
                new FileDescription(FileBaseUrl) { },

                // Заданы либо ключ, либо имя файла, но не оба свойства вместе.
                new FileDescription(FileBaseUrl) { FileUploadKey = fileUploadKey },
                new FileDescription(FileBaseUrl) { FileName = fileName },

                // Заданы либо имя типа объекта данных, либо имя файлового свойства в объекте данных, либо первичный ключ объекта данных,
                // либо любые парные комбинации этих свойств, но не все свойства вместе.
                new FileDescription(FileBaseUrl) { EntityTypeName = entityTypeName },
                new FileDescription(FileBaseUrl) { EntityPropertyName = entityPropertyName },
                new FileDescription(FileBaseUrl) { EntityPrimaryKey = entityPrimaryKey },
                new FileDescription(FileBaseUrl) { EntityTypeName = entityTypeName, EntityPropertyName = entityPropertyName },
                new FileDescription(FileBaseUrl) { EntityTypeName = entityTypeName, EntityPropertyName = entityPrimaryKey },
                new FileDescription(FileBaseUrl) { EntityPropertyName = entityPropertyName, EntityPrimaryKey = entityPrimaryKey },

                // Комбинации предыдущих неполностью заданных наборов свойств.
                new FileDescription(FileBaseUrl) { FileUploadKey = fileUploadKey, EntityTypeName = entityTypeName },
                new FileDescription(FileBaseUrl) { FileUploadKey = fileUploadKey, EntityPropertyName = entityPropertyName },
                new FileDescription(FileBaseUrl) { FileUploadKey = fileUploadKey, EntityPrimaryKey = entityPrimaryKey },
                new FileDescription(FileBaseUrl) { FileUploadKey = fileUploadKey, EntityTypeName = entityTypeName, EntityPropertyName = entityPropertyName },
                new FileDescription(FileBaseUrl) { FileUploadKey = fileUploadKey, EntityTypeName = entityTypeName, EntityPropertyName = entityPrimaryKey },
                new FileDescription(FileBaseUrl) { FileUploadKey = fileUploadKey, EntityPropertyName = entityPropertyName, EntityPrimaryKey = entityPrimaryKey },

                new FileDescription(FileBaseUrl) { FileName = fileName, EntityTypeName = entityTypeName },
                new FileDescription(FileBaseUrl) { FileName = fileName, EntityPropertyName = entityPropertyName },
                new FileDescription(FileBaseUrl) { FileName = fileName, EntityPrimaryKey = entityPrimaryKey },
                new FileDescription(FileBaseUrl) { FileName = fileName, EntityTypeName = entityTypeName, EntityPropertyName = entityPropertyName },
                new FileDescription(FileBaseUrl) { FileName = fileName, EntityTypeName = entityTypeName, EntityPropertyName = entityPrimaryKey },
                new FileDescription(FileBaseUrl) { FileName = fileName, EntityPropertyName = entityPropertyName, EntityPrimaryKey = entityPrimaryKey },
            };

            foreach (FileDescription fileDescription in incompleteFileDescriptions)
            {
                Assert.Null(fileDescription.FileUrl);
                Assert.Null(fileDescription.PreviewUrl);
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/>
        /// корректно формируются по заданным ключу загрузки и имени файла.
        /// </summary>
        [Fact]
        public void TestUrlsByUploadKeyAndFileNameProperties()
        {
            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";

            FileDescription fileDescription = new FileDescription(FileBaseUrl)
            {
                FileUploadKey = fileUploadKey,
                FileName = fileName,
            };

            Uri receivedFileUri = new Uri(fileDescription.FileUrl);
            Uri receivedPreviewUri = new Uri(fileDescription.PreviewUrl);

            string receivedFileBaseUrl = receivedFileUri.GetLeftPart(UriPartial.Path);
            string receivedPreviewBaseUrl = receivedPreviewUri.GetLeftPart(UriPartial.Path);

            NameValueCollection receivedFileQueryParameters = HttpUtility.ParseQueryString(receivedFileUri.Query);
            NameValueCollection receivedPreviewQueryParameters = HttpUtility.ParseQueryString(receivedPreviewUri.Query);

            // Assert.
            Assert.Equal(FileBaseUrl, receivedFileBaseUrl);
            Assert.Equal(FileBaseUrl, receivedPreviewBaseUrl);

            Assert.Equal(2, receivedFileQueryParameters.Count);
            Assert.Equal(fileUploadKey, receivedFileQueryParameters["fileUploadKey"]);
            Assert.Equal(fileName, receivedFileQueryParameters["fileName"]);

            Assert.Equal(3, receivedPreviewQueryParameters.Count);
            Assert.Equal(fileUploadKey, receivedPreviewQueryParameters["fileUploadKey"]);
            Assert.Equal(fileName, receivedPreviewQueryParameters["fileName"]);
            Assert.True(bool.Parse(receivedPreviewQueryParameters["getPreview"]));
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/>
        /// корректно формируются по заданному описанию объекта данных, с которым связан файл.
        /// </summary>
        [Fact]
        public void TestUrlsByEntityProperties()
        {
            // Свойства достаточные для описания файла, который привязан к объекту данных
            // (тип и первичный ключ объекта данных, а так же имя файлового свойства в объекте данных).
            КлассСМножествомТипов entity = new КлассСМножествомТипов();
            string entityTypeName = entity.GetType().AssemblyQualifiedName;
            string entityPropertyName = nameof(entity.PropertyStormnetFile);
            string entityPrimaryKey = entity.__PrimaryKey.ToString();

            FileDescription fileDescription = new FileDescription(FileBaseUrl)
            {
                EntityTypeName = entityTypeName,
                EntityPropertyName = entityPropertyName,
                EntityPrimaryKey = entityPrimaryKey,
            };

            Uri receivedFileUri = new Uri(fileDescription.FileUrl);
            Uri receivedPreviewUri = new Uri(fileDescription.PreviewUrl);

            string receivedFileBaseUrl = receivedFileUri.GetLeftPart(UriPartial.Path);
            string receivedPreviewBaseUrl = receivedPreviewUri.GetLeftPart(UriPartial.Path);

            NameValueCollection receivedFileQueryParameters = HttpUtility.ParseQueryString(receivedFileUri.Query);
            NameValueCollection receivedPreviewQueryParameters = HttpUtility.ParseQueryString(receivedPreviewUri.Query);

            // Assert.
            Assert.Equal(FileBaseUrl, receivedFileBaseUrl);
            Assert.Equal(FileBaseUrl, receivedPreviewBaseUrl);

            Assert.Equal(3, receivedFileQueryParameters.Count);
            Assert.Equal(entityTypeName, receivedFileQueryParameters["entityTypeName"]);
            Assert.Equal(entityPropertyName, receivedFileQueryParameters["entityPropertyName"]);
            Assert.Equal(entityPrimaryKey, receivedFileQueryParameters["entityPrimaryKey"]);

            Assert.Equal(4, receivedPreviewQueryParameters.Count);
            Assert.Equal(entityTypeName, receivedPreviewQueryParameters["entityTypeName"]);
            Assert.Equal(entityPropertyName, receivedPreviewQueryParameters["entityPropertyName"]);
            Assert.Equal(entityPrimaryKey, receivedPreviewQueryParameters["entityPrimaryKey"]);
            Assert.True(bool.Parse(receivedPreviewQueryParameters["getPreview"]));
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/>
        /// корректно формируются, если задан избыточный набор свойств
        /// (все свойства описания: и ключ загрузки с именем файла, и описание объекта данных, с которым связан файл).
        /// При этом соблюдается приоритет в пользу ключа загрузки и имени файла.
        /// </summary>
        [Fact]
        public void TestUrlsByAllProperties()
        {
            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";

            // Свойства достаточные для описания файла, который привязан к объекту данных
            // (тип и первичный ключ объекта данных, а так же имя файлового свойства в объекте данных).
            КлассСМножествомТипов entity = new КлассСМножествомТипов();
            string entityTypeName = entity.GetType().AssemblyQualifiedName;
            string entityPropertyName = nameof(entity.PropertyStormnetFile);
            string entityPrimaryKey = entity.__PrimaryKey.ToString();

            // Описание файла с избыточным набором свойств.
            FileDescription fileDescription = new FileDescription(FileBaseUrl)
            {
                FileUploadKey = fileUploadKey,
                FileName = fileName,
                EntityTypeName = entityTypeName,
                EntityPropertyName = entityPropertyName,
                EntityPrimaryKey = entityPrimaryKey,
            };

            Uri receivedFileUri = new Uri(fileDescription.FileUrl);
            Uri receivedPreviewUri = new Uri(fileDescription.PreviewUrl);

            string receivedFileBaseUrl = receivedFileUri.GetLeftPart(UriPartial.Path);
            string receivedPreviewBaseUrl = receivedPreviewUri.GetLeftPart(UriPartial.Path);

            NameValueCollection receivedFileQueryParameters = HttpUtility.ParseQueryString(receivedFileUri.Query);
            NameValueCollection receivedPreviewQueryParameters = HttpUtility.ParseQueryString(receivedPreviewUri.Query);

            // Assert.
            Assert.Equal(FileBaseUrl, receivedFileBaseUrl);
            Assert.Equal(FileBaseUrl, receivedPreviewBaseUrl);

            Assert.Equal(2, receivedFileQueryParameters.Count);
            Assert.Equal(fileUploadKey, receivedFileQueryParameters["fileUploadKey"]);
            Assert.Equal(fileName, receivedFileQueryParameters["fileName"]);

            Assert.Equal(3, receivedPreviewQueryParameters.Count);
            Assert.Equal(fileUploadKey, receivedPreviewQueryParameters["fileUploadKey"]);
            Assert.Equal(fileName, receivedPreviewQueryParameters["fileName"]);
            Assert.True(bool.Parse(receivedPreviewQueryParameters["getPreview"]));
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription"/>
        /// корректно сериализуется в JSON-строку.
        /// </summary>
        [Fact]
        public void TestSerializationToJson()
        {
            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";

            // Размер и тип файла.
            long fileSize = 1024;
            string fileMimeType = MimeMapping.GetMimeMapping(fileName);

            // Свойства достаточные для описания файла, который привязан к объекту данных
            // (тип и первичный ключ объекта данных, а так же имя файлового свойства в объекте данных).
            КлассСМножествомТипов entity = new КлассСМножествомТипов();
            string entityTypeName = entity.GetType().AssemblyQualifiedName;
            string entityPropertyName = nameof(entity.PropertyStormnetFile);
            string entityPrimaryKey = entity.__PrimaryKey.ToString();

            // Описание файла с избыточным набором свойств.
            FileDescription fileDescription = new FileDescription(FileBaseUrl)
            {
                FileUploadKey = fileUploadKey,
                FileName = fileName,
                FileSize = fileSize,
                FileMimeType = fileMimeType,
                EntityTypeName = entityTypeName,
                EntityPropertyName = entityPropertyName,
                EntityPrimaryKey = entityPrimaryKey,
            };

            FileDescriptionTestController.FileDescriptionGet = fileDescription;

            // Получаем сериализованные описания файлов несколькими способами (вручную, и из ответа, получаемого от сервера).
            List<string> serializedFileDescriptions = new List<string> { fileDescription.ToJson() };
            using (HttpConfiguration config = new HttpConfiguration())
            {
                config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                config.Routes.MapHttpRoute("File", "api/File", new { controller = "FileDescriptionTest" });

                using (HttpServer server = new HttpServer(config))
                {
                    using (HttpClient client = new HttpClient(server, false))
                    {
                        using (HttpResponseMessage response = client.GetAsync(FileBaseUrl).Result)
                        {
                            // Убедимся, что запрос завершился успешно.
                            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                            serializedFileDescriptions.Add(response.Content.ReadAsStringAsync().Result);
                        }
                    }
                }
            }

            // Assert.
            // Проверяем, что результаты сериализации, полученные разными способами - одинаковы и корректны.
            foreach (string serializedFileDescription in serializedFileDescriptions)
            {
                // Преобразуем сериализованное описание файла в словарь.
                Dictionary<string, object> deserializedFileDescription = JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedFileDescription);
                Assert.Equal(5, deserializedFileDescription.Keys.Count);
                Assert.Equal(fileName, deserializedFileDescription["fileName"]);
                Assert.Equal(fileSize, deserializedFileDescription["fileSize"]);
                Assert.Equal(fileMimeType, deserializedFileDescription["fileMimeType"]);
                Assert.Equal(fileDescription.FileUrl, deserializedFileDescription["fileUrl"]);
                Assert.Equal(fileDescription.PreviewUrl, deserializedFileDescription["previewUrl"]);
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription"/> с заданными ключом загрузки и именем файла
        /// корректно десериализуется из JSON-строки.
        /// </summary>
        [Fact]
        public void TestDeserializationFromJsonByUploadKeyAndFileNameProperties()
        {
            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";

            // Размер и тип файла.
            long fileSize = 1024;
            string fileMimeType = MimeMapping.GetMimeMapping(fileName);

            FileDescription fileDescription = new FileDescription(FileBaseUrl)
            {
                FileUploadKey = fileUploadKey,
                FileName = fileName,
                FileSize = fileSize,
                FileMimeType = fileMimeType,
            };

            string serializedFileDescription = fileDescription.ToJson();

            // Получаем десериализованные описания файла, полученные различными способами.
            List<FileDescription> deserializedFileDescriptions = new List<FileDescription>
            {
                FileDescription.FromJson(serializedFileDescription)
            };
            using (HttpConfiguration config = new HttpConfiguration())
            {
                config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                config.Routes.MapHttpRoute("File", "api/File", new { controller = "FileDescriptionTest" });

                using (HttpServer server = new HttpServer(config))
                {
                    using (HttpClient client = new HttpClient(server, false))
                    {
                        // Получаем десериализованное описание файла из тела POST-запроса.
                        using (HttpResponseMessage response = client.PostAsJsonStringAsync(FileBaseUrl, serializedFileDescription).Result)
                        {
                            // Убедимся, что запрос завершился успешно.
                            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                            deserializedFileDescriptions.Add(FileDescriptionTestController.FileDescriptionPost);
                        }

                        // Получаем десериализованное описание файла из URL PUT-запроса.
                        string putUrl = string.Format(
                            "http://localhost/api/File?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                            nameof(FileDescription.FileUploadKey),
                            fileDescription.FileUploadKey,
                            nameof(FileDescription.FileName),
                            fileDescription.FileName,
                            nameof(FileDescription.FileSize),
                            fileDescription.FileSize,
                            nameof(FileDescription.FileMimeType),
                            fileDescription.FileMimeType);
                        using (HttpResponseMessage response = client.SendAsync(new HttpRequestMessage(HttpMethod.Put, putUrl)).Result)
                        {
                            // Убедимся, что запрос завершился успешно.
                            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                            deserializedFileDescriptions.Add(FileDescriptionTestController.FileDescriptionPut);
                        }
                    }
                }
            }

            // Assert.
            // Проверяем, что результаты десериализации, полученные разными способами - одинаковы и корректны.
            foreach (FileDescription deserializedFileDescription in deserializedFileDescriptions)
            {
                Assert.Equal(fileUploadKey, deserializedFileDescription.FileUploadKey);
                Assert.Equal(fileName, deserializedFileDescription.FileName);
                Assert.Equal(fileSize, deserializedFileDescription.FileSize);
                Assert.Equal(fileMimeType, deserializedFileDescription.FileMimeType);
                Assert.Equal(fileDescription.FileUrl, deserializedFileDescription.FileUrl);
                Assert.Equal(fileDescription.PreviewUrl, deserializedFileDescription.PreviewUrl);
                Assert.Null(deserializedFileDescription.EntityTypeName);
                Assert.Null(deserializedFileDescription.EntityPropertyName);
                Assert.Null(deserializedFileDescription.EntityPrimaryKey);
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription"/> с заданными свойствами, описывающими объект данных,
        /// корректно десериализуется из JSON-строки.
        /// </summary>
        [Fact]
        public void TestDeserializationFromJsonByEntityProperties()
        {
            // Имя, размер и тип файла.
            string fileName = "readme.txt";
            long fileSize = 1024;
            string fileMimeType = MimeMapping.GetMimeMapping(fileName);

            // Свойства достаточные для описания файла, который привязан к объекту данных
            // (тип и первичный ключ объекта данных, а так же имя файлового свойства в объекте данных).
            КлассСМножествомТипов entity = new КлассСМножествомТипов();
            string entityTypeName = entity.GetType().AssemblyQualifiedName;
            string entityPropertyName = nameof(entity.PropertyStormnetFile);
            string entityPrimaryKey = entity.__PrimaryKey.ToString();

            // Описание файла с избыточным набором свойств.
            FileDescription fileDescription = new FileDescription(FileBaseUrl)
                                                  {
                                                      FileName = fileName,
                                                      FileSize = fileSize,
                                                      FileMimeType = fileMimeType,
                                                      EntityTypeName = entityTypeName,
                                                      EntityPropertyName = entityPropertyName,
                                                      EntityPrimaryKey = entityPrimaryKey
                                                  };

            string serializedFileDescription = fileDescription.ToJson();

            // Получаем десериализованные описания файла, полученные различными способами.
            List<FileDescription> deserializedFileDescriptions = new List<FileDescription>
            {
                FileDescription.FromJson(serializedFileDescription)
            };
            using (HttpConfiguration config = new HttpConfiguration())
            {
                config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                config.Routes.MapHttpRoute("File", "api/File", new { controller = "FileDescriptionTest" });

                using (HttpServer server = new HttpServer(config))
                {
                    using (HttpClient client = new HttpClient(server, false))
                    {
                        // Получаем десериализованное описание файла из тела POST-запроса.
                        using (HttpResponseMessage response = client.PostAsJsonStringAsync(FileBaseUrl, serializedFileDescription).Result)
                        {
                            // Убедимся, что запрос завершился успешно.
                            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                            deserializedFileDescriptions.Add(FileDescriptionTestController.FileDescriptionPost);
                        }

                        // Получаем десериализованное описание файла из URL PUT-запроса.
                        string putUrl = string.Format(
                            "http://localhost/api/File?{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}&{10}={11}",
                            nameof(FileDescription.FileName),
                            fileDescription.FileName,
                            nameof(FileDescription.FileSize),
                            fileDescription.FileSize,
                            nameof(FileDescription.FileMimeType),
                            fileDescription.FileMimeType,
                            nameof(FileDescription.EntityTypeName),
                            fileDescription.EntityTypeName,
                            nameof(FileDescription.EntityPropertyName),
                            fileDescription.EntityPropertyName,
                            nameof(FileDescription.EntityPrimaryKey),
                            fileDescription.EntityPrimaryKey);
                        using (HttpResponseMessage response = client.SendAsync(new HttpRequestMessage(HttpMethod.Put, putUrl)).Result)
                        {
                            // Убедимся, что запрос завершился успешно.
                            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                            deserializedFileDescriptions.Add(FileDescriptionTestController.FileDescriptionPut);
                        }
                    }
                }
            }

            // Проверяем, что результаты десериализации, полученные разными способами - одинаковы и корректны.
            foreach (FileDescription deserializedFileDescription in deserializedFileDescriptions)
            {
                Assert.Equal(fileName, deserializedFileDescription.FileName);
                Assert.Equal(fileSize, deserializedFileDescription.FileSize);
                Assert.Equal(fileMimeType, deserializedFileDescription.FileMimeType);
                Assert.Equal(entityTypeName, deserializedFileDescription.EntityTypeName);
                Assert.Equal(entityPropertyName, deserializedFileDescription.EntityPropertyName);
                Assert.Equal(entityPrimaryKey, deserializedFileDescription.EntityPrimaryKey);
                Assert.Equal(fileDescription.FileUrl, deserializedFileDescription.FileUrl);
                Assert.Equal(fileDescription.PreviewUrl, deserializedFileDescription.PreviewUrl);
                Assert.Null(deserializedFileDescription.FileUploadKey);
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что метод "FileDescription.FromFile"/>
        /// корректно осуществляет получение метаданных о файле по заданному пути.
        /// </summary>
        [Fact]
        public void TestFromFile()
        {
            // Arrange.
            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            FileInfo fileInfo = new FileInfo(_srcTextFilePath);
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileMimeType = MimeMapping.GetMimeMapping(fileInfo.Name);

            // Копируем тестовый файл в каталог, предназначенный для загруженных на сервер файлов.
            // Тем самым имитируем ситуацию как будто файл был ранее загружен на сервер через файловый контроллер.
            string uploadedFileDirectoryPath = CreateUploadsSubDirectory(fileUploadKey);
            string uploadedFilePath = Path.Combine(uploadedFileDirectoryPath, fileInfo.Name);
            File.Copy(_srcTextFilePath, uploadedFilePath, true);

            // Act.
            FileDescription fileDescription = new FileDescription(FileBaseUrl, uploadedFilePath);

            // Assert.
            Assert.Equal(fileUploadKey, fileDescription.FileUploadKey);
            Assert.Equal(fileInfo.Name, fileDescription.FileName);
            Assert.Equal(fileInfo.Length, fileDescription.FileSize);
            Assert.Equal(fileMimeType, fileDescription.FileMimeType);
            Assert.Null(fileDescription.EntityTypeName);
            Assert.Null(fileDescription.EntityPropertyName);
            Assert.Null(fileDescription.EntityPrimaryKey);
        }

        /// <summary>
        /// Осуществляет проверку того, что при инициализации <see cref="FileDescription"/> по заданному пути,
        /// выбрасывается исключение <see cref="FileNotFoundException"/>, в случае, если по заданному пути нет никакого файла.
        /// </summary>
        [Fact]
        public void TestFromFileNotFoundException()
        {
            // Arrange.
            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileName = "readme.txt";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", fileName);

            // Assert.
            Assert.Throws<FileNotFoundException>(() => new FileDescription(FileBaseUrl, filePath));
        }
    }
}
#endif
