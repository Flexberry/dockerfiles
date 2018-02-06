namespace NewPlatform.Flexberry.ORM.ODataService.Events
{
    /// <summary>
    /// Interface of container with OData Service event handlers.
    /// </summary>
    public interface IEventHandlerContainer
    {
        /// <summary>
        /// Делегат для вызова логики перед выполнением запроса.
        /// </summary>
        DelegateBeforeGet CallbackBeforeGet { get; set; }

        /// <summary>
        /// Делегат для вызова логики перед изменением объекта.
        /// </summary>
        DelegateBeforeUpdate CallbackBeforeUpdate { get; set; }

        /// <summary>
        /// Делегат для вызова логики перед созданием объекта.
        /// </summary>
        DelegateBeforeCreate CallbackBeforeCreate { get; set; }

        /// <summary>
        /// Делегат для вызова логики перед удалением объекта.
        /// </summary>
        DelegateBeforeDelete CallbackBeforeDelete { get; set; }

        /// <summary>
        /// Делегат для вызова логики после вычитывания объектов.
        /// </summary>
        DelegateAfterGet CallbackAfterGet { get; set; }

        /// <summary>
        /// Делегат для вызова логики после сохранения объекта.
        /// </summary>
        DelegateAfterCreate CallbackAfterCreate { get; set; }

        /// <summary>
        /// Делегат для вызова логики после обновления объекта.
        /// </summary>
        DelegateAfterUpdate CallbackAfterUpdate { get; set; }

        /// <summary>
        /// Делегат для вызова логики после удаления объекта.
        /// </summary>
        DelegateAfterDelete CallbackAfterDelete { get; set; }

        /// <summary>
        /// Делегат, вызываемый после возникновения исключения.
        /// </summary>
        DelegateAfterInternalServerError CallbackAfterInternalServerError { get; set; }
    }
}
