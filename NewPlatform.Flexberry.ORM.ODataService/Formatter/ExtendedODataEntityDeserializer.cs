namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System;
    using System.Collections.Generic;
    using System.Web.OData;
    using System.Web.OData.Formatter.Deserialization;
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;

    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using Expressions;

    /// <summary>
    /// Десериализатор для чтения передаваемых данных OData.
    /// </summary>
    public class ExtendedODataEntityDeserializer : ODataEntityDeserializer
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
                    readContext.Request.Properties.Add(OdataBindNull, readContext.ResourceEdmType);
                    return null;
                }

                readContext.Request.Properties.Add(ReadException, ex);
            }

            return obj;
        }

        /// <summary>
        /// Десериалезует <paramref name="structuralProperty"/> в <paramref name="entityResource"/>.
        /// </summary>
        /// <param name="entityResource">Объект, в который  structural property будет прочитано.</param>
        /// <param name="structuralProperty">Объект содержащий structural properties.</param>
        /// <param name="entityType">Тип сущности.</param>
        /// <param name="readContext">Состояние и установки, используемые при чтении.</param>
        public override void ApplyStructuralProperty(object entityResource, ODataProperty structuralProperty,
            IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
        {
            if (entityResource == null)
            {
                throw Error.ArgumentNull("entityResource");
            }

            if (structuralProperty == null)
            {
                throw Error.ArgumentNull("structuralProperty");
            }

            DeserializationHelpers.ApplyProperty(structuralProperty, entityType, entityResource, DeserializerProvider, readContext);
        }

        /// <summary>
        /// Десериализует navigation property.
        /// Также выполняет необходимые действия, чтобы обработка @odata.bind выполнялась по стандарту OData.
        /// </summary>
        /// <param name="entityResource">Объект, в который navigation property будет прочитано.</param>
        /// <param name="navigationLinkWrapper">navigation линк.</param>
        /// <param name="entityType">Тип сущности.</param>
        /// <param name="readContext">Состояние и установки, используемые при чтении.</param>
        public override void ApplyNavigationProperty(
            object entityResource,
            ODataNavigationLinkWithItems navigationLinkWrapper,
            IEdmEntityTypeReference entityType,
            ODataDeserializerContext readContext)
        {
            base.ApplyNavigationProperty(entityResource, navigationLinkWrapper, entityType, readContext);
            EdmEntityObject edmEntity = (EdmEntityObject)entityResource;
            DataObjectEdmModel model = readContext.Model as DataObjectEdmModel;

            foreach (var childItem in navigationLinkWrapper.NestedItems)
            {
                if (!readContext.Request.Properties.ContainsKey(Dictionary))
                {
                    readContext.Request.Properties.Add(Dictionary, new Dictionary<string, object>());
                }

                var dictionary = (Dictionary<string, object>)readContext.Request.Properties[Dictionary];
                var navigationPropertyName = navigationLinkWrapper.NavigationLink.Name;
                var entityReferenceLink = childItem as ODataEntityReferenceLinkBase;

                if (entityReferenceLink != null)
                {
                    Uri referencedEntityUrl = entityReferenceLink.EntityReferenceLink.Url;
                    if (referencedEntityUrl.IsAbsoluteUri)
                    {
                        referencedEntityUrl = referencedEntityUrl.MakeRelativeUri(readContext.Request.RequestUri);
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

                var feed = childItem as ODataFeedWithEntries;
                if (childItem == null || (feed != null && feed.Entries.Count == 0))
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
