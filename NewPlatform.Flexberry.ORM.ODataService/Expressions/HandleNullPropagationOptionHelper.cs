namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System.Linq;
    using System.Web.Http;
    using System.Web.OData.Query;

    /// <summary>
    /// Вспомогательный класс для обработки распространения значения null.
    /// </summary>
    internal static class HandleNullPropagationOptionHelper
    {
        /// <summary>
        /// Возвращает как обрабатывать распространения значения null в зависимости от поставщика запросов при построении запросов.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Значение распространения null.</returns>
        public static HandleNullPropagationOption GetDefaultHandleNullPropagationOption(IQueryable query)
        {
            return HandleNullPropagationOption.False;
        }
    }
}