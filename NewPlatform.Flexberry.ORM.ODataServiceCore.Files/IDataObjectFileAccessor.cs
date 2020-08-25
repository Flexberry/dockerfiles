namespace NewPlatform.Flexberry.ORM.ODataService.Files
{
    using System;
    using System.Collections.Generic;
    using NewPlatform.Flexberry.ORM.ODataService.Files.Providers;

    /// <summary>
    /// Provides an abstraction for access to the file properties of a data object.
    /// </summary>
    public interface IDataObjectFileAccessor
    {
        /// <summary>
        /// Список зарегистрированных провайдеров файловых свойств объектов данных.
        /// </summary>
        List<IDataObjectFileProvider> DataObjectFileProviders { get; }

        /// <summary>
        /// The file route URL.
        /// </summary>
        string RouteUrl { get; }

        /// <summary>
        /// Осуществляет создание подкаталога с заданным именем в каталоге файлового хранилища.
        /// </summary>
        /// <param name="fileUploadKey">Ключ загрузки файла (используется как имя создаваемого подкаталога).</param>
        /// <returns>Путь к созданному подкаталогу.</returns>
        string CreateFileUploadDirectory(string fileUploadKey);

        /// <summary>
        /// Получает зарегистрированный провайдер для заданного типа файловых свойств объектов данных.
        /// </summary>
        /// <param name="dataObjectFilePropertyType">Тип файловыйх свойств объектов данных.</param>
        /// <returns>Провайдер файловых свойств объектов данных.</returns>
        IDataObjectFileProvider GetDataObjectFileProvider(Type dataObjectFilePropertyType);

        /// <summary>
        /// Проверяет, имеется ли зарегистрированный провайдер для заданного типа файловых свойств объектов данных.
        /// </summary>
        /// <param name="dataObjectFilePropertyType">Тип файловыйх свойств объектов данных.</param>
        /// <returns>Флаг: <c>true</c>, если для файловых свойств указанного типа зарегистрирован провайдер, <c>false</c> в противном случае.</returns>
        bool HasDataObjectFileProvider(Type dataObjectFilePropertyType);

        /// <summary>
        /// Осуществляет удаление подкаталога с заданным именем из каталога файлового хранилища.
        /// </summary>
        /// <param name="fileUploadKey">Ключ загрузки файла (используется как имя удаляемого подкаталога).</param>
        void RemoveFileUploadDirectory(string fileUploadKey);

        /// <summary>
        /// Осуществляет удаление подкаталогов, соответствующих загруженным файлам, из каталога файлового хранилища.
        /// </summary>
        /// <param name="removingFileDescriptions">Метаданные файлов, которые требуется удалить.</param>
        void RemoveFileUploadDirectories(List<FileDescription> removingFileDescriptions);
    }
}
