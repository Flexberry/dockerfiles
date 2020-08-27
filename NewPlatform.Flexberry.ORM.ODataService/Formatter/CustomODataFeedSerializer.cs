namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;

    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" />
    /// </summary>
    // The ODataResourceSetSerializer type represents Microsoft OData v5.7.0 ODataFeedSerializer type here.
    internal class CustomODataFeedSerializer : ODataResourceSetSerializer
    {
        /// <summary>
        /// Name for count property in Request.
        /// </summary>
        public const string Count = "CustomODataFeedSerializer_Count";

        /// <returns>
        /// The number of items in the feed.
        /// </returns>
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetSerializer"/>.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public CustomODataFeedSerializer(CustomODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }

        /// <summary>
        /// Create the <see cref="ODataResourceSet"/> to be written for the given feed instance.
        /// </summary>
        /// <param name="resourceSetInstance">The instance representing the resource set being written.</param>
        /// <param name="resourceSetType">The EDM type of the feed being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataResourceSet"/> object.</returns>
        public override ODataResourceSet CreateResourceSet(IEnumerable resourceSetInstance, IEdmCollectionTypeReference resourceSetType, ODataSerializerContext writeContext)
        {
            var resourceSet = base.CreateResourceSet(resourceSetInstance, resourceSetType, writeContext);

#if NETFRAMEWORK
            if (writeContext.Request.Properties.ContainsKey(Count))
            {
                resourceSet.Count = (int)writeContext.Request.Properties[Count];
            }
#endif
#if NETSTANDARD
            if (writeContext.Request.HttpContext.Items.ContainsKey(Count))
            {
                resourceSet.Count = (int)writeContext.Request.HttpContext.Items[Count];
            }
#endif

            return resourceSet;
        }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a whole using the given messageWriter and writeContext.
        /// </summary>
        /// <param name="graph">The object to be written</param>
        /// <param name="type">The type of the object to be written.</param>
        /// <param name="messageWriter">The <see cref="ODataMessageWriter"/> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (graph is EnumerableQuery<IEdmEntityObject>)
            {
                var list = ((EnumerableQuery<IEdmEntityObject>)graph).AsIList();
                var entityCollectionType = new EdmCollectionTypeReference((EdmCollectionType)((EdmEntitySet)writeContext.NavigationSource).Type);
                graph = new EdmEntityObjectCollection(entityCollectionType, list);
            }

            base.WriteObject(graph, type, messageWriter, writeContext);
        }
    }
}
