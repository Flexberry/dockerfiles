namespace NewPlatform.Flexberry.ORM.ODataService.Functions
{
    using Controllers;
    using ICSSoft.STORMNET.Business;
    using Model;
    using System;
    using System.Net.Http;
    using System.Web.OData.Query;

    /// <summary>
    /// Класс для хранения параметров запроса OData.
    /// </summary>
    public class QueryParameters
    {
        /// <summary>
        /// Запрос.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Тело запроса.
        /// </summary>
        public string RequestBody { get; set; }

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

        private DataObjectController _сontroller;

        /// <summary>
        /// Осуществляет получение типа объекта данных, соответствующего заданному имени набора сущностей в EDM-модели.
        /// </summary>
        /// <param name="edmEntitySetName">Имя набора сущностей в EDM-модели, для которого требуется получить представление по умолчанию.</param>
        /// <returns>Типа объекта данных, соответствующий заданному имени набора сущностей в EDM-модели.</returns>
        public Type GetDataObjectType(string edmEntitySetName)
        {
            DataObjectEdmModel model = (DataObjectEdmModel)_сontroller.QueryOptions.Context.Model;
            return model.GetDataObjectType(edmEntitySetName);
        }

        /// <summary>
        /// Создаёт lcs по заданному типу и запросу OData.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <returns>Возвращает lcs.</returns>
        public LoadingCustomizationStruct CreateLcs(Type type, string odataQuery = null)
        {
            HttpRequestMessage request = _сontroller.Request;
            if (odataQuery != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, odataQuery);
            }

            _сontroller.QueryOptions = _сontroller.CreateODataQueryOptions(type, request);
            _сontroller.type = type;
            return _сontroller.CreateLcs();
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="сontroller">Контроллер DataObjectController.</param>
        internal QueryParameters(DataObjectController сontroller)
        {
            _сontroller = сontroller;
            if (сontroller.QueryOptions == null)
            {
                return;
            }

            if (сontroller.QueryOptions.Skip != null)
            {
                Skip = сontroller.QueryOptions.Skip.Value;
            }

            if (сontroller.QueryOptions.Top != null)
            {
                Top = сontroller.QueryOptions.Top.Value;
            }
        }
    }
}
