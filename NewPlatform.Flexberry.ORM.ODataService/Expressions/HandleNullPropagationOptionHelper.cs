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
        /// <summary>
        /// Возвращает как обрабатывать распространения значения null в зависимости от поставщика запросов при построении запросов.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Значение распространения null.</returns>
        public static HandleNullPropagationOption GetDefaultHandleNullPropagationOption(IQueryable query)
        {
            return HandleNullPropagationOption.False;
        }
    }
}