namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.KeyGen;

    /// <summary>
    /// Default implementation of <see cref="IDataObjectEdmModelBuilder"/>.
    /// Builds EDM-model by list of assemblies.
    /// </summary>
    /// <seealso cref="IDataObjectEdmModelBuilder" />
    public class DefaultDataObjectEdmModelBuilder : IDataObjectEdmModelBuilder
    {
        /// <summary>
        /// The list of assemblies for searching types to expose.
        /// </summary>
        private readonly IEnumerable<Assembly> _searchAssemblies;

        /// <summary>
        /// Is need to add the whole type namespace for EDM entity set.
        /// </summary>
        private readonly bool _useNamespaceInEntitySetName;

        /// <summary>
        /// The list of links from master to pseudodetail (pseudoproperty) definitions.
        /// </summary>
        private readonly PseudoDetailDefinitions _pseudoDetailDefinitions;

        /// <summary>
        /// Delegate for additional filtering exposed types.
        /// At the result EDM-model will be added only those types, for that the delegate returned <c>true</c>.
        /// </summary>
        public Func<Type, bool> TypeFilter { get; set; }

        /// <summary>
        /// Delegate for additional filtering exposed properties.
        /// At the result EDM-model will be added only those properties, for that the delegate returned <c>true</c>.
        /// </summary>
        public Func<PropertyInfo, bool> PropertyFilter { get; set; }

        /// <summary>
        /// Delegate for building names for EDM entity sets.
        /// </summary>
        public Func<Type, string> EntitySetNameBuilder { get; set; }

        /// <summary>
        /// Delegate for building namespaces for EDM entity types.
        /// </summary>
        public Func<Type, string> EntityTypeNamespaceBuilder { get; set; }

        /// <summary>
        /// Delegate for building names for EDM entity type.
        /// </summary>
        public Func<Type, string> EntityTypeNameBuilder { get; set; }

        /// <summary>
        /// Delegate for building names for EDM entity property.
        /// </summary>
        public Func<PropertyInfo, string> EntityPropertyNameBuilder { get; set; }

        private readonly PropertyInfo _keyProperty = Information.ExtractPropertyInfo<DataObject>(n => n.__PrimaryKey);

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDataObjectEdmModelBuilder"/> class.
        /// </summary>
        /// <param name="searchAssemblies">The list of assemblies for searching types to expose.</param>
        /// <param name="useNamespaceInEntitySetName">Is need to add the whole type namespace for EDM entity set.</param>
        public DefaultDataObjectEdmModelBuilder(IEnumerable<Assembly> searchAssemblies, bool useNamespaceInEntitySetName = true, PseudoDetailDefinitions pseudoDetailDefinitions = null)
        {
            _searchAssemblies = searchAssemblies ?? throw new ArgumentNullException(nameof(searchAssemblies), "Contract assertion not met: searchAssemblies != null");
            _useNamespaceInEntitySetName = useNamespaceInEntitySetName;
            _pseudoDetailDefinitions = pseudoDetailDefinitions ?? new PseudoDetailDefinitions();

            EntitySetNameBuilder = BuildEntitySetName;
            EntityPropertyNameBuilder = BuildEntityPropertyName;
            EntityTypeNameBuilder = BuildEntityTypeName;
            EntityTypeNamespaceBuilder = BuildEntityTypeNamespace;
        }

        /// <summary>
        /// Builds <see cref="DataObjectEdmModel" /> instance using specified assemblies.
        /// </summary>
        /// <returns>An <see cref="DataObjectEdmModel" /> instance.</returns>
        public DataObjectEdmModel Build()
        {
            var meta = new DataObjectEdmMetadata
            {
                KeyPropertyName = _keyProperty.Name,
                KeyProperty = _keyProperty
            };

            var typeFilter = TypeFilter ?? (t => true);

            foreach (Assembly assembly in _searchAssemblies)
            {
                IEnumerable<Type> dataObjectTypes = assembly
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(DataObject)))
                    .Where(typeFilter);

                foreach (Type dataObjectType in dataObjectTypes)
                {
                    if (!meta.Contains(dataObjectType))
                        AddDataObjectWithHierarchy(meta, dataObjectType);
                }
            }

            if (meta[typeof(DataObject)].KeyType == null)
            {
                var key = meta[typeof(DataObject)].OwnProperties.FirstOrDefault(p => p.Name == _keyProperty.Name);
                if (key == null)
                {
                    throw new ArgumentException("Contract assertion not met: key != null", "value");
                }

                meta[typeof(DataObject)].OwnProperties.Clear();
                foreach (var type in meta.Types)
                {
                    if (type.BaseType == typeof(DataObject) && meta[type].KeyType == null)
                    {
                        meta[type].OwnProperties.Add(key);
                        meta[type].KeyType = typeof(Guid);
                    }
                }
            }

            return new DataObjectEdmModel(meta, this);
        }

        /// <summary>
        /// Returns <see cref="ICSSoft.STORMNET.Business.LINQProvider.PseudoDetail{T, TP}"/> as object.
        /// </summary>
        /// <param name="masterType">The type of master.</param>
        /// <param name="masterToDetailPseudoProperty">The name of the link from master to pseudodetail (pseudoproperty).</param>
        /// <returns>An <see cref="ICSSoft.STORMNET.Business.LINQProvider.PseudoDetail{T, TP}"/> instance as object.</returns>
        public object GetPseudoDetail(Type masterType, string masterToDetailPseudoProperty)
        {
            return _pseudoDetailDefinitions
                .Where(x => x.MasterType == masterType)
                .Where(x => x.MasterToDetailPseudoProperty == masterToDetailPseudoProperty)
                .FirstOrDefault()
                ?.PseudoDetail;
        }

        /// <summary>
        /// Returns <see cref="IPseudoDetailDefinition" /> instance.
        /// </summary>
        /// <param name="pseudoDetail"><see cref="ICSSoft.STORMNET.Business.LINQProvider.PseudoDetail{T, TP}"/> instance as object.</param>
        /// <returns>An <see cref="IPseudoDetailDefinition" /> instance.</returns>
        public IPseudoDetailDefinition GetPseudoDetailDefinition(object pseudoDetail)
        {
            return _pseudoDetailDefinitions
                .Where(x => x.PseudoDetail == pseudoDetail)
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds the property for exposing.
        /// </summary>
        /// <param name="typeSettings">The type settings.</param>
        /// <param name="dataObjectProperty">The data object property.</param>
        private static void AddProperty(DataObjectEdmTypeSettings typeSettings, PropertyInfo dataObjectProperty)
        {
            // Master property.
            if (dataObjectProperty.PropertyType.IsSubclassOf(typeof(DataObject)))
            {
                var masterMetadata = new DataObjectEdmMasterSettings(dataObjectProperty.PropertyType)
                {
                    AllowNull = dataObjectProperty.GetCustomAttribute(typeof(NotNullAttribute)) == null
                };

                typeSettings.MasterProperties.Add(dataObjectProperty, masterMetadata);
                return;
            }

            // Detail property.
            if (dataObjectProperty.PropertyType.IsSubclassOf(typeof(DetailArray)))
            {
                var detailType = dataObjectProperty.PropertyType.GetProperty("Item", new[] { typeof(int) }).PropertyType;
                typeSettings.DetailProperties.Add(dataObjectProperty, new DataObjectEdmDetailSettings(detailType));
                return;
            }

            // Link from master to pseudodetail (pseudoproperty).
            if (dataObjectProperty is PseudoDetailPropertyInfo)
            {
                var detailType = dataObjectProperty.PropertyType.GenericTypeArguments[0];
                typeSettings.PseudoDetailProperties.Add(dataObjectProperty, new DataObjectEdmDetailSettings(detailType));
                return;
            }

            // Own property.
            typeSettings.OwnProperties.Add(dataObjectProperty);
        }

        /// <summary>
        /// Adds the data object for exposing with its whole hierarchy.
        /// </summary>
        /// <param name="meta">The metadata object.</param>
        /// <param name="dataObjectType">The type of the data object.</param>
        private void AddDataObjectWithHierarchy(DataObjectEdmMetadata meta, Type dataObjectType)
        {
            // Some base class can be already added.
            if (meta.Contains(dataObjectType))
                return;

            if (dataObjectType == typeof(DataObject))
            {
                var dataObjectTypeSettings = meta[dataObjectType] = new DataObjectEdmTypeSettings() { KeyType = typeof(Guid), EnableCollection = true, CollectionName = EntitySetNameBuilder(dataObjectType) }; // TODO
                AddProperty(dataObjectTypeSettings, typeof(DataObject).GetProperty(_keyProperty.Name));
                return;
            }

            Type baseType = dataObjectType.BaseType;
            if (baseType == null)
            {
                throw new ArgumentException("Contract assertion not met: baseType != null", nameof(dataObjectType));
            }

            AddDataObjectWithHierarchy(meta, baseType);

            var typeSettings = meta[dataObjectType] = new DataObjectEdmTypeSettings
            {
                EnableCollection = true,
                CollectionName = EntitySetNameBuilder(dataObjectType),
                DefaultView = DynamicView.Create(dataObjectType, null).View
            };
            AddProperties(dataObjectType, typeSettings);
            if (typeSettings.KeyType != null)
                meta[baseType].KeyType = null;
        }

        private Type GetKeyType(Type dataObjectType)
        {
            BaseKeyGenerator generator = Activator.CreateInstance(Information.GetKeyGeneratorType(dataObjectType)) as BaseKeyGenerator;
            return generator.KeyType;
        }

        /// <summary>
        /// Adds the properties for exposing.
        /// </summary>
        /// <param name="dataObjectType">The type of the data object.</param>
        /// <param name="typeSettings">The type settings.</param>
        private void AddProperties(Type dataObjectType, DataObjectEdmTypeSettings typeSettings)
        {
            Func<PropertyInfo, bool> propertyFilter = PropertyFilter ?? (p => true);

            var keyType = GetKeyType(dataObjectType);
            if (keyType == GetKeyType(dataObjectType.BaseType))
            {
                typeSettings.KeyType = null;
            }
            else
            {
                if (dataObjectType.BaseType != typeof(DataObject))
                {
                    throw new ArgumentException($"Запрещено переопределение ключа в типе {dataObjectType.FullName}, т.к он не наследуется непосредственно от DataObject.", nameof(dataObjectType));
                }

                typeSettings.KeyType = keyType;
            }

            IEnumerable<PropertyInfo> properties = dataObjectType
            .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .Where(propertyFilter);

            foreach (var property in properties)
            {
                bool overridden = false;
                foreach (var method in property.GetAccessors(true))
                {
                    if (!method.Equals(method.GetBaseDefinition()))
                    {
                        overridden = true;
                        break;
                    }
                }

                if (overridden && property.Name != _keyProperty.Name)
                    continue;
                AddProperty(typeSettings, property);
            }

            // Add the defined links from master to pseudodetail (pseudoproperties) as properties for exposing.
            foreach (var definition in _pseudoDetailDefinitions.Where(x => x.MasterType == dataObjectType))
            {
                var pi = new PseudoDetailPropertyInfo(definition.MasterToDetailPseudoProperty, definition.PseudoPropertyType, definition.MasterType);
                AddProperty(typeSettings, pi);
            }
        }

        /// <summary>
        /// Builds the name of the entity set.
        /// Default logic, for <see cref="EntitySetNameBuilder"/>.
        /// </summary>
        /// <param name="dataObjectType">Type of the data object.</param>
        /// <returns>The name of appropriate EDM entity set.</returns>
        private string BuildEntitySetName(Type dataObjectType)
        {
            PublishNameAttribute attr = dataObjectType.GetCustomAttribute<PublishNameAttribute>(false);
            if (attr != null && !string.IsNullOrEmpty(attr.EntitySetPublishName))
            {
                return attr.EntitySetPublishName;
            }

            string typeName = BuildEntityTypeName(dataObjectType);
            string nameSpace = BuildEntityTypeNamespace(dataObjectType);
            return string.Concat(_useNamespaceInEntitySetName ? $"{nameSpace}.{typeName}".Replace(".", string.Empty) : typeName, "s"/* "Aliases"*/).Replace("_", string.Empty);
            //return string.Concat(_useNamespaceInEntitySetName ? dataObjectType.FullName.Replace(".", string.Empty) : dataObjectType.Name, "s"/* "Aliases"*/).Replace("_", string.Empty);
        }

        /// <summary>
        /// Builds the name of the entity.
        /// Default logic, for <see cref="EntityTypeNameBuilder"/>.
        /// </summary>
        /// <param name="dataObjectType">Type of the data object.</param>
        /// <returns>The name of appropriate EDM entity.</returns>
        private string BuildEntityTypeName(Type dataObjectType)
        {
            PublishNameAttribute attr = dataObjectType.GetCustomAttribute<PublishNameAttribute>(false);
            if (attr != null)
            {
                int lastPos = attr.TypePublishName.LastIndexOf(".");
                if (lastPos < 0)
                    return attr.TypePublishName;
                return attr.TypePublishName.Substring(lastPos + 1);
            }

            return dataObjectType.Name;
        }

        /// <summary>
        /// Builds the namespace of the entity.
        /// Default logic, for <see cref="EntityTypeNamespaceBuilder"/>.
        /// </summary>
        /// <param name="dataObjectType">Type of the data object.</param>
        /// <returns>The namespace of appropriate EDM entity.</returns>
        private string BuildEntityTypeNamespace(Type dataObjectType)
        {
            PublishNameAttribute attr = dataObjectType.GetCustomAttribute<PublishNameAttribute>(false);
            if (attr != null)
            {
                int lastPos = attr.TypePublishName.LastIndexOf(".");
                if (lastPos < 0)
                    return string.Empty;
                return attr.TypePublishName.Substring(0, lastPos);
            }

            return dataObjectType.Namespace;
        }

        /// <summary>
        /// Builds the name of the property.
        /// Default logic, for <see cref="EntityPropertyNameBuilder"/>.
        /// </summary>
        /// <param name="propertyDataObject">Property of the data object.</param>
        /// <returns>The name of appropriate EDM property.</returns>
        private string BuildEntityPropertyName(PropertyInfo propertyDataObject)
        {
            PublishNameAttribute attr = propertyDataObject.GetCustomAttribute<PublishNameAttribute>(true);
            if (attr != null)
            {
                return attr.TypePublishName;
            }

            return propertyDataObject.Name;
        }
    }
}