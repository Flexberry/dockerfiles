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

#if NETFRAMEWORK
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
#endif
#if NETSTANDARD
        /// <summary>
        /// Создает набор конвенций маршрутизации по умолчанию.
        /// </summary>
        /// <remarks>
        /// Стандартный набор, возвращаемый <see cref="ODataRoutingConventions.CreateDefault"/>, состоит из конвенции маршрутизации
        /// <see cref="MetadataRoutingConvention"/>, и конвенций маршрутизации - наследников <see cref="NavigationSourceRoutingConvention"/>.
        /// Создаваемый набор состоит из конвенции маршрутизации <see cref="DataObjectRoutingConvention"/>, содержащей коллекцию конвенций
        /// маршрутизации - наследников <see cref="NavigationSourceRoutingConvention"/> из стандартного набора, и конвенцию маршрутизации
        /// <see cref="MetadataRoutingConvention"/>.
        /// </remarks>
        /// <returns>
        /// Набор конвенций маршрутизации.
        /// </returns>
        public static IList<IODataRoutingConvention> CreateDefault()
        {
            IEnumerable<NavigationSourceRoutingConvention> routingConventions = ODataRoutingConventions
                .CreateDefault()
                .OfType<NavigationSourceRoutingConvention>();

            return new List<IODataRoutingConvention>()
            {
                new DataObjectRoutingConvention(routingConventions),
                new MetadataRoutingConvention(),
            };
        }
#endif
    }
}
