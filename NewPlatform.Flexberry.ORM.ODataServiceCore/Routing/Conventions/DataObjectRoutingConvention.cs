namespace NewPlatform.Flexberry.ORM.ODataService.Routing.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Adapters;
    using Microsoft.AspNet.OData.Common;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;

    using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

    /// <summary>
    /// Класс, осуществляющий выбор контроллера и действий для OData-запросов.
    /// </summary>
    public class DataObjectRoutingConvention : IODataRoutingConvention
    {
        private IEnumerable<NavigationSourceRoutingConvention> _routingConventions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectRoutingConvention"/> class.
        /// </summary>
        /// <param name="routingConventions">
        /// The <see cref="NavigationSourceRoutingConvention"/> descendants collection from the <see cref="ODataRoutingConventions.CreateDefault"/> result.
        /// </param>
        public DataObjectRoutingConvention(IEnumerable<NavigationSourceRoutingConvention> routingConventions)
        {
            _routingConventions = routingConventions ?? throw Error.ArgumentNull("routingConventions");
        }

        /// <inheritdoc/>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            HttpRequest request = routeContext.HttpContext.Request;

            SelectControllerResult controllerResult = SelectControllerImpl(odataPath);
            if (controllerResult != null)
            {
                // Get a IActionDescriptorCollectionProvider from the global service provider.
                IActionDescriptorCollectionProvider actionCollectionProvider =
                    routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
                Contract.Assert(actionCollectionProvider != null);

                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == controllerResult.ControllerName);

                if (actionDescriptors != null)
                {
                    string actionName = SelectAction(routeContext, controllerResult, actionDescriptors);
                    if (!String.IsNullOrEmpty(actionName))
                    {
                        return actionDescriptors.Where(
                            c => String.Equals(c.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="routeContext">The route context.</param>
        /// <param name="controllerResult">The result of selecting a controller.</param>
        /// <param name="actionDescriptors">The list of action descriptors.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the action descriptor of the selected action.
        /// </returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public virtual string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)     
        {
            string result = SelectActionImpl(
                routeContext.HttpContext.ODataFeature().Path,
                new WebApiControllerContext(routeContext, controllerResult),
                new WebApiActionMap(actionDescriptors));

            if (result == null)
            {
                foreach (NavigationSourceRoutingConvention convention in _routingConventions)
                {
                    result = convention.SelectAction(routeContext, controllerResult, actionDescriptors);
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///  Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The result of selecting a controller.</param>
        /// <param name="actionMap">The <see cref="WebApiActionMap"/> instance to search for an available action.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the action descriptor of the selected action.
        /// </returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMap actionMap)
        {
            if (odataPath.Segments.Any())
            {
                const string GetODataFunctionsExecuteAction = "GetODataFunctionsExecute";
                const string PostODataActionsExecuteAction = "PostODataActionsExecute";

                ODataPathSegment pathSegment = odataPath.Segments[odataPath.Segments.Count - 1];

                // The OperationImportSegment type represents the Microsoft OData v5.7.0 UnboundFunctionPathSegment, UnboundActionPathSegment types here.
                if (pathSegment is OperationImportSegment operationImportSegment)
                {
                    return operationImportSegment.OperationImports.First().IsFunctionImport() ? GetODataFunctionsExecuteAction : PostODataActionsExecuteAction;
                }

                // OperationSegment type represents the Microsoft OData v5.7.0 BoundFunctionPathSegment, BoundActionPathSegment types here.
                if (pathSegment is OperationSegment operationSegment)
                {
                    return operationSegment.Operations.First().IsFunction() ? GetODataFunctionsExecuteAction : PostODataActionsExecuteAction;
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

            if (controllerContext.Request.Method == ODataRequestMethod.Get && odataPath.PathTemplate == "~/entityset/key")
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

            if (controllerContext.Request.Method == ODataRequestMethod.Delete && odataPath.PathTemplate == "~/entityset/key")
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

            return null;
        }

        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller
        /// </returns>
        internal static SelectControllerResult SelectControllerImpl(ODataPath odataPath)
        {
            // Запросы с типами odata, odata/$batch и odata/$metadata должны обрабатываться стандартным образом.
            switch (odataPath.PathTemplate)
            {
                case "~":
                case "~/$batch":
                case "~/$metadata":
                    return null;
            }

            // Остальные запросы должны обрабатываться контроллером Controllers.DataObjectController.
            return new SelectControllerResult("DataObject", null);
        }
    }
}
