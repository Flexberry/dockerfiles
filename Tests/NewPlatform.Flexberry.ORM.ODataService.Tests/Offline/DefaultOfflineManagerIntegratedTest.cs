namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Offline
{
    using System;
    using System.Collections.Generic;

    using ICSSoft.Services;
    using ICSSoft.STORMNET;

    using Unity;
    using Xunit;

    using Moq;

    using NewPlatform.Flexberry.ORM.ODataService.Offline;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;
    using NewPlatform.Flexberry.Services;

    
    public class DefaultOfflineManagerIntegratedTest : BaseODataServiceIntegratedTest
    {
        [Fact]
        public void TestAcquiringWithoutLock()
        {
            ActODataService(args =>
            {
                var lockService = new LockService(args.DataService);
                var currentUser = new Mock<CurrentUserService.IUser>().Object;
                var offlineManager = new DefaultOfflineManager(lockService, currentUser);
                args.UnityContainer.RegisterInstance<BaseOfflineManager>(offlineManager);

                var лес = new Лес();
                args.DataService.UpdateObject(лес);
                AssertLock.IsNotAcquired(lockService, лес);

                var result = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес)).Result;
                Assert.True(result.IsSuccessStatusCode);
                AssertLock.IsNotAcquired(lockService, лес);
            });
        }

        [Fact]
        public void TestAcquiringWithLock()
        {
            ActODataService(args =>
            {
                var userMock = new Mock<CurrentUserService.IUser>();
                var lockService = new LockService(args.DataService);
                var offlineManager = new DefaultOfflineManager(lockService, userMock.Object);
                args.UnityContainer.RegisterInstance<BaseOfflineManager>(offlineManager);

                var лес = new Лес();
                args.DataService.UpdateObject(лес);
                AssertLock.IsNotAcquired(lockService, лес);

                userMock.Setup(u => u.Login).Returns("user1");
                var result = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес), "user1").Result;
                Assert.True(result.IsSuccessStatusCode);
                AssertLock.IsAcquired(lockService, лес, "user1");
            });
        }

        [Fact]
        public void TestAcquiringLockedObject()
        {
            ActODataService(args =>
            {
                var userMock = new Mock<CurrentUserService.IUser>();
                var lockService = new LockService(args.DataService);
                var offlineManager = new DefaultOfflineManager(lockService, userMock.Object);
                args.UnityContainer.RegisterInstance<BaseOfflineManager>(offlineManager);

                var лес = new Лес();
                args.DataService.UpdateObject(лес);
                AssertLock.IsNotAcquired(lockService, лес);

                // Select data with locking for user1.
                userMock.Setup(u => u.Login).Returns("user1");
                var result1 = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес), "user1").Result;
                Assert.True(result1.IsSuccessStatusCode);
                AssertLock.IsAcquired(lockService, лес, "user1");

                // Select data with locking for user2.
                userMock.Setup(u => u.Login).Returns("user2");
                var result2 = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес), "user2").Result;
                Assert.False(result2.IsSuccessStatusCode);
                AssertLock.IsAcquired(lockService, лес, "user1");
            });
        }

        [Fact]
        public void TestUpdateLockedObjectBySameUserWithFlag()
        {
            ActODataService(args =>
            {
                var userMock = new Mock<CurrentUserService.IUser>();
                userMock.Setup(u => u.Login).Returns("user1");

                var lockService = new LockService(args.DataService);
                var offlineManager = new DefaultOfflineManager(lockService, userMock.Object);
                args.UnityContainer.RegisterInstance<BaseOfflineManager>(offlineManager);

                var лес = new Лес();
                args.DataService.UpdateObject(лес);
                AssertLock.IsNotAcquired(lockService, лес);

                // Select data with locking.
                var result1 = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес), "user1").Result;
                Assert.True(result1.IsSuccessStatusCode);
                AssertLock.IsAcquired(lockService, лес, "user1");

                // Update locked data.
                var result2 = args.HttpClient.PatchAsJsonStringAsync(args.Token.Model, лес, Лес.Views.ЛесE, "user1").Result;
                Assert.True(result2.IsSuccessStatusCode);
                AssertLock.IsNotAcquired(lockService, лес);
            });
        }

        [Fact]
        public void TestUpdateLockedObjectBySameUserWithoutUnlockFlag()
        {
            ActODataService(args =>
            {
                var userMock = new Mock<CurrentUserService.IUser>();
                userMock.Setup(u => u.Login).Returns("user1");

                var lockService = new LockService(args.DataService);
                var offlineManager = new DefaultOfflineManager(lockService, userMock.Object);
                args.UnityContainer.RegisterInstance<BaseOfflineManager>(offlineManager);

                var лес = new Лес();
                args.DataService.UpdateObject(лес);
                AssertLock.IsNotAcquired(lockService, лес);

                // Select data with locking by user1.
                var result1 = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес), "user1").Result;
                Assert.True(result1.IsSuccessStatusCode);
                AssertLock.IsAcquired(lockService, лес, "user1");

                // Update locked data by user1.
                var result2 = args.HttpClient.PatchAsJsonStringAsync(args.Token.Model, лес, Лес.Views.ЛесE).Result;
                Assert.True(result2.IsSuccessStatusCode);
                AssertLock.IsAcquired(lockService, лес, "user1");
            });
        }

        [Fact]
        public void TestUpdateLockedObjectByAnotherUserWithUnlockFlag()
        {
            ActODataService(args =>
            {
                var userMock = new Mock<CurrentUserService.IUser>();
                var lockService = new LockService(args.DataService);
                var offlineManager = new DefaultOfflineManager(lockService, userMock.Object);
                args.UnityContainer.RegisterInstance<BaseOfflineManager>(offlineManager);

                var лес = new Лес();
                args.DataService.UpdateObject(лес);
                AssertLock.IsNotAcquired(lockService, лес);

                // Select data with locking by user1.
                userMock.Setup(u => u.Login).Returns("user1");
                var result1 = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес), "user1").Result;
                Assert.True(result1.IsSuccessStatusCode);
                AssertLock.IsAcquired(lockService, лес, "user1");

                // Update data with unlocking by user2.
                userMock.Setup(u => u.Login).Returns("user2");
                var result2 = args.HttpClient.PatchAsJsonStringAsync(args.Token.Model, лес, Лес.Views.ЛесE, "user2").Result;
                Assert.False(result2.IsSuccessStatusCode);
                AssertLock.IsAcquired(lockService, лес, "user1");
            });
        }

        [Fact]
        public void TestUnlockingAfterErrorInLocking()
        {
            ActODataService(args =>
            {
                var dataObjects = new List<Лес>();
                for (int i = 0; i < 10; i++)
                    dataObjects.Add(new Лес());

                var realLockService = new LockService(args.DataService);
                var lockServiceMock = new Mock<ILockService>();

                // Unlock with real service.
                lockServiceMock.Setup(s => s.UnlockObject(It.IsAny<object>())).Returns<object>(key => realLockService.UnlockObject(key));

                // Lock first 5 objects with real service.
                var counter = 0;
                lockServiceMock
                    .Setup(s => s.LockObject(It.IsAny<object>(), It.IsAny<string>()))
                    .Returns<object, string>(
                        (key, userName) =>
                        {
                            if (counter >= 5)
                                throw new InvalidOperationException();

                            counter++;
                            return realLockService.LockObject(key, userName);
                        });

                var userMock = new Mock<CurrentUserService.IUser>();
                var offlineManager = new DefaultOfflineManager(lockServiceMock.Object, userMock.Object);
                args.UnityContainer.RegisterInstance<BaseOfflineManager>(offlineManager);

                foreach (var dataObject in dataObjects)
                {
                    args.DataService.UpdateObject(dataObject);
                    AssertLock.IsNotAcquired(realLockService, dataObject);
                }

                userMock.Setup(u => u.Login).Returns("user1");
                var result = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес), "user1").Result;
                Assert.False(result.IsSuccessStatusCode);

                foreach (var dataObject in dataObjects)
                    AssertLock.IsNotAcquired(realLockService, dataObject);
            });
        }

        [Fact(Skip = "Need to add real checks for relocking multiple objects.")]
        public void TestLockingAfterErrorInUnlocking()
        {
            ActODataService(args =>
            {
                var dataObjects = new List<Лес>();
                for (int i = 0; i < 10; i++)
                    dataObjects.Add(new Лес { Страна = new Страна() });

                var realLockService = new LockService(args.DataService);
                var lockServiceMock = new Mock<ILockService>();

                lockServiceMock
                    .Setup(s => s.LockObject(It.IsAny<object>(), It.IsAny<string>()))
                    .Returns<object, string>((key, userName) => realLockService.LockObject(key, userName));

                var counter = 0;
                lockServiceMock
                    .Setup(s => s.UnlockObject(It.IsAny<object>()))
                    .Returns<object>(
                        key =>
                        {
                            if (counter >= 5)
                                throw new InvalidOperationException();

                            counter++;
                            return realLockService.UnlockObject(key);
                        });

                var userMock = new Mock<CurrentUserService.IUser>();
                var offlineManager = new DefaultOfflineManager(lockServiceMock.Object, userMock.Object);
                args.UnityContainer.RegisterInstance<BaseOfflineManager>(offlineManager);

                foreach (Лес dataObject in dataObjects)
                {
                    args.DataService.UpdateObject(dataObject);
                    AssertLock.IsNotAcquired(realLockService, dataObject);
                    AssertLock.IsNotAcquired(realLockService, dataObject.Страна);
                }

                userMock.Setup(u => u.Login).Returns("user1");
                var result = args.HttpClient.GetAsync(args.Token.Model, typeof(Лес), "user1").Result;
                Assert.True(result.IsSuccessStatusCode);

                foreach (var dataObject in dataObjects)
                {
                    AssertLock.IsAcquired(realLockService, dataObject, "user1");
                }
            });
        }

        private static class AssertLock
        {
            public static void IsAcquired(ILockService lockService, DataObject dataObject, string userName)
            {
                var lockData = lockService.GetLock(dataObject.__PrimaryKey);
                Assert.True(lockData.Acquired);
                Assert.Equal(userName, lockData.UseName);
            }

            public static void IsNotAcquired(ILockService lockService, DataObject dataObject)
            {
                var lockData = lockService.GetLock(dataObject.__PrimaryKey);
                Assert.False(lockData.Acquired);
                Assert.Null(lockData.UseName);
            }
        }
    }
}
