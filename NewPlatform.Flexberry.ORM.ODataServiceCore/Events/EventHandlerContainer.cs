namespace NewPlatform.Flexberry.ORM.ODataService.Events
{
    /// <summary>
    /// Default implementation of <see cref="IEventHandlerContainer"/>.
    /// </summary>
    /// <seealso cref="IEventHandlerContainer" />
    internal class EventHandlerContainer : IEventHandlerContainer
    {
        /// <summary>
        /// Делегат для вызова логики перед выполнением запроса.
        /// </summary>
        public DelegateBeforeGet CallbackBeforeGet { get; set; }

        /// <summary>
        /// Делегат для вызова логики перед изменением объекта.
        /// </summary>
        public DelegateBeforeUpdate CallbackBeforeUpdate { get; set; }

        /// <summary>
        /// Делегат для вызова логики перед созданием объекта.
        /// </summary>
        public DelegateBeforeCreate CallbackBeforeCreate { get; set; }

        /// <summary>
        /// Делегат для вызова логики перед удалением объекта.
        /// </summary>
        public DelegateBeforeDelete CallbackBeforeDelete { get; set; }

        /// <summary>
        /// Делегат для вызова логики после вычитывания объектов.
        /// </summary>
        public DelegateAfterGet CallbackAfterGet { get; set; }

        /// <summary>
        /// Делегат для вызова логики после сохранения объекта.
        /// </summary>
        public DelegateAfterCreate CallbackAfterCreate { get; set; }

        /// <summary>
        /// Делегат для вызова логики после обновления объекта.
        /// </summary>
        public DelegateAfterUpdate CallbackAfterUpdate { get; set; }

        /// <summary>
        /// Делегат для вызова логики после удаления объекта.
        /// </summary>
        public DelegateAfterDelete CallbackAfterDelete { get; set; }

        /// <summary>
        /// Делегат, вызываемый после возникновения исключения.
        /// </summary>
        public DelegateAfterInternalServerError CallbackAfterInternalServerError { get; set; }
    }
}