// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/Common/TypeExtensions.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class TypeExtensions
    {
        /// <summary>
        /// Проверяется возможность присваивать переменной данного типа значение null.
        /// </summary>
        /// <param name="type">Тип переменной.</param>
        /// <returns>Если можно присвоить значение null, то возвращается true, иначе false.</returns>
        public static bool IsNullable(this Type type)
        {
            if (type.IsValueType)
            {
                // value types are only nullable if they are Nullable<T>
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            else
            {
                // reference types are always nullable
                return true;
            }
        }
    }
}
