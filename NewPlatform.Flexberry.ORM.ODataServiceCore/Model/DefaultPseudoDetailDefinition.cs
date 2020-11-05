namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using ICSSoft.STORMNET.Business.LINQProvider;

    /// <summary>
    /// Реализация интерфейса <see cref="IPseudoDetailDefinition" /> по умолчанию.
    /// </summary>
    /// <typeparam name="T"> Тип мастера. </typeparam>
    /// <typeparam name="TP"> Тип детейла. </typeparam>
    public class DefaultPseudoDetailDefinition<T, TP> : IPseudoDetailDefinition
    {
        private readonly PseudoDetail<T, TP> _pseudoDetail;

        private Func<bool> _emptyAny;

        private Func<Expression<Func<TP, bool>>, bool> _nonEmptyAny;

        private Func<Expression<Func<TP, bool>>, bool> _all;

        /// <summary>
        /// Тип мастера.
        /// </summary>
        public Type MasterType => typeof(T);

        /// <summary>
        /// Имя связи от мастера к псевдодетейлу (псевдосвойство).
        /// </summary>
        public string MasterToDetailPseudoProperty => _pseudoDetail.MasterToDetailPseudoProperty;

        /// <summary>
        /// Тип псевдосвойства.
        /// </summary>
        public Type PseudoPropertyType => typeof(List<TP>);

        /// <summary>
        /// Empty "Any" method of associated <see cref="PseudoDetail{T, TP}" /> instance.
        /// </summary>
        public MethodInfo EmptyAnyMethod => _emptyAny.Method;

        /// <summary>
        /// Non empty "Any" method of associated <see cref="PseudoDetail{T, TP}" /> instance.
        /// </summary>
        public MethodInfo NonEmptyAnyMethod => _nonEmptyAny.Method;

        /// <summary>
        /// "All" method of associated <see cref="PseudoDetail{T, TP}" /> instance.
        /// </summary>
        public MethodInfo AllMethod => _all.Method;

        /// <summary>
        /// The associated <see cref="PseudoDetail{T, TP}" /> instance as object.
        /// </summary>
        public object PseudoDetail => _pseudoDetail;

        /// <summary>
        /// Конструктор, вызывающий соответствующий конструктор <see cref="PseudoDetail{T, TP}" />.
        /// </summary>
        /// <param name="view"> Представление псевдодетейла. </param>
        /// <param name="masterLinkName"> Имя связи от псевдодетейла к мастеру. </param>
        public DefaultPseudoDetailDefinition(
            ICSSoft.STORMNET.View view,
            string masterLinkName)
        {
            _pseudoDetail = new PseudoDetail<T, TP>(view, masterLinkName);
            MapMethods();
        }

        /// <summary>
        /// Конструктор, вызывающий соответствующий конструктор <see cref="PseudoDetail{T, TP}" />.
        /// </summary>
        /// <param name="view"> Представление псевдодетейла. </param>
        /// <param name="masterLink"> Метод, определяющий имя связи от псевдодетейла к мастеру (определение идёт через "Information.ExtractPropertyPath(masterLink)"). </param>
        public DefaultPseudoDetailDefinition(
            ICSSoft.STORMNET.View view,
            Expression<Func<TP, object>> masterLink)
        {
            _pseudoDetail = new PseudoDetail<T, TP>(view, masterLink);
            MapMethods();
        }

        /// <summary>
        /// Конструктор, вызывающий соответствующий конструктор <see cref="PseudoDetail{T, TP}" />.
        /// (для псевдодетейлов данный метод будет некорректен).
        /// </summary>
        /// <param name="view"> Представление детейла. </param>
        public DefaultPseudoDetailDefinition(
            ICSSoft.STORMNET.View view)
        {
            _pseudoDetail = new PseudoDetail<T, TP>(view);
            MapMethods();
        }

        /// <summary>
        /// Конструктор, вызывающий соответствующий конструктор <see cref="PseudoDetail{T, TP}" />.
        /// </summary>
        /// <param name="view"> Представление псевдодетейла. </param>
        /// <param name="masterLink"> Метод, определяющий имя связи от псевдодетейла к мастеру (определение идёт через "Information.ExtractPropertyPath(masterLink)"). </param>
        /// <param name="masterToDetailPseudoProperty"> Имя связи от мастера к псевдодетейлу (псевдосвойство). </param>
        public DefaultPseudoDetailDefinition(
            ICSSoft.STORMNET.View view,
            Expression<Func<TP, object>> masterLink,
            string masterToDetailPseudoProperty)
        {
            _pseudoDetail = new PseudoDetail<T, TP>(view, masterLink, masterToDetailPseudoProperty);
            MapMethods();
        }

        /// <summary>
        /// Конструктор, вызывающий соответствующий конструктор <see cref="PseudoDetail{T, TP}" />.
        /// </summary>
        /// <param name="view"> Представление псевдодетейла. </param>
        /// <param name="masterLink"> Метод, определяющий имя связи от псевдодетейла к мастеру (определение идёт через "Information.ExtractPropertyPath(masterLink)"). </param>
        /// <param name="masterToDetailPseudoProperty"> Имя связи от мастера к псевдодетейлу (псевдосвойство). </param>
        /// <param name="masterConnectProperties"> Свойства мастера, по которым можно произвести соединение. Аналог OwnerConnectProp для <see cref="ICSSoft.STORMNET.Windows.Forms.DetailVariableDef"/> в lcs. </param>
        public DefaultPseudoDetailDefinition(
            ICSSoft.STORMNET.View view,
            Expression<Func<TP, object>> masterLink,
            string masterToDetailPseudoProperty,
            string[] masterConnectProperties)
        {
            _pseudoDetail = new PseudoDetail<T, TP>(view, masterLink, masterToDetailPseudoProperty, masterConnectProperties);
            MapMethods();
        }

        /// <summary>
        /// Конструктор, вызывающий соответствующий конструктор <see cref="PseudoDetail{T, TP}" />.
        /// </summary>
        /// <param name="view"> Представление псевдодетейла. </param>
        /// <param name="masterLinkName"> Имя связи от псевдодетейла к мастеру. </param>
        /// <param name="masterToDetailPseudoProperty"> Имя связи от мастера к псевдодетейлу (псевдосвойство). </param>
        public DefaultPseudoDetailDefinition(
            ICSSoft.STORMNET.View view,
            string masterLinkName,
            string masterToDetailPseudoProperty)
        {
            _pseudoDetail = new PseudoDetail<T, TP>(view, masterLinkName, masterToDetailPseudoProperty);
            MapMethods();
        }

        /// <summary>
        /// Конструктор, вызывающий соответствующий конструктор <see cref="PseudoDetail{T, TP}" />.
        /// </summary>
        /// <param name="view"> Представление псевдодетейла. </param>
        /// <param name="masterLinkName"> Имя связи от псевдодетейла к мастеру. </param>
        /// <param name="masterToDetailPseudoProperty"> Имя связи от мастера к псевдодетейлу (псевдосвойство). </param>
        /// <param name="masterConnectProperties"> Свойства мастера, по которым можно произвести соединение. Аналог OwnerConnectProp для <see cref="ICSSoft.STORMNET.Windows.Forms.DetailVariableDef"/> в lcs. </param>
        public DefaultPseudoDetailDefinition(
            ICSSoft.STORMNET.View view,
            string masterLinkName,
            string masterToDetailPseudoProperty,
            string[] masterConnectProperties)
        {
            _pseudoDetail = new PseudoDetail<T, TP>(view, masterLinkName, masterToDetailPseudoProperty, masterConnectProperties);
            MapMethods();
        }

        private void MapMethods()
        {
            _emptyAny = _pseudoDetail.Any;
            _nonEmptyAny = _pseudoDetail.Any;
            _all = _pseudoDetail.All;
        }
    }
}
