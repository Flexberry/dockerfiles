namespace NewPlatform.Flexberry.ORM.ODataService.Files.Providers
{
    using System;
    using System.IO;

    using ICSSoft.STORMNET.Business;

    using WebFile = ICSSoft.STORMNET.UserDataTypes.WebFile;

    /// <summary>
    /// Провайдер для свойства объектов данных типа <see cref="WebFile"/>.
    /// </summary>
    public class DataObjectWebFileProvider : BaseDataObjectFileProvider
    {
        /// <summary>
        /// Конструктор класса <see cref="DataObjectWebFileProvider"/> с параметрами.
        /// </summary>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        public DataObjectWebFileProvider(IDataService dataService)
            : base(dataService)
        {
        }

        /// <summary>
        /// Тип файловых свойств объектов данных, обрабатываемых провайдером (<see cref="WebFile"/>).
        /// </summary>
        public override Type FilePropertyType => typeof(WebFile);

        /// <summary>
        /// Осуществляет получение метаданных с описанием файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">
        /// Файловое свойство объекта данных, для которого требуется получить метаданные файла.
        /// </param>
        /// <returns>
        /// Метаданные с описанием файлового свойства объекта данных.
        /// </returns>
        public override FileDescription GetFileDescription(object fileProperty)
        {
            FileDescription fileDescription = base.GetFileDescription(fileProperty);
            if (fileDescription != null)
            {
                fileDescription.FileUrl = (fileProperty as WebFile)?.Url;
            }

            return fileDescription;
        }

        /// <summary>
        /// Осуществляет получение файлового свойства из файла, расположенного по заданному пути.
        /// </summary>
        /// <param name="filePath">
        /// Путь к файлу.
        /// </param>
        /// <returns>
        /// Значение файлового свойства объекта данных, соответствующее типу <see cref="WebFile"/>.
        /// </returns>
        public override object GetFileProperty(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("File \"{0}\" not found.", filePath));
            }

            FileDescription fileDescription = new FileDescription(FileBaseUrl, filePath);
            WebFile fileProperty = new WebFile
                                       {
                                           Name = fileDescription.FileName,
                                           Size = (int)fileDescription.FileSize,
                                           Url = fileDescription.FileUrl
                                       };

            return fileProperty;
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
        public override string GetFileName(object fileProperty)
        {
            return (fileProperty as WebFile)?.Name;
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
        public override long GetFileSize(object fileProperty)
        {
            return (fileProperty as WebFile)?.Size ?? 0;
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
        public override Stream GetFileStream(object fileProperty)
        {
            FileDescription fileDescription = new FileDescription(FileBaseUrl)
                                                  {
                                                      FileUrl = (fileProperty as WebFile)?.Url
                                                  };

            string filePath = string.Concat(UploadsDirectoryPath, Path.DirectorySeparatorChar, fileDescription.FileUploadKey, Path.DirectorySeparatorChar, fileDescription.FileName);
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("File \"{0}\" not found.", filePath));
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
    }
}