namespace NewPlatform.Flexberry.ORM.ODataService.Files.Providers
{
    using System;
    using System.IO;

    using WebFile = ICSSoft.STORMNET.UserDataTypes.WebFile;

    /// <summary>
    /// Провайдер для свойства объектов данных типа <see cref="WebFile"/>.
    /// </summary>
    public class DataObjectWebFileProvider : BaseDataObjectFileProvider
    {
        /// <summary>
        /// Конструктор класса <see cref="DataObjectWebFileProvider"/>.
        /// </summary>
        public DataObjectWebFileProvider() : base()
        {
        }

        /// <summary>
        /// Тип файловых свойств объектов данных, обрабатываемых провайдером (<see cref="WebFile"/>).
        /// </summary>
        public override Type FilePropertyType => typeof(WebFile);

        /// <inheritdoc/>
        public override FileDescription GetFileDescription(object fileProperty)
        {
            FileDescription fileDescription = base.GetFileDescription(fileProperty);
            if (fileDescription != null)
            {
                fileDescription.FileUrl = (fileProperty as WebFile)?.Url;
            }

            return fileDescription;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override string GetFileName(object fileProperty)
        {
            return (fileProperty as WebFile)?.Name;
        }

        /// <inheritdoc/>
        public override long GetFileSize(object fileProperty)
        {
            return (fileProperty as WebFile)?.Size ?? 0;
        }

        /// <inheritdoc/>
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
