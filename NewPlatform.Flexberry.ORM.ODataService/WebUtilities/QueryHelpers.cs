#if NETSTANDARD
namespace NewPlatform.Flexberry.ORM.ODataService.WebUtilities
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Implements helper methods for a query.
    /// </summary>
    internal static class QueryHelpers
    {
        /// <summary>
        /// Converts a query string collection to the <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="queryCollection">The query string collection.</param>
        /// <returns>A <see cref="NameValueCollection"/> instance with the query string collection component key and value parts.</returns>
        public static NameValueCollection QueryToNameValueCollection(IQueryCollection queryCollection)
        {
            // A direct Keys collection iteration can be used, but we use the convertion to Dictionary<string, StringValues> type
            // in order to share the iteration algorithm implemented in the QueryPartsToNameValueCollection() method.
            Dictionary<string, StringValues> queryParts = queryCollection.ToDictionary(x => x.Key, y => y.Value);

            return QueryPartsToNameValueCollection(queryParts);
        }

        /// <summary>
        /// Converts a query string to the <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <returns>A <see cref="NameValueCollection"/> instance with the query string component key and value parts.</returns>
        public static NameValueCollection QueryToNameValueCollection(string queryString)
        {
            Dictionary<string, StringValues> queryParts = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString);

            return QueryPartsToNameValueCollection(queryParts);
        }

        private static NameValueCollection QueryPartsToNameValueCollection(Dictionary<string, StringValues> queryParts)
        {
            var result = new NameValueCollection();

            // The Keys collection iteration, so we can ignore the result of TryGetValue() call due the appropriate key exists.
            foreach (string key in queryParts.Keys)
            {
                _ = queryParts.TryGetValue(key, out StringValues value);
                result.Add(key, value.ToString());
            }

            return result;
        }
    }
}
#endif
