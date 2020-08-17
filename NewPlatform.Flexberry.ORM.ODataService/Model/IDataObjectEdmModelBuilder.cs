namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Microsoft.OData.Edm;

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

        /// <summary>
        /// Additional mapping of CLR type to edm primitive type. When it's required on the application side.
        /// </summary>
        Dictionary<Type, IEdmPrimitiveType> AdditionalMapping { get; }

        /// <summary>
        /// Returns <see cref="ICSSoft.STORMNET.Business.LINQProvider.PseudoDetail{T, TP}"/> as object.
        /// </summary>
        /// <param name="masterType">The type of master.</param>
        /// <param name="masterToDetailPseudoProperty">The name of the link from master to pseudodetail (pseudoproperty).</param>
        /// <returns>An <see cref="ICSSoft.STORMNET.Business.LINQProvider.PseudoDetail{T, TP}"/> instance as object.</returns>
        object GetPseudoDetail(Type masterType, string masterToDetailPseudoProperty);

        /// <summary>
        /// Returns <see cref="IPseudoDetailDefinition" /> instance.
        /// </summary>
        /// <param name="pseudoDetail"><see cref="ICSSoft.STORMNET.Business.LINQProvider.PseudoDetail{T, TP}"/> instance as object.</param>
        /// <returns>An <see cref="IPseudoDetailDefinition" /> instance.</returns>
        IPseudoDetailDefinition GetPseudoDetailDefinition(object pseudoDetail);
    }
}
