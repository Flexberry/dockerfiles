namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Extensions;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;
    using Expressions;
    using ICSSoft.STORMNET;
    using Microsoft.OData.Edm.Library;
    using Microsoft.OData.Edm.Values;
    using NewPlatform.Flexberry.ORM.ODataService.Formatter;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Handlers;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Routing;
    using Action = NewPlatform.Flexberry.ORM.ODataService.Functions.Action;

    /// <summary>
    /// OData controller class.
    /// Part with OData Service functions.
    /// </summary>
    public partial class DataObjectController
    {
        /// <summary>
        /// Выполняет action.
        /// Имя "PostODataActionsExecute" устанавливается в <see cref="DataObjectRoutingConvention.SelectAction"/>.
        /// </summary>
        /// <param name="parameters">Параметры action.</param>
        /// <returns>
        /// Результат выполнения action, преобразованный к типам сущностей EDM-модели или к примитивным типам.
        /// В случае, если зарегистрированый action не возвращает результат, будет возвращён только код 200 OK.
        /// После преобразования создаётся результат HTTP для ответа.
        /// </returns>
        public IHttpActionResult PostODataActionsExecute(ODataActionParameters parameters)
        {
            try
            {
                QueryOptions = CreateODataQueryOptions(typeof(DataObject));
                return ExecuteAction(parameters);
            }
            catch (Exception ex)
            {
                return ResponseMessage(InternalServerErrorMessage(ex));
            }
        }

        private IHttpActionResult ExecuteAction(ODataActionParameters parameters)
        {
            ODataPath odataPath = Request.ODataProperties().Path;
            UnboundActionPathSegment segment = odataPath.Segments[odataPath.Segments.Count - 1] as UnboundActionPathSegment;
            if (segment == null || !_functions.IsRegistered(segment.ActionName))
            {
                return SetResult("Action not found");
            }

            Action action = _functions.GetFunction(segment.ActionName) as Action;
            if (action == null)
            {
                return SetResult("Action not found");
            }

            QueryParameters queryParameters = new QueryParameters(this);
            queryParameters.Count = null;
            queryParameters.Request = Request;
            queryParameters.RequestBody = (string)Request.Properties[PostPatchHandler.RequestContent];
            var result = action.Handler(queryParameters, parameters);
            if (action.ReturnType == typeof(void))
            {
                return Ok();
            }

            if (result == null)
            {
                return SetResult("Result is null.");
            }

            if (result is DataObject)
            {
                var entityType = _model.GetEdmEntityType(result.GetType());
                return SetResult(GetEdmObject(entityType, result, 1, null));
            }

            if (!(result is string) && result is IEnumerable)
            {
                Type type = null;
                if (result.GetType().IsGenericType)
                {
                    Type[] args = result.GetType().GetGenericArguments();
                    if (args.Length == 1)
                        type = args[0];
                }

                if (result.GetType().IsArray)
                {
                    type = result.GetType().GetElementType();
                }

                if (type != null && (type.IsSubclassOf(typeof(DataObject)) || type == typeof(DataObject)))
                {
                    var coll = GetEdmCollection((IEnumerable)result, type, 1, null);
                    return SetResult(coll);
                }
            }

            return SetResultPrimitive(result.GetType(), result);
        }
    }
}