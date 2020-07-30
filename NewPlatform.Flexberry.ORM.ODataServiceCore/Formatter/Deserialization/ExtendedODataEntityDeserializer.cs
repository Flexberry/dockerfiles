namespace NewPlatform.Flexberry.ORM.ODataService.Formatter.Deserialization
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Common;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.OData;
    using Microsoft.OData.Edm;

    using NewPlatform.Flexberry.ORM.ODataService.Model;

    /// <summary>
    /// Десериализатор для чтения передаваемых данных OData.
    /// </summary>
    // The ODataResourceDeserializer type represents Microsoft OData v5.7.0 ODataEntityDeserializer type here.
    public class ExtendedODataEntityDeserializer : ODataResourceDeserializer
    {
        /// <summary>
        /// Строковая константа, которая используется для доступа свойствам запроса.
        /// </summary>
        public const string Dictionary = "ExtendedODataEntityDeserializer_Dictionary";

        /// <summary>
        /// Строковая константа, которая используется для доступа свойствам запроса.
        /// </summary>
        public const string OdataBindNull = "ExtendedODataEntityDeserializer_OdataBindNull";

        /// <summary>
        /// Строковая константа, которая используется для доступа свойствам запроса.
        /// </summary>
        public const string ReadException = "ExtendedODataEntityDeserializer_ReadException";

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="deserializerProvider">Провайдер.</param>
        public ExtendedODataEntityDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(deserializerProvider)
        {
        }

        /// <summary>
        /// Выполняет чтение передаваемых данных OData.
        /// </summary>
        /// <param name="messageReader">messageReader, который будет использован для чтения.</param>
        /// <param name="type">Тип передаваемых данных.</param>
        /// <param name="readContext">Состояние и установки, используемые при чтении.</param>
        /// <returns>Преобразованные данные.</returns>
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            object obj = null;
            try
            {
                obj = base.Read(messageReader, type, readContext);
            }
            catch (Exception ex)
            {
                if (ex is ODataException && ex.ToString().IndexOf("odata.bind") != -1)
                {
                    readContext.Request.HttpContext.Items.Add(OdataBindNull, readContext.ResourceEdmType);
                    return null;
                }

                readContext.Request.HttpContext.Items.Add(ReadException, ex);
            }

            return obj;
        }

        /// <summary>
        /// Десериалезует <paramref name="structuralProperty"/> в <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">Объект, в который  structural property будет прочитано.</param>
        /// <param name="structuralProperty">Объект содержащий structural properties.</param>
        /// <param name="structuredType">Структурированный тип.</param>
        /// <param name="readContext">Состояние и установки, используемые при чтении.</param>
        public override void ApplyStructuralProperty(object resource, ODataProperty structuralProperty, IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resource == null)
            {
                throw Error.ArgumentNull("entityResource");
            }

            if (structuralProperty == null)
            {
                throw Error.ArgumentNull("structuralProperty");
            }

            DeserializationHelpers.ApplyProperty(structuralProperty, structuredType, resource, DeserializerProvider, readContext);
        }

        /// <summary>
        /// Десериализует nested property.
        /// Также выполняет необходимые действия, чтобы обработка @odata.bind выполнялась по стандарту OData.
        /// </summary>
        /// <param name="resource">Объект, в который nested property будет прочитано.</param>
        /// <param name="resourceInfoWrapper">resource info.</param>
        /// <param name="structuredType">Структурированный тип.</param>
        /// <param name="readContext">Состояние и установки, используемые при чтении.</param>
        public override void ApplyNestedProperty(object resource, ODataNestedResourceInfoWrapper resourceInfoWrapper, IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            base.ApplyNestedProperty(resource, resourceInfoWrapper, structuredType, readContext);
            EdmEntityObject edmEntity = (EdmEntityObject)resource;
            DataObjectEdmModel model = readContext.Model as DataObjectEdmModel;

            foreach (var childItem in resourceInfoWrapper.NestedItems)
            {
                if (!readContext.Request.HttpContext.Items.ContainsKey(Dictionary))
                {
                    readContext.Request.HttpContext.Items.Add(Dictionary, new Dictionary<string, object>());
                }

                var dictionary = (Dictionary<string, object>)readContext.Request.HttpContext.Items[Dictionary];
                var navigationPropertyName = resourceInfoWrapper.NestedResourceInfo.Name;
                var entityReferenceLink = childItem as ODataEntityReferenceLinkBase;

                if (entityReferenceLink != null)
                {
                    Uri referencedEntityUrl = entityReferenceLink.EntityReferenceLink.Url;
                    if (referencedEntityUrl.IsAbsoluteUri)
                    {
                        var requestUri = new Uri(readContext.Request.GetEncodedUrl());
                        referencedEntityUrl = referencedEntityUrl.MakeRelativeUri(requestUri);
                    }

                    var segments = referencedEntityUrl.OriginalString.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length != 2)
                    {
                        throw new ApplicationException($"Invalid @odata.bind: {referencedEntityUrl.OriginalString}");
                    }

                    var type = model.GetDataObjectType(segments[0]);
                    if (type == null)
                    {
                        throw new ApplicationException($"Invalid entity set: {segments[0]}");
                    }

                    Guid guid;
                    try
                    {
                        guid = new Guid(segments[1]);
                    }
                    catch (Exception)
                    {
                        throw new ApplicationException($"Invalid guid: {segments[1]}");
                    }

                    var linkedEdmEntity = new EdmEntityObject(model.GetEdmEntityType(type));
                    linkedEdmEntity.TrySetPropertyValue("__PrimaryKey", guid);
                    edmEntity.TrySetPropertyValue(navigationPropertyName, linkedEdmEntity);
                    if (!dictionary.ContainsKey(navigationPropertyName))
                    {
                        dictionary.Add(navigationPropertyName, navigationPropertyName);
                    }
                }

                var feed = childItem as ODataResourceSetWrapper;
                if (childItem == null || (feed != null && feed.Resources.Count == 0))
                {
                    edmEntity.TrySetPropertyValue(navigationPropertyName, null);
                    if (!dictionary.ContainsKey(navigationPropertyName))
                    {
                        dictionary.Add(navigationPropertyName, null);
                    }
                }
            }
        }
    }
}
