namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers
{
    using Microsoft.Spatial;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Script.Serialization;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.UserDataTypes;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using Newtonsoft.Json;
    using ODataService.Model;
    using ICSSoft.STORMNET.Business.LINQProvider.Extensions;

    /// <summary>
    /// Класс, представляющий объект данных <see cref="DataObject"/> в виде словаря <see cref="Dictionary{String,Object}"/>.
    /// </summary>
    public class DataObjectDictionary : Dictionary<string, object>
    {
        /// <summary>
        /// Префикс для имен свойств, содержащих метаданные.
        /// </summary>
        private const string MetaDataPrefix = "@odata";

        /// <summary>
        /// Инициализирует пустой словарь, представляющий объект данных <see cref="DataObject"/> в виде словаря <see cref="Dictionary{String,Object}"/>.
        /// </summary>
        public DataObjectDictionary()
            : base()
        {
        }

        /// <summary>
        /// Инициализирует словарь, представляющий объект данных <see cref="DataObject"/> в виде словаря <see cref="Dictionary{String,Object}"/>.
        /// </summary>
        /// <param name="dataObject">Объект данных, который нужно представить в виде словаря.</param>
        /// <param name="dataObjectView">Представление, по которому определены свойства для конвертации объекта в словаря.</param>
        /// <param name="model">Edm-модель, указанная в <see cref="ManagementToken"/>.</param>
        /// <param name="serializeValues">Флаг: Нужно ли сериализовывать значения свойств объекта данных.</param>
        public DataObjectDictionary(DataObject dataObject, View dataObjectView, DataObjectEdmModel model, bool serializeValues = false)
            : base()
        {
            if (dataObject == null || dataObjectView == null)
            {
                return;
            }

            Type dataObjectType = dataObject.GetType();
            Type viewDefinedType = dataObjectView.DefineClassType;
            if (dataObjectType != viewDefinedType)
            {
                throw new ArgumentException(string.Format(
                    "Given view is defined for \"{0}\" type, but \"{1}\" is expected.",
                    viewDefinedType.Name,
                    dataObjectType.Name));
            }

            foreach (string propertyName in dataObjectView.GetSelfPrimitivePropertiesNames())
            {
                object propertyValue = Information.GetPropValueByName(dataObject, propertyName);

                if (propertyValue != null && serializeValues)
                {
                    if (propertyValue is KeyGuid)
                        propertyValue = ((KeyGuid)propertyValue).Guid;
                    else if (propertyValue is NullableDateTime)
                        propertyValue = ((NullableDateTime)propertyValue).Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
                    else if (propertyValue is DateTime)
                        propertyValue = ((DateTime)propertyValue).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
                    else if (propertyValue is NullableInt)
                        propertyValue = ((NullableInt)propertyValue).Value;
                    else if (propertyValue is NullableDecimal)
                        propertyValue = ((NullableDecimal)propertyValue).Value;
                    if (propertyValue is Geography)
                    {
                        GeoJsonObjectFormatter formatter = GeoJsonObjectFormatter.Create();
                        propertyValue = formatter.Write(propertyValue as Geography);
                    }
                    else
                    {
                        propertyValue = propertyValue.ToString();
                    }

                }

                var aliasPropertyName = model.GetEdmTypePropertyName(dataObjectType, propertyName);
                Add(aliasPropertyName, propertyValue);
            }

            foreach (string masterName in dataObjectView.GetSelfMasterPropertiesNames())
            {
                DataObject master = (DataObject)Information.GetPropValueByName(dataObject, masterName);
                View masterView = dataObjectView.GetSelfMasterView(masterName);

                object masterValue = master != null
                        ? new DataObjectDictionary(master, masterView, model, serializeValues)
                        : null;

                var aliasMasterName = model.GetEdmTypePropertyName(dataObjectType, masterName);
                Add(aliasMasterName, masterValue);
            }

            foreach (DetailInView detailInView in dataObjectView.Details)
            {
                List<DataObject> details = ((DetailArray)Information.GetPropValueByName(dataObject, detailInView.Name)).GetAllObjects().ToList();
                var aliasDetailInViewName = model.GetEdmTypePropertyName(dataObjectType, detailInView.Name);
                Add(aliasDetailInViewName, new DataObjectDictionaryCollection(details, detailInView.View, model, serializeValues));
            }
        }

        /// <summary>
        /// Инициализирует словарь, представляющий объект данных <see cref="DataObject"/> в виде словаря <see cref="Dictionary{String,Object}"/>.
        /// </summary>
        /// <remarks>
        /// Используется для преобразования типов значений из словаря, полученного через стандартный десериализатор.
        /// </remarks>
        /// <param name="dataObjectAliases">Объект данных, в виде словаря псевдонимов.</param>
        /// <param name="dataObjectView">Представление, по которому определены свойства для конвертации объекта в словарь.</param>
        /// <param name="model">Edm-модель, указанная в ManagementToken.</param>
        /// <param name="serializeValues">Флаг: Нужно ли сериализовывать значения свойств объекта.</param>
        private DataObjectDictionary(Dictionary<string, object> dataObjectAliases, View dataObjectView, DataObjectEdmModel model, bool serializeValues = false)
            : base()
        {
            if (dataObjectAliases == null || dataObjectView == null)
            {
                return;
            }

            Type dataObjectType = dataObjectView.DefineClassType;
            List<string> viewAllPropertiesNames = dataObjectView.GetSelfAllPropertiesNames();
            if (dataObjectAliases.Keys.Count != viewAllPropertiesNames.Count)
            {
                throw new Exception(string.Format(
                    "Received dataObject properties count is different from expected count. Received {0}, but {1} is expected.",
                    dataObjectAliases.Keys.Count,
                    viewAllPropertiesNames.Count));
            }

            for (int i = 0; i < dataObjectAliases.Keys.Count; i++)
            {
                string aliasPropertyName = dataObjectAliases.Keys.ElementAt(i);
                string dataObjectPropertyName = model.GetDataObjectPropertyName(dataObjectType, aliasPropertyName);

                if (!viewAllPropertiesNames.Contains(dataObjectPropertyName))
                {
                    throw new Exception(string.Format(
                        "Received dataObject contains unexpected property. Property name is \"{0}\", but expected properties are: {1}.",
                        dataObjectPropertyName,
                        string.Concat("\"", string.Join("\", \"", viewAllPropertiesNames), "\"")));
                }

                object dataObjectPropertyValue = dataObjectAliases[aliasPropertyName];
                Type dataObjectPropertyType = Information.GetPropertyType(dataObjectView.DefineClassType, dataObjectPropertyName);

                if (dataObjectPropertyType.IsSubclassOf(typeof(DataObject)))
                {
                    Dictionary<string, object> master = dataObjectPropertyValue as Dictionary<string, object>;
                    Add(aliasPropertyName, master != null ? new DataObjectDictionary(master, dataObjectView.GetSelfMasterView(dataObjectPropertyName), model, serializeValues) : null);
                    continue;
                }

                if (dataObjectPropertyType.IsSubclassOf(typeof(DetailArray)))
                {
                    Dictionary<string, object>[] details = (dataObjectPropertyValue as ArrayList)?.Cast<Dictionary<string, object>>().ToArray()
                        ?? new Dictionary<string, object>[] { };
                    DataObjectDictionary[] detailsDictionaries = details
                        .Select(x => new DataObjectDictionary(x, dataObjectView.GetDetail(dataObjectPropertyName).View, model, serializeValues))
                        .ToArray();
                    DataObjectDictionaryCollection detailsCollection = new DataObjectDictionaryCollection();
                    detailsCollection.AddRange(detailsDictionaries);

                    Add(aliasPropertyName, detailsCollection);
                    continue;
                }

                if (serializeValues)
                {
                    Add(aliasPropertyName, dataObjectPropertyValue?.ToString());
                    continue;
                }

                if (dataObjectPropertyValue == null)
                {
                    Add(aliasPropertyName, null);
                    continue;
                }

                Type actualPropertyType = dataObjectPropertyValue.GetType();
                if (actualPropertyType == dataObjectPropertyType)
                {
                    Add(aliasPropertyName, dataObjectPropertyValue);
                    continue;
                }

                if (dataObjectPropertyType.IsEnum && actualPropertyType == typeof(string))
                {
                    Add(aliasPropertyName, Enum.Parse(dataObjectPropertyType, (string)dataObjectPropertyValue));
                    continue;
                }

                MethodInfo dataObjectPropertyParseMethod = dataObjectPropertyType.GetMethod("Parse", new[] { typeof(string) });
                if (dataObjectPropertyParseMethod != null && actualPropertyType == typeof(string))
                {
                    Add(aliasPropertyName, dataObjectPropertyParseMethod.Invoke(null, new[] { dataObjectPropertyValue }));
                    continue;
                }

                ConstructorInfo dataObjectPropertyConstructor = dataObjectPropertyType.GetConstructor(new[] { actualPropertyType });
                if (dataObjectPropertyConstructor != null)
                {
                    Add(aliasPropertyName, dataObjectPropertyConstructor.Invoke(new[] { dataObjectPropertyValue }));
                    continue;
                }
            }
        }

        /// <summary>
        /// Создает новый словарь, представляющий объект данных <see cref="DataObject"/>.
        /// </summary>
        /// <remarks>
        /// Типы значений в словаре, будут совпадают с типами значений в объекте данных.
        /// </remarks>
        /// <param name="jsonDataObject">Строка, содержащая объект данных в формате JSON.</param>
        /// <param name="dataObjectView">Представление, по которому определены свойства для конвертации объекта в словарь.</param>
        /// <param name="model">Edm-модель, указанная в ManagementToken.</param>
        /// <param name="castValues">Флаг: нужно ли приводить типы значений к типам объекта данных.</param>
        /// <returns>Новый словарь, представляющий объект данных <see cref="DataObject"/>.</returns>
        public static DataObjectDictionary Parse(string jsonDataObject, View dataObjectView, DataObjectEdmModel model, bool castValues = true)
        {
            if (jsonDataObject == null || dataObjectView == null)
            {
                return new DataObjectDictionary();
            }

            Dictionary<string, object> result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(jsonDataObject);
            result.Keys.Where(x => x.StartsWith(MetaDataPrefix)).ToList().ForEach(x => result.Remove(x));

            return new DataObjectDictionary(result, dataObjectView, model, !castValues);
        }

        /// <summary>
        /// Осуществляет проверку того, что в словаре есть указанное свойство.
        /// </summary>
        /// <remarks>Свойства можно указывать через точку: Медведь.ЛесОбитания.Страна.Название.</remarks>
        /// <param name="propertyPath">Путь к свойству.</param>
        /// <returns>Флаг: <c>true</c>, если указанное свойство есть в словаре, <c>false</c> в противном случае.</returns>
        public bool HasProperty(string propertyPath)
        {
            try
            {
                GetPropertyValue(propertyPath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Осуществляет получение значений для указанного свойства.
        /// </summary>
        /// <remarks>Свойства можно указывать через точку: Медведь.ЛесОбитания.Страна.Название.</remarks>
        /// <param name="propertyPath">Путь к свойству.</param>
        /// <returns>Значение указанного свойства</returns>
        public object GetPropertyValue(string propertyPath)
        {
            object propertyValue = null;
            DataObjectDictionary dictionary = this;

            string[] properties = propertyPath.Split('.');
            foreach (string property in properties)
            {
                if (dictionary == null || !dictionary.ContainsKey(property))
                {
                    throw new Exception("Property \"{0}\" does not exist in data object dictionary.");
                }

                // Значение свойства.
                propertyValue = dictionary[property];

                // Словарь мастерового объекта;
                dictionary = propertyValue as DataObjectDictionary;
            }

            return propertyValue;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Осуществляет проверку на равенство с указанным объектом.
        /// </summary>
        /// <param name="obj">Объект, с которым требуется сравнить.</param>
        /// <returns>Флаг: <c>true</c>, если объекты равны, и <c>false</c>, в противном случае.</returns>
        public override bool Equals(object obj)
        {
            DataObjectDictionary anotherDataObject = obj as DataObjectDictionary;
            if (anotherDataObject == null)
            {
                return false;
            }

            if (Count != anotherDataObject.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, object> anotherProperty in anotherDataObject)
            {
                string anotherPropertyName = anotherProperty.Key;
                object anotherPropertyValue = anotherProperty.Value;
                Type anotherPropertyType;

                object propertyValue;
                Type propertyType;
                if (!TryGetValue(anotherProperty.Key, out propertyValue))
                {
                    return false;
                }

                if (anotherPropertyValue == null)
                {
                    if (propertyValue != null)
                    {
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }

                anotherPropertyType = anotherPropertyValue.GetType();
                propertyType = propertyValue.GetType();
                if (anotherPropertyType != propertyType)
                {
                    return false;
                }

                MethodInfo anotherPropertyEqualsMethod = anotherPropertyType.GetMethod("Equals", new[] { typeof(object) });
                if (!(bool)anotherPropertyEqualsMethod.Invoke(anotherPropertyValue, new[] { propertyValue }))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Преобразует словарь в JSON-строку.
        /// </summary>
        /// <returns>строка</returns>
        public string Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new KeyGuidToStringConverter());
            return JsonConvert.SerializeObject(this, settings);
        }

        /// <summary>
        /// Класс для преобразования KeyGuid в string.
        /// </summary>
        public class KeyGuidToStringConverter : JsonConverter
        {
            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((KeyGuid)value).Guid.ToString("D"));
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// </summary>
            /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
            /// <returns>
            /// The object value.
            /// </returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Determines whether this instance can convert the specified object type.
            /// </summary>
            /// <param name="objectType">Type of the object.</param>
            /// <returns>
            /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
            /// </returns>
            public override bool CanConvert(Type objectType)
            {
                if (objectType == typeof(KeyGuid))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
