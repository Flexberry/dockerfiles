// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/Query/OrderByQueryOption.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Properties;
    using System.Web.OData.Query;
    using System.Web.OData.Query.Expressions;
    using System.Web.OData.Query.Validators;
    using System.Web.OData.Routing;
    using Microsoft.OData.Core;
    using Microsoft.OData.Core.UriParser;
    using Microsoft.OData.Core.UriParser.Semantic;
    using Microsoft.OData.Edm;

    /// <summary>
    /// This defines a $orderby OData query option for querying.
    /// </summary>
    public class OrderByQueryOption
    {
        /// <summary>
        /// Текущий контекст запроса OData.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Сприсок выражений из $orderby.
        /// </summary>
        public IList<OrderByNode> OrderByNodes { get; private set; }

        private Type _contextElementClrType;

        /// <summary>
        /// Создает экземпляр NewPlatform.Flexberry.ORM.ODataService.Expressions.OrderByQueryOption по экземпляру System.Web.OData.Query.OrderByQueryOption.
        /// </summary>
        /// <param name="orderByQueryOption">Экземпляр System.Web.OData.Query.OrderByQueryOption.</param>
        public OrderByQueryOption(System.Web.OData.Query.OrderByQueryOption orderByQueryOption, Type contextElementClrType)
        {
            Context = orderByQueryOption.Context;
            _contextElementClrType = contextElementClrType;
            OrderByNodes = orderByQueryOption.OrderByNodes;
        }

        /// <summary>
        /// Apply the $orderby query to the given IQueryable.
        /// </summary>
        /// <typeparam name="T">Type/</typeparam>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new <see cref="IQueryable"/> after the orderby query has been applied to.</returns>
        public IOrderedQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings)
        {
            return ApplyToCore(query, querySettings) as IOrderedQueryable<T>;
        }

        private IOrderedQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings)
        {
            if (_contextElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            ICollection<OrderByNode> nodes = OrderByNodes;

            bool alreadyOrdered = false;
            IQueryable querySoFar = query;

            HashSet<IEdmProperty> propertiesSoFar = new HashSet<IEdmProperty>();
            HashSet<string> openPropertiesSoFar = new HashSet<string>();
            bool orderByItSeen = false;

            foreach (OrderByNode node in nodes)
            {
                OrderByPropertyNode propertyNode = node as OrderByPropertyNode;
                OrderByOpenPropertyNode openPropertyNode = node as OrderByOpenPropertyNode;

                if (propertyNode != null)
                {
                    IEdmProperty property = propertyNode.Property;
                    OrderByDirection direction = propertyNode.Direction;

                    // This check prevents queries with duplicate properties (e.g. $orderby=Id,Id,Id,Id...) from causing stack overflows
                    if (propertiesSoFar.Contains(property) && propertiesSoFar.Count > 50)
                    {
                        throw new ODataException(Error.Format(SRResources.OrderByDuplicateProperty, property.Name));
                    }

                    propertiesSoFar.Add(property);

                    if (propertyNode.OrderByClause != null)
                    {
                        querySoFar = AddOrderByQueryForProperty(query, querySettings, propertyNode.OrderByClause, querySoFar, direction, alreadyOrdered);
                    }
                    else
                    {
                        querySoFar = ExpressionHelpers.OrderByProperty(querySoFar, Context.Model, property, direction, _contextElementClrType, alreadyOrdered);
                    }

                    alreadyOrdered = true;
                }
                else if (openPropertyNode != null)
                {
                    // This check prevents queries with duplicate properties (e.g. $orderby=Id,Id,Id,Id...) from causing stack overflows
                    if (openPropertiesSoFar.Contains(openPropertyNode.PropertyName) && openPropertiesSoFar.Count > 50)
                    {
                        throw new ODataException(Error.Format(SRResources.OrderByDuplicateProperty, openPropertyNode.PropertyName));
                    }

                    openPropertiesSoFar.Add(openPropertyNode.PropertyName);
                    Contract.Assert(openPropertyNode.OrderByClause != null);
                    querySoFar = AddOrderByQueryForProperty(query, querySettings, openPropertyNode.OrderByClause, querySoFar, openPropertyNode.Direction, alreadyOrdered);
                    alreadyOrdered = true;
                }
                else
                {
                    // This check prevents queries with duplicate nodes (e.g. $orderby=$it,$it,$it,$it...) from causing stack overflows
                    if (orderByItSeen)
                    {
                        throw new ODataException(Error.Format(SRResources.OrderByDuplicateIt));
                    }

                    querySoFar = ExpressionHelpers.OrderByIt(querySoFar, node.Direction, _contextElementClrType, alreadyOrdered);
                    alreadyOrdered = true;
                    orderByItSeen = true;
                }
            }

            return querySoFar as IOrderedQueryable;
        }

        private IQueryable AddOrderByQueryForProperty(
            IQueryable query, ODataQuerySettings querySettings, OrderByClause orderbyClause, IQueryable querySoFar, OrderByDirection direction, bool alreadyOrdered)
        {
            // Ensure we have decided how to handle null propagation
            ODataQuerySettings updatedSettings = querySettings;
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings = new ODataQuerySettings(updatedSettings);
                updatedSettings.HandleNullPropagation =
                    HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query);
            }

            LambdaExpression orderByExpression =
                FilterBinder.Bind(orderbyClause, _contextElementClrType, Context.Model, updatedSettings);
            querySoFar = ExpressionHelpers.OrderBy(querySoFar, orderByExpression, direction, _contextElementClrType, alreadyOrdered);
            return querySoFar;
        }
    }
}
