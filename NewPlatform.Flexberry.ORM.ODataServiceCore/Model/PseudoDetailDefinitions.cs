namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ICSSoft.STORMNET.Business.LINQProvider;

    /// <summary>
    /// <see cref="IPseudoDetailDefinition" /> unique instances collection implementation.
    /// </summary>
    public sealed class PseudoDetailDefinitions : IEnumerable<IPseudoDetailDefinition>
    {
        private readonly List<IPseudoDetailDefinition> _data = new List<IPseudoDetailDefinition>();

        /// <summary>
        /// Adds new <see cref="IPseudoDetailDefinition" /> instance to the collection.
        /// </summary>
        /// <param name="definition">An <see cref="IPseudoDetailDefinition" /> instance.</param>
        public void Add(IPseudoDetailDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            if (_data
              .Where(x => x.MasterType == definition.MasterType)
              .Where(x => x.MasterToDetailPseudoProperty == definition.MasterToDetailPseudoProperty)
              .Any())
            {
               throw new Exception($"Pseudoproperty with \"{definition.MasterToDetailPseudoProperty}\" name is already defined for {definition.MasterType.FullName} type.");
            }

           _data.Add(definition);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="PseudoDetailDefinitions" />.
        /// </summary>
        /// <returns>An <see cref="PseudoDetailDefinitions" /> enumerator.</returns>
        public IEnumerator<IPseudoDetailDefinition> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

#pragma warning disable SA1600 // Elements must be documented
        IEnumerator IEnumerable.GetEnumerator()
#pragma warning restore SA1600 // Elements must be documented
        {
            throw new NotImplementedException();
        }
    }
}
