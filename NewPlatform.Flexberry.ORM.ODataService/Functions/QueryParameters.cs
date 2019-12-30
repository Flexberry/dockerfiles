namespace NewPlatform.Flexberry.ORM.ODataService.Functions
{
    using System;
    using System.Net.Http;

    using ICSSoft.STORMNET.Business;

    using Microsoft.OData.Core;

    using NewPlatform.Flexberry.ORM.ODataService.Controllers;
    using NewPlatform.Flexberry.ORM.ODataService.Model;

    using Newtonsoft.Json;

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

        private DataObjectController _controller;

        /// <summary>
        /// Осуществляет получение типа объекта данных, соответствующего заданному имени набора сущностей в EDM-модели.
        /// </summary>
        /// <param name="edmEntitySetName">Имя набора сущностей в EDM-модели, для которого требуется получить представление по умолчанию.</param>
        /// <returns>Типа объекта данных, соответствующий заданному имени набора сущностей в EDM-модели.</returns>
        public Type GetDataObjectType(string edmEntitySetName)
        {
            DataObjectEdmModel model = (DataObjectEdmModel)_controller.QueryOptions.Context.Model;
            return model.GetDataObjectType(edmEntitySetName);
        }

        /// <summary>
        /// Создаёт lcs по заданному типу и запросу OData.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <returns>Возвращает lcs.</returns>
        public LoadingCustomizationStruct CreateLcs(Type type, string odataQuery = null)
        {
            HttpRequestMessage request = _controller.Request;
            if (odataQuery != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, odataQuery);
            }

            _controller.QueryOptions = _controller.CreateODataQueryOptions(type, request);
            _controller.type = type;
            return _controller.CreateLcs();
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="controller">Контроллер DataObjectController.</param>
        internal QueryParameters(DataObjectController controller)
        {
            _controller = controller;
            if (controller.QueryOptions == null)
            {
                return;
            }

            try
            {
                if (controller.QueryOptions.Skip != null)
                {
                    Skip = controller.QueryOptions.Skip.Value;
                }

                if (controller.QueryOptions.Top != null)
                {
                    Top = controller.QueryOptions.Top.Value;
                }
            }
            catch (Exception ex)
            {
                throw new ODataException($"Failed to initialize {nameof(QueryParameters)}: {JsonConvert.SerializeObject(controller.QueryOptions.RawValues)}", ex);
            }
        }
    }
}
