namespace NewPlatform.Flexberry.ORM.ODataService.Files.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    /// <summary>
    /// Базовый провайдер для файловых свойств объектов данных.
    /// </summary>
    public abstract class BaseDataObjectFileProvider : IDataObjectFileProvider
    {
        /// <inheritdoc />
        public abstract Type FilePropertyType { get; }

        /// <inheritdoc />
        public string UploadsDirectoryPath { get; set; }

        /// <inheritdoc />
        public string FileBaseUrl { get; set; }

        /// <summary>
        /// Конструктор классa <see cref="BaseDataObjectFileProvider"/>.
        /// </summary>
        protected BaseDataObjectFileProvider()
        {
        }

        /// <inheritdoc />
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
                FileMimeType = GetFileMimeType(fileProperty),
            };
        }

        /// <inheritdoc />
        public virtual FileDescription GetFileDescription(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName)
        {
            FileDescription fileDescription = GetFileDescription(GetFileProperty(dataService, dataObject, dataObjectFilePropertyName));

            if (fileDescription != null)
            {
                fileDescription.EntityTypeName = dataObject.GetType().AssemblyQualifiedName;
                fileDescription.EntityPropertyName = dataObjectFilePropertyName;
                fileDescription.EntityPrimaryKey = dataObject.__PrimaryKey.ToString();
                fileDescription.FilePropertyType = FilePropertyType;
            }

            return fileDescription;
        }

        /// <inheritdoc />
        public virtual List<FileDescription> GetFileDescriptions(IDataService dataService, DataObject dataObject)
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
                    .Select(filePropertyName => GetFileDescription(dataService, dataObject, filePropertyName))
                    .Where(x => x != null));
            }

            return fileDescriptions;
        }

        /// <inheritdoc />
        public virtual object GetFileProperty(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName)
        {
            if (dataObject == null)
            {
                throw new ArgumentNullException(nameof(dataObject));
            }

            Type dataObjectType = dataObject.GetType();
            Type dataObjectFilePropertyType = Information.GetPropertyType(dataObjectType, dataObjectFilePropertyName);
            if (dataObjectFilePropertyType != FilePropertyType)
            {
                throw new Exception(
                    string.Format(
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

                dataService.LoadObject(view, srcDataObject);
            }

            return Information.GetPropValueByName(srcDataObject, dataObjectFilePropertyName);
        }

        /// <inheritdoc />
        public abstract object GetFileProperty(string filePath);

        /// <inheritdoc />
        public virtual object GetFileProperty(IDataService dataService, FileDescription fileDescription)
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

                return GetFileProperty(dataService, dataObject, fileDescription.EntityPropertyName);
            }

            throw new Exception(
                string.Format(
                    "Both \"{0}\" properties: \"{1}\" & \"{2}\" are undefined.",
                    nameof(fileDescription),
                    nameof(FileDescription.FileUploadKey),
                    nameof(FileDescription.EntityPrimaryKey)));
        }

        /// <inheritdoc />
        public virtual List<object> GetFileProperties(IDataService dataService, DataObject dataObject)
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
                    .Select(x => GetFileProperty(dataService, dataObject, x))
                    .Where(x => x != null));
            }

            return fileProperties;
        }

        /// <inheritdoc />
        public abstract string GetFileName(object fileProperty);

        /// <inheritdoc />
        public virtual string GetFileName(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName)
        {
            return GetFileName(GetFileProperty(dataService, dataObject, dataObjectFilePropertyName));
        }

        /// <inheritdoc />
        public virtual string GetFileMimeType(object fileProperty)
        {
            return MimeTypeUtils.GetFileMimeType(GetFileName(fileProperty));
        }

        /// <inheritdoc />
        public virtual string GetFileMimeType(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName)
        {
            return GetFileMimeType(GetFileProperty(dataService, dataObject, dataObjectFilePropertyName));
        }

        /// <inheritdoc />
        public abstract long GetFileSize(object fileProperty);

        /// <inheritdoc />
        public virtual long GetFileSize(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName)
        {
            return GetFileSize(GetFileProperty(dataService, dataObject, dataObjectFilePropertyName));
        }

        /// <inheritdoc />
        public abstract Stream GetFileStream(object fileProperty);

        /// <inheritdoc />
        public virtual Stream GetFileStream(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName)
        {
            return GetFileStream(GetFileProperty(dataService, dataObject, dataObjectFilePropertyName));
        }

        /// <inheritdoc />
        public virtual Stream GetFileStream(IDataService dataService, FileDescription fileDescription)
        {
            return GetFileStream(GetFileProperty(dataService, fileDescription));
        }

        /// <inheritdoc />
        public virtual void RemoveFile(FileDescription fileDescription)
        {
            string fileDirectoryPath = string.Concat(UploadsDirectoryPath, Path.DirectorySeparatorChar, fileDescription.FileUploadKey);

            if (Directory.Exists(fileDirectoryPath))
            {
                Directory.Delete(fileDirectoryPath, true);
            }
        }

        /// <inheritdoc />
        public virtual void RemoveFile(object fileProperty)
        {
            RemoveFile(GetFileDescription(fileProperty));
        }

        /// <inheritdoc />
        public virtual void RemoveFile(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName)
        {
            RemoveFile(GetFileProperty(dataService, dataObject, dataObjectFilePropertyName));
        }
    }
}
