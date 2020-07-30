namespace NewPlatform.Flexberry.ORM.ODataService.Files.Providers
{
    using System;
    using System.IO;

    using File = ICSSoft.STORMNET.FileType.File;

    /// <summary>
    /// Провайдер для файловых свойств объектов данных типа <see cref="File"/>.
    /// </summary>
    public class DataObjectFileProvider : BaseDataObjectFileProvider
    {
        /// <summary>
        /// Конструктор класса <see cref="DataObjectFileProvider"/>.
        /// </summary>
        public DataObjectFileProvider() : base()
        {
        }

        /// <summary>
        /// Тип файловых свойств объектов данных, обрабатываемых провайдером (<see cref="File"/>).
        /// </summary>
        public override Type FilePropertyType => typeof(File);

        /// <inheritdoc/>
        public override object GetFileProperty(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File \"{filePath}\" not found.");
            }

            var fileInfo = new FileInfo(filePath);
            File fileProperty = new File { Name = fileInfo.Name, Size = fileInfo.Length };
            fileProperty.FromNormalToZip(filePath);

            return fileProperty;
        }

        /// <inheritdoc/>
        public override string GetFileName(object fileProperty)
        {
            return (fileProperty as File)?.Name;
        }

        /// <inheritdoc/>
        public override long GetFileSize(object fileProperty)
        {
            return (fileProperty as File)?.Size ?? 0;
        }

        /// <inheritdoc/>
        public override Stream GetFileStream(object fileProperty)
        {
            return (fileProperty as File)?.GetUnzippedFile();
        }
    }
}
