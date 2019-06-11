namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;

    /// <summary>
    /// Metadata class for detail relations.
    /// </summary>
    public sealed class DataObjectEdmDetailSettings
    {
        /// <summary>
        /// The type of detail.
        /// </summary>
        public Type DetailType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectEdmDetailSettings"/> class.
        /// </summary>
        /// <param name="detailType">The type of detail.</param>
        public DataObjectEdmDetailSettings(Type detailType)
        {
            if (detailType == null)
            {
                throw new ArgumentNullException(nameof(detailType), "Contract assertion not met: detailType != null");
            }

            DetailType = detailType;
        }
    }
}