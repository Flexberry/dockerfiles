namespace NewPlatform.Flexberry.ORM.ODataService.Routing
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;

    using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

    /// <summary>
    /// Класс, осуществляющий выбор контроллера и действий для OData-запросов.
    /// </summary>
    public class DataObjectRoutingConvention : EntityRoutingConvention
    {
        /// <summary>
        /// Осуществляет выбор контроллера, который будут обрабатывать запрос.
        /// </summary>
        /// <param name="odataPath">Путь запроса.</param>
        /// <param name="request">Http-запрос.</param>
        /// <returns>Имя контроллера, который будут обрабатывать запрос.</returns>
        public override string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            if (odataPath == null)
            {
                throw new ArgumentNullException(nameof(odataPath));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Запросы типа odata или odata/$metadata должны обрабатываться стандартным образом.
            // The MetadataSegment type represents the Microsoft OData v5.7.0 MetadataPathSegment type here.
            MetadataSegment metadataPathSegment = odataPath.Segments.FirstOrDefault() as MetadataSegment;
            if (odataPath.Segments.Count == 0 || metadataPathSegment != null)
            {
                return base.SelectController(odataPath, request);
            }

            // Запросы типа odata или odata/$batch должны обрабатываться стандартным образом.
            // The BatchSegment type represents the Microsoft OData v5.7.0 BatchPathSegment type here.
            BatchSegment batchPathSegment = odataPath.Segments.FirstOrDefault() as BatchSegment;
            if (odataPath.Segments.Count == 0 || batchPathSegment != null)
            {
                return base.SelectController(odataPath, request);
            }

            // Остальные запросы должны обрабатываться контроллером Controllers.DataObjectController.
            return "DataObject";
        }

        /// <summary>
        /// Осуществляет выбор действия, которое будет выполняться при запросе.
        /// </summary>
        /// <param name="odataPath">Путь запроса.</param>
        /// <param name="controllerContext">Сведения об HTTP-запросе в контексте контроллера.</param>
        /// <param name="actionMap">Соответствие имен действий с описанием их методов.</param>
        /// <returns>Имя действия, которое будет выполнятся при запросе или <c>null</c>, если данная конвенция не может подобрать нужное действие.</returns>
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath.Segments.Any())
            {
                ODataPathSegment pathSegment = odataPath.Segments[odataPath.Segments.Count - 1];

                // The OperationImportSegment type represents the Microsoft OData v5.7.0 UnboundFunctionPathSegment, UnboundActionPathSegment types here.
                if (pathSegment is OperationImportSegment operationImportSegment)
                {
                    return operationImportSegment.OperationImports.First().IsFunctionImport() ? "GetODataFunctionsExecute" : "PostODataActionsExecute";
                }

                // OperationSegment type represents the Microsoft OData v5.7.0 BoundFunctionPathSegment, BoundActionPathSegment types here.
                if (pathSegment is OperationSegment operationSegment)
                {
                    return operationSegment.Operations.First().IsFunction() ? "GetODataFunctionsExecute" : "PostODataActionsExecute";
                }
            }

            // The NavigationPropertySegment type represents the Microsoft OData v5.7.0 NavigationPathSegment type here.
            if ((odataPath.Segments.Count > 1 && odataPath.Segments[odataPath.Segments.Count - 1] is NavigationPropertySegment) ||
                (odataPath.Segments.Count > 2 && odataPath.Segments[odataPath.Segments.Count - 2] is NavigationPropertySegment))
            {
                if (odataPath.EdmType is EdmCollectionType)
                    return "GetCollection";

                if (odataPath.EdmType is EdmEntityType)
                    return "GetEntity";
            }

            if (controllerContext.Request.Method.Method == "GET" && odataPath.PathTemplate == "~/entityset/key")
            {
                var keySegment = odataPath.Segments[1] as KeySegment;
                if (keySegment.Keys.First().Value is Guid)
                {
                    return "GetGuid";
                }
                else
                {
                    return "GetString";
                }
            }

            if (controllerContext.Request.Method.Method == "DELETE" && odataPath.PathTemplate == "~/entityset/key")
            {
                var keySegment = odataPath.Segments[1] as KeySegment;
                if (keySegment.Keys.First().Value is Guid)
                {
                    return "DeleteGuid";
                }
                else
                {
                    return "DeleteString";
                }
            }

            string ret = base.SelectAction(odataPath, controllerContext, actionMap);
            return ret;
        }
    }
}
