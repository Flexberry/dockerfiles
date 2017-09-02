namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    using NewPlatform.Flexberry.ORM.ODataService.Events;

    /// <summary>
    /// OData controller class.
    /// Part with event handlers.
    /// </summary>
    public partial class DataObjectController
    {
        /// <summary>
        /// The container with registered events.
        /// </summary>
        private readonly IEventHandlerContainer _events;

        /// <summary>
        /// Вызов делегата перед запросом.
        /// </summary>
        /// <param name="lcs">.</param>
        /// <returns></returns>
        internal bool ExecuteCallbackBeforeGet(ref LoadingCustomizationStruct lcs)
        {
            return _events.CallbackBeforeGet == null || _events.CallbackBeforeGet(ref lcs);
        }

        /// <summary>
        /// Вызов делегата перед создания объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        /// <returns></returns>
        internal bool ExecuteCallbackBeforeCreate(DataObject obj)
        {
            return _events.CallbackBeforeCreate == null || _events.CallbackBeforeCreate(obj);
        }

        /// <summary>
        /// Вызов делегата перед изменением объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        /// <returns></returns>
        internal bool ExecuteCallbackBeforeUpdate(DataObject obj)
        {
            return _events.CallbackBeforeUpdate == null || _events.CallbackBeforeUpdate(obj);
        }

        /// <summary>
        /// Вызов делегата перед удалением объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        /// <returns></returns>
        internal bool ExecuteCallbackBeforeDelete(DataObject obj)
        {
            return _events.CallbackBeforeDelete == null || _events.CallbackBeforeDelete(obj);
        }

        /// <summary>
        /// Вызов делегата после вычитывания объектов.
        /// </summary>
        /// <param name="objs">Объект после создания.</param>
        internal void ExecuteCallbackAfterGet(ref DataObject[] objs)
        {
            _events.CallbackAfterGet?.Invoke(ref objs);
        }

        /// <summary>
        /// Вызов делегата после создания объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        internal void ExecuteCallbackAfterCreate(DataObject obj)
        {
            _events.CallbackAfterCreate?.Invoke(obj);
        }

        /// <summary>
        /// Вызов делегата после обновления объекта.
        /// </summary>
        /// <param name="obj">Объект после обновления.</param>
        internal void ExecuteCallbackAfterUpdate(DataObject obj)
        {
            _events.CallbackAfterUpdate?.Invoke(obj);
        }

        /// <summary>
        /// Вызов делегата после удаления объекта.
        /// </summary>
        /// <param name="obj">Объект перед удалением.</param>
        internal void ExecuteCallbackAfterDelete(DataObject obj)
        {
            _events.CallbackAfterDelete?.Invoke(obj);
        }
    }
}