namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Results;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Security;
    using ICSSoft.STORMNET.UserDataTypes;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using NewPlatform.Flexberry.ORM.ODataService.Events;
    using NewPlatform.Flexberry.ORM.ODataService.Expressions;
    using NewPlatform.Flexberry.ORM.ODataService.Formatter;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Handlers;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Offline;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers;

    using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
    using OrderByQueryOption = Expressions.OrderByQueryOption;

    /// <summary>
    /// Определяет класс контроллера OData, который поддерживает запись и чтение данных с использованием OData формата.
    /// </summary>
    public partial class DataObjectController : BaseODataController
    {
        private List<string> _filterDetailProperties;
        private DataObject[] _objs;
        private LoadingCustomizationStruct _lcs;

        /// <summary>
        /// Data service for all manipulations with data.
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// Data object cache for sync loading.
        /// </summary>
        private readonly DataObjectCache _dataObjectCache;

        /// <summary>
        /// The current EDM model.
        /// </summary>
        private readonly DataObjectEdmModel _model;

        /// <summary>
        /// Используемые в запросе параметры. Заполняется в методе Init().
        /// </summary>
        public ODataQueryOptions QueryOptions { get; set; }

        /// <summary>
        /// Тип DataObject, который соответствует сущности в наборе из запроса. Заполняется в методе Init().
        /// </summary>
        public Type type { get; set; }

        /// <summary>
        /// Включать или нет в метаданные количество сущностей.
        /// </summary>
        public bool IncludeCount { get; set; }

        /// <summary>
        /// Количество сущностей в результате, которое будет указано в метаданных.
        /// </summary>
        public int Count { get; set; }

        private static readonly IAssembliesResolver _defaultAssembliesResolver = new DefaultAssembliesResolver();
        private List<Type> _lcsLoadingTypes = new List<Type>();
        private DynamicView _dynamicView;
        private Dictionary<SelectItem, ExpandedNavigationSelectItem> _parentExpandedNavigationSelectItem = new Dictionary<SelectItem, ExpandedNavigationSelectItem>();
        private Dictionary<SelectItem, string> _properties = new Dictionary<SelectItem, string>();

        internal BaseOfflineManager OfflineManager { get; set; }

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="dataService">Data service for all manipulations with data.</param>
        /// <param name="dataObjectCache">DataObject cache.</param>
        /// <param name="model">EDM model.</param>
        /// <param name="events">The container with registered events.</param>
        /// <param name="functions">The container with OData Service functions.</param>
        public DataObjectController(
            IDataService dataService,
            DataObjectCache dataObjectCache,
            DataObjectEdmModel model,
            IEventHandlerContainer events,
            IFunctionContainer functions)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService), "Contract assertion not met: dataService != null");

            if (dataObjectCache != null)
            {
                _dataObjectCache = dataObjectCache;
            }
            else
            {
                _dataObjectCache = new DataObjectCache();
                _dataObjectCache.StartCaching(false);
            }

            _model = model;
            _events = events;
            _functions = functions;

            OfflineManager = new DummyOfflineManager();
        }

        /// <summary>
        /// Обрабатывает все несопоставленные запросы OData
        /// </summary>
        /// <param name="odataPath">Путь запроса.</param>
        /// <returns>Содержит сообщение ответа для отправки в клиент после завершения.</returns>
        [AcceptVerbs("GET", "POST", "PUT", "PATCH", "MERGE", "DELETE")]
        public HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
        {
            return null;
        }

        /// <summary>
        /// Обрабатывает запросы GET, которые предпринимают попытку получить отдельную сущность. Имя "GetEntity" устанавливается в DataObjectRoutingConvention.SelectAction.
        /// </summary>
        /// <returns>Сущность</returns>
        [CustomEnableQuery]
        public HttpResponseMessage GetEntity()
        {
            try
            {
                var result = Request.CreateResponse(System.Net.HttpStatusCode.OK, EvaluateOdataPath());
                return result;
            }
            catch (Exception ex)
            {
                return InternalServerErrorMessage(ex);
            }
        }

        /// <summary>
        /// Обрабатывает запросы GET, которые предпринимают попытку получить сущности из набора сущностей.
        /// Этот метод вычисляет совпадающие сущности, применяя параметры запроса. Имя "GetCollection" устанавливается в DataObjectRoutingConvention.SelectAction.
        /// </summary>
        /// <returns>Совпадающие сущности из набора сущностей.</returns>
        [CustomEnableQuery]
        public HttpResponseMessage GetCollection()
        {
            try
            {
                var result = Request.CreateResponse(System.Net.HttpStatusCode.OK, EvaluateOdataPath());
                return result;
            }
            catch (Exception ex)
            {
                return InternalServerErrorMessage(ex);
            }
        }

        [CustomEnableQuery]
        public HttpResponseMessage GetString()
        {
            try
            {
                ODataPath odataPath = Request.ODataProperties().Path;
                var keySegment = odataPath.Segments[1] as KeySegment;
                string key = keySegment.Keys.First().Value.ToString().Trim().Replace("'", string.Empty);

                Init();
                var obj = LoadObject(type, key);
                var result = Request.CreateResponse(
                    System.Net.HttpStatusCode.OK,
                    GetEdmObject(_model.GetEdmEntityType(type), obj, 1, null, _dynamicView));

                return result;
            }
            catch (Exception ex)
            {
                return InternalServerErrorMessage(ex);
            }
        }

        /// <summary>
        /// Обрабатывает запросы GET, которые предпринимают попытку получить отдельную сущность с помощью ключа из набора сущностей.
        /// </summary>
        /// <param name="key">Ключ сущности, которую необходимо получить.</param>
        /// <returns>Сущность</returns>
        [CustomEnableQuery]
        public HttpResponseMessage GetGuid()
        {
            try
            {
                ODataPath odataPath = Request.ODataProperties().Path;
                var keySegment = odataPath.Segments[1] as KeySegment;
                Guid key = new Guid(keySegment.Keys.First().Value.ToString());

                Init();
                var obj = LoadObject(type, key);
                var result = Request.CreateResponse(
                    System.Net.HttpStatusCode.OK,
                    GetEdmObject(_model.GetEdmEntityType(type), obj, 1, null, _dynamicView));

                return result;
            }
            catch (Exception ex)
            {
                return InternalServerErrorMessage(ex);
            }
        }

        /// <summary>
        /// Обрабатывает запросы GET, которые предпринимают попытку получить сущности из набора сущностей.
        /// Этот метод вычисляет совпадающие сущности, применяя параметры запроса.
        /// </summary>
        /// <returns>Совпадающие сущности из набора сущностей.</returns>
        [CustomEnableQuery]
        public HttpResponseMessage Get()
        {
            try
            {
                Init();
                return ExecuteExpression();
            }
            catch (Exception ex)
            {
                return InternalServerErrorMessage(ex);
            }
        }

        /// <summary>
        /// Проверяет, содержит ли исключение OdataError.
        /// </summary>
        /// <param name="exception">Исключение.</param>
        /// <returns>true - если содержит.</returns>
        private bool HasOdataError(Exception exception)
        {
            HttpResponseException httpResponseException = exception as HttpResponseException;
            ObjectContent content = httpResponseException?.Response?.Content as ObjectContent;
            return content?.Value is ODataError;
        }

        /// <summary>
        /// Возвращает количество объектов для linq-выражения соответствующего параметрам запроса OData (только для $filter).
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <param name="queryOptions">Параметры запроса.</param>
        /// <returns>Количество объектов.</returns>
        public int GetObjectsCount(Type type, ODataQueryOptions queryOptions)
        {
            var expr = GetExpressionFilterOnly(type, queryOptions);
            View view = _model.GetDataObjectDefaultView(type);
            var lcs = LinqToLcs.GetLcs(expr, view);
            lcs.View = view;
            lcs.LoadingTypes = new[] { type };
            lcs.ReturnType = LcsReturnType.Objects;

            return _dataService.GetObjectsCount(lcs);
        }

        internal HttpResponseMessage CreateExcel(NameValueCollection queryParams)
        {
            View view = _dynamicView.View;
            if (_lcs != null)
            {
                view = _lcs.View;
            }

            view.Name = "View";
            ExportParams par = new ExportParams
            {
                PropertiesOrder = new List<string>(),
                View = view,
                DataObjectTypes = null,
                LimitFunction = null
            };

            var colsOrder = queryParams.Get("colsOrder").Split(',').ToList();
            par.PropertiesOrder = new List<string>();
            par.HeaderCaptions = new List<IHeaderCaption>();

            foreach (string column in colsOrder)
            {
                var columnInfo = column.Split(new char[] { '/' }, 2);
                var decodeColumnInfo0 = HttpUtility.UrlDecode(columnInfo[0]);
                var decodeColumnInfo1 = HttpUtility.UrlDecode(columnInfo[1]);

                par.PropertiesOrder.Add(decodeColumnInfo0);
                par.HeaderCaptions.Add(new HeaderCaption { PropertyName = decodeColumnInfo0, Caption = decodeColumnInfo1 });
            }

            for (int i = 0; i < view.Details.Length; i++)
            {
                DetailInView detail = view.Details[i];
                detail.View.Name = string.IsNullOrEmpty(detail.Name) ? $"ViewDetail{i}" : detail.Name;
                var column = par.HeaderCaptions.FirstOrDefault(col => col.PropertyName == detail.Name);
                if (column != null)
                {
                    column.MasterName = string.IsNullOrEmpty(view.Name) ? "View" : view.Name;
                    column.DetailName = detail.View.Name;
                }

                var properties = new List<PropertyInView>();
                for (int j = 0; j < detail.View.Properties.Length; j++)
                {
                    PropertyInView propDetail = detail.View.Properties[j];
                    if (_properties.Keys.FirstOrDefault(p => !(p is PathSelectItem) && _properties[p] == $"{detail.Name}.{propDetail.Name}") != null)
                        continue;
                    if (_properties.Keys.FirstOrDefault(p => p is PathSelectItem && _properties[p] == $"{detail.Name}.{propDetail.Name}") != null)
                        properties.Add(propDetail);
                }

                detail.View.Properties = properties.ToArray();
            }

            par.DetailsInSeparateColumns = Convert.ToBoolean(queryParams.Get("detSeparateCols"));
            par.DetailsInSeparateRows = Convert.ToBoolean(queryParams.Get("detSeparateRows"));
            MemoryStream result;
            if (_model.ODataExportService != null)
            {
                result = _model.ODataExportService.CreateExportStream(_dataService, par, _objs, queryParams);
            }
            else
            {
                result = _model.ExportService.CreateExportStream(_dataService, par, _objs);
            }

            HttpResponseMessage msg = Request.CreateResponse(HttpStatusCode.OK);
            RawOutputFormatter.PrepareHttpResponseMessage(ref msg, "application/ms-excel", _model, result.ToArray());
            msg.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            msg.Content.Headers.ContentDisposition.FileName = "list.xlsx";
            return msg;
        }

        /// <summary>
        /// Преобразует коллекцию объектов DataObject в набор сущностей.
        /// </summary>
        /// <param name="objs">Коллекция объектов DataObject.</param>
        /// <param name="type">Тип объекта DataObject.</param>
        /// <param name="level">Глубина раскрытия для навигационных свойств. Пока всегда должно быть равно 1, а если это рекурсивный вызов, то может быть равно 0.</param>
        /// <param name="expandedNavigationSelectItem">Навигационное свойство.</param>
        /// <param name="dynamicView">Динамическое представление.</param>
        /// <returns>Набор сущностей.</returns>
        internal EdmEntityObjectCollection GetEdmCollection(IEnumerable objs, Type type, int level, ExpandedNavigationSelectItem expandedNavigationSelectItem, DynamicView dynamicView = null)
        {
            if (level == 0)
                return null;
            var entityType = _model.GetEdmEntityType(type);
            List<IEdmEntityObject> edmObjList = new List<IEdmEntityObject>();

            foreach (var obj in objs)
            {
                var realType = _model.GetEdmEntityType(obj.GetType());
                var edmObj = GetEdmObject(realType, obj, level, expandedNavigationSelectItem, dynamicView);
                if (edmObj != null)
                    edmObjList.Add(edmObj);
            }

            if (IncludeCount && expandedNavigationSelectItem == null)
            {
                Request.Properties.Add(CustomODataFeedSerializer.Count, Count);
            }

            IEdmCollectionTypeReference entityCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(entityType, false)));
            EdmEntityObjectCollection collection = new EdmEntityObjectCollection(entityCollectionType, edmObjList);
            return collection;
        }

        /// <summary>
        /// Преобразует объект DataObject в сущность.
        /// </summary>
        /// <param name="entityType">Тип сущности.</param>
        /// <param name="obj">Объект DataObject.</param>
        /// <param name="level">Глубина раскрытия для навигационных свойств. Пока всегда должно быть равно 1, а если это рекурсивный вызов, то может быть равно 0.</param>
        /// <param name="expandedNavigationSelectItem">Навигационное свойство.</param>
        /// <returns>Сущность.</returns>
        public EdmEntityObject GetEdmObject(IEdmEntityType entityType, object obj, int level, ExpandedNavigationSelectItem expandedNavigationSelectItem)
        {
            return GetEdmObject(entityType, obj, level, expandedNavigationSelectItem, null);
        }

        /// <summary>
        /// Преобразует объект DataObject в сущность.
        /// </summary>
        /// <param name="entityType">Тип сущности.</param>
        /// <param name="obj">Объект DataObject.</param>
        /// <param name="level">Глубина раскрытия для навигационных свойств. Пока всегда должно быть равно 1.</param>
        /// <param name="expandedNavigationSelectItem">Навигационное свойство.</param>
        /// <param name="dynamicView">Динамическое представление.</param>
        /// <returns>Сущность.</returns>
        internal EdmEntityObject GetEdmObject(IEdmEntityType entityType, object obj, int level, ExpandedNavigationSelectItem expandedNavigationSelectItem, DynamicView dynamicView)
        {
            if (level == 0 || obj == null || (obj is DataObject dataObject && dataObject.__PrimaryKey == null))
                return null;
            EdmEntityObject entity = new EdmEntityObject(entityType);

            var expandedProperties = new Dictionary<string, ExpandedNavigationSelectItem>();
            var selectedProperties = new Dictionary<string, SelectItem>();
            IEnumerable<SelectItem> selectedItems = null;
            if (expandedNavigationSelectItem == null)
            {
                if (QueryOptions?.SelectExpand != null)
                    selectedItems = QueryOptions.SelectExpand.SelectExpandClause.SelectedItems;
            }
            else
            {
                selectedItems = expandedNavigationSelectItem.SelectAndExpand.SelectedItems;
            }

            if (selectedItems != null)
            {
                foreach (var item in selectedItems)
                {
                    var expandedItem = CastExpandedNavigationSelectItem(item);
                    if (expandedItem == null)
                    {
                        if (item is PathSelectItem pathSelectItem && pathSelectItem.SelectedPath.FirstSegment is PropertySegment propertySegment)
                        {
                            string key = propertySegment.Property.Name;
                            if (!selectedProperties.ContainsKey(key))
                            {
                                selectedProperties.Add(key, pathSelectItem);
                            }
                        }
                    }
                    else
                    {
                        expandedProperties.Add(((NavigationPropertySegment)expandedItem.PathToNavigationProperty.FirstSegment).NavigationProperty.Name, expandedItem);
                    }
                }
            }

            foreach (var prop in entityType.Properties())
            {
                string dataObjectPropName;
                try
                {
                    dataObjectPropName = _model.GetDataObjectProperty(entityType.FullTypeName(), prop.Name).Name;
                }
                catch (KeyNotFoundException)
                {
                    // Check if prop value is the link from master to pseudodetail (pseudoproperty).
                    if (HasPseudoproperty(entityType, prop.Name))
                    {
                        continue;
                    }

                    throw;
                }

                Type objectType = obj.GetType();
                PropertyInfo propertyInfo = objectType.GetProperty(dataObjectPropName);
                if (prop is EdmNavigationProperty navProp)
                {
                    if (expandedProperties.ContainsKey(navProp.Name))
                    {
                        var expandedItem = expandedProperties[navProp.Name];
                        string propPath = _properties.ContainsKey(expandedItem) ? _properties[expandedItem] : null;
                        EdmMultiplicity targetMultiplicity = navProp.TargetMultiplicity();
                        if (targetMultiplicity == EdmMultiplicity.One || targetMultiplicity == EdmMultiplicity.ZeroOrOne)
                        {
                            DataObject master = propertyInfo.GetValue(obj, null) as DataObject;
                            EdmEntityObject edmObj = null;
                            if (dynamicView == null)
                            {
                                View view;
                                if (master == null)
                                {
                                    view = _model.GetDataObjectDefaultView(objectType);
                                    obj = LoadObject(view, (DataObject)obj);
                                }

                                master = propertyInfo.GetValue(obj, null) as DataObject;
                                if (master != null)
                                {
                                    view = _model.GetDataObjectDefaultView(master.GetType());
                                    if (view != null)
                                    {
                                        master = LoadObject(view, master);
                                        edmObj = GetEdmObject(_model.GetEdmEntityType(master.GetType()), master, level, expandedItem);
                                    }
                                }
                            }
                            else
                            {
                                if (master != null)
                                {
                                    if (!DynamicView.ContainsPoperty(dynamicView.View, propPath))
                                    {
                                        _dataService.LoadObject(dynamicView.View, master, false, true, _dataObjectCache);
                                    }

                                    edmObj = GetEdmObject(_model.GetEdmEntityType(master.GetType()), master, level, expandedItem, dynamicView);
                                }
                            }

                            entity.TrySetPropertyValue(navProp.Name, edmObj);
                        }

                        if (targetMultiplicity == EdmMultiplicity.Many)
                        {
                            View view = _model.GetDataObjectDefaultView(objectType);
                            if (dynamicView == null || !DynamicView.ContainsPoperty(dynamicView.View, propPath))
                            {
                                obj = LoadObject(view, (DataObject)obj);
                            }

                            var detail = (DetailArray)propertyInfo.GetValue(obj, null);
                            IEnumerable<DataObject> objs = detail.GetAllObjects();
                            if (expandedItem.SkipOption != null)
                                objs = objs.Skip((int)expandedItem.SkipOption);
                            if (expandedItem.TopOption != null)
                                objs = objs.Take((int)expandedItem.TopOption);
                            var coll = GetEdmCollection(objs, detail.ItemType, 1, expandedItem, dynamicView);
                            if (coll != null && coll.Count > 0)
                            {
                                entity.TrySetPropertyValue(navProp.Name, coll);
                            }
                        }
                    }
                }
                else
                {
                    if (prop.Name == _model.KeyPropertyName)
                    {
                        object key = propertyInfo.GetValue(obj, null);
                        if (key is KeyGuid keyGuid)
                        {
                            entity.TrySetPropertyValue(prop.Name, keyGuid.Guid);
                        }
                        else
                        {
                            entity.TrySetPropertyValue(prop.Name, key);
                            //entity.TrySetPropertyValue(prop.Name, new Guid((string)key));
                        }

                        //KeyGuid keyGuid = (KeyGuid)obj.GetType().GetProperty(prop.Name).GetValue(obj, null);
                        //entity.TrySetPropertyValue(prop.Name, keyGuid.Guid);
                    }

                    // Обрабатывать свойство, если $select пуст, включен в $select или пустое значение недопустимо (например, Enum).
                    else if (!selectedProperties.Any()
                             || selectedProperties.ContainsKey(prop.Name)
                             || !prop.Type.IsNullable)
                    {
                        object value;
                        if (propertyInfo == null)
                        {
                            propertyInfo = objectType.GetProperty(dataObjectPropName, BindingFlags.Public | BindingFlags.FlattenHierarchy);
                            if (propertyInfo == null)
                                continue;
                            value = propertyInfo.GetValue(null);
                        }
                        else
                        {
                            try
                            {
                                value = propertyInfo.GetValue(obj, null);
                            }
                            catch (System.Exception)
                            {
                                continue;
                            }
                        }

                        Type propType = propertyInfo.PropertyType;
                        if (propType == typeof(DataObject))
                            continue;

                        // Если тип свойства относится к одному из зарегистрированных провайдеров файловых свойств,
                        // значит свойство файловое, и его нужно обработать особым образом.
                        if (FileController.HasDataObjectFileProvider(propType))
                        {
                            // Обработка файловых свойств объектов данных.
                            // ODataService будет возвращать строку с сериализованными метаданными файлового свойства.
                            if (!selectedProperties.Any() || (selectedProperties.Any() && selectedProperties.ContainsKey(dataObjectPropName)))
                            {
                                value = FileController.GetDataObjectFileProvider(propType)
                                    .GetFileDescription((DataObject)obj, dataObjectPropName)
                                    ?.ToJson();
                            }
                        }
                        else
                        {
                            // Преобразование типов для примитивных свойств.
                            if (value is KeyGuid)
                                value = ((KeyGuid)value).Guid;
                            if (value is NullableDateTime)
                                value = new DateTime(((NullableDateTime)value).Value.Ticks, DateTimeKind.Utc);
                            if (value is NullableInt)
                                value = ((NullableInt)value).Value;
                            if (value is NullableDecimal)
                                value = ((NullableDecimal)value).Value;
                            if (value is Contact)
                                value = (string)((Contact)value);
                            if (value is Event)
                                value = (string)((Event)value);
                            if (value is GeoData)
                                value = (string)((GeoData)value);
                            if (value is Image)
                                value = (string)((Image)value);
                            if (value is DateTime)
                                value = new DateTime(((DateTime)value).Ticks, DateTimeKind.Utc);
                        }

                        entity.TrySetPropertyValue(prop.Name, value);
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Возвращает linq-выражение соответствующее параметрам запроса OData.
        /// </summary>
        /// <param name="queryOpt">Параметры запроса.</param>
        /// <typeparam name="TElement">Параметр.</typeparam>
        /// <returns>Linq-выражение.</returns>
        public Expression ToExpression<TElement>(ODataQueryOptions queryOpt)
        {
            if (queryOpt == null)
                return null;
            IQueryable<TElement> queryable = Enumerable.Empty<TElement>().AsQueryable();

            // if (queryOpt.Filter != null) queryable = (IQueryable<TElement>)queryOpt.Filter.ApplyTo(queryable, new ODataQuerySettings());
            if (queryOpt.Filter != null)
                queryable = (IQueryable<TElement>)FilterApplyTo(queryOpt.Filter, queryable);
            if (queryOpt.OrderBy != null)
            {
                // queryable = queryOpt.OrderBy.ApplyTo(queryable, new ODataQuerySettings());
                queryable = new OrderByQueryOption(queryOpt.OrderBy, type).ApplyTo(queryable, new ODataQuerySettings());
            }

            if (queryOpt.Skip != null)
                queryable = queryOpt.Skip.ApplyTo(queryable, new ODataQuerySettings());
            if (queryOpt.Top != null)
                queryable = queryOpt.Top.ApplyTo(queryable, new ODataQuerySettings());
            return queryable.Expression;
        }

        /// <summary>
        /// Возвращает linq-выражение соответствующее параметрам запроса OData (только для $filter).
        /// </summary>
        /// <param name="queryOpt">Параметры запроса.</param>
        /// <typeparam name="TElement">Параметр.</typeparam>
        /// <returns>Linq-выражение.</returns>
        public Expression ToExpressionFilterOnly<TElement>(ODataQueryOptions queryOpt)
        {
            if (queryOpt == null)
                return null;
            IQueryable<TElement> queryable = Enumerable.Empty<TElement>().AsQueryable();

            // if (queryOpt.Filter != null) queryable = (IQueryable<TElement>)queryOpt.Filter.ApplyTo(queryable, new ODataQuerySettings());
            if (queryOpt.Filter != null)
                queryable = (IQueryable<TElement>)FilterApplyTo(queryOpt.Filter, queryable);
            return queryable.Expression;
        }

        /// <summary>
        /// Применяет linq-выражение соответствующее параметрам запроса OData к массиву объектов DataObject.
        /// </summary>
        /// <param name="queryOpt">Параметры запроса.</param>
        /// <param name="objs">Массив объектов DataObject.</param>
        /// <typeparam name="TElement">Параметр.</typeparam>
        /// <returns>Совпадающие с запросом OData объекты DataObject.</returns>
        public IQueryable ApplyTo<TElement>(ODataQueryOptions queryOpt, DataObject[] objs)
        {
            if (queryOpt == null)
                return null;

            IQueryable<TElement> queryable = objs.AsQueryable().Cast<TElement>();

            if (queryOpt.Filter != null)
                queryable = (IQueryable<TElement>)FilterApplyTo(queryOpt.Filter, queryable);

            if (queryOpt.Skip != null)
                queryable = queryOpt.Skip.ApplyTo(queryable, new ODataQuerySettings());

            if (queryOpt.Top != null)
                queryable = queryOpt.Top.ApplyTo(queryable, new ODataQuerySettings());

            if (queryOpt.OrderBy != null)
                queryable = queryOpt.OrderBy.ApplyTo(queryable, new ODataQuerySettings());

            return queryable;
        }

        /// <summary>
        /// Преобразует примитивное значение в результат Http для ответа.
        /// </summary>
        /// <param name="type">Тип содержимого.</param>
        /// <param name="content">Содержимое.</param>
        /// <returns>Результат Http для ответа.</returns>
        public IHttpActionResult SetResultPrimitive(Type type, object content)
        {
            MethodInfo methodSetResult = GetType().GetMethod("SetResult").MakeGenericMethod(type);
            return (IHttpActionResult)methodSetResult.Invoke(this, new[] { content });
        }

        /// <summary>
        /// Преобразует сущность или набор сущностей в результат Http для ответа.
        /// </summary>
        /// <param name="content">Содержимое.</param>
        /// <typeparam name="T">Параметр.</typeparam>
        /// <returns>Результат Http для ответа.</returns>
        public OkNegotiatedContentResult<T> SetResult<T>(T content)
        {
            return Ok(content);
        }

        /// <summary>
        /// Создаёт параметры запроса OData.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <returns>Параметры запроса OData.</returns>
        public ODataQueryOptions CreateODataQueryOptions(Type type)
        {
            return CreateODataQueryOptions(type, Request);
        }

        /// <summary>
        /// Создаёт параметры запроса OData.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <returns>Параметры запроса OData.</returns>
        public ODataQueryOptions CreateODataQueryOptions(Type type, HttpRequestMessage request)
        {
            return new ODataQueryOptions(CreateODataQueryContext(type), request);
        }

        private IQueryable FilterApplyTo(FilterQueryOption filter, IQueryable query)
        {
            ODataQuerySettings querySettings = new ODataQuerySettings();
            IAssembliesResolver assembliesResolver = _defaultAssembliesResolver;
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull("querySettings");
            }

            if (assembliesResolver == null)
            {
                throw Error.ArgumentNull("assembliesResolver");
            }

            if (type == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            FilterClause filterClause = filter.FilterClause;
            if (filterClause == null)
            {
                throw Error.ArgumentNull("filterClause");
            }

            // Ensure we have decided how to handle null propagation
            ODataQuerySettings updatedSettings = querySettings;
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings.HandleNullPropagation = HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query);
            }

            FilterBinder binder;
            try
            {
                binder = FilterBinder.Transform(filterClause, type, filter.Context.Model, assembliesResolver, updatedSettings);
            }
            catch (Exception ex)
            {
                LogService.LogDebug($"Failed to transform: {QueryOptions.Context.Path}", ex);
                throw;
            }

            _filterDetailProperties = binder.FilterDetailProperties;
            if (binder.IsOfTypesList.Count > 0)
            {
                _lcsLoadingTypes = _model.GetTypes(binder.IsOfTypesList);
            }
            else
            {
                _lcsLoadingTypes.Clear();
            }
            query = ExpressionHelpers.Where(query, binder.LinqExpression, type);
            return query;
        }

        /// <summary>
        /// Создаёт контекст запроса OData.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <returns>Контекст запроса OData.</returns>
        private ODataQueryContext CreateODataQueryContext(Type type)
        {
            // The EntitySetSegment type represents the Microsoft OData v5.7.0 EntitySetPathSegment type here.
            ODataPath path = new ODataPath(new EntitySetSegment(_model.GetEdmEntitySet(_model.GetEdmEntityType(type))));
            return new ODataQueryContext(_model, type, path);
        }

        /// <summary>
        /// Определяет сущность или коллекцию сущностей по пути OData в запросе.
        /// </summary>
        /// <returns>Сущность или коллекция сущностей.</returns>
        private IEdmObject EvaluateOdataPath()
        {
            // The EntitySetSegment type represents the Microsoft OData v5.7.0 EntitySetPathSegment type here.
            type = _model.GetDataObjectType(Request.ODataProperties().Path.Segments.OfType<EntitySetSegment>().First().Identifier);
            DetailArray detail = null;
            ODataPath odataPath = Request.ODataProperties().Path;
            var keySegment = odataPath.Segments[1] as KeySegment;
            Guid key = new Guid(keySegment.Keys.First().Value.ToString());
            IEdmEntityType entityType = null;
            var obj = LoadObject(type, key);
            if (obj == null)
            {
                throw new InvalidOperationException("Not Found OData Path Segment " + 1);
            }

            bool returnCollection = false;
            for (int i = 2; i < odataPath.Segments.Count; i++)
            {
                type = obj.GetType();
                entityType = _model.GetEdmEntityType(type);
                string propName = odataPath.Segments[i].ToString();
                EdmNavigationProperty navProp = (EdmNavigationProperty)entityType.FindProperty(propName);

                if (navProp.TargetMultiplicity() == EdmMultiplicity.One || navProp.TargetMultiplicity() == EdmMultiplicity.ZeroOrOne)
                {
                    DataObject master = (DataObject)obj.GetType().GetProperty(propName).GetValue(obj, null);
                    if (master == null)
                    {
                        View view = _model.GetDataObjectDefaultView(obj.GetType());
                        obj = LoadObject(view, obj);
                    }

                    if (master == null)
                    {
                        throw new InvalidOperationException("Not Found OData Path Segment " + i);
                    }

                    if (master != null && _model.GetDataObjectDefaultView(master.GetType()) != null)
                    {
                        master = LoadObject(_model.GetDataObjectDefaultView(master.GetType()), master);
                    }

                    obj = master;
                }

                if (navProp.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    View view = _model.GetDataObjectDefaultView(obj.GetType());
                    obj = LoadObject(view, obj);
                    detail = (DetailArray)obj.GetType().GetProperty(propName).GetValue(obj, null);
                    i++;
                    if (i == odataPath.Segments.Count)
                    {
                        returnCollection = true;
                        break;
                    }

                    keySegment = odataPath.Segments[i] as KeySegment;
                    key = new Guid(keySegment.Keys.First().Value.ToString());
                    obj = detail.GetAllObjects().FirstOrDefault(o => ((KeyGuid)o.__PrimaryKey).Guid == key);
                    if (obj == null)
                    {
                        throw new InvalidOperationException("Not Found OData Path Segment " + i);
                    }
                }
            }

            entityType = _model.GetEdmEntityType(obj.GetType());
            if (returnCollection)
            {
                type = detail.ItemType;
            }
            else
            {
                type = obj.GetType();
            }

            QueryOptions = new ODataQueryOptions(new ODataQueryContext(_model, type, Request.ODataProperties().Path), Request);
            if (QueryOptions.SelectExpand != null && QueryOptions.SelectExpand.SelectExpandClause != null)
            {
                Request.ODataProperties().SelectExpandClause = QueryOptions.SelectExpand.SelectExpandClause;
            }

            if (returnCollection)
            {
                IQueryable queryable = ApplyExpression(type, QueryOptions, detail.GetAllObjects());
                return GetEdmCollection(queryable, type, 1, null);
            }

            return GetEdmObject(entityType, obj, 1, null);
        }

        /// <summary>
        /// Возвращает linq-выражение соответствующее параметрам запроса OData.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <param name="queryOptions">Параметры запроса.</param>
        /// <returns>Linq-выражение.</returns>
        private Expression GetExpression(Type type, ODataQueryOptions queryOptions)
        {
            MethodInfo methodToExpression = GetType().GetMethod("ToExpression").MakeGenericMethod(type);
            return (Expression)methodToExpression.Invoke(this, new object[] { queryOptions });
        }

        /// <summary>
        /// Возвращает linq-выражение соответствующее параметрам запроса OData (только для $filter).
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <param name="queryOptions">Параметры запроса.</param>
        /// <returns>Linq-выражение.</returns>
        private Expression GetExpressionFilterOnly(Type type, ODataQueryOptions queryOptions)
        {
            MethodInfo methodToExpression = GetType().GetMethod("ToExpressionFilterOnly").MakeGenericMethod(type);
            return (Expression)methodToExpression.Invoke(this, new object[] { queryOptions });
        }

        /// <summary>
        /// Применяет linq-выражение соответствующее параметрам запроса OData к массиву объектов DataObject.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <param name="queryOptions">Параметры запроса.</param>
        /// <param name="objs">Массив объектов DataObject.</param>
        /// <returns>Совпадающие с запросом OData объекты DataObject.</returns>
        private IQueryable ApplyExpression(Type type, ODataQueryOptions queryOptions, DataObject[] objs)
        {
            MethodInfo methodToExpression = GetType().GetMethod("ApplyTo").MakeGenericMethod(type);
            return (IQueryable)methodToExpression.Invoke(this, new object[] { queryOptions, objs });
        }

        /// <summary>
        /// Возвращает набор сущностей, соответствующий параметрам запроса OData.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <returns>Набор сущностей.</returns>
        private HttpResponseMessage ExecuteExpression()
        {
            _objs = new DataObject[0];
            _lcs = CreateLcs();
            int count = -1;
            bool callExecuteCallbackBeforeGet = true;
            IncludeCount = false;
            if (QueryOptions.Count != null && QueryOptions.Count.Value)
            {
                IncludeCount = true;
                int returnTop = _lcs.ReturnTop;
                var rowNumber = _lcs.RowNumber;
                _lcs.RowNumber = null;
                _lcs.ReturnTop = 0;
                _objs = LoadObjects(_lcs, out count, callExecuteCallbackBeforeGet, true);
                _lcs.RowNumber = rowNumber;
                _lcs.ReturnTop = returnTop;
                callExecuteCallbackBeforeGet = false;
                Count = count;
            }

            if (!IncludeCount || count != 0)
                _objs = LoadObjects(_lcs, out count, callExecuteCallbackBeforeGet, false);

            NameValueCollection queryParams = Request.RequestUri.ParseQueryString();

            if ((_model.ExportService != null || _model.ODataExportService != null) && (Request.Properties.ContainsKey(PostPatchHandler.AcceptApplicationMsExcel) || Convert.ToBoolean(queryParams.Get("exportExcel"))))
            {
                return CreateExcel(queryParams);
            }

            HttpResponseMessage msg = null;
            EdmEntityObjectCollection edmCol = null;
            edmCol = GetEdmCollection(_objs, type, 1, null, _dynamicView);
            msg = Request.CreateResponse(HttpStatusCode.OK, edmCol);
            return msg;
        }

        public LoadingCustomizationStruct CreateLcs()
        {
            Expression expr = GetExpression(type, QueryOptions);
            if (_filterDetailProperties != null && _filterDetailProperties.Count > 0)
            {
                CreateDynamicView();
                _filterDetailProperties = null;
            }

            View view = _model.GetDataObjectDefaultView(type);
            if (_dynamicView != null)
                view = _dynamicView.View;
            IEnumerable<View> resolvingViews;
            view = DynamicView.GetViewWithPropertiesUsedInExpression(expr, type, view, _dataService, out resolvingViews);
            if (_lcsLoadingTypes.Count == 0)
                _lcsLoadingTypes = _model.GetDerivedTypes(type).ToList();

            for (int i = 0; i < _lcsLoadingTypes.Count; i++)
            {
                if (!Information.IsStoredType(_lcsLoadingTypes[i]))
                {
                    _lcsLoadingTypes.RemoveAt(i);
                    i--;
                }
            }

            LoadingCustomizationStruct lcs = new LoadingCustomizationStruct(null);
            if (expr != null)
            {
                lcs = LinqToLcs.GetLcs(expr, view, resolvingViews);
            }

            lcs.View = view;
            lcs.LoadingTypes = new Type[_lcsLoadingTypes.Count];

            for (int i = 0; i < _lcsLoadingTypes.Count; i++)
            {
                lcs.LoadingTypes[i] = _lcsLoadingTypes[i];
            }

            return lcs;
        }

        /// <summary>
        /// Инициализирует переменные класса значениями из запроса OData.
        /// </summary>
        private void Init()
        {
            // The EntitySetSegment type represents the Microsoft OData v5.7.0 EntitySetPathSegment type here.
            type = _model.GetDataObjectType(Request.ODataProperties().Path.Segments.OfType<EntitySetSegment>().First().Identifier);
            QueryOptions = new ODataQueryOptions(new ODataQueryContext(_model, type, Request.ODataProperties().Path), Request);
            try
            {
                var selectExpandClause = QueryOptions.SelectExpand?.SelectExpandClause;
                if (selectExpandClause != null)
                {
                    Request.ODataProperties().SelectExpandClause = selectExpandClause;
                }
            }
            catch (Exception e)
            {
                LogService.LogDebug($"Failed to get {nameof(SelectExpandQueryOption.SelectExpandClause)}: {QueryOptions.Context.Path}", e);
                throw;
            }

            CreateDynamicView();
        }

        /// <summary>
        /// Возвращает объект DataObject для данного ключа.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <param name="key">Ключ объекта DataObject.</param>
        /// <returns>Объект DataObject для данного ключа.</returns>
        private DataObject LoadObject(Type type, string key)
        {
            View view = _model.GetDataObjectDefaultView(type);
            return LoadObject(type, view, key);
        }

        /// <summary>
        /// Возвращает объект DataObject для данного ключа.
        /// </summary>
        /// <param name="type">Тип DataObject.</param>
        /// <param name="key">Ключ объекта DataObject.</param>
        /// <returns>Объект DataObject для данного ключа.</returns>
        private DataObject LoadObject(Type type, Guid key)
        {
            View view = _model.GetDataObjectDefaultView(type);
            return LoadObject(type, view, key);
        }

        private DataObject LoadObject(View view, DataObject obj)
        {
            return LoadObject(obj.GetType(), view, obj.__PrimaryKey);
        }

        /// <summary>
        /// Получить объект данных по ключу.
        /// </summary>
        /// <param name="objType"> Тип объекта.</param>
        /// <param name="view"> Представление.</param>
        /// <param name="keyValue"> Значение ключа.</param>
        /// <returns> Объект данных.</returns>
        private DataObject LoadObject(Type objType, View view, object keyValue)
        {
            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(objType, _dynamicView.View);
            lcs.LimitFunction = FunctionBuilder.BuildEquals(keyValue);
            int count = -1;
            DataObject[] dobjs = LoadObjects(lcs, out count);
            if (dobjs.Length > 0)
                return dobjs[0];
            return null;
        }

        /// <summary>
        /// Получает объекты или количество объектов для заданной lcs.
        /// </summary>
        /// <param name="lcs">LoadingCustomizationStruct.</param>
        /// <param name="count">В этом параметре веренётся количество объектов, если параметр callGetObjectsCount установлен в true, иначе -1.</param>
        /// <param name="callExecuteCallbackBeforeGet">Задаёт будет ли вызваться метод ExecuteCallbackBeforeGet.</param>
        /// <param name="callGetObjectsCount">Задаёт будет ли вызваться метод GetObjectsCount вместо LoadObjects у сервиса данных.</param>
        /// <returns>Если параметр callGetObjectsCount установлен в false, то возвращаются объекты, иначе пустой массив объектов.</returns>
        private DataObject[] LoadObjects(LoadingCustomizationStruct lcs, out int count, bool callExecuteCallbackBeforeGet = true, bool callGetObjectsCount = false, bool callExecuteCallbackAfterGet = true)
        {
            foreach (var propType in Information.GetAllTypesFromView(lcs.View))
            {
                if (!_dataService.SecurityManager.AccessObjectCheck(propType, tTypeAccess.Full, false))
                {
                    _dataService.SecurityManager.AccessObjectCheck(propType, tTypeAccess.Read, true);
                }
            }

            DataObject[] dobjs = new DataObject[0];
            bool doLoad = true;
            count = -1;
            if (callExecuteCallbackBeforeGet)
                doLoad = ExecuteCallbackBeforeGet(ref lcs);
            if (doLoad)
            {
                if (!callGetObjectsCount)
                {
                    dobjs = _dataService.LoadObjects(lcs, _dataObjectCache);
                }
                else
                {
                    count = _dataService.GetObjectsCount(lcs);
                }
            }

            if (!OfflineManager.LockObjects(QueryOptions, dobjs))
                throw new OperationCanceledException(); // TODO

            if (callExecuteCallbackAfterGet)
                ExecuteCallbackAfterGet(ref dobjs);

            return dobjs;
        }

        private ExpandedNavigationSelectItem CastExpandedNavigationSelectItem(SelectItem item)
        {
            if (!(item is ExpandedNavigationSelectItem))
                return null;
            var expandedItem = (ExpandedNavigationSelectItem)item;
            if (!(expandedItem.PathToNavigationProperty.FirstSegment is NavigationPropertySegment))
                return null;
            return expandedItem;
        }

        private void CreateDynamicView()
        {
            if (QueryOptions.SelectExpand == null || QueryOptions.SelectExpand.SelectExpandClause == null)
            {
                var properties = DynamicView.GetProperties(type);
                if (_filterDetailProperties != null && _filterDetailProperties.Count > 0)
                {
                    properties.AddRange(_filterDetailProperties);
                }

                _dynamicView = DynamicView.Create(type, properties /*, _model.DynamicViewCache */); // TODO: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                return;
            }

            List<string> props = new List<string>();
            if (QueryOptions.SelectExpand.SelectExpandClause.AllSelected)
            {
                var props2 = DynamicView.GetProperties(type);
                props.AddRange(props2);
            }

            GetPropertiesForDynamicView(null, QueryOptions.SelectExpand.SelectExpandClause.SelectedItems);
            foreach (SelectItem item in _properties.Keys)
            {
                var name = _properties[item];
                ExpandedNavigationSelectItem expandedItem = item as ExpandedNavigationSelectItem;
                if (expandedItem != null && expandedItem.SelectAndExpand.AllSelected)
                {
                    var edmType = ((NavigationPropertySegment)expandedItem.PathToNavigationProperty.FirstSegment).EdmType;
                    string typeName;
                    if (edmType is EdmCollectionType)
                    {
                        typeName = (edmType as EdmCollectionType).ElementType.FullName();
                    }
                    else
                    {
                        typeName = (edmType as EdmEntityType).FullName();
                    }

                    var types = _model.GetTypes(new List<string>() { typeName });
                    var props2 = DynamicView.GetProperties(types[0]);
                    for (int i = 0; i < props2.Count; i++)
                    {
                        props2[i] = $"{name}.{props2[i]}";
                    }

                    props.AddRange(props2);
                }
                else
                {
                    props.Add(name);
                }
            }

            if (_filterDetailProperties != null && _filterDetailProperties.Count > 0)
            {
                props.AddRange(_filterDetailProperties);
            }

            _dynamicView = DynamicView.Create(type, props /*, _model.DynamicViewCache */); // TODO: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        private void GetPropertiesForDynamicView(ExpandedNavigationSelectItem parent, IEnumerable<SelectItem> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                _parentExpandedNavigationSelectItem[item] = parent;
                _properties[item] = GetPropertyName(item);
                var expandedItem = CastExpandedNavigationSelectItem(item);
                if (expandedItem == null)
                {
                    continue;
                }

                GetPropertiesForDynamicView(expandedItem, expandedItem.SelectAndExpand.SelectedItems);
            }
        }

        private string GetPropertyName(SelectItem item)
        {
            if (item == null)
                return null;
            IEdmProperty itemProperty;
            PathSelectItem pathSelectItem = item as PathSelectItem;
            ExpandedNavigationSelectItem expandedItem = item as ExpandedNavigationSelectItem;
            if (pathSelectItem != null && pathSelectItem.SelectedPath.FirstSegment is NavigationPropertySegment)
            {
                itemProperty = (pathSelectItem.SelectedPath.FirstSegment as NavigationPropertySegment).NavigationProperty;
            }
            else
            {
                if (pathSelectItem != null)
                {
                    var firstSegment = pathSelectItem.SelectedPath.FirstSegment as PropertySegment;
                    if (firstSegment != null)
                    {
                        itemProperty = firstSegment.Property;
                    }
                    else
                    {
                        throw new NotImplementedException("-solo-");
                        /*-solo-
                        var openPropertySegment = pathSelectItem.SelectedPath.FirstSegment as OpenPropertySegment;
                        if (openPropertySegment != null)
                        {
                            throw new Exception($"Property name does not exist: {openPropertySegment.PropertyName}");
                        }

                        throw new Exception($"Invalid segment: {pathSelectItem.SelectedPath.FirstSegment.ToString()}");
                        */
                    }
                }
                else
                {
                    itemProperty = (expandedItem.PathToNavigationProperty.FirstSegment as NavigationPropertySegment).NavigationProperty;
                }
            }

            string itemName = _model.GetDataObjectProperty(itemProperty.DeclaringType.FullTypeName(), itemProperty.Name).Name;
            string parentName = null;
            var parentExpandedItem = _parentExpandedNavigationSelectItem[item];
            while (parentExpandedItem != null)
            {
                IEdmProperty property = (parentExpandedItem.PathToNavigationProperty.FirstSegment as NavigationPropertySegment).NavigationProperty;
                string name = _model.GetDataObjectProperty(property.DeclaringType.FullTypeName(), property.Name).Name;
                if (parentName == null)
                {
                    parentName = name;
                }
                else
                {
                    parentName = $"{name}.{parentName}";
                }

                parentExpandedItem = _parentExpandedNavigationSelectItem[parentExpandedItem];
            }

            if (parentName == null)
                return itemName;
            return $"{parentName}.{itemName}";
        }

        private bool HasPseudoproperty(IEdmEntityType entityType, string propertyName)
        {
            Type masterType = EdmLibHelpers.GetClrType(entityType.ToEdmTypeReference(true), _model);
            IDataObjectEdmModelBuilder builder = (_model as DataObjectEdmModel).EdmModelBuilder;

            return builder.GetPseudoDetail(masterType, propertyName) != null;
        }
    }
}
