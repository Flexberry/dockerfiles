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

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Formatter;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Routing;
    using Expressions;
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm.Library;
    using Microsoft.OData.Edm.Values;

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
            return ExecuteUserFunction(new QueryParameters(this));
        }

        /// <summary>
        /// Выполняет пользовательскую функцию.
        /// </summary>
        /// <param name="queryParameters">Параметры запроса.</param>
        /// <returns>Результат выполнения пользовательской функции, преобразованный к типам сущностей EDM-модели или к примитивным типам.</returns>
        internal IHttpActionResult ExecuteUserFunction(QueryParameters queryParameters)
        {
            queryParameters.Count = null;
            queryParameters.Request = Request;
            ODataPath odataPath = Request.ODataProperties().Path;
            UnboundFunctionPathSegment segment = odataPath.Segments[odataPath.Segments.Count - 1] as UnboundFunctionPathSegment;

            if (segment == null || !_functions.IsRegistered(segment.FunctionName))
                return SetResult("Function not found");

            Function function = _functions.GetFunction(segment.FunctionName);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            foreach (var parameterName in function.ParametersTypes.Keys)
            {
                var parameterValue = segment.GetParameterValue(parameterName);
                if (parameterValue is ODataEnumValue)
                {
                    parameterValue = Enum.Parse(function.ParametersTypes[parameterName], (parameterValue as ODataEnumValue).Value);
                }

                parameters.Add(parameterName, parameterValue);
            }

            var result = function.Handler(queryParameters, parameters);
            if (result == null)
            {
                return SetResult("Result is null.");
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
                    var queryOpt = CreateODataQueryOptions(type);

                    QueryOptions = queryOpt;
                    if (QueryOptions.SelectExpand != null && QueryOptions.SelectExpand.SelectExpandClause != null)
                    {
                        Request.ODataProperties().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
                    }

                    this.type = type;
                    CreateDynamicView();
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

                    var coll = GetEdmCollection((IEnumerable)result, type, 1, null, _dynamicView);
                    return SetResult(coll);
                }

                return SetResult(result);
            }

            if (result is DataObject)
            {
                QueryOptions = CreateODataQueryOptions(result.GetType());
                if (QueryOptions.SelectExpand != null && QueryOptions.SelectExpand.SelectExpandClause != null)
                {
                    Request.ODataProperties().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
                }

                this.type = result.GetType();
                CreateDynamicView();
                var entityType = _model.GetEdmEntityType(this.type);
                return SetResult(GetEdmObject(entityType, result, 1, null, _dynamicView));
            }

            return SetResultPrimitive(result.GetType(), result);
        }
    }
}