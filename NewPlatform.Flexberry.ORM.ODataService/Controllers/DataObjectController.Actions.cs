namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using System;
    using System.Collections;
    using System.Reflection;
    using ICSSoft.STORMNET;
    using Microsoft.AspNet.OData;
    using Microsoft.OData.UriParser;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Routing;

    using Action = NewPlatform.Flexberry.ORM.ODataService.Functions.Action;

#if NETFRAMEWORK
    using System.Web.Http;
    using NewPlatform.Flexberry.ORM.ODataService.Handlers;
#endif
#if NETSTANDARD
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OData;
    using NewPlatform.Flexberry.ORM.ODataService.Middleware;
#endif

    /// <summary>
    /// OData controller class.
    /// Part with OData Service functions.
    /// </summary>
    public partial class DataObjectController
    {
#if NETFRAMEWORK
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
                    return ResponseMessage(((HttpResponseException)ex.InnerException).Response);
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
        /// Выполняет action.
        /// Имя "PostODataActionsExecute" устанавливается в <see cref="DataObjectRoutingConvention.SelectActionImpl"/>.
        /// </summary>
        /// <param name="parameters">Параметры action.</param>
        /// <returns>
        /// Результат выполнения action, преобразованный к типам сущностей EDM-модели или к примитивным типам.
        /// В случае, если зарегистрированый action не возвращает результат, будет возвращён только код 200 OK.
        /// После преобразования создаётся результат HTTP для ответа.
        /// </returns>
        public IActionResult PostODataActionsExecute(ODataActionParameters parameters)
        {
            try
            {
                try
                {
                    QueryOptions = CreateODataQueryOptions(typeof(DataObject));
                    return ExecuteAction(parameters);
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

#if NETFRAMEWORK
        private IHttpActionResult ExecuteAction(ODataActionParameters parameters)
#elif NETSTANDARD
        private IActionResult ExecuteAction(ODataActionParameters parameters)
#endif
        {
            // The OperationImportSegment type represents the Microsoft OData v5.7.0 UnboundActionPathSegment here.
            OperationImportSegment segment = ODataPath.Segments[ODataPath.Segments.Count - 1] as OperationImportSegment;

            // The OperationImportSegment.Identifier property represents the Microsoft OData v5.7.0 UnboundActionPathSegment.ActionName property here.
            if (segment == null || !_functions.IsRegistered(segment.Identifier))
            {
                const string msg = "Action not found";
#if NETFRAMEWORK
                return SetResult(msg);
#elif NETSTANDARD
                return Ok(msg);
#endif
            }

            Action action = _functions.GetFunction(segment.Identifier) as Action;
            if (action == null)
            {
                const string msg = "Action not found";
#if NETFRAMEWORK
                return SetResult(msg);
#elif NETSTANDARD
                return Ok(msg);
#endif
            }

            QueryParameters queryParameters = new QueryParameters(this);
            queryParameters.Count = null;
            queryParameters.Request = Request;
#if NETFRAMEWORK
            queryParameters.RequestBody = (string)Request.Properties[PostPatchHandler.RequestContent];
#elif NETSTANDARD
            queryParameters.RequestBody = (string)Request.HttpContext.Items[RequestHeadersHookMiddleware.PropertyKeyRequestContent];
#endif
            var result = action.Handler(queryParameters, parameters);
            if (action.ReturnType == typeof(void))
            {
                return Ok();
            }

            if (result == null)
            {
                const string msg = "Result is null.";
#if NETFRAMEWORK
                return SetResult(msg);
#elif NETSTANDARD
                return Ok(msg);
#endif
            }

            if (result is DataObject)
            {
                var entityType = _model.GetEdmEntityType(result.GetType());
                var edmObj = GetEdmObject(entityType, result, 1, null);
#if NETFRAMEWORK
                return SetResult(edmObj);
#elif NETSTANDARD
                return Ok(edmObj);
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
                    var coll = GetEdmCollection((IEnumerable)result, type, 1, null);
#if NETFRAMEWORK
                    return SetResult(coll);
#elif NETSTANDARD
                    return Ok(coll);
#endif
                }
            }

#if NETFRAMEWORK
            return SetResultPrimitive(result.GetType(), result);
#elif NETSTANDARD
            return Ok(result);
#endif
        }
    }
}
