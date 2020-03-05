namespace NewPlatform.Flexberry.ORM.ODataService.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.OData.Routing;
    using Microsoft.OData.Core;
    using Microsoft.OData.Core.UriParser;
    using Microsoft.OData.Core.UriParser.Metadata;
    using Microsoft.OData.Core.UriParser.TreeNodeKinds;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm;
    using NewPlatform.Flexberry.ORM.ODataService.Expressions;
    using Semantic = Microsoft.OData.Core.UriParser.Semantic;
    using SingleValueNode = Microsoft.OData.Core.UriParser.Semantic.SingleValueNode;

    /// <inheritdoc cref="DefaultODataPathHandler"/>
    public class ExtendedODataPathHandler : DefaultODataPathHandler
    {
        private ODataUriResolverSetttings _resolverSettings = new ODataUriResolverSetttings();

        /// <inheritdoc cref="DefaultODataPathHandler"/>
        public override ODataPath Parse(IEdmModel model, string serviceRoot, string odataPath)
        {
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

            ODataPath path = null;
            path = Parse(model, serviceRoot, odataPath, _resolverSettings, false);
            return path;
        }

        private static ODataPath Parse(
            IEdmModel model,
            string serviceRoot,
            string odataPath,
            ODataUriResolverSetttings resolverSettings,
            bool enableUriTemplateParsing)
        {
            ODataUriParser uriParser;
            Uri serviceRootUri = null;
            Uri fullUri = null;
            NameValueCollection queryString = null;

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
                fullUri = new Uri($"{serviceRootUri}{odataPath}");
                queryString = fullUri.ParseQueryString();
                uriParser = new ODataUriParser(model, serviceRootUri, fullUri);
            }

            uriParser.Resolver = resolverSettings.CreateResolver(model);

            Semantic.ODataPath path;
            UnresolvedPathSegment unresolvedPathSegment = null;
            Semantic.KeySegment id = null;
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
                        path = new Semantic.ODataPath(ex.ParsedSegments);
                        unresolvedPathSegment = new UnresolvedPathSegment(ex.CurrentSegment);
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

            if (!enableUriTemplateParsing && path.LastSegment is Semantic.NavigationPropertyLinkSegment)
            {
                IEdmCollectionType lastSegmentEdmType = path.LastSegment.EdmType as IEdmCollectionType;

                if (lastSegmentEdmType != null)
                {
                    Semantic.EntityIdSegment entityIdSegment = null;
                    bool exceptionThrown = false;

                    try
                    {
                        entityIdSegment = uriParser.ParseEntityId();

                        if (entityIdSegment != null)
                        {
                            // Create another ODataUriParser to parse $id, which is absolute or relative.
                            ODataUriParser parser = new ODataUriParser(model, serviceRootUri, entityIdSegment.Id);
                            id = parser.ParsePath().LastSegment as Semantic.KeySegment;
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

            ODataPath webAPIPath = ODataPathSegmentTranslator.TranslateODataLibPathToWebApiPath(
                path,
                model,
                unresolvedPathSegment,
                id,
                enableUriTemplateParsing,
                uriParser.ParameterAliasNodes);

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
                NavigationPathSegment navigationPathSegment = segment as NavigationPathSegment;

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

    internal class ODataUriResolverSetttings
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
