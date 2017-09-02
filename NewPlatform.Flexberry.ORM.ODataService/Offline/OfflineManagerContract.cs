namespace NewPlatform.Flexberry.ORM.ODataService.Offline
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Query;

    using ICSSoft.STORMNET;

    [ContractClassFor(typeof(BaseOfflineManager))]
    internal abstract class OfflineManagerContract : BaseOfflineManager
    {
        protected override bool IsLockingRequired(ODataQueryOptions queryOptions, DataObject dataObject)
        {
            Contract.Requires<ArgumentNullException>(queryOptions != null);
            Contract.Requires<ArgumentNullException>(dataObject != null);

            throw new InvalidOperationException();
        }

        protected override bool IsUnlockingRequired(ODataQueryOptions queryOptions, DataObject dataObject)
        {
            Contract.Requires<ArgumentNullException>(queryOptions != null);
            Contract.Requires<ArgumentNullException>(dataObject != null);

            throw new InvalidOperationException();
        }

        public override bool LockObjects(ODataQueryOptions queryOptions, IEnumerable<DataObject> dataObjects)
        {
            Contract.Requires<ArgumentNullException>(queryOptions != null);
            Contract.Requires<ArgumentNullException>(dataObjects != null);

            throw new InvalidOperationException();
        }

        public override bool UnlockObjects(ODataQueryOptions queryOptions, IEnumerable<DataObject> dataObjects)
        {
            Contract.Requires<ArgumentNullException>(queryOptions != null);
            Contract.Requires<ArgumentNullException>(dataObjects != null);

            throw new InvalidOperationException();
        }
    }
}