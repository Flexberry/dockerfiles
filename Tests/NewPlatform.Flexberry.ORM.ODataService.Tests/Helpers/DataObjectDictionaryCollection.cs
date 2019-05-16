namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Model;

    using Newtonsoft.Json;

    /// <summary>
    /// Класс, представляющий коллекцию объектов данных, в виде коллекции словарей.
    /// </summary>
    /// <remarks>
    /// Предназначен в основном для представления детейлов в виде словарей.
    /// </remarks>
    public class DataObjectDictionaryCollection : List<DataObjectDictionary>
    {
        /// <summary>
        /// Инициализирует пустую коллекцию объектов данных, представленных в виде словарей.
        /// </summary>
        public DataObjectDictionaryCollection()
            : base()
        {
        }

        /// <summary>
        /// Инициализирует коллекцию объектов данных, представленных в виде словарей.
        /// </summary>
        /// <param name="dataObjects">Коллекция объектов данных.</param>
        /// <param name="dataObjectsView">Представление, по которому определены свойства для конвертации объектов в коллекцию словарей.</param>
        /// <param name="model">Edm-модель, указанная в <see cref="ManagementToken"/>.</param>
        /// <param name="serializeValues">Флаг: Нужно ли сериализовывать значения свойств объекта данных.</param>
        public DataObjectDictionaryCollection(List<DataObject> dataObjects, View dataObjectsView, DataObjectEdmModel model, bool serializeValues = false)
            : base()
        {
            if (dataObjects == null || dataObjectsView == null)
            {
                return;
            }

            dataObjects.ForEach(x => Add(new DataObjectDictionary(x, dataObjectsView, model, serializeValues)));
        }

        /// <summary>
        /// Создает новую коллекцию словарей, представляющую объекты данных <see cref="DataObject"/>.
        /// </summary>
        /// <remarks>
        /// Типы значений в словарях коллекции, будут совпадать с типами значений в объектах данных.
        /// </remarks>
        /// <param name="jsonDataObjects">Строка, содержащая массив объектов данных в формате JSON (массив должен быть доступен по ключу "value").</param>
        /// <param name="dataObjectsView">Представление, по которому определены свойства для конвертации объектов в коллекцию словарей.</param>
        /// <param name="model">Edm-модель, указанная в ManagementToken.</param>
        /// <param name="castValues">Флаг: нужно ли приводить типы значений к типам объекта данных.</param>
        /// <returns>Новая коллекция словарей, представляющая объекты данных <see cref="DataObject"/>.</returns>
        public static DataObjectDictionaryCollection Parse(string jsonDataObjects, View dataObjectsView, DataObjectEdmModel model, bool castValues = true)
        {
            DataObjectDictionaryCollection result = new DataObjectDictionaryCollection();

            if (jsonDataObjects == null || dataObjectsView == null)
            {
                return result;
            }

            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonDataObjects);

            object value;
            if (dictionary.TryGetValue("value", out value) && value is ArrayList)
            {
                Dictionary<string, object>[] dataObjects = ((ArrayList)value).Cast<Dictionary<string, object>>().ToArray();
                DataObjectDictionary[] dataObjectsDictionaries = dataObjects
                    .Select(x => DataObjectDictionary.Parse(JsonConvert.SerializeObject(x), dataObjectsView, model, castValues))
                    .ToArray();

                result.AddRange(dataObjectsDictionaries);
            }
            else
            {
                result.Add(DataObjectDictionary.Parse(jsonDataObjects, dataObjectsView, model, castValues));
            }

            return result;
        }

        /// <summary>
        /// Осуществляет проверку на равенство с указанным объектом.
        /// </summary>
        /// <remarks>При сравнении не учитывается порядок следования объектов в коллекции.</remarks>
        /// <param name="obj">Объект, с которым требуется сравнить.</param>
        /// <returns>Флаг: <c>true</c>, если объекты равны, и <c>false</c>, в противном случае.</returns>
        public override bool Equals(object obj)
        {
            DataObjectDictionaryCollection anotherCollection = obj as DataObjectDictionaryCollection;
            if (anotherCollection == null)
            {
                return false;
            }

            if (Count != anotherCollection.Count)
            {
                return false;
            }

            List<DataObjectDictionary> alreadyCheckedDictionaries = new List<DataObjectDictionary>();
            foreach (DataObjectDictionary anotherDictionary in anotherCollection)
            {
                foreach (DataObjectDictionary dictionary in this.Except(alreadyCheckedDictionaries))
                {
                    if (anotherDictionary.Equals(dictionary))
                    {
                        alreadyCheckedDictionaries.Add(dictionary);
                        break;
                    }
                }
            }

            if (this.Except(alreadyCheckedDictionaries).Any())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Играет роль хэш-функции для определённого типа.
        /// </summary>
        /// <returns>Возвращаемое значение.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
