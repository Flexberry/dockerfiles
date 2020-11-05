namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System.Collections.Generic;
    using System.Reflection;

    using ICSSoft.STORMNET;
    using System;

    /// <summary>
    /// Metadata class for types.
    /// </summary>
    public sealed class DataObjectEdmTypeSettings
    {
        /// <summary>
        /// Type of key, if null then used from based type.
        /// </summary>
        public Type KeyType { get; set; }

        /// <summary>
        /// The name of appropriate EDM entity set.
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Is EDM entity set is required.
        /// </summary>
        public bool EnableCollection { get; set; }

        /// <summary>
        /// Default view for queries without $select parameter.
        /// </summary>
        public View DefaultView { get; set; }

        /// <summary>
        /// The list of exposed details.
        /// </summary>
        public IDictionary<PropertyInfo, DataObjectEdmDetailSettings> DetailProperties { get; } = new Dictionary<PropertyInfo, DataObjectEdmDetailSettings>();

        /// <summary>
        /// The list of exposed masters.
        /// </summary>
        public IDictionary<PropertyInfo, DataObjectEdmMasterSettings> MasterProperties { get; } = new Dictionary<PropertyInfo, DataObjectEdmMasterSettings>();

        /// <summary>
        /// The list of exposed properties.
        /// </summary>
        public IList<PropertyInfo> OwnProperties { get; } = new List<PropertyInfo>();

        /// <summary>
        /// The list of exposed links from master to pseudodetail (pseudoproperties) as properties.
        /// </summary>
        public IDictionary<PropertyInfo, DataObjectEdmDetailSettings> PseudoDetailProperties { get; } = new Dictionary<PropertyInfo, DataObjectEdmDetailSettings>();
    }
}