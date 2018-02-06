namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using Microsoft.Spatial;
    using System;
    using Action = NewPlatform.Flexberry.ORM.ODataService.Functions.Action;
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

        /// <summary>
        /// Ссылка на IDataObjectEdmModelBuilder.
        /// </summary>
        public IDataObjectEdmModelBuilder EdmModelBuilder { get; set; }
        
        /// <summary>
        /// Имя свойства ключа.
        /// </summary>
        public string KeyPropertyName => _metadata.KeyPropertyName;
        
        /// <summary>
        /// Описание свойства ключа.
        /// </summary>
        public PropertyInfo KeyProperty => _metadata.KeyProperty;

        private const string DefaultNamespace = "DataObject";

        /// <summary>
        /// Словарь, в котором ключ алиас типа, а значение сам тип.
        /// </summary>
        private readonly IDictionary<string, Type> _aliasesNameToType = new Dictionary<string, Type>();

        /// <summary>
        /// Словарь, в котором ключ сам тип, а значение - алиас типа.
        /// </summary>
        private readonly IDictionary<Type, string> _aliasesTypeToName = new Dictionary<Type, string>();

        /// <summary>
        /// Словарь, в котором ключ алиас пространства имен, а значение - сам тип.
        /// </summary>
        private readonly IDictionary<string, Type> _aliasesNamespaceToType = new Dictionary<string, Type>();

        /// <summary>
        /// Словарь, в котором составной ключ - это алиас полного имени типа и алиас свойства, а значение - само свойство.
        /// </summary>
        private readonly IDictionary<string, IDictionary<string, PropertyInfo>> _aliasesNameToProperty = new Dictionary<string, IDictionary<string, PropertyInfo>>();

        /// <summary>
        /// Словарь, в котором составной ключ - это тип и имя свойства, а значение - алиас имени свойства.
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
            RegisterGeoIntersectsFunction();
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
                    GetEntityTypeNamespace(dataObjectType),
                    GetEntityTypeName(dataObjectType),
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

                    EdmStructuralProperty edmProp = edmEntityType.AddStructuralProperty(GetEntityPropertName(propertyInfo), new EdmEnumTypeReference(edmEnumType, false));
                    this.SetAnnotationValue(edmProp, new ClrPropertyInfoAnnotation(propertyInfo));
                }

                IEdmPrimitiveType edmPrimitiveType = EdmTypeMap.GetEdmPrimitiveType(propertyType);
                if (edmPrimitiveType != null)
                {
                    EdmStructuralProperty edmProp = edmEntityType.AddStructuralProperty(GetEntityPropertName(propertyInfo) , edmPrimitiveType.PrimitiveKind);
                    this.SetAnnotationValue(edmProp, new ClrPropertyInfoAnnotation(propertyInfo));
                }
            }
        }

        private void BuildEntitySets()
        {
            var container = new EdmEntityContainer(DefaultNamespace, "DataObjectContainer");
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
                    if (!_registeredEdmEntityTypes.ContainsKey(masterProperty.Value.MasterType))
                    {
                        throw new Exception($"Тип мастера {masterProperty.Value.MasterType.FullName} не найден для типа {dataObjectType.FullName}.");
                    }

                    EdmEntityType edmTargetEntityType = _registeredEdmEntityTypes[masterProperty.Value.MasterType];
                    bool allowNull = masterProperty.Value.AllowNull;

                    var navigationProperty = new EdmNavigationPropertyInfo
                    {
                        Name = GetEntityPropertName(masterProperty.Key),
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

                foreach (var detailProperty in typeSettings.DetailProperties)
                {
                    if (!_registeredEdmEntityTypes.ContainsKey(detailProperty.Value.DetailType))
                    {
                        throw new Exception($"Тип детейла {detailProperty.Value.DetailType.FullName} не найден для типа {dataObjectType.FullName}.");
                    }

                    EdmEntityType edmTargetEntityType = _registeredEdmEntityTypes[detailProperty.Value.DetailType];

                    var navigationProperty = new EdmNavigationPropertyInfo
                    {
                        Name = GetEntityPropertName(detailProperty.Key),
                        Target = edmTargetEntityType,
                        TargetMultiplicity = EdmMultiplicity.Many
                    };

                    EdmNavigationProperty unidirectionalNavigation = edmEntityType.AddUnidirectionalNavigation(navigationProperty);
                    this.SetAnnotationValue(unidirectionalNavigation, new ClrPropertyInfoAnnotation(detailProperty.Key));

                    EdmEntitySet thisEdmEntitySet = _registeredEntitySets[dataObjectType];
                    EdmEntitySet targetEdmEntitySet = _registeredEntitySets[detailProperty.Value.DetailType];
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
        /// Осуществляет получение типа EDM-перечисления, соответствующего заданному типу clr-перечисления.
        /// </summary>
        /// <param name="enumType">Тип объекта данных, для которого необходимо получить, соответствующий ему тип EDM-перечисления.</param>
        /// <returns>Тип EDM-перечисления, соответствующий заданному типу clr-перечисления.</returns>
        public EdmEnumType GetEdmEnumType(Type enumType)
        {
            Contract.Requires<ArgumentNullException>(enumType != null);
            if (!_registeredEnums.ContainsKey(enumType))
                return null;
            return _registeredEnums[enumType];
        }

        /// <summary>
        /// Получает свойство <see cref="DataObject"/> по имени типа и свойству в edm-модели.
        /// Поиск производится по всей иерархии заданного типа.
        /// </summary>
        /// <param name="aliasFullTypeName">Полное имя типа в edm-модели.</param>
        /// <param name="aliasPropertyName">Имя свойства для типа в edm-модели.</param>
        /// <returns>Возваращает свойство.</returns>
        public PropertyInfo GetDataObjectProperty(string aliasFullTypeName, string aliasPropertyName)
        {
            if (aliasPropertyName == KeyPropertyName)
                return KeyProperty;
            IDictionary<string, PropertyInfo> dicType = _aliasesNameToProperty[aliasFullTypeName];
            while (!dicType.ContainsKey(aliasPropertyName))
            {
                Type type = _aliasesNameToType[aliasFullTypeName];
                aliasFullTypeName = _aliasesTypeToName[type.BaseType];
                dicType = _aliasesNameToProperty[aliasFullTypeName];
            }

            return dicType[aliasPropertyName];
        }

        /// <summary>
        /// Получает имя свойства <see cref="DataObject"/> по типу DataObject и имени свойства EdmType.
        /// Поиск производится по всей иерархии заданного типа.
        /// </summary>
        /// <param name="dataObjectType">Дочерний тип DataObject.</param>
        /// <param name="aliasPropertyName">Имя свойства для EdmType.</param>
        /// <returns>Возваращает имя свойства.</returns>
        public string GetDataObjectPropertyName(Type dataObjectType, string aliasPropertyName)
        {
            if (aliasPropertyName == KeyPropertyName)
                return aliasPropertyName;
            IDictionary<string, string> dicType = _namePropertyToAlias[dataObjectType];
            while (!dicType.Values.Contains(aliasPropertyName))
            {
                dataObjectType = dataObjectType.BaseType;
                dicType = _namePropertyToAlias[dataObjectType];
            }

            KeyValuePair<string, string> pair = dicType.FirstOrDefault(x => x.Value == aliasPropertyName);
            return pair.Key;
        }

        /// <summary>
        /// Получает имя свойства <see cref="EdmType"/> по типу и имени свойства DataObject.
        /// Поиск производится по всей иерархии заданного типа.
        /// </summary>
        /// <param name="dataObjectType">Дочерний тип <see cref="DataObject"/>.</param>
        /// <param name="propertyName">Имя свойства для <see cref="DataObject"/>.</param>
        /// <returns>Возваращает имя свойства.</returns>
        public string GetEdmTypePropertyName(Type dataObjectType, string propertyName)
        {
            if (propertyName == KeyPropertyName)
                return propertyName;
            IDictionary<string, string> dicType = _namePropertyToAlias[dataObjectType];
            while (!dicType.ContainsKey(propertyName))
            {
                dataObjectType = dataObjectType.BaseType;
                dicType = _namePropertyToAlias[dataObjectType];
            }

            return dicType[propertyName];
        }

        /// <summary>
        /// Строит имя типа сущности по clr типу.
        /// </summary>
        /// <param name="type">Тип</param>
        /// <returns></returns>
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

        /// <summary>
        /// Строит Namespace типа сущности по clr типу.
        /// </summary>
        /// <param name="type">Тип</param>
        /// <returns></returns>
        private string GetEntityTypeNamespace(Type type)
        {
            var name = type.Namespace;
            if (type != typeof(DataObject) && EdmModelBuilder != null && EdmModelBuilder.EntityTypeNamespaceBuilder != null)
                name = EdmModelBuilder.EntityTypeNamespaceBuilder(type);
            if (!_aliasesNamespaceToType.ContainsKey(name))
                _aliasesNamespaceToType.Add(name, type);
            return name;
        }

        /// <summary>
        /// Строит имя типа свойства сущности по clr типу.
        /// </summary>
        /// <param name="type">Тип</param>
        /// <returns></returns>
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

        internal void AddAction(Action action)
        {
            EdmAction edmAction;
            EdmEntityContainer entityContainer = (EdmEntityContainer)EntityContainer;
            IEdmTypeReference edmTypeReference = GetNotEntityTypeReference(action.ReturnType);
            if (edmTypeReference == null)
            {
                edmTypeReference = GetCollectionNotEntityTypeReference(action.ReturnType);
            }

            if (edmTypeReference != null)
            {
                edmAction = new EdmAction(DefaultNamespace, action.Name, edmTypeReference);
                entityContainer.AddActionImport(edmAction);
            }
            else if (action.ReturnType == typeof(void))
            {
                edmAction = new EdmAction(DefaultNamespace, action.Name, null);
                entityContainer.AddActionImport(edmAction);
            }
            else
            {
                IEdmTypeReference returnEdmTypeReference = GetEdmTypeReference(action.ReturnType, out IEdmEntityType returnEntityType, out bool isCollection);
                if (returnEntityType == null)
                {
                    throw new ArgumentNullException("Тип возвращаемого результата action не найден в модели OData.");
                }

                edmAction = new EdmAction(DefaultNamespace, action.Name, returnEdmTypeReference);
                edmAction.AddParameter("bindingParameter", returnEdmTypeReference);
                entityContainer.AddActionImport(action.Name, edmAction, new EdmEntitySetReferenceExpression(GetEdmEntitySet(returnEntityType)));
            }

            AddElement(edmAction);
            foreach (var parameter in action.ParametersTypes.Keys)
            {
                Type paramType = action.ParametersTypes[parameter];
                edmTypeReference = GetEdmTypeReference(paramType, out IEdmEntityType entityType, out bool isCollection);
                if (edmTypeReference == null)
                {
                    edmTypeReference = GetNotEntityTypeReference(paramType);
                    if (edmTypeReference == null)
                    {
                        edmTypeReference = GetCollectionNotEntityTypeReference(paramType);
                    }
                }

                if (edmTypeReference != null)
                {
                    edmAction.AddParameter(parameter, edmTypeReference);
                }
            }
        }

        internal void AddUserFunction(Function function)
        {
            if (function is Action)
            {
                AddAction(function as Action);
                return;
            }

            EdmFunction edmFunction;
            EdmEntityContainer entityContainer = (EdmEntityContainer)EntityContainer;
            IEdmTypeReference edmTypeReference = GetNotEntityTypeReference(function.ReturnType);
            if (edmTypeReference == null)
            {
                edmTypeReference = GetCollectionNotEntityTypeReference(function.ReturnType);
            }

            if (edmTypeReference != null)
            {
                edmFunction = new EdmFunction(DefaultNamespace, function.Name, edmTypeReference, false, null, false);
                entityContainer.AddFunctionImport(edmFunction);
            }
            else
            {
                IEdmTypeReference returnEdmTypeReference = GetEdmTypeReference(function.ReturnType, out IEdmEntityType returnEntityType, out bool isCollection);
                if (returnEntityType == null)
                {
                    throw new ArgumentNullException("Тип возвращаемого результата пользовательской функции не найден в модели OData.");
                }

                edmFunction = new EdmFunction(DefaultNamespace, function.Name, returnEdmTypeReference, true, null, true);
                edmFunction.AddParameter("bindingParameter", returnEdmTypeReference);
                entityContainer.AddFunctionImport(function.Name, edmFunction, new EdmEntitySetReferenceExpression(GetEdmEntitySet(returnEntityType)), true);
            }

            AddElement(edmFunction);
            foreach (var parameter in function.ParametersTypes.Keys)
            {
                Type paramType = function.ParametersTypes[parameter];
                IEdmEnumType enumType = GetEdmEnumType(paramType);
                var edmParameterType = EdmTypeMap.GetEdmPrimitiveType(paramType);
                if (edmParameterType != null)
                {
                    edmFunction.AddParameter(parameter, new EdmPrimitiveTypeReference(edmParameterType, false));
                }

                if (enumType != null)
                {
                    edmFunction.AddParameter(parameter, new EdmEnumTypeReference(enumType, false));
                }
            }
        }

        private IEdmTypeReference GetEdmTypeReference(Type clrType, out IEdmEntityType entityType, out bool isCollection)
        {
            isCollection = false;
            IEdmTypeReference returnEdmTypeReference = null;
            entityType = GetEdmEntityType(clrType);

            if (entityType == null)
            {
                entityType = GetCollectionItemEdmEntityType(clrType);
                if (entityType != null)
                {
                    returnEdmTypeReference = new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(entityType, true)));
                    isCollection = true;
                }
            }
            else
            {
                returnEdmTypeReference = new EdmEntityTypeReference(entityType, true);
            }

            return returnEdmTypeReference;
        }

        private IEdmTypeReference GetCollectionNotEntityTypeReference(Type clrType)
        {
            IEdmTypeReference typeRef = null;
            if (typeof(IEnumerable).IsAssignableFrom(clrType) && clrType.IsGenericType)
            {
                Type[] typeParameters = clrType.GetGenericArguments();
                if (typeParameters.Length == 1)
                {
                    typeRef = GetNotEntityTypeReference(typeParameters[0]);
                }
            }
            else if (clrType.IsArray)
            {
                typeRef = GetNotEntityTypeReference(clrType.GetElementType());
            }

            if (typeRef != null)
            {
                typeRef = new EdmCollectionTypeReference(new EdmCollectionType(typeRef));
            }

            return typeRef;
        }

        private IEdmTypeReference GetNotEntityTypeReference(Type clrType)
        {
            IEdmType edmType = EdmTypeMap.GetEdmPrimitiveType(clrType);
            if (edmType != null)
            {
                return new EdmPrimitiveTypeReference(edmType as IEdmPrimitiveType, false);
            }

            edmType = GetEdmEnumType(clrType);
            if (edmType != null)
            {
                return new EdmEnumTypeReference(edmType as IEdmEnumType, false);
            }

            return null;
        }

        private IEdmEntityType GetCollectionItemEdmEntityType(Type clrType)
        {
            IEdmEntityType entityType = null;
            if (typeof(IEnumerable).IsAssignableFrom(clrType) && clrType.IsGenericType)
            {
                Type[] typeParameters = clrType.GetGenericArguments();
                if (typeParameters.Length == 1)
                {
                    entityType = GetEdmEntityType(typeParameters[0]);
                }
            }

            if (clrType.IsArray)
            {
                entityType = GetEdmEntityType(clrType.GetElementType());
            }

            return entityType;
        }

        private void RegisterGeoIntersectsFunction()
        {
            EdmFunction edmFunction;
            var edmReturnType = EdmTypeMap.GetEdmPrimitiveType(typeof(bool));
            edmFunction = new EdmFunction("geo", "intersects", new EdmPrimitiveTypeReference(edmReturnType, false), false, null, true);
            AddElement(edmFunction);
            var edmParameterType = EdmTypeMap.GetEdmPrimitiveType(typeof(Geography));
            edmFunction.AddParameter("geography1", new EdmPrimitiveTypeReference(edmParameterType, false));
            edmFunction.AddParameter("geography2", new EdmPrimitiveTypeReference(edmParameterType, false));
        }
    }
}
