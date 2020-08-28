#if NETFRAMEWORK
namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using ICSSoft.STORMNET;
    using Microsoft.AspNet.OData;
    using Microsoft.OData.Edm;
    using Model;

    public class RawOutputFormatter : BufferedMediaTypeFormatter
    {
        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            object val;
            var col1 = value as EnumerableQuery<IEdmEntityObject>;
            var col2 = value as EdmEntityObjectCollection;
            if (col1 != null)
                col1.ToList()[0].TryGetPropertyValue("__PrimaryKey", out val);
            else
                col2[0].TryGetPropertyValue("__PrimaryKey", out val);
            var buffer = val as byte[];
            writeStream.Write(buffer, 0, buffer.Length);
            writeStream.Close();
        }

        public static void PrepareHttpResponseMessage(ref HttpResponseMessage msg, string mediaType, DataObjectEdmModel model, byte[] buffer)
        {
            List<IEdmEntityObject> edmObjList = new List<IEdmEntityObject>();
            var edmObj = new EdmEntityObject(model.GetEdmEntityType(typeof(DataObject)));
            edmObj.TrySetPropertyValue("__PrimaryKey", buffer);
            edmObjList.Add(edmObj);
            IEdmCollectionTypeReference entityCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(edmObj.GetEdmType()));
            EdmEntityObjectCollection collection = new EdmEntityObjectCollection(entityCollectionType, edmObjList);
            msg.Content = new ObjectContent(typeof(EdmEntityObjectCollection), collection, new RawOutputFormatter(), mediaType);
        }
    }
}
#endif
