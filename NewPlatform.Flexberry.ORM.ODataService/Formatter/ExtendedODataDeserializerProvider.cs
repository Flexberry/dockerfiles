namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.OData.Edm;

    /// <inheritdoc/>
    public class ExtendedODataDeserializerProvider : DefaultODataDeserializerProvider
    {
        public ExtendedODataDeserializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
            _instance = new DefaultODataDeserializerProvider(rootContainer);
        }

        /// <inheritdoc/>
        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            return _instance.GetEdmTypeDeserializer(edmType);
        }

        /// <inheritdoc/>
        public override ODataDeserializer GetODataDeserializer(
             Type type,
#if NETFRAMEWORK
             System.Net.Http.HttpRequestMessage request
#endif
#if NETSTANDARD
             Microsoft.AspNetCore.Http.HttpRequest request
#endif
            )
        {
            if (type == typeof(Uri))
            {
                return base.GetODataDeserializer(type, request);
            }

            if (type == typeof(ODataActionParameters) ||
                type == typeof(ODataUntypedActionParameters))
            {
                return new ExtendedODataActionPayloadDeserializer(_instance);
            }

            return new ExtendedODataEntityDeserializer(_instance);
        }

        private readonly DefaultODataDeserializerProvider _instance;
    }
}
