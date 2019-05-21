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

    /// <summary>
    /// Класс, содержащий модульные тесты для метаданных, описывающих файловые свойства объектов данных.
    /// </summary>
    //[TestFixture]
    public class FileDescriptionTest
    {
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
        /// Осуществляет очистку результатов работы тестов.
        /// </summary>
        //[TestFixtureTearDown]
        public void Cleanup()
        {
            FileDescriptionTestController.FileDescriptionGet = null;
            FileDescriptionTestController.FileDescriptionPost = null;
            FileDescriptionTestController.FileDescriptionPut = null;
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/> не формируются
        /// если не задано свойтсво <see cref="FileDescription.FileBaseUrl"/>.
        /// </summary>
        //[Test]
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
                //Assert.AreEqual(null, fileDescription.FileUrl);
                //Assert.AreEqual(null, fileDescription.PreviewUrl);
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/> не формируются,
        /// если заданный набор свойств недостаточен для описания файла.
        /// </summary>
        //[Test]
        public void TestUrlsByIncompleteProperties()
        {
            string fileBaseUrl = "http://localhost/api/File";

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
                new FileDescription(fileBaseUrl) { },

                // Заданы либо ключ, либо имя файла, но не оба свойства вместе.
                new FileDescription(fileBaseUrl) { FileUploadKey = fileUploadKey },
                new FileDescription(fileBaseUrl) { FileName = fileName },

                // Заданы либо имя типа объекта данных, либо имя файлового свойства в объекте данных, либо первичный ключ объекта данных,
                // либо любые парные комбинации этих свойств, но не все свойства вместе.
                new FileDescription(fileBaseUrl) { EntityTypeName = entityTypeName },
                new FileDescription(fileBaseUrl) { EntityPropertyName = entityPropertyName },
                new FileDescription(fileBaseUrl) { EntityPrimaryKey = entityPrimaryKey },
                new FileDescription(fileBaseUrl) { EntityTypeName = entityTypeName, EntityPropertyName = entityPropertyName },
                new FileDescription(fileBaseUrl) { EntityTypeName = entityTypeName, EntityPropertyName = entityPrimaryKey },
                new FileDescription(fileBaseUrl) { EntityPropertyName = entityPropertyName, EntityPrimaryKey = entityPrimaryKey },

                // Комбинации предыдущих неполностью заданных наборов свойств.
                new FileDescription(fileBaseUrl) { FileUploadKey = fileUploadKey, EntityTypeName = entityTypeName },
                new FileDescription(fileBaseUrl) { FileUploadKey = fileUploadKey, EntityPropertyName = entityPropertyName },
                new FileDescription(fileBaseUrl) { FileUploadKey = fileUploadKey, EntityPrimaryKey = entityPrimaryKey },
                new FileDescription(fileBaseUrl) { FileUploadKey = fileUploadKey, EntityTypeName = entityTypeName, EntityPropertyName = entityPropertyName },
                new FileDescription(fileBaseUrl) { FileUploadKey = fileUploadKey, EntityTypeName = entityTypeName, EntityPropertyName = entityPrimaryKey },
                new FileDescription(fileBaseUrl) { FileUploadKey = fileUploadKey, EntityPropertyName = entityPropertyName, EntityPrimaryKey = entityPrimaryKey },

                new FileDescription(fileBaseUrl) { FileName = fileName, EntityTypeName = entityTypeName },
                new FileDescription(fileBaseUrl) { FileName = fileName, EntityPropertyName = entityPropertyName },
                new FileDescription(fileBaseUrl) { FileName = fileName, EntityPrimaryKey = entityPrimaryKey },
                new FileDescription(fileBaseUrl) { FileName = fileName, EntityTypeName = entityTypeName, EntityPropertyName = entityPropertyName },
                new FileDescription(fileBaseUrl) { FileName = fileName, EntityTypeName = entityTypeName, EntityPropertyName = entityPrimaryKey },
                new FileDescription(fileBaseUrl) { FileName = fileName, EntityPropertyName = entityPropertyName, EntityPrimaryKey = entityPrimaryKey }
            };

            foreach (FileDescription fileDescription in incompleteFileDescriptions)
            {
                //Assert.AreEqual(null, fileDescription.FileUrl);
                //Assert.AreEqual(null, fileDescription.PreviewUrl);
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/>
        /// корректно формируются по заданным ключу загрузки и имени файла.
        /// </summary>
        //[Test]
        public void TestUrlsByUploadKeyAndFileNameProperties()
        {
            string fileBaseUrl = "http://localhost/api/File";

            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";

            FileDescription fileDescription = new FileDescription(fileBaseUrl)
                                                  {
                                                      FileUploadKey = fileUploadKey,
                                                      FileName = fileName
                                                  };

            Uri receivedFileUri = new Uri(fileDescription.FileUrl);
            Uri receivedPreviewUri = new Uri(fileDescription.PreviewUrl);

            string receivedFileBaseUrl = receivedFileUri.GetLeftPart(UriPartial.Path);
            string receivedPreviewBaseUrl = receivedPreviewUri.GetLeftPart(UriPartial.Path);

            NameValueCollection receivedFileQueryParameters = HttpUtility.ParseQueryString(receivedFileUri.Query);
            NameValueCollection receivedPreviewQueryParameters = HttpUtility.ParseQueryString(receivedPreviewUri.Query);
            /*
            Assert.AreEqual(fileBaseUrl, receivedFileBaseUrl);
            Assert.AreEqual(fileBaseUrl, receivedPreviewBaseUrl);

            Assert.AreEqual(2, receivedFileQueryParameters.Count);
            Assert.AreEqual(fileUploadKey, receivedFileQueryParameters["fileUploadKey"]);
            Assert.AreEqual(fileName, receivedFileQueryParameters["fileName"]);

            Assert.AreEqual(3, receivedPreviewQueryParameters.Count);
            Assert.AreEqual(fileUploadKey, receivedPreviewQueryParameters["fileUploadKey"]);
            Assert.AreEqual(fileName, receivedPreviewQueryParameters["fileName"]);
            Assert.AreEqual(true, bool.Parse(receivedPreviewQueryParameters["getPreview"]));
            */
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/>
        /// корректно формируются по заданному описанию объекта данных, с которым связан файл.
        /// </summary>
        //[Test]
        public void TestUrlsByEntityProperties()
        {
            string fileBaseUrl = "http://localhost/api/File";

            // Свойства достаточные для описания файла, который привязан к объекту данных
            // (тип и первичный ключ объекта данных, а так же имя файлового свойства в объекте данных).
            КлассСМножествомТипов entity = new КлассСМножествомТипов();
            string entityTypeName = entity.GetType().AssemblyQualifiedName;
            string entityPropertyName = nameof(entity.PropertyStormnetFile);
            string entityPrimaryKey = entity.__PrimaryKey.ToString();

            FileDescription fileDescription = new FileDescription(fileBaseUrl)
                                                  {
                                                      EntityTypeName = entityTypeName,
                                                      EntityPropertyName = entityPropertyName,
                                                      EntityPrimaryKey = entityPrimaryKey
                                                  };

            Uri receivedFileUri = new Uri(fileDescription.FileUrl);
            Uri receivedPreviewUri = new Uri(fileDescription.PreviewUrl);

            string receivedFileBaseUrl = receivedFileUri.GetLeftPart(UriPartial.Path);
            string receivedPreviewBaseUrl = receivedPreviewUri.GetLeftPart(UriPartial.Path);

            NameValueCollection receivedFileQueryParameters = HttpUtility.ParseQueryString(receivedFileUri.Query);
            NameValueCollection receivedPreviewQueryParameters = HttpUtility.ParseQueryString(receivedPreviewUri.Query);
            /*
            Assert.AreEqual(fileBaseUrl, receivedFileBaseUrl);
            Assert.AreEqual(fileBaseUrl, receivedPreviewBaseUrl);

            Assert.AreEqual(3, receivedFileQueryParameters.Count);
            Assert.AreEqual(entityTypeName, receivedFileQueryParameters["entityTypeName"]);
            Assert.AreEqual(entityPropertyName, receivedFileQueryParameters["entityPropertyName"]);
            Assert.AreEqual(entityPrimaryKey, receivedFileQueryParameters["entityPrimaryKey"]);

            Assert.AreEqual(4, receivedPreviewQueryParameters.Count);
            Assert.AreEqual(entityTypeName, receivedPreviewQueryParameters["entityTypeName"]);
            Assert.AreEqual(entityPropertyName, receivedPreviewQueryParameters["entityPropertyName"]);
            Assert.AreEqual(entityPrimaryKey, receivedPreviewQueryParameters["entityPrimaryKey"]);
            Assert.AreEqual(true, bool.Parse(receivedPreviewQueryParameters["getPreview"]));
            */
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription.FileUrl"/> и <see cref="FileDescription.PreviewUrl"/>
        /// корректно формируются, если задан избыточный набор свойств
        /// (все свойства описания: и ключ загрузки с именем файла, и описание объекта данных, с которым связан файл).
        /// При этом соблюдается приоритет в пользу ключа загрузки и имени файла.
        /// </summary>
        //[Test]
        public void TestUrlsByAllProperties()
        {
            string fileBaseUrl = "http://localhost/api/File";

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
            FileDescription fileDescription = new FileDescription(fileBaseUrl)
                                                  {
                                                      FileUploadKey = fileUploadKey,
                                                      FileName = fileName,
                                                      EntityTypeName = entityTypeName,
                                                      EntityPropertyName = entityPropertyName,
                                                      EntityPrimaryKey = entityPrimaryKey
                                                  };

            Uri receivedFileUri = new Uri(fileDescription.FileUrl);
            Uri receivedPreviewUri = new Uri(fileDescription.PreviewUrl);

            string receivedFileBaseUrl = receivedFileUri.GetLeftPart(UriPartial.Path);
            string receivedPreviewBaseUrl = receivedPreviewUri.GetLeftPart(UriPartial.Path);

            NameValueCollection receivedFileQueryParameters = HttpUtility.ParseQueryString(receivedFileUri.Query);
            NameValueCollection receivedPreviewQueryParameters = HttpUtility.ParseQueryString(receivedPreviewUri.Query);
            /*
            Assert.AreEqual(fileBaseUrl, receivedFileBaseUrl);
            Assert.AreEqual(fileBaseUrl, receivedPreviewBaseUrl);

            Assert.AreEqual(2, receivedFileQueryParameters.Count);
            Assert.AreEqual(fileUploadKey, receivedFileQueryParameters["fileUploadKey"]);
            Assert.AreEqual(fileName, receivedFileQueryParameters["fileName"]);

            Assert.AreEqual(3, receivedPreviewQueryParameters.Count);
            Assert.AreEqual(fileUploadKey, receivedPreviewQueryParameters["fileUploadKey"]);
            Assert.AreEqual(fileName, receivedPreviewQueryParameters["fileName"]);
            Assert.AreEqual(true, bool.Parse(receivedPreviewQueryParameters["getPreview"]));
            */
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription"/>
        /// корректно сериализуется в JSON-строку.
        /// </summary>
        //[Test]
        public void TestSerializationToJson()
        {
            string fileBaseUrl = "http://localhost/api/File";

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
            FileDescription fileDescription = new FileDescription(fileBaseUrl)
                                                  {
                                                      FileUploadKey = fileUploadKey,
                                                      FileName = fileName,
                                                      FileSize = fileSize,
                                                      FileMimeType = fileMimeType,
                                                      EntityTypeName = entityTypeName,
                                                      EntityPropertyName = entityPropertyName,
                                                      EntityPrimaryKey = entityPrimaryKey
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
                        using (HttpResponseMessage response = client.GetAsync("http://localhost/api/File").Result)
                        {
                            // Убедимся, что запрос завершился успешно.
                            //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                            serializedFileDescriptions.Add(response.Content.ReadAsStringAsync().Result);
                        }
                    }
                }
            }

            // Проверяем, что результаты сериализации, полученные разными способами - одинаковы и корректны.
            foreach (string serializedFileDescription in serializedFileDescriptions)
            {
                // Преобразуем сериализованное описание файла в словарь.
                Dictionary<string, object> deserializedFileDescription = JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedFileDescription);
                /*
                Assert.AreEqual(5, deserializedFileDescription.Keys.Count);
                Assert.AreEqual(fileName, deserializedFileDescription["fileName"]);
                Assert.AreEqual((int)fileSize, (int)deserializedFileDescription["fileSize"]);
                Assert.AreEqual(fileMimeType, deserializedFileDescription["fileMimeType"]);
                Assert.AreEqual(fileDescription.FileUrl, deserializedFileDescription["fileUrl"]);
                Assert.AreEqual(fileDescription.PreviewUrl, deserializedFileDescription["previewUrl"]);
                */
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription"/> с заданными ключом загрузки и именем файла
        /// корректно десериализуется из JSON-строки.
        /// </summary>
        //[Test]
        public void TestDeserializationFromJsonByUploadKeyAndFileNameProperties()
        {
            string fileBaseUrl = "http://localhost/api/File";

            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";

            // Размер и тип файла.
            long fileSize = 1024;
            string fileMimeType = MimeMapping.GetMimeMapping(fileName);

            FileDescription fileDescription = new FileDescription(fileBaseUrl)
                                                  {
                                                      FileUploadKey = fileUploadKey,
                                                      FileName = fileName,
                                                      FileSize = fileSize,
                                                      FileMimeType = fileMimeType
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
                        using (HttpResponseMessage response = client.PostAsJsonStringAsync("http://localhost/api/File", serializedFileDescription).Result)
                        {
                            // Убедимся, что запрос завершился успешно.
                            //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

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
                            //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                            deserializedFileDescriptions.Add(FileDescriptionTestController.FileDescriptionPut);
                        }
                    }
                }
            }

            // Проверяем, что результаты десериализации, полученные разными способами - одинаковы и корректны.
            foreach (FileDescription deserializedFileDescription in deserializedFileDescriptions)
            {
                /*
                Assert.AreEqual(fileUploadKey, deserializedFileDescription.FileUploadKey);
                Assert.AreEqual(fileName, deserializedFileDescription.FileName);
                Assert.AreEqual(fileSize, deserializedFileDescription.FileSize);
                Assert.AreEqual(fileMimeType, deserializedFileDescription.FileMimeType);
                Assert.AreEqual(fileDescription.FileUrl, deserializedFileDescription.FileUrl);
                Assert.AreEqual(fileDescription.PreviewUrl, deserializedFileDescription.PreviewUrl);
                Assert.AreEqual(null, deserializedFileDescription.EntityTypeName);
                Assert.AreEqual(null, deserializedFileDescription.EntityPropertyName);
                Assert.AreEqual(null, deserializedFileDescription.EntityPrimaryKey);
                */
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что <see cref="FileDescription"/> с заданными свойствами, описывающими объект данных,
        /// корректно десериализуется из JSON-строки.
        /// </summary>
        //[Test]
        public void TestDeserializationFromJsonByEntityProperties()
        {
            string fileBaseUrl = "http://localhost/api/File";

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
            FileDescription fileDescription = new FileDescription(fileBaseUrl)
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
                        using (HttpResponseMessage response = client.PostAsJsonStringAsync("http://localhost/api/File", serializedFileDescription).Result)
                        {
                            // Убедимся, что запрос завершился успешно.
                            //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

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
                            //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                            deserializedFileDescriptions.Add(FileDescriptionTestController.FileDescriptionPut);
                        }
                    }
                }
            }

            // Проверяем, что результаты десериализации, полученные разными способами - одинаковы и корректны.
            foreach (FileDescription deserializedFileDescription in deserializedFileDescriptions)
            {
                /*
                Assert.AreEqual(fileName, deserializedFileDescription.FileName);
                Assert.AreEqual(fileSize, deserializedFileDescription.FileSize);
                Assert.AreEqual(fileMimeType, deserializedFileDescription.FileMimeType);
                Assert.AreEqual(entityTypeName, deserializedFileDescription.EntityTypeName);
                Assert.AreEqual(entityPropertyName, deserializedFileDescription.EntityPropertyName);
                Assert.AreEqual(entityPrimaryKey, deserializedFileDescription.EntityPrimaryKey);
                Assert.AreEqual(fileDescription.FileUrl, deserializedFileDescription.FileUrl);
                Assert.AreEqual(fileDescription.PreviewUrl, deserializedFileDescription.PreviewUrl);
                Assert.AreEqual(null, deserializedFileDescription.FileUploadKey);
                */
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что метод "FileDescription.FromFile"/>
        /// корректно осуществляет получение метаданных о файле по заданному пути.
        /// </summary>
        //[Test]
        public void TestFromFile()
        {
            string fileBaseUrl = "http://localhost/api/File";

            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";
            string filePath = string.Format("C:\\Uploads\\{0}\\{1}", fileUploadKey, fileName);
            string fileMimeType = MimeMapping.GetMimeMapping(fileName);
            long fileSize = 1024;

            //using (ShimsContext.Create())
            {
                //System.IO.Fakes.ShimFile.ExistsString = (path) => { return true; };
                //System.IO.Fakes.ShimFileInfo.AllInstances.NameGet = (@this) => { return fileName; };
                //System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (@this) => { return fileSize; };

                FileDescription fileDescription = new FileDescription(fileBaseUrl, filePath);
                /*
                Assert.AreEqual(fileUploadKey, fileDescription.FileUploadKey);
                Assert.AreEqual(fileName, fileDescription.FileName);
                Assert.AreEqual(fileSize, fileDescription.FileSize);
                Assert.AreEqual(fileMimeType, fileDescription.FileMimeType);
                Assert.AreEqual(null, fileDescription.EntityTypeName);
                Assert.AreEqual(null, fileDescription.EntityPropertyName);
                Assert.AreEqual(null, fileDescription.EntityPrimaryKey);
                */
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что при инициализации <see cref="FileDescription"/> по заданному пути,
        /// выбрасывается исключение <see cref="FileNotFoundException"/>, в случае, если по заданному пути нет никакого файла.
        /// </summary>
        //[Test]
        //[ExpectedException(typeof(FileNotFoundException))]
        public void TestFromFileNotFoundException()
        {
            string fileBaseUrl = "http://localhost/api/File";

            // Свойства достаточные для описания файла, который не привязан к объекту данных (ключ загрузки и имя файла).
            string fileUploadKey = Guid.NewGuid().ToString("D");
            string fileName = "readme.txt";
            string filePath = string.Format("C:\\Uploads\\{0}\\{1}", fileUploadKey, fileName);

            //using (ShimsContext.Create())
            {
                //System.IO.Fakes.ShimFile.ExistsString = (path) => { return false; };

                FileDescription fileDescription = new FileDescription(fileBaseUrl, filePath);
            }
        }
    }
}
