// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/ExpressionHelperMethods.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Класс содержит статические определения reflection различных методов для Queryable и Enumerable.
    /// </summary>
    internal class ExpressionHelperMethods
    {
        private static MethodInfo _queryableCastMethod = GenericMethodOf(_ => Queryable.Cast<int>(default(IQueryable)));
        private static MethodInfo _enumerableCastMethod = GenericMethodOf(_ => Enumerable.Cast<int>(default(IEnumerable)));

        private static MethodInfo _orderByMethod = GenericMethodOf(_ => Queryable.OrderBy<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _enumerableOrderByMethod = GenericMethodOf(_ => Enumerable.OrderBy<int, int>(default(IEnumerable<int>), default(Func<int, int>)));
        private static MethodInfo _orderByDescendingMethod = GenericMethodOf(_ => Queryable.OrderByDescending<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _thenByMethod = GenericMethodOf(_ => Queryable.ThenBy<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _enumerableThenByMethod = GenericMethodOf(_ => Enumerable.ThenBy<int, int>(default(IOrderedEnumerable<int>), default(Func<int, int>)));
        private static MethodInfo _thenByDescendingMethod = GenericMethodOf(_ => Queryable.ThenByDescending<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _countMethod = GenericMethodOf(_ => Queryable.LongCount<int>(default(IQueryable<int>)));
        private static MethodInfo _skipMethod = GenericMethodOf(_ => Queryable.Skip<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _whereMethod = GenericMethodOf(_ => Queryable.Where<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _queryableEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>)));
        private static MethodInfo _queryableNonEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));
        private static MethodInfo _queryableAllMethod = GenericMethodOf(_ => Queryable.All(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _enumerableEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>)));
        private static MethodInfo _enumerableNonEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>), default(Func<int, bool>)));
        private static MethodInfo _enumerableAllMethod = GenericMethodOf(_ => Enumerable.All<int>(default(IEnumerable<int>), default(Func<int, bool>)));

        private static MethodInfo _enumerableOfTypeMethod = GenericMethodOf(_ => Enumerable.OfType<int>(default(IEnumerable)));
        private static MethodInfo _queryableOfTypeMethod = GenericMethodOf(_ => Queryable.OfType<int>(default(IQueryable)));

        private static MethodInfo _enumerableSelectMethod = GenericMethodOf(_ => Enumerable.Select<int, int>(default(IEnumerable<int>), i => i));
        private static MethodInfo _queryableSelectMethod = GenericMethodOf(_ => Queryable.Select<int, int>(default(IQueryable<int>), i => i));

        private static MethodInfo _queryableTakeMethod = GenericMethodOf(_ => Queryable.Take<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _enumerableTakeMethod = GenericMethodOf(_ => Enumerable.Take<int>(default(IEnumerable<int>), default(int)));

        private static MethodInfo _queryableAsQueryableMethod = GenericMethodOf(_ => Queryable.AsQueryable<int>(default(IEnumerable<int>)));

        public static MethodInfo QueryableOrderByGeneric
        {
            get { return _orderByMethod; }
        }

        public static MethodInfo QueryableOrderByDescendingGeneric
        {
            get { return _orderByDescendingMethod; }
        }

        public static MethodInfo QueryableThenByGeneric
        {
            get { return _thenByMethod; }
        }

        public static MethodInfo QueryableThenByDescendingGeneric
        {
            get { return _thenByDescendingMethod; }
        }

        /// <summary>
        /// Определение reflection для Queryable.Where
        /// </summary>
        public static MethodInfo QueryableWhereGeneric
        {
            get { return _whereMethod; }
        }

        /// <summary>
        /// Определение reflection для Queryable.Any
        /// </summary>
        public static MethodInfo QueryableEmptyAnyGeneric
        {
            get { return _queryableEmptyAnyMethod; }
        }

        /// <summary>
        /// Определение reflection для Queryable.Any
        /// </summary>
        public static MethodInfo QueryableNonEmptyAnyGeneric
        {
            get { return _queryableNonEmptyAnyMethod; }
        }

        /// <summary>
        /// Определение reflection для Queryable.All
        /// </summary>
        public static MethodInfo QueryableAllGeneric
        {
            get { return _queryableAllMethod; }
        }

        /// <summary>
        /// Определение reflection для Enumerable.Any
        /// </summary>
        public static MethodInfo EnumerableEmptyAnyGeneric
        {
            get { return _enumerableEmptyAnyMethod; }
        }

        /// <summary>
        /// Определение reflection для Enumerable.Any
        /// </summary>
        public static MethodInfo EnumerableNonEmptyAnyGeneric
        {
            get { return _enumerableNonEmptyAnyMethod; }
        }

        /// <summary>
        /// Определение reflection для Enumerable.All
        /// </summary>
        public static MethodInfo EnumerableAllGeneric
        {
            get { return _enumerableAllMethod; }
        }

        /// <summary>
        /// Определение reflection для Enumerable.OfType
        /// </summary>
        public static MethodInfo EnumerableOfType
        {
            get { return _enumerableOfTypeMethod; }
        }

        /// <summary>
        /// Определение reflection для Queryable.OfType
        /// </summary>
        public static MethodInfo QueryableOfType
        {
            get { return _queryableOfTypeMethod; }
        }

        /// <summary>
        /// Определение reflection для Queryable.Cast
        /// </summary>
        public static MethodInfo QueryableCast
        {
            get { return _queryableCastMethod; }
        }

        /// <summary>
        /// Определение reflection для Queryable.Cast
        /// </summary>
        public static MethodInfo EnumerableCast
        {
            get { return _enumerableCastMethod; }
        }

        private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return GenericMethodOf(expression as Expression);
        }

        private static MethodInfo GenericMethodOf(Expression expression)
        {
            LambdaExpression lambdaExpression = expression as LambdaExpression;

            Contract.Assert(expression.NodeType == ExpressionType.Lambda);
            Contract.Assert(lambdaExpression != null);
            Contract.Assert(lambdaExpression.Body.NodeType == ExpressionType.Call);

            return (lambdaExpression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }
    }
}
