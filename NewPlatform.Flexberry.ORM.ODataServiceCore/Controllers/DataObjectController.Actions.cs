namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using System;
    using System.Collections;
    using System.Reflection;
    using ICSSoft.STORMNET;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OData;
    using Microsoft.OData.UriParser;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataServiceCore.Middleware;

    using Action = Functions.Action;
    using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

    /// <summary>
    /// The <see cref="DataObject"/> OData controller class.
    /// The ODataService actions part.
    /// </summary>
    public partial class DataObjectController
    {
        /// <summary>
        /// Выполняет action.
        /// Имя "PostODataActionsExecute" устанавливается в <see cref="Routing.Conventions.DataObjectRoutingConvention.SelectActionImpl"/>.
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

        private IActionResult ExecuteAction(ODataActionParameters parameters)
        {
            ODataPath odataPath = Request.HttpContext.ODataFeature().Path;

            // The OperationImportSegment type represents the Microsoft OData v5.7.0 UnboundActionPathSegment here.
            OperationImportSegment segment = odataPath.Segments[odataPath.Segments.Count - 1] as OperationImportSegment;

            // The OperationImportSegment.Identifier property represents the Microsoft OData v5.7.0 UnboundActionPathSegment.ActionName property here.
            if (segment == null || !Functions.IsRegistered(segment.Identifier))
            {
                return Ok("Action not found");
            }

            Action action = Functions.GetFunction(segment.Identifier) as Action;
            if (action == null)
            {
                return Ok("Action not found");
            }

            QueryParameters queryParameters = new QueryParameters(this);
            queryParameters.Count = null;
            queryParameters.Request = Request;
            queryParameters.RequestBody = (string)Request.HttpContext.Items[RequestHeadersHookMiddleware.PropertyKeyRequestContent];
            var result = action.Handler(queryParameters, parameters);
            if (action.ReturnType == typeof(void))
            {
                return Ok();
            }

            if (result == null)
            {
                return Ok("Result is null.");
            }

            if (result is DataObject)
            {
                var entityType = EdmModel.GetEdmEntityType(result.GetType());
                return Ok(GetEdmObject(entityType, result, 1, null));
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
                    return Ok(coll);
                }
            }

            return Ok(result);
        }
    }
}
