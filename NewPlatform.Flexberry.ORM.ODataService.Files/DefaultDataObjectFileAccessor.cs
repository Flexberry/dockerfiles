namespace NewPlatform.Flexberry.ORM.ODataService.Files
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    using NewPlatform.Flexberry.ORM.ODataService.Files.Providers;

    /// <summary>
    /// The default <see cref="IDataObjectFileAccessor"/> implementation.
    /// </summary>
    public class DefaultDataObjectFileAccessor : IDataObjectFileAccessor
    {
        /// <summary>
        /// The file uploads directory path.
        /// </summary>
        private readonly string uploadsDirectoryPath;

        /// <inheritdoc/>
        public List<IDataObjectFileProvider> DataObjectFileProviders { get; } = new List<IDataObjectFileProvider>();

        /// <inheritdoc/>
        public Uri BaseUri { get; }

        /// <inheritdoc/>
        public string RouteUrl { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDataObjectFileAccessor"/> class.
        /// </summary>
        /// <param name="baseUrl">The file service base URL.</param>
        /// <param name="routeUrl">The file route URL.</param>
        /// <param name="uploadsDirectoryPath">The file uploads directory path.</param>
        /// <param name="dataObjectFileProviders">The user defined set of data object file providers.</param>
        public DefaultDataObjectFileAccessor(Uri baseUrl, string routeUrl, string uploadsDirectoryPath, List<IDataObjectFileProvider> dataObjectFileProviders = null)
        {
            RouteUrl = routeUrl;

            BaseUri = new Uri(baseUrl, RouteUrl);

            this.uploadsDirectoryPath = uploadsDirectoryPath;

            // Создаем провайдеры файловых свойств объектов данных по умолчанию.
            List<IDataObjectFileProvider> providers = new List<IDataObjectFileProvider>
            {
                new DataObjectFileProvider(),
                new DataObjectWebFileProvider(),
            };

            // Добавляем пользовательские провайдеры файловых свойств объектов данных.
            providers.AddRange(dataObjectFileProviders ?? new List<IDataObjectFileProvider>());

            // Регистрируем провайдеры файловых свойств объектов данных.
            foreach (IDataObjectFileProvider provider in providers)
            {
                RegisterDataObjectFileProvider(provider);
            }
        }

        /// <inheritdoc/>
        public string CreateFileUploadDirectory(string fileUploadKey)
        {
            string directoryPath = GetFileUploadDirectoryPath(fileUploadKey);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        /// <inheritdoc/>
        public IDataObjectFileProvider GetDataObjectFileProvider(Type dataObjectFilePropertyType)
        {
            return DataObjectFileProviders.FirstOrDefault(x => x.FilePropertyType == dataObjectFilePropertyType);
        }

        /// <summary>
        /// Получает путь к подкаталогу с заданным именем внутри каталога файлового хранилища.
        /// </summary>
        /// <param name="fileUploadKey">Ключ загрузки файла (используется как имя подкаталога).</param>
        /// <returns>Путь к подкаталогу.</returns>
        public string GetFileUploadDirectoryPath(string fileUploadKey)
        {
            if (string.IsNullOrEmpty(fileUploadKey))
            {
                return null;
            }

            return Path.Combine(uploadsDirectoryPath, fileUploadKey);
        }

        /// <inheritdoc/>
        public bool HasDataObjectFileProvider(Type dataObjectFilePropertyType)
        {
            return DataObjectFileProviders.Any(x => x.FilePropertyType == dataObjectFilePropertyType);
        }

        /// <inheritdoc/>
        public void RemoveFileUploadDirectory(string fileUploadKey)
        {
            string directoryPath = GetFileUploadDirectoryPath(fileUploadKey);
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        /// <inheritdoc/>
        public void RemoveFileUploadDirectories(List<FileDescription> removingFileDescriptions)
        {
            if (removingFileDescriptions == null)
                return;

            DataObjectFileProviders.ForEach(
                fp =>
                {
                    var selectedDescriptions = removingFileDescriptions
                        .Where(x => x != null && fp.FilePropertyType == x.FilePropertyType)
                        .ToList();
                    selectedDescriptions.ForEach(fp.RemoveFile);
                });
        }

        /// <inheritdoc />
        public List<FileDescription> GetDataObjectFileDescriptions(IDataService dataService, DataObject dataObject, List<Type> excludedFilePropertiesTypes = null)
        {
            List<FileDescription> fileDescriptions = new List<FileDescription>();

            if (dataObject != null)
            {
                List<IDataObjectFileProvider> includedDataObjectFileProviders = DataObjectFileProviders
                    .Where(x => excludedFilePropertiesTypes == null || !excludedFilePropertiesTypes.Contains(x.FilePropertyType))
                    .ToList();

                foreach (IDataObjectFileProvider dataObjectFileProvider in includedDataObjectFileProviders)
                {
                    fileDescriptions.AddRange(dataObjectFileProvider.GetFileDescriptions(dataService, dataObject));
                }
            }

            return fileDescriptions;
        }

        /// <summary>
        /// Осуществляет регистрацию провайдера файловых свойств для объекта данных.
        /// </summary>
        /// <param name="dataObjectFileProvider">
        /// Провайдер файловых свойств для объекта данных.
        /// </param>
        private void RegisterDataObjectFileProvider(IDataObjectFileProvider dataObjectFileProvider)
        {
            IDataObjectFileProvider alreadyRegisteredProvider = DataObjectFileProviders.FirstOrDefault(x => x.FilePropertyType == dataObjectFileProvider.FilePropertyType);
            if (alreadyRegisteredProvider != null)
            {
                DataObjectFileProviders.Remove(alreadyRegisteredProvider);
            }

            dataObjectFileProvider.FileBaseUrl = BaseUri.AbsoluteUri;
            dataObjectFileProvider.UploadsDirectoryPath = uploadsDirectoryPath;

            DataObjectFileProviders.Add(dataObjectFileProvider);
        }
    }
}
