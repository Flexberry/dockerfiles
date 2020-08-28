namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using ICSSoft.STORMNET;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Common;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using NewPlatform.Flexberry.ORM.ODataService.Expressions;
    using NewPlatform.Flexberry.ORM.ODataService.Model;

    using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
    using SRResources = Expressions.SRResources;

    /// <inheritdoc />
    public class ExtendedODataActionPayloadDeserializer : ODataDeserializer
    {
        private static readonly MethodInfo _castMethodInfo = typeof(Enumerable).GetMethod("Cast");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionPayloadDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ExtendedODataActionPayloadDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Parameter)
        {
            DeserializerProvider = deserializerProvider ?? throw new ArgumentNullException(nameof(deserializerProvider));
        }

        /// <summary>
        /// Gets the deserializer provider to use to read inner objects.
        /// </summary>
        public ODataDeserializerProvider DeserializerProvider { get; private set; }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
            Justification = "The majority of types referenced by this method are EdmLib types this method needs to know about to operate correctly")]
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmAction action = GetAction(readContext);
            Contract.Assert(action != null);

            // Create the correct resource type;
            Dictionary<string, object> payload;
            if (type == typeof(ODataActionParameters))
            {
                payload = new ODataActionParameters();
            }
            else
            {
                payload = new ODataUntypedActionParameters(action);
            }

            ODataParameterReader reader = messageReader.CreateODataParameterReader(action);

            while (reader.Read())
            {
                string parameterName = null;
                IEdmOperationParameter parameter = null;

                switch (reader.State)
                {
                    case ODataParameterReaderState.Value:
                        parameterName = reader.Name;
                        parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        // ODataLib protects against this but asserting just in case.
                        Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));
                        if (parameter.Type.IsPrimitive())
                        {
                            payload[parameterName] = reader.Value;
                        }
                        else
                        {
                            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(parameter.Type);
                            payload[parameterName] = deserializer.ReadInline(reader.Value, parameter.Type, readContext);
                        }
                        break;

                    case ODataParameterReaderState.Collection:
                        parameterName = reader.Name;
                        parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        // ODataLib protects against this but asserting just in case.
                        Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));
                        IEdmCollectionTypeReference collectionType = parameter.Type as IEdmCollectionTypeReference;
                        Contract.Assert(collectionType != null);
                        ODataCollectionValue value = ReadCollection(reader.CreateCollectionReader());
                        ODataCollectionDeserializer collectionDeserializer = (ODataCollectionDeserializer)DeserializerProvider.GetEdmTypeDeserializer(collectionType);
                        payload[parameterName] = collectionDeserializer.ReadInline(value, collectionType, readContext);
                        break;

                    case ODataParameterReaderState.Resource:
                        parameterName = reader.Name;
                        parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));
                        Contract.Assert(parameter.Type.IsStructured());

                        ODataReader resourceReader = reader.CreateResourceReader();
                        object item = resourceReader.ReadResourceOrResourceSet();

                        IEdmEntityTypeReference entityTypeReference = parameter.Type as IEdmEntityTypeReference;
                        if (entityTypeReference == null)
                        {
                            throw new ArgumentException("Contract assertion not met: entityTypeReference != null", "value");
                        }

                        var savedProps = new List<ODataProperty>();
                        if (item is ODataResourceWrapper)
                        {
                            var obj = CreateDataObject(readContext.Model as DataObjectEdmModel, entityTypeReference, item as ODataResourceWrapper, out Type objType);
                            payload[parameterName] = obj;
                            break;
                        }

                        ODataResourceDeserializer resourceDeserializer = (ODataResourceDeserializer)DeserializerProvider.GetEdmTypeDeserializer(parameter.Type);
                        payload[parameterName] = resourceDeserializer.ReadInline(item, parameter.Type, readContext);
                        break;

                    case ODataParameterReaderState.ResourceSet:
                        parameterName = reader.Name;
                        parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));

                        IEdmCollectionTypeReference resourceSetType = parameter.Type as IEdmCollectionTypeReference;
                        Contract.Assert(resourceSetType != null);

                        ODataReader resourceSetReader = reader.CreateResourceSetReader();
                        object feed = resourceSetReader.ReadResourceOrResourceSet();
                        ODataResourceSetDeserializer resourceSetDeserializer = (ODataResourceSetDeserializer)DeserializerProvider.GetEdmTypeDeserializer(resourceSetType);

                        IEnumerable enumerable;
                        ODataResourceSetWrapper odataFeedWithEntries = feed as ODataResourceSetWrapper;
                        if (odataFeedWithEntries != null)
                        {
                            List<DataObject> list = new List<DataObject>();
                            Type objType = null;
                            foreach (ODataResourceWrapper entry in odataFeedWithEntries.Resources)
                            {
                                list.Add(CreateDataObject(readContext.Model as DataObjectEdmModel, resourceSetType.ElementType() as IEdmEntityTypeReference, entry, out objType));
                            }

                            IEnumerable castedResult =
                                _castMethodInfo.MakeGenericMethod(objType)
                                    .Invoke(null, new[] { list }) as IEnumerable;
                            payload[parameterName] = castedResult;
                            break;
                        }

                        object result = resourceSetDeserializer.ReadInline(feed, resourceSetType, readContext);

                        IEdmTypeReference elementTypeReference = resourceSetType.ElementType();
                        Contract.Assert(elementTypeReference.IsStructured());

                        enumerable = result as IEnumerable;
                        if (enumerable != null)
                        {
                            var isUntypedProp = readContext.GetType().GetProperty("IsUntyped", BindingFlags.NonPublic | BindingFlags.Instance);
                            bool isUntyped = (bool)isUntypedProp.GetValue(readContext, null);

                            //if (readContext.IsUntyped)
                            if (isUntyped)
                            {
                                payload[parameterName] = ConvertToEdmObject(enumerable, resourceSetType);
                            }
                            else
                            {
                                Type elementClrType = EdmLibHelpers.GetClrType(elementTypeReference, readContext.Model);
                                IEnumerable castedResult =
                                    _castMethodInfo.MakeGenericMethod(elementClrType)
                                        .Invoke(null, new[] { result }) as IEnumerable;
                                payload[parameterName] = castedResult;
                            }
                        }

                        break;
                }
            }

            return payload;
        }

        public static IEdmObject ConvertToEdmObject(IEnumerable enumerable, IEdmCollectionTypeReference collectionType)
        {
            Contract.Assert(enumerable != null);
            Contract.Assert(collectionType != null);

            IEdmTypeReference elementType = collectionType.ElementType();

            if (elementType.IsEntity())
            {
                EdmEntityObjectCollection entityCollection =
                                        new EdmEntityObjectCollection(collectionType);

                foreach (EdmEntityObject entityObject in enumerable)
                {
                    entityCollection.Add(entityObject);
                }

                return entityCollection;
            }
            else if (elementType.IsComplex())
            {
                EdmComplexObjectCollection complexCollection =
                                        new EdmComplexObjectCollection(collectionType);

                foreach (EdmComplexObject complexObject in enumerable)
                {
                    complexCollection.Add(complexObject);
                }

                return complexCollection;
            }
            else if (elementType.IsEnum())
            {
                EdmEnumObjectCollection enumCollection =
                                        new EdmEnumObjectCollection(collectionType);

                foreach (EdmEnumObject enumObject in enumerable)
                {
                    enumCollection.Add(enumObject);
                }

                return enumCollection;
            }

            return null;
        }

        internal static IEdmAction GetAction(ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            ODataPath path = readContext.Path;
            if (path == null || path.Segments.Count == 0)
            {
                throw new SerializationException(SRResources.ODataPathMissing);
            }

            IEdmAction action = null;
            if (path.PathTemplate == "~/unboundaction")
            {
                // only one segment, it may be an unbound action
                // The OperationImportSegment type represents the Microsoft OData v5.7.0 UnboundActionPathSegment type here.
                OperationImportSegment unboundActionSegment = path.Segments.Last() as OperationImportSegment;
                if (unboundActionSegment != null)
                {
                    action = unboundActionSegment.OperationImports.FirstOrDefault()?.Operation as IEdmAction;
                }
            }
            else
            {
                // otherwise, it may be a bound action
                // The OperationImportSegment type represents the Microsoft OData v5.7.0 BoundActionPathSegment type here.
                OperationSegment boundActionSegment = path.Segments.Last() as OperationSegment;
                if (boundActionSegment != null)
                {
                    action = boundActionSegment.Operations.FirstOrDefault() as IEdmAction;
                }
            }

            if (action == null)
            {
                string message = Error.Format(SRResources.RequestNotActionInvocation, path.ToString());
                throw new SerializationException(message);
            }

            return action;
        }

        internal static ODataCollectionValue ReadCollection(ODataCollectionReader reader)
        {
            ArrayList items = new ArrayList();
            string typeName = null;

            while (reader.Read())
            {
                if (ODataCollectionReaderState.Value == reader.State)
                {
                    items.Add(reader.Item);
                }
                else if (ODataCollectionReaderState.CollectionStart == reader.State)
                {
                    typeName = reader.Item.ToString();
                }
            }

            return new ODataCollectionValue { Items = items.Cast<object>(), TypeName = typeName };
        }

        private DataObject CreateDataObject(DataObjectEdmModel model, IEdmEntityTypeReference entityTypeReference, ODataResourceWrapper entry, out Type objType)
        {
            IEdmEntityType entityType = entityTypeReference.EntityDefinition();
            objType = model.GetDataObjectType(model.GetEdmEntitySet(entityType).Name);
            var obj = (DataObject)Activator.CreateInstance(objType);
            foreach (ODataProperty odataProp in entry.Resource.Properties)
            {
                string clrPropName = model.GetDataObjectPropertyName(objType, odataProp.Name);
                Information.SetPropValueByName(obj, clrPropName, odataProp.Value);
            }

            return obj;
        }
    }
}
