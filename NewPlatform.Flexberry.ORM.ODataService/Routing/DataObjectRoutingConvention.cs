namespace NewPlatform.Flexberry.ORM.ODataService.Routing
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.OData.Routing;
    using System.Web.OData.Routing.Conventions;

    using Microsoft.OData.Edm.Library;

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
            MetadataPathSegment metadataPathSegment = odataPath.Segments.FirstOrDefault() as MetadataPathSegment;
            if (odataPath.Segments.Count == 0 || metadataPathSegment != null)
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
            if (odataPath.Segments.Count > 0 && odataPath.Segments[odataPath.Segments.Count - 1] is UnboundFunctionPathSegment)
            {
                return "GetODataFunctionsExecute";
            }

            if (odataPath.Segments.Count > 0 && odataPath.Segments[odataPath.Segments.Count - 1] is UnboundActionPathSegment)
            {
                return "PostODataActionsExecute";
            }

            if ((odataPath.Segments.Count > 1 && odataPath.Segments[odataPath.Segments.Count - 1] is NavigationPathSegment) ||
                (odataPath.Segments.Count > 2 && odataPath.Segments[odataPath.Segments.Count - 2] is NavigationPathSegment))
            {
                if (odataPath.EdmType is EdmCollectionType)
                    return "GetCollection";

                if (odataPath.EdmType is EdmEntityType)
                    return "GetEntity";
            }

            if (controllerContext.Request.Method.Method == "GET" && odataPath.PathTemplate == "~/entityset/key")
            {
                Guid guid;
                if (Guid.TryParse(odataPath.Segments[1].ToString(), out guid))
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
                Guid guid;
                if (Guid.TryParse(odataPath.Segments[1].ToString(), out guid))
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