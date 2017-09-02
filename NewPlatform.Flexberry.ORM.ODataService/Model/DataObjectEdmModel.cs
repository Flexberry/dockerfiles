namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    using ICSSoft.STORMNET;

    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Library;
    using Microsoft.OData.Edm.Library.Expressions;
    using Microsoft.OData.Edm.Library.Values;

    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.Configuration;
    using System.Web.OData;

    /// <summary>
    /// EDM-модель, которая строится на основе сборок с объектами данных (унаследованными от <see cref="DataObject"/>).
    /// </summary>
    public class DataObjectEdmModel : EdmModel
    {
        public IExportService ExportService { get; set; }

        public IDataObjectEdmModelBuilder EdmModelBuilder { get; set; }
        public string KeyPropertyName => _metadata.KeyPropertyName;
        public PropertyInfo KeyProperty => _metadata.KeyProperty;

        /// <summary>
        /// Словарь, в котором ключ алиас типа, а значение сам тип.
        /// </summary>
        private readonly IDictionary<string, Type> _aliasesNameToType = new Dictionary<string, Type>();

        /// <summary>
        /// Словарь, в котором ключ сам тип, а значение алиас типа.
        /// </summary>
        private readonly IDictionary<Type, string> _aliasesTypeToName = new Dictionary<Type, string>();

        /// <summary>
        /// Словарь, в котором ключ алиас пространства имен, а значение сам тип.
        /// </summary>
        private readonly IDictionary<string, Type> _aliasesNamespaceToType = new Dictionary<string, Type>();

        /// <summary>
        /// Словарь, в котором составной ключ это - алиас полного имени типа и алиас свойства, а значение само свойство.
        /// </summary>
        private readonly IDictionary<string, IDictionary<string, PropertyInfo>> _aliasesNameToProperty = new Dictionary<string, IDictionary<string, PropertyInfo>>();

        /// <summary>
        /// Словарь, в котором составной ключ это - тип и имя свойства, а значение алиас имени свойства.
        /// </summary>
        private readonly IDictionary<Type, IDictionary<string, string>> _namePropertyToAlias = new Dictionary<Type, IDictionary<string, string>>();

        private readonly DataObjectEdmMetadata _metadata;
        private readonly IDictionary<Type, EdmEntityType> _registeredEdmEntityTypes = new Dictionary<Type, EdmEntityType>();
        private readonly IDictionary<Type, EdmEnumType> _registeredEnums = new Dictionary<Type, EdmEnumType>();
        private readonly IDictionary<string, Type> _registeredCollections = new Dictionary<string, Type>();
        private readonly IDictionary<Type, EdmEntitySet> _registeredEntitySets = new Dictionary<Type, EdmEntitySet>();
        private readonly IDictionary<Type, IList<Type>> _typeHierarchy = new Dictionary<Type, IList<Type>>();

        public DataObjectEdmModel(DataObjectEdmMetadata metadata, IDataObjectEdmModelBuilder edmModelBuilder = null)
        {
            Contract.Requires<ArgumentNullException>(metadata != null);
            EdmModelBuilder = edmModelBuilder;
            var container = new UnityContainer();
            if (container != null)
            {
                container.LoadConfiguration();
                if (container.IsRegistered<IExportService>("Export"))
                    ExportService = container.Resolve<IExportService>("Export");
            }

            _metadata = metadata;

            BuildTypeHierarchy();
            BuildEdmEntityTypes();
            BuildEntitySets();
            RegisterMasters();
            RegisterDetails();
        }

        private void BuildTypeHierarchy()
        {
            foreach (Type dataObjectType in _metadata.Types)
            {
                Type baseDataObjectType = dataObjectType.BaseType;
                Contract.Assume(baseDataObjectType != null);

                if (!_typeHierarchy.ContainsKey(baseDataObjectType))
                    _typeHierarchy[baseDataObjectType] = new List<Type> { dataObjectType };
                else
                    _typeHierarchy[baseDataObjectType].Add(dataObjectType);

                var typeFullName = $"{GetEntityTypeNamespace(dataObjectType)}.{GetEntityTypeName(dataObjectType)}";
                if (!_aliasesNameToProperty.ContainsKey(typeFullName))
                {
                    _aliasesNameToProperty.Add(typeFullName, new Dictionary<string, PropertyInfo>());
                    _namePropertyToAlias.Add(dataObjectType, new Dictionary<string, string>());
                }
            }
        }

        private void BuildEdmEntityTypes()
        {
            foreach (Type dataObjectType in _metadata.Types)
            {
                Type baseType = dataObjectType.BaseType;
                Contract.Assume(baseType != null);

                // TODO: гарантирована сортировка от базового типа к дочернему?
                EdmEntityType baseEdmEntityType;
                _registeredEdmEntityTypes.TryGetValue(baseType, out baseEdmEntityType);

                EdmEntityType edmEntityType = new EdmEntityType(
                    GetEntityTypeNamespace(dataObjectType) ,//dataObjectType.Namespace,
                    GetEntityTypeName(dataObjectType), //dataObjectType.Name + "Alias", //"Alias",
                    baseEdmEntityType,
                    dataObjectType.IsAbstract,
                    !dataObjectType.IsSealed);

                BuildOwnProperties(edmEntityType, dataObjectType);

                _registeredEdmEntityTypes[dataObjectType] = edmEntityType;
                AddElement(edmEntityType);
                this.SetAnnotationValue<ClrTypeAnnotation>(edmEntityType, new ClrTypeAnnotation(dataObjectType));
            }
        }

        private void BuildOwnProperties(EdmEntityType edmEntityType, Type dataObjectType)
        {
            var settings = _metadata[dataObjectType];
            var keyPropertyType = settings.KeyType == null ? null : EdmTypeMap.GetEdmPrimitiveType(settings.KeyType);
            IEnumerable<PropertyInfo> ownProperties = settings.OwnProperties;
            foreach (var propertyInfo in ownProperties)
            {
                if (propertyInfo.Name == _metadata.KeyPropertyName && keyPropertyType != null)
                {
                    EdmStructuralProperty key = edmEntityType.AddStructuralProperty(propertyInfo.Name, keyPropertyType.PrimitiveKind);
                    edmEntityType.AddKeys(key);
                    continue;
                }

                Type propertyType = propertyInfo.PropertyType;
                if (propertyType.IsEnum)
                {
                    EdmEnumType edmEnumType;
                    if (!_registeredEnums.TryGetValue(propertyType, out edmEnumType))
                    {
                        edmEnumType = new EdmEnumType(propertyType.Namespace, propertyType.Name);

                        Array enumValues = Enum.GetValues(propertyType);
                        string[] enumNames = Enum.GetNames(propertyType);
                        for (int i = 0; i < enumNames.Length; i++)
                        {
                            int intValue = (int)enumValues.GetValue(i);
                            edmEnumType.AddMember(new EdmEnumMember(edmEnumType, enumNames[i], new EdmIntegerConstant(intValue)));
                        }

                        _registeredEnums.Add(propertyType, edmEnumType);
                        AddElement(edmEnumType);
                    }

                    var edmProp = edmEntityType.AddStructuralProperty(GetEntityPropertName(propertyInfo) /*propertyInfo.Name + "Alias"*//* "Alias"*/, new EdmEnumTypeReference(edmEnumType, false));
                    this.SetAnnotationValue(edmProp, new ClrPropertyInfoAnnotation(propertyInfo));
                }

                IEdmPrimitiveType edmPrimitiveType = EdmTypeMap.GetEdmPrimitiveType(propertyType);
                if (edmPrimitiveType != null)
                {
                    var edmProp = edmEntityType.AddStructuralProperty(GetEntityPropertName(propertyInfo) /*propertyInfo.Name + "Alias"*//*"Alias"*/, edmPrimitiveType.PrimitiveKind);
                    this.SetAnnotationValue(edmProp, new ClrPropertyInfoAnnotation(propertyInfo));
                }
            }
        }

        private void BuildEntitySets()
        {
            var container = new EdmEntityContainer("DataObject", "DataObjectContainer");
            AddElement(container);

            foreach (Type dataObjectType in _metadata.Types)
            {
                DataObjectEdmTypeSettings typeSettings = _metadata[dataObjectType];

                if (!typeSettings.EnableCollection)
                    continue;

                EdmEntityType edmEntityType = _registeredEdmEntityTypes[dataObjectType];
                EdmEntitySet edmEntitySet = container.AddEntitySet(typeSettings.CollectionName, edmEntityType);

                _registeredCollections.Add(typeSettings.CollectionName, dataObjectType);
                _registeredEntitySets.Add(dataObjectType, edmEntitySet);
            }
        }

        private void RegisterMasters()
        {
            foreach (Type dataObjectType in _metadata.Types)
            {
                DataObjectEdmTypeSettings typeSettings = _metadata[dataObjectType];
                EdmEntityType edmEntityType = _registeredEdmEntityTypes[dataObjectType];

                foreach (var masterProperty in typeSettings.MasterProperties)
                {
                    EdmEntityType edmTargetEntityType = _registeredEdmEntityTypes[masterProperty.Value.MasterType];
                    bool allowNull = masterProperty.Value.AllowNull;

                    var navigationProperty = new EdmNavigationPropertyInfo
                    {
                        Name = GetEntityPropertName(masterProperty.Key), //masterProperty.Key.Name,
                        Target = edmTargetEntityType,
                        TargetMultiplicity = allowNull
                            ? EdmMultiplicity.ZeroOrOne
                            : EdmMultiplicity.One
                    };

                    EdmNavigationProperty unidirectionalNavigation = edmEntityType.AddUnidirectionalNavigation(navigationProperty);
                    this.SetAnnotationValue(unidirectionalNavigation, new ClrPropertyInfoAnnotation(masterProperty.Key));

                    EdmEntitySet thisEdmEntitySet = _registeredEntitySets[dataObjectType];
                    EdmEntitySet targetEdmEntitySet = _registeredEntitySets[masterProperty.Value.MasterType];
                    thisEdmEntitySet.AddNavigationTarget(unidirectionalNavigation, targetEdmEntitySet);

                    // Add relation for all derived types.
                    if (_typeHierarchy.ContainsKey(dataObjectType))
                    {
                        foreach (Type derivedDataObjectType in _typeHierarchy[dataObjectType])
                        {
                            GetEdmEntitySet(derivedDataObjectType).AddNavigationTarget(unidirectionalNavigation, targetEdmEntitySet);
                        }
                    }
                }
            }
        }

        private void RegisterDetails()
        {
            foreach (Type dataObjectType in _metadata.Types)
            {
                DataObjectEdmTypeSettings typeSettings = _metadata[dataObjectType];
                EdmEntityType edmEntityType = _registeredEdmEntityTypes[dataObjectType];

                foreach (var masterProperty in typeSettings.DetailProperties)
                {
                    EdmEntityType edmTargetEntityType = _registeredEdmEntityTypes[masterProperty.Value.DetailType];

                    var navigationProperty = new EdmNavigationPropertyInfo
                    {
                        Name = GetEntityPropertName(masterProperty.Key), //masterProperty.Key.Name,
                        Target = edmTargetEntityType,
                        TargetMultiplicity = EdmMultiplicity.Many
                    };

                    EdmNavigationProperty unidirectionalNavigation = edmEntityType.AddUnidirectionalNavigation(navigationProperty);
                    this.SetAnnotationValue(unidirectionalNavigation, new ClrPropertyInfoAnnotation(masterProperty.Key));

                    EdmEntitySet thisEdmEntitySet = _registeredEntitySets[dataObjectType];
                    EdmEntitySet targetEdmEntitySet = _registeredEntitySets[masterProperty.Value.DetailType];
                    thisEdmEntitySet.AddNavigationTarget(unidirectionalNavigation, targetEdmEntitySet);

                    // Add relation for all derived types.
                    if (_typeHierarchy.ContainsKey(dataObjectType))
                    {
                        foreach (Type derivedDataObjectType in _typeHierarchy[dataObjectType])
                        {
                            GetEdmEntitySet(derivedDataObjectType).AddNavigationTarget(unidirectionalNavigation, targetEdmEntitySet);
                        }
                    }
                }
            }
        }

        public bool IsDataObjectRegistered(Type dataObjectType)
        {
            Contract.Requires<ArgumentNullException>(dataObjectType != null);

            return _metadata.Contains(dataObjectType);
        }

        /// <summary>
        /// Осуществляет получение типа EDM-сущности, соответствующего заданному типу объекта данных.
        /// </summary>
        /// <param name="dataObjectType">Тип объекта данных, для которого необходимо получить, соответствующий ему тип EDM-сущности.</param>
        /// <returns>Тип EDM-сущности, соответствующий заданному типу объекта данных.</returns>
        public IEdmEntityType GetEdmEntityType(Type dataObjectType)
        {
            Contract.Requires<ArgumentNullException>(dataObjectType != null);
            if (!_registeredEdmEntityTypes.ContainsKey(dataObjectType))
                return null;
            return _registeredEdmEntityTypes[dataObjectType];
        }

        /// <summary>
        /// Получает свойство DataObject по имени типа и свойству в edm-модели.
        /// Поиск производится по всей иерархии заданного типа.
        /// </summary>
        /// <param name="aliasFullTypeName">Полное имя типа в edm-модели.</param>
        /// <param name="aliasPropertyName">Имя свойства для типа в edm-модели.</param>
        /// <returns>Возваращает свойство.</returns>
        public PropertyInfo GetDataObjectProperty(string aliasFullTypeName, string aliasPropertyName)
        {
            if (aliasPropertyName == KeyPropertyName)
                return KeyProperty;
            var dicType = _aliasesNameToProperty[aliasFullTypeName];
            while (!dicType.ContainsKey(aliasPropertyName))
            {
                var type = _aliasesNameToType[aliasFullTypeName];
                aliasFullTypeName = _aliasesTypeToName[type.BaseType];
                dicType = _aliasesNameToProperty[aliasFullTypeName];
            }

            return dicType[aliasPropertyName];
        }

        /// <summary>
        /// Получает имя свойства DataObject по типу DataObject и имени свойства EdmType.
        /// Поиск производится по всей иерархии заданного типа.
        /// </summary>
        /// <param name="dataObjectType">Дочерний тип DataObject.</param>
        /// <param name="aliasPropertyName">Имя свойства для EdmType.</param>
        /// <returns>Возваращает имя свойства.</returns>
        public string GetDataObjectPropertyName(Type dataObjectType, string aliasPropertyName)
        {
            if (aliasPropertyName == KeyPropertyName)
                return aliasPropertyName;
            var dicType = _namePropertyToAlias[dataObjectType];
            while (!dicType.Values.Contains(aliasPropertyName))
            {
                dataObjectType = dataObjectType.BaseType;
                dicType = _namePropertyToAlias[dataObjectType];
            }

            var pair = dicType.FirstOrDefault(x => x.Value == aliasPropertyName);
            return pair.Key;
        }

        /// <summary>
        /// Получает имя свойства EdmType по типу и имени свойства DataObject.
        /// Поиск производится по всей иерархии заданного типа.
        /// </summary>
        /// <param name="dataObjectType">Дочерний тип DataObject.</param>
        /// <param name="propertyName">Имя свойства для DataObject.</param>
        /// <returns>Возваращает имя свойства.</returns>
        public string GetEdmTypePropertyName(Type dataObjectType, string propertyName)
        {
            if (propertyName == KeyPropertyName)
                return propertyName;
            var dicType = _namePropertyToAlias[dataObjectType];
            while (!dicType.ContainsKey(propertyName))
            {
                dataObjectType = dataObjectType.BaseType;
                dicType = _namePropertyToAlias[dataObjectType];
            }

            return dicType[propertyName];
        }

        private string GetEntityTypeName(Type type)
        {
            var name = type.Name;
            var nameSpace = GetEntityTypeNamespace(type);
            if (type != typeof(DataObject) && EdmModelBuilder != null && EdmModelBuilder.EntityTypeNameBuilder != null)
                name = EdmModelBuilder.EntityTypeNameBuilder(type);
            var fullname = $"{nameSpace}.{name}";
            if (!_aliasesNameToType.ContainsKey(fullname))
                _aliasesNameToType.Add(fullname, type);
            if (!_aliasesTypeToName.ContainsKey(type))
                _aliasesTypeToName.Add(type, fullname);
            return name;
        }

        private string GetEntityTypeNamespace(Type type)
        {
            var name = type.Namespace;
            if (type != typeof(DataObject) && EdmModelBuilder != null && EdmModelBuilder.EntityTypeNamespaceBuilder != null)
                name = EdmModelBuilder.EntityTypeNamespaceBuilder(type);
            if (!_aliasesNamespaceToType.ContainsKey(name))
                _aliasesNamespaceToType.Add(name, type);
            return name;
        }

        private string GetEntityPropertName(PropertyInfo prop)
        {
            var name = prop.Name;
            if (name != KeyPropertyName && prop.DeclaringType != typeof(DataObject) && EdmModelBuilder != null && EdmModelBuilder.EntityPropertyNameBuilder != null)
                name = EdmModelBuilder.EntityPropertyNameBuilder(prop);
            var typeFullName = $"{GetEntityTypeNamespace(prop.DeclaringType)}.{GetEntityTypeName(prop.DeclaringType)}";
            if (!_aliasesNameToProperty.ContainsKey(typeFullName))
            {
                _aliasesNameToProperty.Add(typeFullName, new Dictionary<string, PropertyInfo>());
                _namePropertyToAlias.Add(prop.DeclaringType, new Dictionary<string, string>());
            }

            if (!_aliasesNameToProperty[typeFullName].ContainsKey(name))
            {
                _aliasesNameToProperty[typeFullName].Add(name, prop);
                _namePropertyToAlias[prop.DeclaringType].Add(prop.Name, name);
            }

            return name;
        }

        /// <summary>
        /// Осуществляет получение представления по умолчанию, соответствующего заданному типу объекта данных.
        /// </summary>
        /// <param name="dataObjectType">Тип объекта данных, для которого требуется получить представление по умолчанию.</param>
        /// <returns>Представление по умолчанию, соответствующее заданному типу объекта данных.</returns>
        public View GetDataObjectDefaultView(Type dataObjectType)
        {
            Contract.Requires<ArgumentNullException>(dataObjectType != null);

            return _metadata[dataObjectType].DefaultView;
        }

        /// <summary>
        /// Получает список зарегистрированных в модели типов по списку имён типов.
        /// </summary>
        /// <param name="strTypes">Список имен типов.</param>
        /// <returns>Список типов.</returns>
        public List<Type> GetTypes(List<string> strTypes)
        {
            List<Type> listTypes = new List<Type>();
            foreach (string strEdmType in strTypes)
            {
                EdmEntitySet edmEntSet = (EdmEntitySet)EntityContainer.EntitySets().FirstOrDefault(el => el.Type.TypeKind == EdmTypeKind.Collection
                && ((EdmCollectionType)el.Type).ElementType.FullName() == strEdmType);
                listTypes.Add(GetDataObjectType(edmEntSet.Name));
            }

            return listTypes;
        }

        /// <summary>
        /// Осуществляет получение типа объекта данных, соответствующего заданному имени набора сущностей в EDM-модели.
        /// </summary>
        /// <param name="edmEntitySetName">Имя набора сущностей в EDM-модели, для которого требуется получить представление по умолчанию.</param>
        /// <returns>Типа объекта данных, соответствующий заданному имени набора сущностей в EDM-модели.</returns>
        public Type GetDataObjectType(string edmEntitySetName)
        {
            Contract.Requires<ArgumentNullException>(edmEntitySetName != null);
            Contract.Requires<ArgumentException>(edmEntitySetName != string.Empty);

            Type dataObjectType;
            _registeredCollections.TryGetValue(edmEntitySetName, out dataObjectType);

            return dataObjectType;
        }

        /// <summary>
        /// Получает список зарегистрированных в модели типов, которые являются дочерними к данному родительскому типу.
        /// В список добавляется также сам родительский тип.
        /// </summary>
        /// <param name="type">Родительский тип.</param>
        /// <returns>Список типов.</returns>
        public IEnumerable<Type> GetDerivedTypes(Type type)
        {
            IList<Type> list;
            if (!_typeHierarchy.TryGetValue(type, out list))
                return new[] { type };

            return list.Union(new[] { type });
        }

        /// <summary>
        /// Осуществляет получение набора EDM-сущностей, соответствующего заданному типу объекта данных.
        /// </summary>
        /// <param name="dataObjectType">Тип объекта данных, для которого необходимо получить, соответствующий ему набор.</param>
        /// <returns>Набор EDM-сущностей, соответствующий заданному типу объекта данных.</returns>
        public EdmEntitySet GetEdmEntitySet(Type dataObjectType)
        {
            Contract.Requires<ArgumentNullException>(dataObjectType != null);

            return GetEdmEntitySet(GetEdmEntityType(dataObjectType));
        }

        /// <summary>
        /// Осуществляет получение набора EDM-сущностей, соответствующего заданному типу EDM-сущности.
        /// </summary>
        /// <param name="entityType">Тип EDM-сущности, для которого необходимо получить, соответствующий ему набор.</param>
        /// <returns>Набор EDM-сущностей, соответствующий заданному типу EDM-сущности.</returns>
        public EdmEntitySet GetEdmEntitySet(IEdmEntityType entityType)
        {
            if (entityType == null)
            {
                return null;
            }

            return (EdmEntitySet)EntityContainer.EntitySets().FirstOrDefault(el => el.Type.TypeKind == EdmTypeKind.Collection
                && ((EdmCollectionType)el.Type).ElementType.FullName() == entityType.FullTypeName());
        }

        internal void AddUserFunction(Function function)
        {
            EdmFunction edmFunction;
            EdmEntityContainer entityContainer = (EdmEntityContainer)EntityContainer;
            var edmReturnType = EdmTypeMap.GetEdmPrimitiveType(function.ReturnType);
            if (edmReturnType != null)
            {
                edmFunction = new EdmFunction("DataObject", function.FunctionName, new EdmPrimitiveTypeReference(edmReturnType, false), false, null, false);
                entityContainer.AddFunctionImport(edmFunction);
            }
            else
            {
                IEdmEntityType entityType = GetEdmEntityType(function.ReturnType);
                bool isCollection = false;
                if (entityType == null)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(function.ReturnType) && function.ReturnType.IsGenericType)
                    {
                        Type[] typeParameters = function.ReturnType.GetGenericArguments();
                        if (typeParameters.Length == 1)
                        {
                            entityType = GetEdmEntityType(typeParameters[0]);
                            isCollection = true;
                        }
                    }

                    if (function.ReturnType.IsArray)
                    {
                        entityType = GetEdmEntityType(function.ReturnType.GetElementType());
                        isCollection = true;
                    }
                }

                if (entityType == null)
                    throw new ArgumentNullException("Тип возвращаемого функцией результата не найден в модели OData.");

                IEdmTypeReference returnEdmTypeReference;
                if (isCollection)
                {
                    returnEdmTypeReference = new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(entityType, false)));
                }
                else
                {
                    returnEdmTypeReference = new EdmEntityTypeReference(entityType, false);
                }

                edmFunction = new EdmFunction("DataObject", function.FunctionName, returnEdmTypeReference, true, null, true);
                edmFunction.AddParameter("bindingParameter", returnEdmTypeReference);
                entityContainer.AddFunctionImport(function.FunctionName, edmFunction, new EdmEntitySetReferenceExpression(GetEdmEntitySet(entityType)), true);
            }

            AddElement(edmFunction);

            foreach (var parameter in function.ParametersTypes.Keys)
            {
                var edmParameterType = EdmTypeMap.GetEdmPrimitiveType(function.ParametersTypes[parameter]);
                edmFunction.AddParameter(parameter, new EdmPrimitiveTypeReference(edmParameterType, false));
            }
        }
    }
}
