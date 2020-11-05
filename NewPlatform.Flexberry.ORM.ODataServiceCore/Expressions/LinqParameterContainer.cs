// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/Query/Expressions/LinqParameterContainer.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///  Класс для параметризации констант.
    /// </summary>
    internal abstract class LinqParameterContainer
    {
        private static ConcurrentDictionary<Type, Func<object, LinqParameterContainer>> _ctors = new ConcurrentDictionary<Type, Func<object, LinqParameterContainer>>();

        /// <summary>
        /// Значение константы.
        /// </summary>
        public abstract object Property { get; }

        /// <summary>
        /// Статический метод для параметризации константы.
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Выражение, содержащее константу.</returns>
        public static Expression Parameterize(Type type, object value)
        {
            // () => new LinqParameterContainer(constant).Property
            // instead of returning a constant expression node, wrap that constant in a class the way compiler
            // does a closure, so that EF can parameterize the constant (resulting in better performance due to expression translation caching).
            LinqParameterContainer containedValue = LinqParameterContainer.Create(type, value);
            return Expression.Property(Expression.Constant(containedValue), "TypedProperty");
        }

        /// <summary>
        /// Вызывается динамически в runtime.
        /// </summary>
        /// <typeparam name="T">Тип константы.</typeparam>
        /// <param name="value">Значение константы.</param>
        /// <returns>Возвращает класс LinqParameterContainer.</returns>
        public static LinqParameterContainer CreateInternal<T>(T value)
        {
            return new TypedLinqParameterContainer<T>(value);
        }

        // having a strongly typed property avoids the a cast in the property access expression that would be
        // generated for this constant.

        /// <summary>
        ///  Типизированный класс для параметризации констант.
        /// </summary>
        /// <typeparam name="T">Тип константы.</typeparam>
        internal class TypedLinqParameterContainer<T> : LinqParameterContainer
        {
            /// <summary>
            /// Конструктор с одним параметром.
            /// </summary>
            /// <param name="value">Значение константы.</param>
            public TypedLinqParameterContainer(T value)
            {
                TypedProperty = value;
            }

            /// <summary>
            /// Типизированное значение константы.
            /// </summary>
            public T TypedProperty { get; set; }

            /// <summary>
            /// Значение константы.
            /// </summary>
            public override object Property
            {
                get { return TypedProperty; }
            }
        }

        private static LinqParameterContainer Create(Type type, object value)
        {
            return _ctors.GetOrAdd(type, t =>
            {
                MethodInfo createMethod = typeof(LinqParameterContainer).GetMethod("CreateInternal").MakeGenericMethod(t);
                ParameterExpression valueParameter = Expression.Parameter(typeof(object));
                return
                    Expression.Lambda<Func<object, LinqParameterContainer>>(
                        Expression.Call(
                            createMethod,
                            Expression.Convert(valueParameter, t)),
                        valueParameter)
                    .Compile();
            })(value);
        }
    }
}
