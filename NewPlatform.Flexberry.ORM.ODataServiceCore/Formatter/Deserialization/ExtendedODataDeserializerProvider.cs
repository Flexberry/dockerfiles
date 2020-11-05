namespace NewPlatform.Flexberry.ORM.ODataService.Formatter.Deserialization
{
    using System;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.AspNetCore.Http;
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
        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequest request)
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
