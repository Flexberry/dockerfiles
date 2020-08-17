namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.Files.Providers;

    /// <summary>
    /// WebApi-контроллер, предназначенный для загрузки файлов на сервер и скачивания ранее загруженных файлов.
    /// </summary>
    public partial class FileController : BaseApiController
    {
        /// <summary>
        /// Зарегистрированные провайдеры файловых свойств для объектов данных.
        /// </summary>
        private static List<IDataObjectFileProvider> dataObjectFileProviders = new List<IDataObjectFileProvider>();

        /// <summary>
        /// Путь до каталога, предназначенного для хранения загруженных файлов.
        /// </summary>
        private static string uploadsDirectoryPath;

        /// <summary>
        /// URL, по которому доступен контроллер.
        /// </summary>
        private static string baseUrl;

        /// <summary>
        /// Получает или задает URL, по которому доступен контроллер.
        /// </summary>
        public static string RouteName { get; internal set; }

        /// <summary>
        /// Получает или задает путь до каталога, предназначенного для хранения загруженных файлов.
        /// </summary>
        /// <remarks>
        /// Инициализируется при назначении роута, соответствующего этому контролеру.
        /// </remarks>
        public static string UploadsDirectoryPath
        {
            get
            {
                return uploadsDirectoryPath;
            }

            internal set
            {
                uploadsDirectoryPath = value;
                dataObjectFileProviders.ForEach(x => x.UploadsDirectoryPath = uploadsDirectoryPath);
            }
        }

        /// <summary>
        /// Получает или задает URL, по которому доступен контроллер.
        /// </summary>
        public static string BaseUrl
        {
            get
            {
                return baseUrl;
            }

            set
            {
                baseUrl = value;
                dataObjectFileProviders.ForEach(x => x.FileBaseUrl = baseUrl);
            }
        }

        /// <summary>
        /// Получает путь к подкаталогу с заданным именем внутри каталога <see cref="UploadsDirectoryPath"/>.
        /// </summary>
        /// <param name="fileUploadKey">Ключ загрузки файла (используется как имя подкаталога).</param>
        /// <returns>Путь к подкаталогу.</returns>
        public static string GetFileUploadDirectoryPath(string fileUploadKey)
        {
            if (string.IsNullOrEmpty(fileUploadKey))
            {
                return null;
            }

            return string.Concat(UploadsDirectoryPath, Path.DirectorySeparatorChar, fileUploadKey);
        }

        /// <summary>
        /// Осуществляет создание подкаталога с заданным именем в каталоге <see cref="UploadsDirectoryPath"/>.
        /// </summary>
        /// <param name="fileUploadKey">Ключ загрузки файла (используется как имя создаваемого подкаталога).</param>
        /// <returns>Путь к созданному подкаталогу.</returns>
        public static string CreateFileUploadDirectory(string fileUploadKey)
        {
            string directoryPath = GetFileUploadDirectoryPath(fileUploadKey);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        /// <summary>
        /// Осуществляет удаление подкаталога с заданным именем из каталога <see cref="UploadsDirectoryPath"/>.
        /// </summary>
        /// <param name="fileUploadKey">Ключ загрузки файла (используется как имя удаляемого подкаталога).</param>
        public static void RemoveFileUploadDirectory(string fileUploadKey)
        {
            string directoryPath = GetFileUploadDirectoryPath(fileUploadKey);
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        /// <summary>
        /// Осуществляет удаление подкаталогов, соответствующих загруженным файлам, из каталога <see cref="UploadsDirectoryPath"/>.
        /// </summary>
        /// <param name="removingFileDescriptions">
        /// Метаданные файлов, которые требуется удалить.
        /// </param>
        public static void RemoveFileUploadDirectories(List<FileDescription> removingFileDescriptions)
        {
            if (removingFileDescriptions == null)
                return;

            dataObjectFileProviders.ForEach(
                fp =>
                {
                    var selectedDescriptions = removingFileDescriptions
                        .Where(x => x != null && fp.FilePropertyType == x.FilePropertyType)
                        .ToList();
                    selectedDescriptions.ForEach(fp.RemoveFile);
                });
        }

        /// <summary>
        /// Проверяет имеется ли в контроллере зарегистрированный провайдер для заданного типа файловых свойств объектов данных.
        /// </summary>
        /// <param name="dataObjectFilePropertyType">
        /// Тип файловыйх свойств объектов данных.
        /// </param>
        /// <returns>
        /// Флаг: <c>true</c>, если для файловых свойств указанного типа зарегистрирован провайдер, <c>false</c> в противном случае.
        /// </returns>
        public static bool HasDataObjectFileProvider(Type dataObjectFilePropertyType)
        {
            return dataObjectFileProviders.Any(x => x.FilePropertyType == dataObjectFilePropertyType);
        }

        /// <summary>
        /// Осуществляет регистрацию провайдера файловых свойств для объекта данных.
        /// </summary>
        /// <param name="dataObjectFileProvider">
        /// Провайдер файловых свойств для объекта данных.
        /// </param>
        public static void RegisterDataObjectFileProvider(IDataObjectFileProvider dataObjectFileProvider)
        {
            IDataObjectFileProvider alreadyRegisteredProvider = dataObjectFileProviders.FirstOrDefault(x => x.FilePropertyType == dataObjectFileProvider.FilePropertyType);
            if (alreadyRegisteredProvider != null)
            {
                dataObjectFileProviders.Remove(alreadyRegisteredProvider);
            }

            dataObjectFileProvider.FileBaseUrl = BaseUrl;
            dataObjectFileProviders.Add(dataObjectFileProvider);
        }

        /// <summary>
        /// Получает зарегистрированный провайдер для заданного типа файловых свойств объектов данных.
        /// </summary>
        /// <param name="dataObjectFilePropertyType">
        /// Тип файловыйх свойств объектов данных.
        /// </param>
        /// <returns>
        /// Провайдер файловых свойств объектов данных.
        /// </returns>
        public static IDataObjectFileProvider GetDataObjectFileProvider(Type dataObjectFilePropertyType)
        {
            return dataObjectFileProviders.FirstOrDefault(x => x.FilePropertyType == dataObjectFilePropertyType);
        }

        /// <summary>
        /// Осуществляет получение списка метаданных с описанием файловых свойств объекта данных,
        /// соответствующих всем типам файловых свойств, для которых есть зарегистрированные провайдеры.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловые свойства.
        /// </param>
        /// <param name="excludedFilePropertiesTypes">
        /// Список типов файловых свойств объекта данных, для которых не требуется получение метаданных.
        /// </param>
        /// <returns>
        /// Список метаданных с описанием файловых свойств объекта данных,
        /// соответствующих всем типам файловых свойств, для которых есть зарегистрированные провайдеры.
        /// </returns>
        public static List<FileDescription> GetDataObjectFileDescriptions(DataObject dataObject, List<Type> excludedFilePropertiesTypes = null)
        {
            List<FileDescription> fileDescriptions = new List<FileDescription>();

            if (dataObject != null)
            {
                excludedFilePropertiesTypes = excludedFilePropertiesTypes ?? new List<Type>();
                List<IDataObjectFileProvider> includedDataObjectFileProviders = dataObjectFileProviders
                    .Where(x => !excludedFilePropertiesTypes.Contains(x.FilePropertyType))
                    .ToList();

                foreach (IDataObjectFileProvider dataObjectFileProvider in includedDataObjectFileProviders)
                {
                    fileDescriptions.AddRange(dataObjectFileProvider.GetFileDescriptions(dataObject));
                }
            }

            return fileDescriptions;
        }
    }
}