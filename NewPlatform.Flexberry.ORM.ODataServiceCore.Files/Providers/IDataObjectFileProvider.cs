namespace NewPlatform.Flexberry.ORM.ODataService.Files.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    /// <summary>
    /// Интерфейс для провайдеров файловых свойств объектов данных.
    /// </summary>
    public interface IDataObjectFileProvider
    {
        /// <summary>
        /// Получат или задает базовую часть URL-а для ссылок на скачивание / удаление файлов.
        /// </summary>
        string FileBaseUrl { get; set; }

        /// <summary>
        /// Получает тип файловых свойств объектов данных, обрабатываемых провайдером.
        /// </summary>
        Type FilePropertyType { get; }

        /// <summary>
        /// Получает или задает путь к каталогу, в котором должны храниться файлы, загруженные на сервер при помощи провайдера.
        /// </summary>
        string UploadsDirectoryPath { get; set; }

        /// <summary>
        /// Осуществляет получение метаданных с описанием файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">Файловое свойство объекта данных, для которого требуется получить метаданные файла.</param>
        /// <returns>Метаданные с описанием файлового свойства объекта данных.</returns>
        FileDescription GetFileDescription(object fileProperty);

        /// <summary>
        /// Осуществляет получение метаданных с описанием файлового свойства объекта данных.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловое свойство.</param>
        /// <param name="dataObjectFilePropertyName">Имя файлового свойства в объекте данных</param>
        /// <returns>Метаданные с описанием файлового свойства объекта данных.</returns>
        FileDescription GetFileDescription(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName);

        /// <summary>
        /// Осуществляет получение списка метаданных с описанием файловых свойств объекта данных, соответствующих типу <see cref="FilePropertyType"/>.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловые свойства.</param>
        /// <returns>Список метаданных с описанием файловых свойств объекта данных, соответствующих типу <see cref="FilePropertyType"/>.</returns>
        List<FileDescription> GetFileDescriptions(IDataService dataService, DataObject dataObject);

        /// <summary>
        /// Осуществляет получение MIME-типа для файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">Файловое свойство объекта данных, для которого требуется получить MIME-тип.</param>
        /// <returns>MIME-тип файла, соответствующего заданному файловому свойству.</returns>
        string GetFileMimeType(object fileProperty);

        /// <summary>
        /// Осуществляет получение MIME-типа для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловое свойство, для которого требуется получить MIME-тип.</param>
        /// <param name="dataObjectFilePropertyName">Имя файлового свойства в объекте данных.</param>
        /// <returns>MIME-тип файла, соответствующего заданному файловому свойству.</returns>
        string GetFileMimeType(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName);

        /// <summary>
        /// Осуществляет получение имени файла для файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">Файловое свойство объекта данных, для которого требуется получить имя файла.</param>
        /// <returns>Имя файла.</returns>
        string GetFileName(object fileProperty);

        /// <summary>
        /// Осуществляет получение имени файла для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловое свойство, для которого требуется получить имя.</param>
        /// <param name="dataObjectFilePropertyName">Имя файлового свойства в объекте данных.</param>
        /// <returns>Имя файла.</returns>
        string GetFileName(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName);

        /// <summary>
        /// Осуществляет получение файлового свойства из файла, расположенного по заданному пути.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Значение файлового свойства объекта данных.</returns>
        object GetFileProperty(string filePath);

        /// <summary>
        /// Осуществляет получение файлового свойства объекта данных, по его метаданным.
        /// </summary>
        /// <remarks>При необходимости будет  вычитан объект данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="fileDescription">Метаданные с описанием файлового свойства объекта данных.</param>
        /// <returns>Значение файлового свойства объекта данных.</returns>
        object GetFileProperty(IDataService dataService, FileDescription fileDescription);

        /// <summary>    
        /// Осуществляет получение файлового свойства объекта данных.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловое свойство.</param>
        /// <param name="dataObjectFilePropertyName">Имя файлового свойства в объекте данных.</param>
        /// <returns>Значение файлового свойства объекта данных.</returns>
        object GetFileProperty(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName);

        /// <summary>
        /// Осуществляет получение списка файловых свойств объекта данных, соответствующих типу <see cref="FilePropertyType"/>.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловые свойства.</param>
        /// <returns>Список файловых свойств объекта данных, соответствующих типу <see cref="FilePropertyType"/>.</returns>
        List<object> GetFileProperties(IDataService dataService, DataObject dataObject);

        /// <summary>
        /// Осуществляет получение размера файла, связанного с объектом данных, в байтах.
        /// </summary>
        /// <param name="fileProperty">Значение файлового свойства объекта данных, для которого требуется получить размер файла.</param>
        /// <returns>Размер файла в байтах.</returns>
        long GetFileSize(object fileProperty);

        /// <summary>    
        /// Осуществляет получение размера файла, связанного с объектом данных, в байтах.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловое свойство.</param>
        /// <param name="dataObjectFilePropertyName">Имя файлового свойства в объекте данных, для которого требуется получить размер файла.</param>
        /// <returns>Размер файла в байтах.</returns>
        long GetFileSize(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName);

        /// <summary>
        /// Осуществляет получение потока данных для файлового свойства объекта данных.
        /// </summary>
        /// <param name="fileProperty">Значение файлового свойства объекта данных, для которого требуется получить поток данных.</param>
        /// <returns>Поток данных.</returns>
        Stream GetFileStream(object fileProperty);

        /// <summary>
        /// Осуществляет получение потока данных для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>При необходимости будет  вычитан объект данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="fileDescription">Метаданные с описанием файлового свойства объекта данных, для которого требуется получить поток данных.</param>
        /// <returns>Поток данных.</returns>
        Stream GetFileStream(IDataService dataService, FileDescription fileDescription);

        /// <summary>
        /// Осуществляет получение потока данных для файлового свойства объекта данных.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловое свойство, для которого требуется получить поток данных.</param>
        /// <param name="dataObjectFilePropertyName">Имя файлового свойства в объекте данных.</param>
        /// <returns>Поток данных.</returns>
        Stream GetFileStream(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName);

        /// <summary>
        /// Осуществляет удаление из файловой системы файла, соответствующего файловому свойству объекта данных.
        /// </summary>
        /// <param name="fileDescription">Метаданные удаляемого файла.</param>
        void RemoveFile(FileDescription fileDescription);

        /// <summary>
        /// Осуществляет удаление из файловой системы файла, соответствующего файловому свойству объекта данных.
        /// </summary>
        /// <param name="fileProperty">Значение файлового свойства объекта данных, для которого требуется выполнить удаление.</param>
        void RemoveFile(object fileProperty);

        /// <summary>
        /// Осуществляет удаление из файловой системы файла, соответствующего файловому свойству объекта данных.
        /// </summary>
        /// <remarks>При необходимости будет произведена дочитка объекта данных.</remarks>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <param name="dataObject">Объект данных, содержащий файловое свойство.</param>
        /// <param name="dataObjectFilePropertyName">Имя файлового свойства в объекте данных.</param>
        void RemoveFile(IDataService dataService, DataObject dataObject, string dataObjectFilePropertyName);
    }
}
