namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Interface of builder for <see cref="DataObjectEdmModel"/> instances.
    /// </summary>
    public interface IDataObjectEdmModelBuilder
    {
        /// <summary>
        /// Builds <see cref="DataObjectEdmModel"/> instance.
        /// </summary>
        /// <returns>An <see cref="DataObjectEdmModel"/> instance.</returns>
        DataObjectEdmModel Build();

        /// <summary>
        /// Delegate for building names for EDM entity sets.
        /// </summary>
        Func<Type, string> EntitySetNameBuilder { get; set; }

        /// <summary>
        /// Delegate for building namespaces for EDM entity types.
        /// </summary>
        Func<Type, string> EntityTypeNamespaceBuilder { get; set; }

        /// <summary>
        /// Delegate for building names for EDM entity types.
        /// </summary>
        Func<Type, string> EntityTypeNameBuilder { get; set; }

        /// <summary>
        /// Delegate for building names for EDM entity properties.
        /// </summary>
        Func<PropertyInfo, string> EntityPropertyNameBuilder { get; set; }
    }
}
