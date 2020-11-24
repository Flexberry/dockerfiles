namespace NewPlatform.Flexberry.ORM.ODataService.Files
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web;

    using Newtonsoft.Json;

    /// <summary>
    /// Метаданные файлов, связываемых с объектами данных.
    /// </summary>
    [JsonObject]
    public class FileDescription
    {
        /// <summary>
        /// Свойства, помеченные атрибутом <see cref="JsonPropertyAttribute"/>,
        /// соответствующие заданным значениям <see cref="JsonPropertyAttribute.PropertyName"/>.
        /// </summary>
        private Dictionary<string, PropertyInfo> _jsonProperties;

        /// <summary>
        /// Имена свойств класса, помеченных атрибутом <see cref="JsonPropertyAttribute"/>,
        /// соответствующие заданным значениям <see cref="JsonPropertyAttribute.PropertyName"/>.
        /// </summary>
        private Dictionary<string, string> _jsonPropertiesNames;

        /// <summary>
        /// Получает или задает базовую часть URL-а, по которому будут осуществляться запросы на скачивание/удаление файла.
        /// </summary>
        [JsonIgnore]
        public string FileBaseUrl { get; set; }

        /// <summary>
        /// Тип файлового свойства.
        /// </summary>
        [JsonIgnore]
        public Type FilePropertyType { get; set; }

        /// <summary>
        /// Получает или задает URL, по которому будут осуществляться запросы на скачивание/удаление файла.
        /// </summary>
        [JsonProperty(PropertyName = "fileUrl")]
        public string FileUrl
        {
            get
            {
                if (!(string.IsNullOrEmpty(FileBaseUrl)
                    || string.IsNullOrEmpty(FileUploadKey)
                    || string.IsNullOrEmpty(FileName)))
                {
                    return string.Format(
                        "{0}?{1}={2}&{3}={4}",
                        FileBaseUrl,
                        _jsonPropertiesNames[nameof(FileUploadKey)],
                        FileUploadKey,
                        _jsonPropertiesNames[nameof(FileName)],
                        FileName);
                }

                if (!(string.IsNullOrEmpty(FileBaseUrl)
                    || string.IsNullOrEmpty(EntityTypeName)
                    || string.IsNullOrEmpty(EntityPropertyName)
                    || string.IsNullOrEmpty(EntityPrimaryKey)))
                {
                    return string.Format(
                        "{0}?{1}={2}&{3}={4}&{5}={6}",
                        FileBaseUrl,
                        _jsonPropertiesNames[nameof(EntityTypeName)],
                        EntityTypeName,
                        _jsonPropertiesNames[nameof(EntityPropertyName)],
                        EntityPropertyName,
                        _jsonPropertiesNames[nameof(EntityPrimaryKey)],
                        EntityPrimaryKey);
                }

                return null;
            }

            set
            {
                Uri fileUri = new Uri(value ?? string.Empty);
                NameValueCollection queryParameters = HttpUtility.ParseQueryString(fileUri.Query);

                if (queryParameters.Keys.Count == 0)
                {
                    return;
                }

                // Инициализируем свойства метаданных из параметров запроса в полученном URL.
                // например из URL вида 'http://localhost:8080/File?fileUploadKey=12345&fileName=readme.txt
                // соответствующими значениями будут проинициализированы свойства FileUploadKey и FileName.
                foreach (KeyValuePair<string, PropertyInfo> pair in _jsonProperties)
                {
                    string propertyName = pair.Key;
                    PropertyInfo property = pair.Value;

                    string serializedPropertyValue = queryParameters.Get(propertyName);
                    if (!string.IsNullOrEmpty(serializedPropertyValue))
                    {
                        var propertyValue = Convert.ChangeType(serializedPropertyValue, property.PropertyType);
                        property.SetValue(this, propertyValue, null);
                    }
                }

                if (string.IsNullOrEmpty(FileBaseUrl))
                {
                    FileBaseUrl = fileUri.GetLeftPart(UriPartial.Path);
                }
            }
        }

        /// <summary>
        /// Получает или задает URL, по которому будут осуществляться запросы на скачивание preview-изображения файла (в виде Base64String).
        /// </summary>
        [JsonProperty(PropertyName = "previewUrl")]
        public string PreviewUrl
        {
            get
            {
                string fileUrl = FileUrl;

                return string.IsNullOrEmpty(fileUrl)
                   ? null
                   : string.Format("{0}{1}getPreview=true", fileUrl, fileUrl.Contains("?") ? "&" : "?");
            }

            set
            {
                // Сеттер нужен только для того, чтобы метаданные без ошибок проходили
                // автоматическую десериализацию из JSON-строки и/или параметров запроса через атрибут [FromUri].
            }
        }

        /// <summary>
        /// Получает или задает имя файла.
        /// </summary>
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Получает или задает размер файла в байтах.
        /// </summary>
        [JsonProperty(PropertyName = "fileSize")]
        public long FileSize { get; set; }

        /// <summary>
        /// Получает или задает MIME-тип, соответствующий файлу.
        /// </summary>
        [JsonProperty(PropertyName = "fileMimeType")]
        public string FileMimeType { get; set; }

        /// <summary>
        /// Получает или задает ключ файла, по которому он был загружен в файловую систему.
        /// </summary>
        /// <remarks>
        /// Этим ключом именуется каталог, в который был загружен файл.
        /// </remarks>
        [JsonIgnore]
        [JsonProperty(PropertyName = "fileUploadKey")]
        public string FileUploadKey { get; set; }

        /// <summary>
        /// Получает или задает имя типа объекта данных, с которым связан файл.
        /// </summary>
        [JsonIgnore]
        [JsonProperty(PropertyName = "entityTypeName")]
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Получает или задет имя свойства в объекте данных, с которым связан файл.
        /// </summary>
        [JsonIgnore]
        [JsonProperty(PropertyName = "entityPropertyName")]
        public string EntityPropertyName { get; set; }

        /// <summary>
        /// Получает или задает первичный ключ объекта данных, с которым связан файл.
        /// </summary>
        [JsonIgnore]
        [JsonProperty(PropertyName = "entityPrimaryKey")]
        public string EntityPrimaryKey { get; set; }

        /// <summary>
        /// Инициализирует метаданные файла, связываемого с объектами данных.
        /// </summary>
        public FileDescription()
        {
            PropertyInfo[] jsonProperties = GetType()
                .GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Length > 0)
                .ToArray();

            _jsonProperties = jsonProperties.ToDictionary(
                x => (x.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault() as JsonPropertyAttribute)?.PropertyName ?? x.Name,
                x => x);

            _jsonPropertiesNames = jsonProperties.ToDictionary(
                x => x.Name,
                x => (x.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault() as JsonPropertyAttribute)?.PropertyName ?? x.Name);
        }

        /// <summary>
        /// Инициализирует метаданные файла, связываемого с объектами данных.
        /// </summary>
        /// <param name="fileBaseUrl">Базовая часть URL-а, по которому будут осуществляться запросы на скачивание/удаление файла.</param>
        public FileDescription(string fileBaseUrl)
            : this()
        {
            FileBaseUrl = fileBaseUrl;
        }

        /// <summary>
        /// Инициализирует метаданные файла, связываемого с объектами данных.
        /// </summary>
        /// <param name="fileBaseUrl">Базовая часть URL-а, по которому будут осуществляться запросы на скачивание/удаление файла.</param>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Метаданные о файле.</returns>
        /// <exception cref="FileNotFoundException">Выбрасывается, если по заданному пути отсутствует какой-либо файл.</exception>
        public FileDescription(string fileBaseUrl, string filePath)
            : this()
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("File \"{0}\" not found.", filePath));
            }

            string fileUploadKey = Path.GetDirectoryName(filePath)?.Split(Path.DirectorySeparatorChar).LastOrDefault();
            FileInfo fileInfo = new FileInfo(filePath);

            FileBaseUrl = fileBaseUrl;
            FileUploadKey = fileUploadKey;
            FileName = fileInfo.Name;
            FileSize = fileInfo.Length;
            FileMimeType = MimeTypeUtils.GetFileMimeType(fileInfo.Name);
        }

        /// <summary>
        /// Осуществляет десериализацию метаданных о файле из JSON-строки.
        /// </summary>
        /// <param name="jsonFileDescription">Строка, содержащая метаданные о файле в формате JSON.</param>
        /// <returns>
        /// Десериализованные метаданные о файле.
        /// </returns>
        public static FileDescription FromJson(string jsonFileDescription)
        {
            if (string.IsNullOrEmpty(jsonFileDescription))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<FileDescription>(jsonFileDescription);
        }

        /// <summary>
        /// Осуществляет сериализацию метаданных о файле в JSON-строку.
        /// </summary>
        /// <returns>
        /// Сериализованные метаданные о файле.
        /// </returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}