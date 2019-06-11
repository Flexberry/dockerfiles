namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;

    /// <summary>
    /// Metadata class for master relations.
    /// </summary>
    public sealed class DataObjectEdmMasterSettings
    {
        /// <summary>
        /// The type of the master.
        /// </summary>
        public Type MasterType { get; }

        /// <summary>
        /// Is <c>null</c> allowed for the master.
        /// </summary>
        public bool AllowNull { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectEdmMasterSettings"/> class.
        /// </summary>
        /// <param name="masterType">The type of the master.</param>
        public DataObjectEdmMasterSettings(Type masterType)
        {
            if (masterType == null)
            {
                throw new ArgumentNullException(nameof(masterType), "Contract assertion not met: masterType != null");
            }

            MasterType = masterType;
        }
    }
}