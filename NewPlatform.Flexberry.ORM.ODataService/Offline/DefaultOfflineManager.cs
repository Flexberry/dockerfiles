namespace NewPlatform.Flexberry.ORM.ODataService.Offline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.OData.Query;

    using ICSSoft.Services;
    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.Services;

    public class DefaultOfflineManager : BaseOfflineManager
    {
        private readonly ILockService _lockService;

        private readonly CurrentUserService.IUser _currentUser;

        public DefaultOfflineManager(ILockService lockService, CurrentUserService.IUser currentUser)
        {
            _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService), "Contract assertion not met: lockService != null");
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser), "Contract assertion not met: currentUser != null");
        }

        protected override bool IsLockingRequired(ODataQueryOptions queryOptions, DataObject dataObject)
        {
            if (queryOptions == null)
            {
                throw new ArgumentNullException(nameof(queryOptions), "Contract assertion not met: queryOptions != null");
            }

            if (dataObject == null)
            {
                throw new ArgumentNullException(nameof(dataObject), "Contract assertion not met: dataObject != null");
            }

            return queryOptions.Request.Headers.Any(i => i.Key == "Flexberry-Sync" && i.Value.Contains("Lock"));
        }

        protected override bool IsUnlockingRequired(ODataQueryOptions queryOptions, DataObject dataObject)
        {
            if (queryOptions == null)
            {
                throw new ArgumentNullException(nameof(queryOptions), "Contract assertion not met: queryOptions != null");
            }

            if (dataObject == null)
            {
                throw new ArgumentNullException(nameof(dataObject), "Contract assertion not met: dataObject != null");
            }

            return queryOptions.Request.Headers.Any(i => i.Key == "Flexberry-Sync" && i.Value.Contains("Unlock"));
        }

        public override bool LockObjects(ODataQueryOptions queryOptions, IEnumerable<DataObject> dataObjects)
        {
            if (queryOptions == null)
            {
                throw new ArgumentNullException(nameof(queryOptions), "Contract assertion not met: queryOptions != null");
            }

            if (dataObjects == null)
            {
                throw new ArgumentNullException(nameof(dataObjects), "Contract assertion not met: dataObjects != null");
            }

            var lockKeys = new List<object>();
            try
            {
                foreach (var dataObject in dataObjects)
                {
                    if (!IsLockingRequired(queryOptions, dataObject))
                        continue;

                    var lockData = _lockService.LockObject(dataObject.__PrimaryKey, _currentUser.Login);
                    if (!lockData.Acquired)
                    {
                        Unlock(lockKeys);
                        return false;
                    }

                    lockKeys.Add(lockData.Key);
                }

                return true;
            }
            catch (Exception)
            {
                Unlock(lockKeys);
            }

            return false;
        }

        public override bool UnlockObjects(ODataQueryOptions queryOptions, IEnumerable<DataObject> dataObjects)
        {
            if (queryOptions == null)
            {
                throw new ArgumentNullException(nameof(queryOptions), "Contract assertion not met: queryOptions != null");
            }

            if (dataObjects == null)
            {
                throw new ArgumentNullException(nameof(dataObjects), "Contract assertion not met: dataObjects != null");
            }

            var unlockedKeys = new List<object>();
            try
            {
                foreach (var dataObject in dataObjects)
                {
                    if (!IsUnlockingRequired(queryOptions, dataObject))
                        continue;

                    var lockData = _lockService.GetLock(dataObject.__PrimaryKey);
                    if (lockData.Key != null)
                    {
                        if (lockData.UseName != _currentUser.Login)
                        {
                            // TODO: rollback
                            return false;
                        }

                        _lockService.UnlockObject(lockData.Key);
                        unlockedKeys.Add(lockData.Key);
                    }
                }

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        private void Unlock(IEnumerable<object> lockKeys)
        {
            foreach (var lockKey in lockKeys)
            {
                _lockService.UnlockObject(lockKey);
            }
        }
    }
}