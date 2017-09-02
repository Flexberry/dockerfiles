namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.OData;
    using System.Web.OData.Formatter.Deserialization;

    /// <inheritdoc/>
    public class ExtendedODataDeserializerProvider : DefaultODataDeserializerProvider
    {
        /// <inheritdoc/>
        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(Microsoft.OData.Edm.IEdmTypeReference edmType)
        {
            return Instance.GetEdmTypeDeserializer(edmType);
        }

        /// <inheritdoc/>
        public override ODataDeserializer GetODataDeserializer(
             Microsoft.OData.Edm.IEdmModel model,
             Type type,
             System.Net.Http.HttpRequestMessage request)
        {
            if (type == typeof(ODataActionParameters) ||
                type == typeof(ODataUntypedActionParameters) ||
                type == typeof(Uri))
            {
                return base.GetODataDeserializer(model, type, request);
            }

            return new ExtendedODataEntityDeserializer(Instance);
        }
    }
}
