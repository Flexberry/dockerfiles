// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/Query/HandleNullPropagationOptionHelper.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http;
    using System.Web.OData.Query;

    /// <summary>
    /// Вспомогательный класс для обработки распространения значения null.
    /// </summary>
    internal static class HandleNullPropagationOptionHelper
    {
        private const string EntityFrameworkQueryProviderNamespace = "System.Data.Entity.Internal.Linq";

        private const string ObjectContextQueryProviderNamespaceEF5 = "System.Data.Objects.ELinq";
        private const string ObjectContextQueryProviderNamespaceEF6 = "System.Data.Entity.Core.Objects.ELinq";

        private const string Linq2SqlQueryProviderNamespace = "System.Data.Linq";
        private const string Linq2ObjectsQueryProviderNamespace = "System.Linq";

        /// <summary>
        /// Возвращает как обрабатывать распространения значения null в зависимости от поставщика запросов при построении запросов.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Значение распространения null.</returns>
        public static HandleNullPropagationOption GetDefaultHandleNullPropagationOption(IQueryable query)
        {
            Contract.Assert(query != null);

            HandleNullPropagationOption options;

            string queryProviderNamespace = query.Provider.GetType().Namespace;
            switch (queryProviderNamespace)
            {
                case EntityFrameworkQueryProviderNamespace:
                case Linq2SqlQueryProviderNamespace:
                case ObjectContextQueryProviderNamespaceEF5:
                case ObjectContextQueryProviderNamespaceEF6:
                    options = HandleNullPropagationOption.False;
                    break;

                case Linq2ObjectsQueryProviderNamespace:
                default:
                    options = HandleNullPropagationOption.True;
                    break;
            }

            return options;
        }
    }
}