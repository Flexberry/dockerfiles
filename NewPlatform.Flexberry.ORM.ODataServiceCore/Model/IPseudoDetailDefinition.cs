namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Reflection;
    using ICSSoft.STORMNET.Business.LINQProvider;

    /// <summary>
    /// OData definition of the link from master to pseudodetail (pseudoproperty).
    /// </summary>
    public interface IPseudoDetailDefinition
    {
        /// <summary>
        /// Тип мастера.
        /// </summary>
        Type MasterType { get; }

        /// <summary>
        /// Имя св¤зи от мастера к псевдодетейлу (псевдосвойство).
        /// </summary>
        string MasterToDetailPseudoProperty { get; }

        /// <summary>
        /// Тип псевдосвойства.
        /// </summary>
        Type PseudoPropertyType { get; }

        /// <summary>
        /// Empty "Any" method of associated <see cref="PseudoDetail{T, TP}" /> instance.
        /// </summary>
        MethodInfo EmptyAnyMethod { get; }

        /// <summary>
        /// Non empty "Any" method of associated <see cref="PseudoDetail{T, TP}" /> instance.
        /// </summary>
        MethodInfo NonEmptyAnyMethod { get; }

        /// <summary>
        /// "All" method of associated <see cref="PseudoDetail{T, TP}" /> instance.
        /// </summary>
        MethodInfo AllMethod { get; }

        /// <summary>
        /// The associated <see cref="PseudoDetail{T, TP}" /> instance as object.
        /// </summary>
        object PseudoDetail { get; }
    }
}
