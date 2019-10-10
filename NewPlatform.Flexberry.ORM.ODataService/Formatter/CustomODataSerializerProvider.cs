namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Web.OData;
    using System.Web.OData.Formatter.Serialization;

    using ICSSoft.STORMNET;

    using Microsoft.OData.Edm;

    /// <summary>
    /// An CustomODataSerializerProvider is a factory for creating <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/>s.
    ///
    /// </summary>
    public class CustomODataSerializerProvider : DefaultODataSerializerProvider
    {
        private readonly CustomODataFeedSerializer _feedSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomODataSerializerProvider"/> class.
        /// </summary>
        public CustomODataSerializerProvider()
            : base()
        {
            _feedSerializer = new CustomODataFeedSerializer(this);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Web.OData.Formatter.Serialization.ODataEdmTypeSerializer"/> for the given edmType.
        ///
        /// </summary>
        /// <param name="edmType">The <see cref="T:Microsoft.OData.Edm.IEdmTypeReference"/>.</param>
        /// <returns>
        /// The <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/>.
        /// </returns>
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            ODataEdmTypeSerializer serializer = base.GetEdmTypeSerializer(edmType);
            if (serializer is ODataFeedSerializer)
            {
                serializer = _feedSerializer;
            }

            if (serializer == null)
            {
                EdmTypeKind edmKind = edmType.TypeKind();
                LogService.LogDebug($"'{edmType.ToTraceString()}' ({nameof(EdmTypeKind)}='{edmKind.ToString()}') cannot be serialized using the '{nameof(CustomODataSerializerProvider)}'");
            }

            return serializer;
        }

        /// <summary>
        /// Gets an <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/> for the given <paramref name="model"/> and <paramref name="type"/>.
        ///
        /// </summary>
        /// <param name="model">The EDM model associated with the request.</param><param name="type">The <see cref="T:System.Type"/> for which the serializer is being requested.</param><param name="request">The request for which the response is being serialized.</param>
        /// <returns>
        /// The <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/> for the given type.
        /// </returns>
        public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type, HttpRequestMessage request)
        {
            if (type == typeof(EnumerableQuery<IEdmEntityObject>))
            {
                return _feedSerializer;
            }

            ODataSerializer serializer = base.GetODataPayloadSerializer(model, type, request);

            if (serializer == null)
            {
                LogService.LogDebug($"'{type.Name}' ({nameof(IEdmModel)} type='{model.GetType().Name}') cannot be serialized using the '{nameof(CustomODataSerializerProvider)}'");
            }

            return serializer;
        }
    }
}
