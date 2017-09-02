namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.OData;
    using System.Web.OData.Formatter.Serialization;
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Library;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;

    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" />
    /// </summary>
    internal class CustomODataFeedSerializer : ODataFeedSerializer
    {
        /// <summary>
        /// Name for count property in Request.
        /// </summary>
        public const string Count = "CustomODataFeedSerializer_Count";

        /// <returns>
        /// The number of items in the feed.
        /// </returns>
        /// <summary>
        /// Initializes a new instance of <see cref="ODataFeedSerializer"/>.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public CustomODataFeedSerializer(CustomODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }

        /// <summary>
        /// Create the <see cref="ODataFeed"/> to be written for the given feed instance.
        /// </summary>
        /// <param name="feedInstance">The instance representing the feed being written.</param>
        /// <param name="feedType">The EDM type of the feed being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataFeed"/> object.</returns>
        public override ODataFeed CreateODataFeed(IEnumerable feedInstance, IEdmCollectionTypeReference feedType, ODataSerializerContext writeContext)
        {
            var feed = base.CreateODataFeed(feedInstance, feedType, writeContext);

            if (writeContext.Request.Properties.ContainsKey(Count))
            {
                feed.Count = (int)writeContext.Request.Properties[Count];
            }

            return feed;
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
