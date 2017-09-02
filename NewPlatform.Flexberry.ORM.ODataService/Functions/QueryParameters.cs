namespace NewPlatform.Flexberry.ORM.ODataService.Functions
{
    using System.Web.OData.Query;

    /// <summary>
    /// Класс для хранения параметров запроса OData.
    /// </summary>
    public class QueryParameters
    {
        /// <summary>
        /// Параметр запроса $top.
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// Параметр запроса $skip.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Хранит количество обработанных сущностей в пользовательской функции. Используется при формировании результата, если в запросе был параметр $count=true.
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="odataOptions">Параметры запроса OData.</param>
        internal QueryParameters(ODataQueryOptions odataOptions)
        {
            if (odataOptions.Skip != null)
                Skip = odataOptions.Skip.Value;

            if (odataOptions.Top != null)
                Top = odataOptions.Top.Value;
        }
    }
}
