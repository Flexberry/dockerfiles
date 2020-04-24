namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using System.Reflection;

    /// <summary>
    /// Metadata-container class for building EDM model in <see cref="DataObjectEdmModel"/>.
    /// </summary>
    public sealed class DataObjectEdmMetadata
    {
        /// <summary>
        /// Internal data storage.
        /// </summary>
        private readonly IDictionary<Type, DataObjectEdmTypeSettings> _data = new Dictionary<Type, DataObjectEdmTypeSettings>();

        /// <summary>
        /// The name of keys in the model.
        /// </summary>
        public string KeyPropertyName { get; set; }

        /// <summary>
        /// The property of keys in the model.
        /// </summary>
        public PropertyInfo KeyProperty { get; set; }

        /// <summary>
        /// All types, that model have to expose.
        /// </summary>
        public IEnumerable<Type> Types => _data.Keys.ToList();

        /// <summary>
        /// Gets or sets the <see cref="DataObjectEdmTypeSettings" /> for the specified data object type.
        /// </summary>
        /// <param name="dataObjectType">The type of the data object.</param>
        public DataObjectEdmTypeSettings this[Type dataObjectType]
        {
            get { return _data[dataObjectType]; }
            set { _data[dataObjectType] = value; }
        }

        /// <summary>
        /// Determines whether metadata contains the specified data object type.
        /// </summary>
        /// <param name="dataObjectType">The type of the data object.</param>
        /// <returns>Returns <c>true</c> if the data object is exist in metadata.</returns>
        public bool Contains(Type dataObjectType)
        {
            return _data.ContainsKey(dataObjectType);
        }
    }
}