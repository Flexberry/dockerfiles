namespace NewPlatform.Flexberry.ORM.ODataService.Events
{
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using System;
    using System.Net;

    /// <summary>
    /// Тип делегата, вызываемого перед выполнением запроса.
    /// </summary>
    /// <param name="lcs">.</param>
    /// <returns></returns>
    public delegate bool DelegateBeforeGet(ref LoadingCustomizationStruct lcs);

    /// <summary>
    /// Тип делегата, вызываемого перед созданием объекта.
    /// </summary>
    /// <param name="obj">Объект.</param>
    /// <returns></returns>
    public delegate bool DelegateBeforeCreate(DataObject obj);

    /// <summary>
    /// Тип делегата, вызываемого перед изменением объекта.
    /// </summary>
    /// <param name="obj">Объект.</param>
    /// <returns></returns>
    public delegate bool DelegateBeforeUpdate(DataObject obj);

    /// <summary>
    /// Тип делегата, вызываемого перед удалением объекта.
    /// </summary>
    /// <param name="obj">Объект.</param>
    /// <returns></returns>
    public delegate bool DelegateBeforeDelete(DataObject obj);

    /// <summary>
    /// Тип делегата, вызываемого после вычитывания объектов.
    /// </summary>
    /// <param name="objs">Вычитанные объекты.</param>
    public delegate void DelegateAfterGet(ref DataObject[] objs);

    /// <summary>
    /// Тип делегата, вызываемого после создания объекта.
    /// </summary>
    /// <param name="obj">Объект после создания.</param>
    public delegate void DelegateAfterCreate(DataObject obj);

    /// <summary>
    /// Тип делегата, вызываемого после обновления объекта.
    /// </summary>
    /// <param name="obj">Объект после обновления.</param>
    public delegate void DelegateAfterUpdate(DataObject obj);

    /// <summary>
    /// Тип делегата, вызываемого после удаления объекта.
    /// </summary>
    /// <param name="obj">Объект перед удалением.</param>
    public delegate void DelegateAfterDelete(DataObject obj);

    /// <summary>
    /// Тип делегата, вызываемого после возникновения исключения.
    /// </summary>
    /// <param name="ex">Исключение, которое возникло внутри ODataService.</param>
    /// <param name="code">Возвращаемый код HTTP. По-умолчанияю 500.</param>
    /// <returns>Исключение, которое будет отправлено клиенту.</returns>
    public delegate Exception DelegateAfterInternalServerError(Exception ex, ref HttpStatusCode code);

}
