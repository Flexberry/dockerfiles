namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Extensions
{
    using System.Web.Http;
    using System.Web.Http.Routing;

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
        /// <returns>Зарегистрированный маршрут.</returns>
        public static IHttpRoute MapODataServiceFileRoute(
            this HttpConfiguration httpConfiguration,
            string routeName,
            string routeTemplate)
        {
            // Регистрируем маршрут для загрузки/скачивания файлов через отдельный контроллер.
            IHttpRoute route = httpConfiguration.Routes.MapHttpRoute(routeName, routeTemplate, defaults: new { controller = "File" });

            // FileController.BaseUrl регистрируется перед обработкой какого-либо запроса (см. Controllers/BaseApiController и Controllers/BaseODataController),
            // т.к. при инициализации приложения нельзя получить корректный URL, по которому будет доступен тот или иной контроллер,
            // это возможно только в контексте обработки какого-либо запроса.
            return route;
        }
    }
}
