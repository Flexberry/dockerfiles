namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Helper extension methods for fast use of collections.
    /// </summary>
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Return the enumerable as a IList of T, copying if required. Avoid mutating the return value.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="enumerable">enumerable</param>
        /// <returns>IList</returns>
        public static IList<T> AsIList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable), "Contract assertion not met: enumerable != null");
            }

            IList<T> list = enumerable as IList<T>;
            if (list != null)
            {
                return list;
            }

            return new List<T>(enumerable);
        }
    }
}
