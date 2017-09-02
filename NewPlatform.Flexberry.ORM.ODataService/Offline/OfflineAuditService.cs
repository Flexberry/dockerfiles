namespace NewPlatform.Flexberry.ORM.ODataService.Offline
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business.Audit;
    using ICSSoft.STORMNET.Business.Audit.Exceptions;
    using ICSSoft.STORMNET.Business.Audit.HelpStructures;
    using ICSSoft.STORMNET.Business.Audit.Objects;

    /// <summary>
    /// Implementation of <see cref="AuditService"/> for offline audit mode.
    /// Instead of manual setting audit fields (<see cref="IDataObjectWithAuditFields"/>) on saving changes,
    /// this implementation preserves existed values.
    /// </summary>
    /// <seealso cref="AuditService" />
    public class OfflineAuditService : AuditService
    {
        /// <summary>
        /// Flag indicates that sorage contains UTC audit dates. If <c>true</c> then UTC dates else if <c>false</c> then local timezone dates. Default is <c>false</c>.
        /// </summary>
        public bool PersistUtcDates { get; set; }

        /// <summary>
        /// Adds audit information for specified new data object.
        /// Does nothing in order to preserve data at <see cref="IDataObjectWithAuditFields"/> fields.
        /// </summary>
        /// <param name="operationedObject">New data object.</param>
        public override void AddCreateAuditInformation(DataObject operationedObject)
        {
        }

        /// <summary>
        /// Adds audit information for specified edited data object.
        /// Does nothing in order to preserve data at <see cref="IDataObjectWithAuditFields"/> fields.
        /// </summary>
        /// <param name="operationedObject">Edited data object.</param>
        public override void AddEditAuditInformation(DataObject operationedObject)
        {
        }

        /// <summary>
        /// Gets the time when auditable operation occurred with the specified object.
        /// </summary>
        /// <remarks>
        /// In the audit update queue of the data service not only objects do from OData (where fields of <see cref="IDataObjectWithAuditFields"/>
        /// should be already set), can appear. For example, objects from a business server or detail aggregator.
        /// Therefore, audit operation time cannot always be recognized and server time will be used for this situation.
        /// </remarks>
        /// <param name="operatedObject">The operated object.</param>
        /// <returns>
        /// Returns time when auditable operation occurred using <see cref="IDataObjectWithAuditFields.CreateTime"/> for new object
        /// or <see cref="IDataObjectWithAuditFields.EditTime"/> for changed object.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="operatedObject"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when data object doesn't implement <see cref="IDataObjectWithAuditFields"/> in order to use with offline audit.</exception>
        protected override DateTime GetAuditOperationTime(DataObject operatedObject)
        {
            Contract.Requires<ArgumentNullException>(operatedObject != null);

            var auditableDataObject = operatedObject as IDataObjectWithAuditFields;
            if (auditableDataObject == null)
                throw new ArgumentException("Data object must implement IDataObjectWithAuditFields in order to use with offline audit.");

            ObjectStatus objectStatus = operatedObject.GetStatus(false);
            switch (objectStatus)
            {
                case ObjectStatus.Created:
                    if (auditableDataObject.CreateTime == null)
                    {
                        if (LogService.Log.IsWarnEnabled)
                        {
                            LogService.LogWarn($"Data object {operatedObject.GetType().FullName}({operatedObject.__PrimaryKey}) is created but doesn't contain create time.");
                        }

                        break;
                    }

                    return auditableDataObject.CreateTime.Value;

                case ObjectStatus.Altered:
                    if (auditableDataObject.EditTime == null)
                    {
                        if (LogService.Log.IsWarnEnabled)
                        {
                            LogService.LogWarn($"Data object {operatedObject.GetType().FullName}({operatedObject.__PrimaryKey}) is changed but doesn't contain edit time.");
                        }

                        break;
                    }

                    return auditableDataObject.EditTime.Value;

                case ObjectStatus.Deleted:
                    // TODO: deleting query through OData doesn't contain body, therefore we cannot set right audit operation time
                    break;

                case ObjectStatus.UnAltered:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unknown DataObject status: {objectStatus}");
            }

            if (PersistUtcDates)
            {
                return DateTimeOffset.UtcNow.DateTime;
            }

            return base.GetAuditOperationTime(operatedObject);
        }

        /// <summary>
        /// Подтверждение созданных ранее операций аудита
        ///             (если аудит идёт в одну БД с приложением, то будет использован сервис данных по умолчанию).
        /// </summary>
        /// <param name="executionVariant">Какой статус будет присвоен операции.</param><param name="auditOperationInfoList">Информация о том, что и куда в аудит нужно добавить.</param><param name="dataServiceConnectionString">Строка соединения сервиса данных, который выполняет запись в БД приложения. </param><param name="dataServiceType">Тип сервиса данных, который выполняет запись в БД приложения. </param><param name="throwExceptions">Следует ли пробрасывать дальше возникшее исключение.</param><param name="checkClassAuditSettings">Следует ли проверять настройки аудита самого класса.</param>
        /// <returns>
        /// <c>True</c>, если всё закончилось без ошибок. 
        /// </returns>
        protected override bool RatifyAuditOperation(tExecutionVariant executionVariant, List<AuditAdditionalInfo> auditOperationInfoList, string dataServiceConnectionString, Type dataServiceType, bool throwExceptions, bool checkClassAuditSettings = false)
        {
            if (PersistUtcDates)
            {
                try
                {
                    if (AppSetting == null || !AppSetting.AuditEnabled)
                    {
                        throw new DisabledAuditException();
                    }

                    if (auditOperationInfoList != null && auditOperationInfoList.Count > 0)
                    {
                        // Настройки вообще есть и аудит для приложения включён.
                        var auditRatifyParameters = new RatificationAuditParameters(
                            executionVariant,
                            DateTimeOffset.UtcNow.DateTime,
                            auditOperationInfoList,
                            AppSetting.DefaultWriteMode,
                            ApplicationMode,
                            AppSetting.IsDatabaseLocal ? GetConnectionStringName(dataServiceConnectionString, dataServiceType) : AppSetting.AuditConnectionStringName,
                            IsAuditRemote)
                        { ThrowExceptions = throwExceptions };

                        CheckAndSendToAudit(auditRatifyParameters, checkClassAuditSettings);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    ErrorProcesser.ProcessAuditError(ex, "AuditService, RatifyAuditOperation", throwExceptions);
                    return false;
                }
            }

            return base.RatifyAuditOperation(executionVariant, auditOperationInfoList, dataServiceConnectionString, dataServiceType, throwExceptions, checkClassAuditSettings);
        }
    }
}
