#if NETFRAMEWORK
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/ExpressionHelpers.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;

    /// <summary>
    /// Класс содержащий вспомогательные методы для работы с linq-выражениями.
    /// </summary>
    internal static class ExpressionHelpers
    {
        public static IQueryable OrderByIt(IQueryable query, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            ParameterExpression odataItParameter = Expression.Parameter(type, "$it");
            LambdaExpression orderByLambda = Expression.Lambda(odataItParameter, odataItParameter);
            return OrderBy(query, orderByLambda, direction, type, alreadyOrdered);
        }

        public static IQueryable OrderByProperty(IQueryable query, IEdmModel model, IEdmProperty property, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            // property aliasing
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, model);
            LambdaExpression orderByLambda = GetPropertyAccessLambda(type, propertyName);
            return OrderBy(query, orderByLambda, direction, type, alreadyOrdered);
        }

        public static IQueryable OrderBy(IQueryable query, LambdaExpression orderByLambda, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            Type returnType = orderByLambda.Body.Type;

            MethodInfo orderByMethod = null;
            IOrderedQueryable orderedQuery = null;

            // unfortunately unordered L2O.AsQueryable implements IOrderedQueryable
            // so we can't try casting to IOrderedQueryable to provide a clue to whether
            // we should be calling ThenBy or ThenByDescending
            if (alreadyOrdered)
            {
                if (direction == OrderByDirection.Ascending)
                {
                    orderByMethod = ExpressionHelperMethods.QueryableThenByGeneric.MakeGenericMethod(type, returnType);
                }
                else
                {
                    orderByMethod = ExpressionHelperMethods.QueryableThenByDescendingGeneric.MakeGenericMethod(type, returnType);
                }

                orderedQuery = query as IOrderedQueryable;
                orderedQuery = orderByMethod.Invoke(null, new object[] { orderedQuery, orderByLambda }) as IOrderedQueryable;
            }
            else
            {
                if (direction == OrderByDirection.Ascending)
                {
                    orderByMethod = ExpressionHelperMethods.QueryableOrderByGeneric.MakeGenericMethod(type, returnType);
                }
                else
                {
                    orderByMethod = ExpressionHelperMethods.QueryableOrderByDescendingGeneric.MakeGenericMethod(type, returnType);
                }

                orderedQuery = orderByMethod.Invoke(null, new object[] { query, orderByLambda }) as IOrderedQueryable;
            }

            return orderedQuery;
        }

        /// <summary>
        /// Выполняет фильтрацию последовательности значений на основе заданного linq-выражения.
        /// </summary>
        /// <param name="query">Последовательности значений.</param>
        /// <param name="where">Linq-выражение.</param>
        /// <param name="type">Тип значений.</param>
        /// <returns>Последовательность значений после фильтрации.</returns>
        public static IQueryable Where(IQueryable query, Expression where, Type type)
        {
            MethodInfo whereMethod = ExpressionHelperMethods.QueryableWhereGeneric.MakeGenericMethod(type);
            return whereMethod.Invoke(null, new object[] { query, where }) as IQueryable;
        }

        private static LambdaExpression GetPropertyAccessLambda(Type type, string propertyName)
        {
            ParameterExpression odataItParameter = Expression.Parameter(type, "$it");
            MemberExpression propertyAccess = Expression.Property(odataItParameter, propertyName);
            return Expression.Lambda(propertyAccess, odataItParameter);
        }


    }
}
#endif
