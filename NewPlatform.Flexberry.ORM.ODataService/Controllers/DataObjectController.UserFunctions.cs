namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using ICSSoft.STORMNET;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.OData;
    using Microsoft.OData.UriParser;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Routing;

#if NETFRAMEWORK
    using System.Net.Http;
    using System.Web.Http;
    using NewPlatform.Flexberry.ORM.ODataService.Formatter;
    using NewPlatform.Flexberry.ORM.ODataService.Handlers;
#endif
#if NETSTANDARD
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using NewPlatform.Flexberry.ORM.ODataService.Middleware;
    using NewPlatform.Flexberry.ORM.ODataService.WebUtilities;
#endif

    /// <summary>
    /// OData controller class.
    /// Part with OData Service functions.
    /// </summary>
    public partial class DataObjectController
    {
        /// <summary>
        /// The container with OData Service functions.
        /// </summary>
#if NETFRAMEWORK
        private readonly IFunctionContainer _functions;
#elif NETSTANDARD
        private IFunctionContainer _functions => ManagementToken?.Functions;
#endif

#if NETFRAMEWORK
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
            try
            {
                QueryOptions = CreateODataQueryOptions(typeof(DataObject));
                return ExecuteUserFunction(new QueryParameters(this));
            }
            catch (HttpResponseException ex)
            {
                if (HasOdataError(ex))
                {
                    return ResponseMessage(ex.Response);
                }
                else
                {
                    return ResponseMessage(InternalServerErrorMessage(ex));
                }
            }
            catch (TargetInvocationException ex)
            {
                if (HasOdataError(ex.InnerException))
                {
                    return ResponseMessage(((HttpResponseException)ex.InnerException)?.Response);
                }
                else
                {
                    return ResponseMessage(InternalServerErrorMessage(ex));
                }
            }
            catch (Exception ex)
            {
                return ResponseMessage(InternalServerErrorMessage(ex));
            }
        }
#elif NETSTANDARD
        /// <summary>
        /// Выполняет пользовательскую функцию.
        /// Имя "GetODataFunctionsExecute" устанавливается в <see cref="DataObjectRoutingConvention.SelectActionImpl"/>.
        /// </summary>
        /// <returns>
        /// Результат выполнения пользовательской функции, преобразованный к типам сущностей EDM-модели или к примитивным типам.
        /// После преобразования создаётся результат HTTP для ответа.
        /// </returns>
        [CustomEnableQuery]
        public IActionResult GetODataFunctionsExecute()
        {
            try
            {
                try
                {
                    QueryOptions = CreateODataQueryOptions(typeof(DataObject));
                    return ExecuteUserFunction(new QueryParameters(this));
                }
                catch (ODataException oDataException)
                {
                    return BadRequest(new ODataError() { ErrorCode = StatusCodes.Status400BadRequest.ToString(), Message = oDataException.Message });
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is ODataException oDataException)
                    {
                        return BadRequest(new ODataError() { ErrorCode = StatusCodes.Status400BadRequest.ToString(), Message = oDataException.Message });
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                throw CustomException(ex);
            }
        }
#endif

        /// <summary>
        /// Выполняет пользовательскую функцию.
        /// </summary>
        /// <param name="queryParameters">Параметры запроса.</param>
        /// <returns>Результат выполнения пользовательской функции, преобразованный к типам сущностей EDM-модели или к примитивным типам.</returns>
#if NETFRAMEWORK
        internal IHttpActionResult ExecuteUserFunction(QueryParameters queryParameters)
#elif NETSTANDARD
        internal IActionResult ExecuteUserFunction(QueryParameters queryParameters)
#endif
        {
            queryParameters.Count = null;
            queryParameters.Request = Request;

            // The OperationImportSegment type represents the Microsoft OData v5.7.0 UnboundFunctionPathSegment type here.
            OperationImportSegment segment = ODataPath.Segments[ODataPath.Segments.Count - 1] as OperationImportSegment;

            // The OperationImportSegment.Identifier property represents the Microsoft OData v5.7.0 UnboundFunctionPathSegment.FunctionName property here.
            if (segment == null || !_functions.IsRegistered(segment.Identifier))
            {
                const string msg = "Function not found";
#if NETFRAMEWORK
                return SetResult(msg);
#elif NETSTANDARD
                return Ok(msg);
#endif
            }

            Function function = _functions.GetFunction(segment.Identifier);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            foreach (string parameterName in function.ParametersTypes.Keys)
            {
                try
                {
                    var parameterValue = segment.GetParameterValue(parameterName);
                    if (parameterValue is ODataEnumValue enumParameterValue)
                    {
                        parameterValue = Enum.Parse(function.ParametersTypes[parameterName], enumParameterValue.Value);
                    }

                    parameters.Add(parameterName, parameterValue);
                }
                catch (Exception ex)
                {
                    throw new ODataException($"Failed to convert parameter: {parameterName}", ex);
                }
            }

            var result = function.Handler(queryParameters, parameters);
            if (result == null)
            {
                const string msg = "Result is null.";
#if NETFRAMEWORK
                return SetResult(msg);
#elif NETSTANDARD
                return Ok(msg);
#endif
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
#if NETFRAMEWORK
                        Request.ODataProperties().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
#elif NETSTANDARD
                        Request.HttpContext.ODataFeature().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
#endif
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

#if NETFRAMEWORK
                    NameValueCollection queryParams = Request.RequestUri.ParseQueryString();
#elif NETSTANDARD
                    NameValueCollection queryParams = QueryHelpers.QueryToNameValueCollection(Request.Query);
#endif

#if NETFRAMEWORK
                    if ((_model.ExportService != null || _model.ODataExportService != null) && (Request.Properties.ContainsKey(PostPatchHandler.AcceptApplicationMsExcel) || Convert.ToBoolean(queryParams.Get("exportExcel"))))
#elif NETSTANDARD
                    if ((_model.ExportService != null || _model.ODataExportService != null) && (Request.HttpContext.Items.ContainsKey(RequestHeadersHookMiddleware.AcceptApplicationMsExcel) || Convert.ToBoolean(queryParams["exportExcel"])))
#endif
                    {
                        _objs = (result as IEnumerable).Cast<DataObject>().ToArray();
                        var excel = CreateExcel(queryParams);
#if NETFRAMEWORK
                        return ResponseMessage(excel);
#elif NETSTANDARD
                        return excel;
#endif
                    }

                    var coll = GetEdmCollection((IEnumerable)result, type, 1, null, _dynamicView);
#if NETFRAMEWORK
                    return SetResult(coll);
#elif NETSTANDARD
                    return Ok(coll);
#endif
                }

#if NETFRAMEWORK
                return SetResult(result);
#elif NETSTANDARD
                return Ok(result);
#endif
            }

            if (result is DataObject)
            {
                QueryOptions = CreateODataQueryOptions(result.GetType());
                if (QueryOptions.SelectExpand != null && QueryOptions.SelectExpand.SelectExpandClause != null)
                {
#if NETFRAMEWORK
                    Request.ODataProperties().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
#elif NETSTANDARD
                    Request.HttpContext.ODataFeature().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
#endif
                }

                this.type = result.GetType();
                CreateDynamicView();
                var entityType = _model.GetEdmEntityType(this.type);
                var edmObj = GetEdmObject(entityType, result, 1, null, _dynamicView);
#if NETFRAMEWORK
                return SetResult(edmObj);
#elif NETSTANDARD
                return Ok(edmObj);
#endif
            }

#if NETFRAMEWORK
            return SetResultPrimitive(result.GetType(), result);
#elif NETSTANDARD
            return Ok(result);
#endif
        }
    }
}
