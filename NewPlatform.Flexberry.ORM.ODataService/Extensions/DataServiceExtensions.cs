namespace NewPlatform.Flexberry.ORM.ODataService.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;

    /// <summary>
    /// Класс, содержащий расширения для сервиса данных.
    /// </summary>
    public static class DataServiceExtensions
    {
        /// <summary>
        /// Осуществляет вычитку объектов данных заданного типа по заданному представлению и LINQ-выражению.
        /// </summary>
        /// <param name="dataService">Экземпляр сервиса данных.</param>
        /// <param name="dataObjectType">Тип вычитываемых объектов данных, наследуемый от <see cref="DataObject"/>.</param>
        /// <param name="dataObjectView">Представления для вычитки объектов данных.</param>
        /// <param name="expression">LINQ-выражение, ограничивающее набор вычитываемых объектов.</param>
        /// <returns>Коллекция вычитанных объектов.</returns>
        public static object Execute(this SQLDataService dataService, Type dataObjectType, View dataObjectView, Expression expression)
        {
            IQueryProvider queryProvider = typeof(LcsQueryProvider<,>)
                .MakeGenericType(dataObjectType, typeof(LcsGeneratorQueryModelVisitor))
                .GetConstructor(new Type[3] { typeof(SQLDataService), typeof(View), typeof(IEnumerable<View>) })
                .Invoke(new object[3] { dataService, dataObjectView, null }) as IQueryProvider;

            return queryProvider.Execute(expression);
        }
    }
}
