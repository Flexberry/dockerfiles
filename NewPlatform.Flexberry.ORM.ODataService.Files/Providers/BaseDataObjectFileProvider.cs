namespace NewPlatform.Flexberry.ORM.ODataService.Files.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    /// <summary>
    /// Базовый провайдер для файловых свойств объектов данных.
    /// </summary>
    public abstract class BaseDataObjectFileProvider : IDataObjectFileProvider
    {
        /// <summary>
        /// Получает тип файловых свойств объектов данных, обрабатываемых провайдером.
        /// </summary>
        public abstract Type FilePropertyType { get; }

        /// <summary>
        /// Получает или задает путь к каталогу, в котором должны храниться файлы, загруженные на сервер при помощи провайдера.
        /// </summary>
        public string UploadsDirectoryPath { get; set; }

        /// <summary>
        /// Получат или задает базовую часть URL-а для ссылок на скачивание / удаление файлов.
        /// </summary>
        public string FileBaseUrl { get; set; }

        /// <summary>
        /// Сервис данных для операций с БД.
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// Конструктор класс <see cref="BaseDataObjectFileProvider"/> с параметрами.
        /// </summary>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        protected BaseDataObjectFileProvider(IDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService), "Contract assertion not met: dataService != null");
        }

        /// <summary>
        /// Осуществляет получение метаданных с описанием файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">
        /// Файловое свойство объекта данных, для которого требуется получить метаданные файла.
        /// </param>
        /// <returns>
        /// Метаданные с описанием файлового свойства объекта данных.
        /// </returns>
        public virtual FileDescription GetFileDescription(object fileProperty)
        {
            if (fileProperty == null)
            {
                return null;
            }

            return new FileDescription
            {
                FileBaseUrl = FileBaseUrl,
                FileName = GetFileName(fileProperty),
                FileSize = GetFileSize(fileProperty),
                FileMimeType = GetFileMimeType(fileProperty)
            };
        }

        /// <summary>
        /// Осуществляет получение метаданных с описанием файлового свойства объекта данных.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловое свойство.
        /// </param>
        /// <param name="dataObjectFilePropertyName">
        /// Имя файлового свойства в объекте данных.
        /// </param>
        /// <returns>
        /// Метаданные с описанием файлового свойства объекта данных.
        /// </returns>
        public virtual FileDescription GetFileDescription(DataObject dataObject, string dataObjectFilePropertyName)
        {
            FileDescription fileDescription = GetFileDescription(GetFileProperty(dataObject, dataObjectFilePropertyName));

            if (fileDescription != null)
            {
                fileDescription.EntityTypeName = dataObject.GetType().AssemblyQualifiedName;
                fileDescription.EntityPropertyName = dataObjectFilePropertyName;
                fileDescription.EntityPrimaryKey = dataObject.__PrimaryKey.ToString();
                fileDescription.FilePropertyType = FilePropertyType;
            }

            return fileDescription;
        }

        /// <summary>
        /// Осуществляет получение списка метаданных с описанием файловых свойств объекта данных, соответствующих типу <see cref="FilePropertyType"/>.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловые свойства.
        /// </param>
        /// <returns>
        /// Список метаданных с описанием файловых свойств объекта данных, соответствующих типу <see cref="FilePropertyType"/>.
        /// </returns>
        public virtual List<FileDescription> GetFileDescriptions(DataObject dataObject)
        {
            List<FileDescription> fileDescriptions = new List<FileDescription>();

            if (dataObject != null)
            {
                string[] filePropertiesNames = dataObject
                    .GetType()
                    .GetProperties()
                    .Where(x => x.PropertyType == FilePropertyType)
                    .Select(x => x.Name)
                    .ToArray();

                fileDescriptions.AddRange(
                    filePropertiesNames
                    .Select(filePropertyName => GetFileDescription(dataObject, filePropertyName))
                    .Where(x => x != null));
            }

            return fileDescriptions;
        }

        /// <summary>
        /// Осуществляет получение файлового свойства объекта данных.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловое свойство.
        /// </param>
        /// <param name="dataObjectFilePropertyName">
        /// Имя файлового свойства в объекте данных.
        /// </param>
        /// <returns>
        /// Значение файлового свойства объекта данных.
        /// </returns>
        public virtual object GetFileProperty(DataObject dataObject, string dataObjectFilePropertyName)
        {
            if (dataObject == null)
            {
                throw new ArgumentNullException(nameof(dataObject));
            }

            Type dataObjectType = dataObject.GetType();
            Type dataObjectFilePropertyType = Information.GetPropertyType(dataObjectType, dataObjectFilePropertyName);
            if (dataObjectFilePropertyType != FilePropertyType)
            {
                throw new Exception(string.Format(
                    "Wrong type of {0}.{1} property. Actual is {2}, but {3} is expected.",
                    nameof(dataObject),
                    dataObjectFilePropertyName,
                    dataObjectFilePropertyType.FullName,
                    FilePropertyType.FullName));
            }

            // Выполняем дочитку объекта данных, если в переданном объекте на загружено искомое файловое свойство.
            DataObject srcDataObject = dataObject;
            if (dataObject.GetStatus() != ObjectStatus.Created && !dataObject.CheckLoadedProperty(dataObjectFilePropertyName))
            {
                var view = new View { DefineClassType = dataObjectType, Name = "FilePropertyView" };
                view.AddProperty(dataObjectFilePropertyName);

                srcDataObject = (DataObject)Activator.CreateInstance(dataObjectType);
                srcDataObject.SetExistObjectPrimaryKey(dataObject.__PrimaryKey);

                _dataService.LoadObject(view, srcDataObject);
            }

            return Information.GetPropValueByName(srcDataObject, dataObjectFilePropertyName);
        }

        /// <summary>
        /// Осуществляет получение файлового свойства из файла, расположенного по заданному пути.
        /// </summary>
        /// <param name="filePath">
        /// Путь к файлу.
        /// </param>
        /// <returns>
        /// Значение файлового свойства объекта данных.
        /// </returns>
        public abstract object GetFileProperty(string filePath);

        /// <summary>
        /// Осуществляет получение файлового свойства объекта данных, по его метаданным.
        /// </summary>
        /// <remarks>
        /// При необходимости будет  вычитан объект данных.
        /// </remarks>
        /// <param name="fileDescription">
        /// Метаданные с описанием файлового свойства объекта данных.
        /// </param>
        /// <returns>
        /// Значение файлового свойства объекта данных.
        /// </returns>
        public virtual object GetFileProperty(FileDescription fileDescription)
        {
            if (fileDescription == null)
            {
                throw new ArgumentNullException(nameof(fileDescription));
            }

            // Если в описании запрашиваемого файла присутствует FileUploadKey,
            // значит файл был загружен на сервер, но еще не был привязан к объекту данных,
            // и нужно сформировать файловое свойство на основе загруженного файла.
            if (!string.IsNullOrEmpty(fileDescription.FileUploadKey))
            {
                string filePath = string.Concat(UploadsDirectoryPath, Path.DirectorySeparatorChar, fileDescription.FileUploadKey, Path.DirectorySeparatorChar, fileDescription.FileName);

                return GetFileProperty(filePath);
            }

            // Если в описании запрашиваемого файла присутствует первичный ключ объекта данных,
            // значит файл уже был связан с объектом данных, и нужно вычитать файловое свойство из него.
            if (!string.IsNullOrEmpty(fileDescription.EntityPrimaryKey))
            {
                Type dataObjectType = Type.GetType(fileDescription.EntityTypeName, true);

                var dataObject = (DataObject)Activator.CreateInstance(dataObjectType);
                dataObject.SetExistObjectPrimaryKey(fileDescription.EntityPrimaryKey);

                return GetFileProperty(dataObject, fileDescription.EntityPropertyName);
            }

            throw new Exception(
                string.Format(
                    "Both \"{0}\" properties: \"{1}\" & \"{2}\" are undefined.",
                nameof(fileDescription),
                nameof(FileDescription.FileUploadKey),
                nameof(FileDescription.EntityPrimaryKey)));
        }

        /// <summary>
        /// Осуществляет получение списка файловых свойств объекта данных, соответствующих типу <see cref="FilePropertyType"/>.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловые свойства.
        /// </param>
        /// <returns>
        /// Список файловых свойств объекта данных, соответствующих типу <see cref="FilePropertyType"/>.
        /// </returns>
        public virtual List<object> GetFileProperties(DataObject dataObject)
        {
            List<object> fileProperties = new List<object>();

            if (dataObject != null)
            {
                string[] filePropertiesNames = dataObject
                    .GetType()
                    .GetProperties()
                    .Where(x => x.PropertyType == FilePropertyType)
                    .Select(x => x.Name)
                    .ToArray();

                fileProperties.AddRange(
                    filePropertiesNames
                    .Select(x => GetFileProperty(dataObject, x))
                    .Where(x => x != null));
            }

            return fileProperties;
        }

        /// <summary>
        /// Осуществляет получение имени файла для файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">
        /// Файловому свойству объекта данных, для которого требуется получить имя файла.
        /// </param>
        /// <returns>
        /// Имя файла.
        /// </returns>
        public abstract string GetFileName(object fileProperty);

        /// <summary>
        /// Осуществляет получение имени файла для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловое свойство, для которого требуется получить имя.
        /// </param>
        /// <param name="dataObjectFilePropertyName">
        /// Имя файлового свойства в объекте данных.
        /// </param>
        /// <returns>
        /// Имя файла.
        /// </returns>
        public virtual string GetFileName(DataObject dataObject, string dataObjectFilePropertyName)
        {
            return GetFileName(GetFileProperty(dataObject, dataObjectFilePropertyName));
        }

        /// <summary>
        /// Осуществляет получение MIME-типа для файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">
        /// Файловому свойству объекта данных, для которого требуется получить MIME-тип.
        /// </param>
        /// <returns>
        /// MIME-тип файла, соответствующего заданному файловому свойству.
        /// </returns>
        public virtual string GetFileMimeType(object fileProperty)
        {
            return MimeMapping.GetMimeMapping(GetFileName(fileProperty));
        }

        /// <summary>
        /// Осуществляет получение MIME-типа для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловое свойство, для которого требуется получить MIME-тип.
        /// </param>
        /// <param name="dataObjectFilePropertyName">
        /// Имя файлового свойства в объекте данных.
        /// </param>
        /// <returns>
        /// MIME-тип файла, соответствующего заданному файловому свойству.
        /// </returns>
        public virtual string GetFileMimeType(DataObject dataObject, string dataObjectFilePropertyName)
        {
            return GetFileMimeType(GetFileProperty(dataObject, dataObjectFilePropertyName));
        }

        /// <summary>
        /// Осуществляет получение размера файла, связанного с объектом данных, в байтах.
        /// </summary>
        /// <param name="fileProperty">
        /// Файловое свойство объекта данных, для которого требуется получить размер файла.
        /// </param>
        /// <returns>
        /// Размер файла в байтах.
        /// </returns>
        public abstract long GetFileSize(object fileProperty);

        /// <summary>
        /// Осуществляет получение MIME-типа для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловое свойство, для которого требуется получить MIME-тип.
        /// </param>
        /// <param name="dataObjectFilePropertyName">
        /// Имя файлового свойства в объекте данных.
        /// </param>
        /// <returns>
        /// MIME-тип файла, соответствующего заданному файловому свойству.
        /// </returns>
        public virtual long GetFileSize(DataObject dataObject, string dataObjectFilePropertyName)
        {
            return GetFileSize(GetFileProperty(dataObject, dataObjectFilePropertyName));
        }

        /// <summary>
        /// Осуществляет получение потока данных для файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">
        /// Значение файлового свойства объекта данных, для которого требуется получить поток данных.
        /// </param>
        /// <returns>
        /// Поток данных.
        /// </returns>
        public abstract Stream GetFileStream(object fileProperty);

        /// <summary>
        /// Осуществляет получение потока данных для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловое свойство, для которого требуется получить поток данных.
        /// </param>
        /// <param name="dataObjectFilePropertyName">
        /// Имя файлового свойства в объекте данных.
        /// </param>
        /// <returns>
        /// Поток данных.
        /// </returns>
        public virtual Stream GetFileStream(DataObject dataObject, string dataObjectFilePropertyName)
        {
            return GetFileStream(GetFileProperty(dataObject, dataObjectFilePropertyName));
        }

        /// <summary>
        /// Осуществляет получение потока данных для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>
        /// При необходимости будет  вычитан объект данных.
        /// </remarks>
        /// <param name="fileDescription">
        /// Метаданные с описанием файлового свойства объекта данных, для которого требуется получить поток данных.
        /// </param>
        /// <returns>
        /// Поток данных.
        /// </returns>
        public virtual Stream GetFileStream(FileDescription fileDescription)
        {
            return GetFileStream(GetFileProperty(fileDescription));
        }

        /// <summary>
        /// Осуществляет удаление из файловой системы файла, соответствующего файловому свойству объекта данных.
        /// </summary>
        /// <param name="fileDescription">
        /// Метаданные удаляемого файла.
        /// </param>
        public virtual void RemoveFile(FileDescription fileDescription)
        {
            string fileDirectoryPath = string.Concat(UploadsDirectoryPath, Path.DirectorySeparatorChar, fileDescription.FileUploadKey);

            if (Directory.Exists(fileDirectoryPath))
            {
                Directory.Delete(fileDirectoryPath, true);
            }
        }

        /// <summary>
        /// Осуществляет удаление из файловой системы файла, соответствующего файловому свойству объекта данных.
        /// </summary>
        /// <param name="fileProperty">
        /// Значение файлового свойства объекта данных, для которого требуется выполнить удаление.
        /// </param>
        public virtual void RemoveFile(object fileProperty)
        {
            RemoveFile(GetFileDescription(fileProperty));
        }

        /// <summary>
        /// Осуществляет удаление из файловой системы файла, соответствующего файловому свойству объекта данных.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataObject">
        /// Объект данных, содержащий файловое свойство.
        /// </param>
        /// <param name="dataObjectFilePropertyName">
        /// Имя файлового свойства в объекте данных.
        /// </param>
        public virtual void RemoveFile(DataObject dataObject, string dataObjectFilePropertyName)
        {
            RemoveFile(GetFileProperty(dataObject, dataObjectFilePropertyName));
        }
    }
}
