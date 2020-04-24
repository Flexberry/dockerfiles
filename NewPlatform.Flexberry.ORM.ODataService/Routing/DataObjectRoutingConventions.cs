namespace NewPlatform.Flexberry.ORM.ODataService.Routing
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNet.OData.Routing.Conventions;

    /// <summary>
    /// Класс, который предоставляет вспомогательные методы для создания наборов конвенций маршрутизации.
    /// </summary>
    public static class DataObjectRoutingConventions
    {
        /// <summary>
        /// Наименование маршрута для осуществления odata-запросов, используемое по умолчанию.
        /// </summary>
        public const string DefaultRouteName = "odata";

        /// <summary>
        /// Префикс маршрутов в odata-запросах, используемый по умолчанию.
        /// </summary>
        public const string DefaultRoutePrefix = "odata";

        /// <summary>
        /// Создает набор конвенций маршрутизации по умолчанию.
        /// </summary>
        /// <remarks>
        /// Набор состоит из <see cref="DataObjectRoutingConvention"/> и стандартных реализаций <see cref="IODataRoutingConvention"/>.
        /// </remarks>
        /// <returns>
        /// Набор конвенций маршрутизации.
        /// </returns>
        public static List<IODataRoutingConvention> CreateDefault()
        {
            List<IODataRoutingConvention> routingConventions = ODataRoutingConventions.CreateDefault().ToList();
            routingConventions.Insert(0, new DataObjectRoutingConvention());

            return routingConventions;
        }
    }
}
