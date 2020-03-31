namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Extensions
{
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using ICSSoft.STORMNET.Business;

    using NewPlatform.Flexberry.ORM.ODataService.Files.Providers;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers;

    /// <summary>
    /// Класс, содержащий расширения для сервиса данных.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Осуществляет регистрацию маршрута для загрузки/скачивания файлов.
        /// </summary>
        /// <param name="httpConfiguration">Используемая конфигурация.</param>
        /// <param name="routeName">Имя регистрируемого маршрута.</param>
        /// <param name="routeTemplate">Шаблон регистрируемого маршрута.</param>
        /// <param name="uploadsDirectoryPath">Пути к каталогу, который предназначен для хранения файлов загружаемых на сервер.</param>
        /// <param name="dataObjectFileProviders">
        /// Провайдеры файловых свойств объектов данных, которые будут использоваться для связывания файлов с объектами данных.
        /// </param>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <returns>Зарегистрированный маршрут.</returns>
        public static IHttpRoute MapODataServiceFileRoute(
            this HttpConfiguration httpConfiguration,
            string routeName,
            string routeTemplate,
            string uploadsDirectoryPath,
            IEnumerable<IDataObjectFileProvider> dataObjectFileProviders,
            IDataService dataService)
        {
            // Регистрируем маршрут для загрузки/скачивания файлов через отдельный контроллер.
            IHttpRoute route = httpConfiguration.Routes.MapHttpRoute(routeName, routeTemplate, defaults: new { controller = "File" });

            // Регистрируем провайдеры файловых свойств объектов данных.
            List<IDataObjectFileProvider> providers = new List<IDataObjectFileProvider>
            {
                new DataObjectFileProvider(dataService),
                new DataObjectWebFileProvider(dataService)
            };
            providers.AddRange(dataObjectFileProviders ?? new List<IDataObjectFileProvider>());

            foreach (IDataObjectFileProvider provider in providers)
            {
                FileController.RegisterDataObjectFileProvider(provider);
            }

            // Регистрируем имя маршрута.
            FileController.RouteName = routeName;

            // Регистрируем каталог для загрузок.
            FileController.UploadsDirectoryPath = uploadsDirectoryPath;

            // FileController.BaseUrl регистрируется перед обработкой какого-либо запроса (см. Controllers/BaseApiController и Controllers/BaseODataController),
            // т.к. при инициализации приложения нельзя получить корректный URL, по которому будет доступен тот или иной контроллер,
            // это возможно только в контексте обработки какого-либо запроса.
            return route;
        }

        /// <summary>
        /// Осуществляет регистрацию маршрута для загрузки/скачивания файлов.
        /// </summary>
        /// <param name="httpConfiguration">Используемая конфигурация.</param>
        /// <param name="routeName">Имя регистрируемого маршрута.</param>
        /// <param name="routeTemplate">Шаблон регистрируемого маршрута.</param>
        /// <param name="uploadsDirectoryPath">Пути к каталогу, который предназначен для хранения файлов загружаемых на сервер.</param>
        /// <param name="dataService">Сервис данных для операций с БД.</param>
        /// <returns>Зарегистрированный маршрут.</returns>
        public static IHttpRoute MapODataServiceFileRoute(
            this HttpConfiguration httpConfiguration,
            string routeName,
            string routeTemplate,
            string uploadsDirectoryPath,
            IDataService dataService)
        {
            return httpConfiguration.MapODataServiceFileRoute(routeName, routeTemplate, uploadsDirectoryPath, null, dataService);
        }
    }
}
