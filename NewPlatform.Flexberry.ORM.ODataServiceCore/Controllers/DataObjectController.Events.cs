namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using System;
    using System.Net;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    using NewPlatform.Flexberry.ORM.ODataService.Events;

    /// <summary>
    /// The <see cref="DataObject"/> OData controller class.
    /// The ODataService event handlers part.
    /// </summary>
    public partial class DataObjectController
    {
        /// <summary>
        /// The container with registered events.
        /// </summary>
        private IEventHandlerContainer Events => ManagementToken?.Events;

        /// <summary>
        /// Вызов делегата перед запросом.
        /// </summary>
        /// <param name="lcs">.</param>
        /// <returns></returns>
        internal bool ExecuteCallbackBeforeGet(ref LoadingCustomizationStruct lcs)
        {
            return  Events.CallbackBeforeGet == null || Events.CallbackBeforeGet(ref lcs);
        }

        /// <summary>
        /// Вызов делегата перед создания объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        /// <returns></returns>
        internal bool ExecuteCallbackBeforeCreate(DataObject obj)
        {
            return Events.CallbackBeforeCreate == null || Events.CallbackBeforeCreate(obj);
        }

        /// <summary>
        /// Вызов делегата перед изменением объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        /// <returns></returns>
        internal bool ExecuteCallbackBeforeUpdate(DataObject obj)
        {
            return Events.CallbackBeforeUpdate == null || Events.CallbackBeforeUpdate(obj);
        }

        /// <summary>
        /// Вызов делегата перед удалением объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        /// <returns></returns>
        internal bool ExecuteCallbackBeforeDelete(DataObject obj)
        {
            return Events.CallbackBeforeDelete == null || Events.CallbackBeforeDelete(obj);
        }

        /// <summary>
        /// Вызов делегата после вычитывания объектов.
        /// </summary>
        /// <param name="objs">Объект после создания.</param>
        internal void ExecuteCallbackAfterGet(ref DataObject[] objs)
        {
            Events.CallbackAfterGet?.Invoke(ref objs);
        }

        /// <summary>
        /// Вызов делегата после создания объекта.
        /// </summary>
        /// <param name="obj">Объект после создания.</param>
        internal void ExecuteCallbackAfterCreate(DataObject obj)
        {
            Events.CallbackAfterCreate?.Invoke(obj);
        }

        /// <summary>
        /// Вызов делегата после обновления объекта.
        /// </summary>
        /// <param name="obj">Объект после обновления.</param>
        internal void ExecuteCallbackAfterUpdate(DataObject obj)
        {
            Events.CallbackAfterUpdate?.Invoke(obj);
        }

        /// <summary>
        /// Вызов делегата после удаления объекта.
        /// </summary>
        /// <param name="obj">Объект перед удалением.</param>
        internal void ExecuteCallbackAfterDelete(DataObject obj)
        {
            Events.CallbackAfterDelete?.Invoke(obj);
        }

        /// <summary>
        /// Вызов делегата после возникновения исключения.
        /// </summary>
        /// <param name="ex">Исключение, которое возникло внутри ODataService.</param>
        /// <param name="code">Возвращаемый код HTTP. По-умолчанияю 500.</param>
        /// <returns>Исключение, которое будет отправлено клиенту.</returns>
        internal Exception ExecuteCallbackAfterInternalServerError(Exception ex, ref HttpStatusCode code)
        {
            if (Events.CallbackAfterInternalServerError == null)
            {
                return ex;
            }

            return Events.CallbackAfterInternalServerError(ex, ref code);
        }
    }
}