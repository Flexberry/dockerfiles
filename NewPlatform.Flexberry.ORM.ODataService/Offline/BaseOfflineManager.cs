namespace NewPlatform.Flexberry.ORM.ODataService.Offline
{
    using System.Collections.Generic;
    using System.Web.OData.Query;

    using ICSSoft.STORMNET;

    public abstract class BaseOfflineManager
    {
        protected abstract bool IsLockingRequired(ODataQueryOptions queryOptions, DataObject dataObject);

        protected abstract bool IsUnlockingRequired(ODataQueryOptions queryOptions, DataObject dataObject);

        public abstract bool LockObjects(ODataQueryOptions queryOptions, IEnumerable<DataObject> dataObjects);

        public abstract bool UnlockObjects(ODataQueryOptions queryOptions, IEnumerable<DataObject> dataObjects);
    }
}
