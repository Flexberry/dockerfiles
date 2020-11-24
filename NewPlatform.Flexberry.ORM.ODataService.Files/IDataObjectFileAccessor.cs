namespace NewPlatform.Flexberry.ORM.ODataService.Files
{
    using System;
    using System.Collections.Generic;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

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
        /// The file controller base URL.
        /// </summary>
        Uri BaseUri { get; }

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

        /// <summary>
        /// Осуществляет получение списка метаданных с описанием файловых свойств объекта данных,
        /// соответствующих всем типам файловых свойств, для которых есть зарегистрированные провайдеры.
        /// </summary>
        /// <remarks>
        /// При необходимости будет произведена дочитка объекта данных.
        /// </remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловые свойства.</param>
        /// <param name="excludedFilePropertiesTypes">Список типов файловых свойств объекта данных, для которых не требуется получение метаданных.</param>
        /// <returns>
        /// Список метаданных с описанием файловых свойств объекта данных,
        /// соответствующих всем типам файловых свойств, для которых есть зарегистрированные провайдеры.
        /// </returns>
        List<FileDescription> GetDataObjectFileDescriptions(IDataService dataService, DataObject dataObject, List<Type> excludedFilePropertiesTypes = null);
    }
}
