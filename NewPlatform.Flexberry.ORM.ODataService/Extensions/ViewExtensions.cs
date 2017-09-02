namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using ICSSoft.STORMNET;

    /// <summary>
    /// Класс, содержащий вспомогательные методы для работы с представлениями (<see cref="View"/>>).
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Осуществляет получение списка имен примитивных свойств, содержащихся в корне представления.
        /// </summary>
        /// <param name="view">Представление, для которого требуется получить список имен примитивных свойств содержащихся в корне.</param>
        /// <returns>Список имен примитивных свойств, содержащихся в корне представления.</returns>
        public static List<string> GetSelfPrimitivePropertiesNames(this View view)
        {
            Type dataObjectType = view.DefineClassType;
            List<string> allMastersNames = dataObjectType.GetProperties()
                .Where(x => x.PropertyType.IsSubclassOf(typeof(DataObject)))
                .Select(x => x.Name)
                .ToList();

            return view.Properties.Select(x => x.Name.Split('.').First())
                .Where(x => !allMastersNames.Contains(x))
                .ToList();
        }

        /// <summary>
        /// Осуществляет получение списка имен мастеровых свойств, содержащихся в корне представления.
        /// </summary>
        /// <param name="view">Представление, для которого требуется получить список имен мастеровых свойств содержащихся в корне.</param>
        /// <returns>Список имен мастеровых свойств, содержащихся в корне представления.</returns>
        public static List<string> GetSelfMasterPropertiesNames(this View view)
        {
            List<string> allMastersNames = view.DefineClassType.GetProperties()
                .Where(x => x.PropertyType.IsSubclassOf(typeof(DataObject)))
                .Select(x => x.Name)
                .ToList();

            return view.Properties.Select(x => x.Name.Split('.').First())
                .Where(x => allMastersNames.Contains(x))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Осуществляет получение списка имен детейловых свойств, содержащихся в корне представления.
        /// </summary>
        /// <param name="view">Представление, для которого требуется получить список имен детейловых свойств содержащихся в корне.</param>
        /// <returns>Список имен детейловых свойств, содержащихся в корне представления.</returns>
        public static List<string> GetSelfDetailPropertiesNames(this View view)
        {
            return view.Details.Select(x => x.Name).ToList();
        }

        /// <summary>
        /// Осуществляет получение списка имен всех свойств, содержащихся в корне представления.
        /// </summary>
        /// <param name="view">Представление, для которого требуется получить список имен всех свойств содержащихся в корне.</param>
        /// <returns>Список имен всех свойств, содержащихся в корне представления.</returns>
        public static List<string> GetSelfAllPropertiesNames(this View view)
        {
            return view.GetSelfPrimitivePropertiesNames()
                .Concat(view.GetSelfMasterPropertiesNames())
                .Concat(view.GetSelfDetailPropertiesNames())
                .ToList();
        }

        /// <summary>
        /// Осуществляет получение представления, для заданного мастерового свойства, содержащегося в корне представления.
        /// </summary>
        /// <param name="view">Представление, в корне которого содержится интересующее мастеровое свойство.</param>
        /// <param name="masterPropertyName">Имя мастерового свойства, для которого требуется получить представление.</param>
        /// <returns>Сформированное представление, для заданного мастерового свойства.</returns>
        public static View GetSelfMasterView(this View view, string masterPropertyName)
        {
            View masterView = view.GetViewForMaster(masterPropertyName);
            if (view.Properties.Any(x => x.Name == masterPropertyName))
            {
                masterView.AddProperty(Information.ExtractPropertyName<DataObject>(y => y.__PrimaryKey));
            }

            return masterView;
        }

        /// <summary>
        /// Осуществляет получение OData-запроса, состоящего из комбинации параметров $select, $expand.
        /// </summary>
        /// <param name="view">Представление, на основе которого требуется сформировать запрос.</param>
        /// <returns>OData-запрос, состоящий из комбинации параметров $select, $expand.</returns>
        public static string ToODataQuery(this View view)
        {
            return view.ToODataQuery("&");
        }

        /// <summary>
        /// Осуществляет получение OData-запроса, состоящего из комбинации параметров $select, $expand.
        /// </summary>
        /// <param name="view">Представление, на основе которого требуется сформировать запрос.</param>
        /// <param name="delimiter">Разделитель, который будет использоваться между параметрами $select и $expand.</param>
        /// <returns>OData-запрос, состоящий из комбинации параметров $select, $expand.</returns>
        private static string ToODataQuery(this View view, string delimiter)
        {
            StringBuilder queryBuilder = new StringBuilder(string.Empty);

            List<string> mastersNames = view.GetSelfMasterPropertiesNames();
            List<string> detailsNames = view.GetSelfDetailPropertiesNames();
            List<string> primitivePropertiesNames = view.GetSelfPrimitivePropertiesNames();

            if (primitivePropertiesNames.Count > 0 || mastersNames.Count > 0 || detailsNames.Count > 0)
            {
                queryBuilder.Append("$select=");
                queryBuilder.Append(string.Join(",", primitivePropertiesNames.Concat(mastersNames).Concat(detailsNames)));
            }

            if (mastersNames.Count > 0 || detailsNames.Count > 0)
            {
                queryBuilder.AppendFormat("{0}$expand=", delimiter);

                Dictionary<string, View> propertiesToExpand = new Dictionary<string, View>();
                mastersNames.ForEach(x => propertiesToExpand.Add(x, view.GetSelfMasterView(x)));
                view.Details.ToList().ForEach(x => propertiesToExpand.Add(x.Name, x.View));

                queryBuilder.Append(string.Join(",", propertiesToExpand.Select(x => string.Format("{0}({1})", x.Key, x.Value.ToODataQuery(";")))));
            }

            return queryBuilder.ToString();
        }
    }
}
