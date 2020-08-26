namespace NewPlatform.Flexberry.ORM.ODataService.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using Microsoft.AspNet.OData.Common;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using NewPlatform.Flexberry.ORM.ODataService.Expressions;
    using NewPlatform.Flexberry.ORM.ODataServiceCore.WebUtilities;

    using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
    using SRResources = Expressions.SRResources;

    /// <inheritdoc cref="DefaultODataPathHandler"/>
    public class ExtendedODataPathHandler : DefaultODataPathHandler
    {
        private readonly ODataUriResolverSettings _resolverSettings = new ODataUriResolverSettings();

        /// <inheritdoc cref="DefaultODataPathHandler"/>
        public override ODataPath Parse(string serviceRoot, string odataPath, IServiceProvider requestContainer)
        {
            var model = requestContainer.GetService(typeof(IEdmModel)) as IEdmModel;
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (serviceRoot == null)
            {
                throw Error.ArgumentNull("serviceRoot");
            }

            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            ODataPath path = Parse(model, serviceRoot, odataPath, _resolverSettings, false);
            return path;
        }

        private static ODataPath Parse(
            IEdmModel model,
            string serviceRoot,
            string odataPath,
            ODataUriResolverSettings resolverSettings,
            bool enableUriTemplateParsing)
        {
            ODataUriParser uriParser;
            Uri serviceRootUri = null;
            var queryString = new NameValueCollection();

            if (enableUriTemplateParsing)
            {
                uriParser = new ODataUriParser(model, new Uri(odataPath, UriKind.Relative));
                uriParser.EnableUriTemplateParsing = true;
            }
            else
            {
                if (serviceRoot == null)
                {
                    throw new ArgumentNullException(nameof(serviceRoot), "Contract assertion not met: serviceRoot != null");
                }

                serviceRootUri = new Uri(
                    serviceRoot.EndsWith("/", StringComparison.Ordinal) ?
                        serviceRoot :
                       serviceRoot + "/");
                /*Из-за ошибки в mono, при вызове конструктора new Uri(serviceRootUri, odataPath);
                 * будем использовать конструктор new Uri($"{serviceRootUri}{odataPath}");
                 */
                var fullUri = new Uri($"{serviceRootUri}{odataPath}");
                queryString = QueryHelpers.QueryToNameValueCollection(fullUri.Query);
                uriParser = new ODataUriParser(model, serviceRootUri, fullUri);
            }

            uriParser.Resolver = resolverSettings.CreateResolver(model);

            Microsoft.OData.UriParser.ODataPath path;
            UnresolvedPathSegment unresolvedPathSegment;
            KeySegment id = null;
            try
            {
                path = uriParser.ParsePath();
            }
            catch (ODataUnrecognizedPathException ex)
            {
                if (ex.ParsedSegments != null &&
                    ex.ParsedSegments.Count() > 0 &&
                    (ex.ParsedSegments.Last().EdmType is IEdmComplexType ||
                     ex.ParsedSegments.Last().EdmType is IEdmEntityType) &&
                    ex.CurrentSegment != ODataSegmentKinds.Count)
                {
                    if (ex.UnparsedSegments.Count() == 0)
                    {
                        unresolvedPathSegment = new UnresolvedPathSegment(ex.CurrentSegment);
                        path = new Microsoft.OData.UriParser.ODataPath(ex.ParsedSegments.Concat(new UnresolvedPathSegment[] { unresolvedPathSegment }));
                    }
                    else
                    {
                        // Throw ODataException if there is some segment following the unresolved segment.
                        throw new ODataException(Error.Format(
                            SRResources.InvalidPathSegment,
                            ex.UnparsedSegments.First(),
                            ex.CurrentSegment));
                    }
                }
                else
                {
                    throw;
                }
            }

            if (!enableUriTemplateParsing && path.LastSegment is NavigationPropertyLinkSegment)
            {
                IEdmCollectionType lastSegmentEdmType = path.LastSegment.EdmType as IEdmCollectionType;

                if (lastSegmentEdmType != null)
                {
                    EntityIdSegment entityIdSegment = null;
                    bool exceptionThrown = false;

                    try
                    {
                        entityIdSegment = uriParser.ParseEntityId();

                        if (entityIdSegment != null)
                        {
                            // Create another ODataUriParser to parse $id, which is absolute or relative.
                            ODataUriParser parser = new ODataUriParser(model, serviceRootUri, entityIdSegment.Id);
                            id = parser.ParsePath().LastSegment as KeySegment;
                        }
                    }
                    catch (ODataException)
                    {
                        // Exception was thrown while parsing the $id.
                        // We will throw another exception about the invalid $id.
                        exceptionThrown = true;
                    }

                    if (exceptionThrown ||
                        (entityIdSegment != null &&
                            (id == null ||
                                !(id.EdmType.IsOrInheritsFrom(lastSegmentEdmType.ElementType.Definition) ||
                                  lastSegmentEdmType.ElementType.Definition.IsOrInheritsFrom(id.EdmType)))))
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidDollarId, queryString.Get("$id")));
                    }
                }
            }

            IEnumerable<ODataPathSegment> segments = ODataPathSegmentTranslator.Translate(
                model, path, uriParser.ParameterAliasNodes);
            ODataPath webAPIPath = new ODataPath(segments);

            CheckNavigableProperty(webAPIPath, model);
            return webAPIPath;
        }

        private static void CheckNavigableProperty(ODataPath path, IEdmModel model)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "Contract assertion not met: path != null");
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Contract assertion not met: model != null");
            }

            foreach (ODataPathSegment segment in path.Segments)
            {
                // The NavigationPropertySegment type represents the Microsoft OData v5.7.0 NavigationPathSegment type here.
                NavigationPropertySegment navigationPathSegment = segment as NavigationPropertySegment;

                if (navigationPathSegment != null)
                {
                    if (EdmLibHelpers.IsNotNavigable(navigationPathSegment.NavigationProperty, model))
                    {
                        throw new ODataException(Error.Format(
                            SRResources.NotNavigablePropertyUsedInNavigation,
                            navigationPathSegment.NavigationProperty.Name));
                    }
                }
            }
        }
    }

    internal class ODataUriResolverSettings
    {
        public bool CaseInsensitive { get; set; }

        public bool UnqualifiedNameCall { get; set; }

        public bool EnumPrefixFree { get; set; }

        public bool AlternateKeys { get; set; }

        public ODataUriResolver CreateResolver(IEdmModel model)
        {
            ODataUriResolver resolver;
            if (UnqualifiedNameCall && EnumPrefixFree)
            {
                resolver = new UnqualifiedCallAndEnumPrefixFreeResolver();
            }
            else if (UnqualifiedNameCall)
            {
                resolver = new UnqualifiedODataUriResolver();
            }
            else if (EnumPrefixFree)
            {
                resolver = new StringAsEnumResolver();
            }
            else if (AlternateKeys)
            {
                resolver = new AlternateKeysODataUriResolver(model);
            }
            else
            {
                resolver = new ODataUriResolver();
            }

            resolver.EnableCaseInsensitive = CaseInsensitive;
            return resolver;
        }
    }

    internal class UnqualifiedCallAndEnumPrefixFreeResolver : ODataUriResolver
    {
        private readonly StringAsEnumResolver _stringAsEnum = new StringAsEnumResolver();
        private readonly UnqualifiedODataUriResolver _unqualified = new UnqualifiedODataUriResolver();

        private bool _enableCaseInsensitive;

        public override bool EnableCaseInsensitive
        {
            get { return this._enableCaseInsensitive; }
            set
            {
                this._enableCaseInsensitive = value;
                _stringAsEnum.EnableCaseInsensitive = this._enableCaseInsensitive;
                _unqualified.EnableCaseInsensitive = this._enableCaseInsensitive;
            }
        }

        public override IEnumerable<IEdmOperation> ResolveBoundOperations(IEdmModel model, string identifier,
            IEdmType bindingType)
        {
            return _unqualified.ResolveBoundOperations(model, identifier, bindingType);
        }

        public override void PromoteBinaryOperandTypes(BinaryOperatorKind binaryOperatorKind,
            ref SingleValueNode leftNode, ref SingleValueNode rightNode, out IEdmTypeReference typeReference)
        {
            _stringAsEnum.PromoteBinaryOperandTypes(binaryOperatorKind, ref leftNode, ref rightNode, out typeReference);
        }

        public override IEnumerable<KeyValuePair<string, object>> ResolveKeys(IEdmEntityType type,
            IDictionary<string, string> namedValues, Func<IEdmTypeReference, string, object> convertFunc)
        {
            return _stringAsEnum.ResolveKeys(type, namedValues, convertFunc);
        }

        public override IEnumerable<KeyValuePair<string, object>> ResolveKeys(IEdmEntityType type,
            IList<string> positionalValues, Func<IEdmTypeReference, string, object> convertFunc)
        {
            return _stringAsEnum.ResolveKeys(type, positionalValues, convertFunc);
        }

        public override IDictionary<IEdmOperationParameter, SingleValueNode> ResolveOperationParameters(
            IEdmOperation operation, IDictionary<string, SingleValueNode> input)
        {
            return _stringAsEnum.ResolveOperationParameters(operation, input);
        }
    }
}
