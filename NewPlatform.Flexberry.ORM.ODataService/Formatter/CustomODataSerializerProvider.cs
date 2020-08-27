namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using ICSSoft.STORMNET;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.OData.Edm;

#if NETSTANDARD
    using Microsoft.AspNetCore.Http;
#endif

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
        /// <param name="rootContainer">The root container.</param>
        public CustomODataSerializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
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

            // The ODataResourceSetSerializer type represents the Microsoft OData v5.7.0 ODataFeedSerializer type here.
            if (serializer is ODataResourceSetSerializer)
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
        /// Gets an <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/> for the model associated with of the given <paramref name="request"/> and <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="T:System.Type"/> for which the serializer is being requested.</param>
        /// <param name="request">The request for which the response is being serialized.</param>
        /// <returns>
        /// The <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/> for the given type.
        /// </returns>
        public override ODataSerializer GetODataPayloadSerializer(Type type,
#if NETFRAMEWORK
            HttpRequestMessage request
#endif
#if NETSTANDARD
            HttpRequest request
#endif
            )
        {
            if (type == typeof(EnumerableQuery<IEdmEntityObject>))
            {
                return _feedSerializer;
            }

            ODataSerializer serializer = base.GetODataPayloadSerializer(type, request);

            if (serializer == null)
            {
                IEdmModel model = request.GetModel();
                LogService.LogDebug($"'{type.Name}' ({nameof(IEdmModel)} type='{model.GetType().Name}') cannot be serialized using the '{nameof(CustomODataSerializerProvider)}'");
            }

            return serializer;
        }
    }
}
