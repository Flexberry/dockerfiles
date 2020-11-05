namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Offline
{
    using System;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.Audit;
    using ICSSoft.STORMNET.Business.Audit.Objects;
    using ICSSoft.STORMNET.Security;
    using NewPlatform.Flexberry.ORM.ODataService.Offline;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ORM-integrated unit test for <see cref="OfflineAuditService"/>.
    /// </summary>
    /// <seealso cref="BaseIntegratedTest" />
    public class OfflineAuditServiceIntegratedTest : BaseIntegratedTest
    {
#if NETFRAMEWORK
        /// <summary>
        /// Initializes a new instance of the <see cref="OfflineAuditServiceIntegratedTest"/> class.
        /// </summary>
        public OfflineAuditServiceIntegratedTest()
            : base("offline")
        {
        }
#endif
#if NETCOREAPP
        /// <summary>
        /// Initializes a new instance of the <see cref="OfflineAuditServiceIntegratedTest"/> class.
        /// </summary>
        public OfflineAuditServiceIntegratedTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, ITestOutputHelper output)
            : base(factory, output, "offline")
        {
        }
#endif

        /// <summary>
        /// Unit test for <see cref="AuditService.AddCreateAuditInformation"/> and <see cref="AuditService.AddEditAuditInformation"/>.
        /// Tests correctness of setting of audit fields from <see cref="IDataObjectWithAuditFields"/> for the type with enabled audit.
        /// </summary>
        [Fact(Skip = "Add check of real writing after TFS 113885.")]
        public void TestSettingAuditFieldsForTypeWithEnabledAudit()
        {
            foreach (var dataService in DataServices)
            {
                var d1 = new Медведь();

                // Audit fields are empty after creating object.
                Assert.Null(d1.CreateTime);
                Assert.Null(d1.Creator);
                Assert.Null(d1.Editor);
                Assert.Null(d1.EditTime);

                var createTime = DateTime.Now.AddDays(new Random().Next(42));
                var creator = "Bruce the Hunter";
                d1.CreateTime = createTime;
                d1.Creator = creator;

                dataService.UpdateObject(d1);

                // Audit fields are empty after persisting.
                Assert.Equal(creator, d1.Creator);
                Assert.Equal(createTime, d1.CreateTime);
                Assert.Null(d1.Editor);
                Assert.Null(d1.EditTime);

                var d2 = new Медведь();
                d2.SetExistObjectPrimaryKey(d1.__PrimaryKey);
                dataService.LoadObject(d2);

                // Audit fields are persisted.
                Assert.Equal(creator, d2.Creator);
                Assert.NotNull(d2.CreateTime);
                Assert.Equal(0, (int)(createTime - d2.CreateTime.Value).TotalSeconds);
                Assert.Null(d2.Editor);
                Assert.Null(d2.EditTime);

                d2.ПорядковыйНомер = 42;

                var editTime = createTime.AddDays(new Random().Next(42));
                var editor = "Mike the Hunter";
                d2.EditTime = editTime;
                d2.Editor = editor;

                dataService.UpdateObject(d2);

                // Audit fields are empty after persisting.
                Assert.Equal(creator, d2.Creator);
                Assert.NotNull(d2.CreateTime);
                Assert.Equal(0, (int)(createTime - d2.CreateTime.Value).TotalSeconds);
                Assert.Equal(editor, d2.Editor);
                Assert.Equal(editTime, d2.EditTime);

                var d3 = new Медведь();
                d3.SetExistObjectPrimaryKey(d2.__PrimaryKey);
                dataService.LoadObject(d3);

                // Audit fields are persisted.
                Assert.Equal(creator, d3.Creator);
                Assert.NotNull(d3.CreateTime);
                Assert.Equal(0, (int)(createTime - d3.CreateTime.Value).TotalSeconds);
                Assert.Equal(editor, d3.Editor);
                Assert.NotNull(d3.EditTime);
                Assert.Equal(0, (int)(editTime - d3.EditTime.Value).TotalSeconds);
            }
        }

        /// <summary>
        /// Creates the <see cref="MSSQLDataService" /> instance for temp database with offline audit service for testing.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="MSSQLDataService" /> instance.</returns>
        protected override MSSQLDataService CreateMssqlDataService(string connectionString)
        {
            return new MSSQLDataService(new EmptySecurityManager(), GetAuditServiceForTest()) { CustomizationString = connectionString };
        }

        /// <summary>
        /// Creates the <see cref="PostgresDataService" /> instance for temp database with offline audit service for testing.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="PostgresDataService" /> instance.</returns>
        protected override PostgresDataService CreatePostgresDataService(string connectionString)
        {
            return new PostgresDataService(new EmptySecurityManager(), GetAuditServiceForTest()) { CustomizationString = connectionString };
        }

        /// <summary>
        /// Gets the offline audit service for the test.
        /// </summary>
        /// <returns>Returns instance of the <see cref="OfflineAuditService" /> class that will be used for the test.</returns>
        protected AuditService GetAuditServiceForTest()
        {
            return new OfflineAuditService
            {
                AppSetting = new AuditAppSetting { AuditEnabled = true },
                ApplicationMode = AppMode.Win,
                Audit = new Audit()
            };
        }
    }
}
