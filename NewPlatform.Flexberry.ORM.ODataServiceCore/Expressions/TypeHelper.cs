// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/TypeHelper.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNet.OData.Common;

    /// <summary>
    /// Класс расширения для работы с типами.
    /// </summary>
    internal static class TypeHelper
    {
        /// <summary>
        /// Преобразовывает тип в Nullable.
        /// </summary>
        /// <param name="t">Тип.</param>
        /// <returns>Преобразованный тип.</returns>
        public static Type ToNullable(this Type t)
        {
            if (t.IsNullable())
            {
                return t;
            }
            else
            {
                return typeof(Nullable<>).MakeGenericType(t);
            }
        }

        /// <summary>
        /// Проверяет является ли тип коллекцией, а также возвращает тип элемента коллекции.
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <param name="elementType">Тип элемента коллекции.</param>
        /// <returns>Является ли коллекцией.</returns>
        public static bool IsCollection(this Type type, out Type elementType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            elementType = type;

            // see if this type should be ignored.
            if (type == typeof(string))
            {
                return false;
            }

            Type collectionInterface
                = type.GetInterfaces()
                    .Union(new[] { type })
                    .FirstOrDefault(
                        t => t.IsGenericType
                             && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает лежащий в остнове тип для Nullable, если тип Nullable.
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <returns>Возвращает лежащий в остнове тип.</returns>
        public static Type GetUnderlyingTypeOrSelf(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// Проверяет является ли тип перечислением.
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <returns>Является ли тип перечислением.</returns>
        public static bool IsEnum(Type type)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(type);
            return underlyingTypeOrSelf.IsEnum;
        }
    }
}
