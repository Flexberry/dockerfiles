namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Extensions;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Formatter;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Routing;

    /// <summary>
    /// OData controller class.
    /// Part with OData Service functions.
    /// </summary>
    public partial class DataObjectController
    {
        /// <summary>
        /// The container with OData Service functions.
        /// </summary>
        private readonly IFunctionContainer _functions;

        /// <summary>
        /// Выполняет пользовательскую функцию.
        /// Имя "GetODataFunctionsExecute" устанавливается в <see cref="DataObjectRoutingConvention.SelectAction"/>.
        /// </summary>
        /// <returns>
        /// Результат выполнения пользовательской функции, преобразованный к типам сущностей EDM-модели или к примитивным типам.
        /// После преобразования создаётся результат HTTP для ответа.
        /// </returns>
        [CustomEnableQuery]
        public IHttpActionResult GetODataFunctionsExecute()
        {
            QueryOptions = CreateODataQueryOptions(typeof(DataObject));
            return ExecuteUserFunction(new QueryParameters(QueryOptions));
        }

        /// <summary>
        /// Выполняет пользовательскую функцию.
        /// </summary>
        /// <param name="queryParameters">Параметры запроса.</param>
        /// <returns>Результат выполнения пользовательской функции, преобразованный к типам сущностей EDM-модели или к примитивным типам.</returns>
        internal IHttpActionResult ExecuteUserFunction(QueryParameters queryParameters)
        {
            queryParameters.Count = null;
            ODataPath odataPath = Request.ODataProperties().Path;
            UnboundFunctionPathSegment segment = odataPath.Segments[odataPath.Segments.Count - 1] as UnboundFunctionPathSegment;

            if (segment == null || !_functions.IsRegistered(segment.FunctionName))
                return SetResult("Function not found");

            Function function = _functions.GetFunction(segment.FunctionName);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            foreach (var parameterName in function.ParametersTypes.Keys)
            {
                var parameterValue = segment.GetParameterValue(parameterName);
                parameters.Add(parameterName, parameterValue);
            }

            var result = function.Handler(queryParameters, parameters);
            if (EdmTypeMap.GetEdmPrimitiveType(result.GetType()) != null)
            {
                return SetResultPrimitive(result.GetType(), result);
            }

            if (result is IEnumerable)
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

                if (type != null && type.IsSubclassOf(typeof(DataObject)))
                {
                    var queryOpt = CreateODataQueryOptions(type);

                    QueryOptions = new ODataQueryOptions(new ODataQueryContext(Request.ODataProperties().Model, type, Request.ODataProperties().Path), Request);
                    if (QueryOptions.SelectExpand != null && QueryOptions.SelectExpand.SelectExpandClause != null)
                    {
                        Request.ODataProperties().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
                    }

                    IncludeCount = false;
                    if (queryOpt.Count != null && queryOpt.Count.Value)
                    {
                        IncludeCount = true;
                        if (queryParameters.Count != null)
                        {
                            Count = (int)queryParameters.Count;
                        }
                        else
                        {
                            Count = GetObjectsCount(type, queryOpt);
                        }
                    }

                    QueryOptions = queryOpt;
                    var coll = GetEdmCollection((IEnumerable)result, type, 1, null);
                    return SetResult(coll);
                }
            }

            if (result is DataObject)
            {
                QueryOptions = new ODataQueryOptions(new ODataQueryContext(Request.ODataProperties().Model, result.GetType(), Request.ODataProperties().Path), Request);
                if (QueryOptions.SelectExpand != null && QueryOptions.SelectExpand.SelectExpandClause != null)
                {
                    Request.ODataProperties().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
                }

                var entityType = _model.GetEdmEntityType(result.GetType());
                return SetResult(GetEdmObject(entityType, result, 1, null));
            }

            return SetResult("Function not found");
        }
    }
}