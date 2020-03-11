/*

Create tables.
Create user Administrator (login=admin, password=admin).
Create permissions for Administrator.

*/

CREATE TABLE SubStatisticsMonitor (

 primaryKey UUID NOT NULL,

 Код INT NOT NULL,

 Категория VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Подписка UUID NOT NULL,

 StatisticsMonitor UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Шина (

 primaryKey UUID NOT NULL,

 InteropАдрес VARCHAR(255) NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Ид VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 Description VARCHAR NULL,

 Адрес VARCHAR(255) NULL,

 DnsIdentity VARCHAR(255) NULL,

 ConnectionsLimit INT NULL,

 SequentialSent BOOLEAN NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatRecord (

 primaryKey UUID NOT NULL,

 Since TIMESTAMP(3) NOT NULL,

 "To" TIMESTAMP(3) NOT NULL,

 StatInterval VARCHAR(12) NOT NULL,

 SentCount INT NULL,

 ReceivedCount INT NULL,

 ErrorsCount INT NULL,

 UniqueErrorsCount INT NULL,

 ConnectionCount INT NULL,

 QueueLength INT NULL,

 AvgTimeSent INT NULL,

 AvgTimeSql INT NULL,

 StatSetting UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatSetting (

 primaryKey UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Подписка UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Клиент (

 primaryKey UUID NOT NULL,

 Ид VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 Description VARCHAR NULL,

 Адрес VARCHAR(255) NULL,

 DnsIdentity VARCHAR(255) NULL,

 ConnectionsLimit INT NULL,

 SequentialSent BOOLEAN NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE CompressionSetting (

 primaryKey UUID NOT NULL,

 TargetCompression VARCHAR(12) NOT NULL,

 LifetimeLimit INT NOT NULL,

 LifetimeUnits VARCHAR(6) NOT NULL,

 Period INT NOT NULL,

 PeriodUnits VARCHAR(6) NOT NULL,

 NextCompressionTime TIMESTAMP(3) NOT NULL,

 LastCompressionTime TIMESTAMP(3) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 StatSetting UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Сообщение (

 primaryKey UUID NOT NULL,

 ВремяСледующейОтправки TIMESTAMP(3) NOT NULL,

 ВремяФормирования TIMESTAMP(3) NOT NULL,

 Отправляется BOOLEAN NULL,

 FailsCount INT NULL,

 Отправитель VARCHAR(255) NULL,

 Тело TEXT NULL,

 ВложениеДляБазы TEXT NULL,

 Приоритет INT NULL,

 ИмяГруппы VARCHAR(255) NULL,

 Тэги VARCHAR NULL,

 LogMessages VARCHAR NULL,

 ТипСообщения_m0 UUID NOT NULL,

 Получатель_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE OutboundMessageTypeRestriction (

 primaryKey UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 ТипСообщения UUID NOT NULL,

 Клиент UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Подписка (

 primaryKey UUID NOT NULL,

 Описание VARCHAR NULL,

 ExpiryDate TIMESTAMP(3) NOT NULL,

 IsCallback BOOLEAN NULL,

 ПередаватьПо VARCHAR(4) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 ТипСообщения_m0 UUID NOT NULL,

 Клиент_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatisticsMonitor (

 primaryKey UUID NOT NULL,

 Логин VARCHAR(255) NULL,

 Наименование VARCHAR(255) NOT NULL,

 ДоступенДругимПользователям BOOLEAN NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE ТипСообщения (

 primaryKey UUID NOT NULL,

 Ид VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 Комментарий VARCHAR NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMNETLOCKDATA (

 LockKey VARCHAR(300) NOT NULL,

 UserName VARCHAR(300) NOT NULL,

 LockDate TIMESTAMP(3) NULL,

 PRIMARY KEY (LockKey));


CREATE TABLE STORMSETTINGS (

 primaryKey UUID NOT NULL,

 Module VARCHAR(1000) NULL,

 Name VARCHAR(255) NULL,

 Value TEXT NULL,

 "User" VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAdvLimit (

 primaryKey UUID NOT NULL,

 "User" VARCHAR(255) NULL,

 Published BOOLEAN NULL,

 Module VARCHAR(255) NULL,

 Name VARCHAR(255) NULL,

 Value TEXT NULL,

 HotKeyData INT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMFILTERSETTING (

 primaryKey UUID NOT NULL,

 Name VARCHAR(255) NOT NULL,

 DataObjectView VARCHAR(255) NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMWEBSEARCH (

 primaryKey UUID NOT NULL,

 Name VARCHAR(255) NOT NULL,

 "Order" INT NOT NULL,

 PresentView VARCHAR(255) NOT NULL,

 DetailedView VARCHAR(255) NOT NULL,

 FilterSetting_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMFILTERDETAIL (

 primaryKey UUID NOT NULL,

 Caption VARCHAR(255) NOT NULL,

 DataObjectView VARCHAR(255) NOT NULL,

 ConnectMasterProp VARCHAR(255) NOT NULL,

 OwnerConnectProp VARCHAR(255) NULL,

 FilterSetting_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMFILTERLOOKUP (

 primaryKey UUID NOT NULL,

 DataObjectType VARCHAR(255) NOT NULL,

 Container VARCHAR(255) NULL,

 ContainerTag VARCHAR(255) NULL,

 FieldsToView VARCHAR(255) NULL,

 FilterSetting_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE UserSetting (

 primaryKey UUID NOT NULL,

 AppName VARCHAR(256) NULL,

 UserName VARCHAR(512) NULL,

 UserGuid UUID NULL,

 ModuleName VARCHAR(1024) NULL,

 ModuleGuid UUID NULL,

 SettName VARCHAR(256) NULL,

 SettGuid UUID NULL,

 SettLastAccessTime TIMESTAMP(3) NULL,

 StrVal VARCHAR(256) NULL,

 TxtVal TEXT NULL,

 IntVal INT NULL,

 BoolVal BOOLEAN NULL,

 GuidVal UUID NULL,

 DecimalVal DECIMAL(20,10) NULL,

 DateTimeVal TIMESTAMP(3) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE ApplicationLog (

 primaryKey UUID NOT NULL,

 Category VARCHAR(64) NULL,

 EventId INT NULL,

 Priority INT NULL,

 Severity VARCHAR(32) NULL,

 Title VARCHAR(256) NULL,

 Timestamp TIMESTAMP(3) NULL,

 MachineName VARCHAR(32) NULL,

 AppDomainName VARCHAR(512) NULL,

 ProcessId VARCHAR(256) NULL,

 ProcessName VARCHAR(512) NULL,

 ThreadName VARCHAR(512) NULL,

 Win32ThreadId VARCHAR(128) NULL,

 Message VARCHAR(2500) NULL,

 FormattedMessage TEXT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAuObjType (

 primaryKey UUID NOT NULL,

 Name VARCHAR(255) NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAuEntity (

 primaryKey UUID NOT NULL,

 ObjectPrimaryKey VARCHAR(38) NOT NULL,

 OperationTime TIMESTAMP(3) NOT NULL,

 OperationType VARCHAR(100) NOT NULL,

 ExecutionResult VARCHAR(12) NOT NULL,

 Source VARCHAR(255) NOT NULL,

 SerializedField TEXT NULL,

 User_m0 UUID NOT NULL,

 ObjectType_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAuField (

 primaryKey UUID NOT NULL,

 Field VARCHAR(100) NOT NULL,

 OldValue TEXT NULL,

 NewValue TEXT NULL,

 MainChange_m0 UUID NULL,

 AuditEntity_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAG (

 primaryKey UUID NOT NULL,

 Name VARCHAR(80) NOT NULL,

 Login VARCHAR(50) NULL,

 Pwd VARCHAR(50) NULL,

 IsUser BOOLEAN NOT NULL,

 IsGroup BOOLEAN NOT NULL,

 IsRole BOOLEAN NOT NULL,

 ConnString VARCHAR(255) NULL,

 Enabled BOOLEAN NULL,

 Email VARCHAR(80) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLG (

 primaryKey UUID NOT NULL,

 Group_m0 UUID NOT NULL,

 User_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMI (

 primaryKey UUID NOT NULL,

 User_m0 UUID NOT NULL,

 Agent_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Session (

 primaryKey UUID NOT NULL,

 UserKey UUID NULL,

 StartedAt TIMESTAMP(3) NULL,

 LastAccess TIMESTAMP(3) NULL,

 Closed BOOLEAN NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMS (

 primaryKey UUID NOT NULL,

 Name VARCHAR(100) NOT NULL,

 Type VARCHAR(100) NULL,

 IsAttribute BOOLEAN NOT NULL,

 IsOperation BOOLEAN NOT NULL,

 IsView BOOLEAN NOT NULL,

 IsClass BOOLEAN NOT NULL,

 SharedOper BOOLEAN NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMP (

 primaryKey UUID NOT NULL,

 Subject_m0 UUID NOT NULL,

 Agent_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMF (

 primaryKey UUID NOT NULL,

 FilterText TEXT NULL,

 Name VARCHAR(255) NULL,

 FilterTypeNView VARCHAR(255) NULL,

 Subject_m0 UUID NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAC (

 primaryKey UUID NOT NULL,

 TypeAccess VARCHAR(7) NULL,

 Filter_m0 UUID NULL,

 Permition_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLO (

 primaryKey UUID NOT NULL,

 Class_m0 UUID NOT NULL,

 Operation_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLA (

 primaryKey UUID NOT NULL,

 View_m0 UUID NOT NULL,

 Attribute_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLV (

 primaryKey UUID NOT NULL,

 Class_m0 UUID NOT NULL,

 View_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLR (

 primaryKey UUID NOT NULL,

 StartDate TIMESTAMP(3) NULL,

 EndDate TIMESTAMP(3) NULL,

 Agent_m0 UUID NOT NULL,

 Role_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));




 ALTER TABLE SubStatisticsMonitor ADD CONSTRAINT FK8a6a2b4b3a9444639b4fd5141a7ec704 FOREIGN KEY (Подписка) REFERENCES Подписка; 
CREATE INDEX Index41cd22aa89dc4b2c9d67e82fb2112eae on SubStatisticsMonitor (Подписка); 

 ALTER TABLE SubStatisticsMonitor ADD CONSTRAINT FK74e37725d4c047d58570033fdcf1ea59 FOREIGN KEY (StatisticsMonitor) REFERENCES StatisticsMonitor; 
CREATE INDEX Indexb8ac335353a2453d9288aefcaac24d8e on SubStatisticsMonitor (StatisticsMonitor); 

 ALTER TABLE StatRecord ADD CONSTRAINT FK04e3af39ebd040608ee40fe06cc46dbb FOREIGN KEY (StatSetting) REFERENCES StatSetting; 
CREATE INDEX Indexae2b72fce5be4f32b86361063c4da916 on StatRecord (StatSetting); 

 ALTER TABLE StatSetting ADD CONSTRAINT FK5727b3a8084a4ed78629920e414cb587 FOREIGN KEY (Подписка) REFERENCES Подписка; 
CREATE INDEX Indexd3dbabd961e54d4cb4547d74443d2650 on StatSetting (Подписка); 

 ALTER TABLE CompressionSetting ADD CONSTRAINT FK4dc3421968c8449ba61dfade856d9581 FOREIGN KEY (StatSetting) REFERENCES StatSetting; 
CREATE INDEX Index9bea919738a04b7aa2d8839e68266a7e on CompressionSetting (StatSetting); 

 ALTER TABLE Сообщение ADD CONSTRAINT FKe14fae1656244b66ac3e5bb5e7e3c926 FOREIGN KEY (ТипСообщения_m0) REFERENCES ТипСообщения; 
CREATE INDEX Indexe268a65845da4067896b10eae38be2d7 on Сообщение (ТипСообщения_m0); 

 ALTER TABLE Сообщение ADD CONSTRAINT FKbc8d7040a74b41cd9b377153bd332910 FOREIGN KEY (Получатель_m0) REFERENCES Клиент; 
CREATE INDEX Index83f028fbaf0f45ff8a5382637f051b51 on Сообщение (Получатель_m0); 

 ALTER TABLE OutboundMessageTypeRestriction ADD CONSTRAINT FKc95ff00794304667b360604f946d0283 FOREIGN KEY (ТипСообщения) REFERENCES ТипСообщения; 
CREATE INDEX Indexb75f93e5af19413cb5f6ef6884a3b1c6 on OutboundMessageTypeRestriction (ТипСообщения); 

 ALTER TABLE OutboundMessageTypeRestriction ADD CONSTRAINT FKa3ceae544fa94d32b165c6082735e64b FOREIGN KEY (Клиент) REFERENCES Клиент; 
CREATE INDEX Index23dac33a56ab42eabd8926308ffeb2a9 on OutboundMessageTypeRestriction (Клиент); 

 ALTER TABLE Подписка ADD CONSTRAINT FKdc3c6041a1d845498e2f17170759315d FOREIGN KEY (ТипСообщения_m0) REFERENCES ТипСообщения; 
CREATE INDEX Indexecbfde96a74d41a69582a43fd4481d5b on Подписка (ТипСообщения_m0); 

 ALTER TABLE Подписка ADD CONSTRAINT FKdc6e2fc1509244928f90b66f1b1c4666 FOREIGN KEY (Клиент_m0) REFERENCES Клиент; 
CREATE INDEX Indexa4afd486ce2a43f3bad76e57d264051a on Подписка (Клиент_m0); 

 ALTER TABLE STORMWEBSEARCH ADD CONSTRAINT FKaeb0ff228056437fa5002b4e429bc05d FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERDETAIL ADD CONSTRAINT FKfb1ba536529c4bca80cf14bf92410d7a FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERLOOKUP ADD CONSTRAINT FK89ac92cc6df44f0d9e9758ad29e0800a FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMAuEntity ADD CONSTRAINT FKe59895b9d59f431aaedcc5e799e0e5ae FOREIGN KEY (ObjectType_m0) REFERENCES STORMAuObjType; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK3aef9d933ac54e5487f1cd10b9dfb096 FOREIGN KEY (MainChange_m0) REFERENCES STORMAuField; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FKa2077ff3d2ab400c880321c19eb02efe FOREIGN KEY (AuditEntity_m0) REFERENCES STORMAuEntity; 

 ALTER TABLE STORMLG ADD CONSTRAINT FK091280e60ab944a9b2e3363d4c124850 FOREIGN KEY (Group_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMLG ADD CONSTRAINT FK3cd5a41855b94fb2b70a466c99c829ea FOREIGN KEY (User_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMI ADD CONSTRAINT FK679b1cb8d99b4676a65f066578151712 FOREIGN KEY (User_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMI ADD CONSTRAINT FK155765f56af44871ae94b8950a3dd897 FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMP ADD CONSTRAINT FK3b479ec75bec4fd7913c04feee1bc9a7 FOREIGN KEY (Subject_m0) REFERENCES STORMS; 

 ALTER TABLE STORMP ADD CONSTRAINT FK32768c55824240c3a8a3e28eb3f53ff2 FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMF ADD CONSTRAINT FK48a07ea23aa948be9e5a4b44e3afeb61 FOREIGN KEY (Subject_m0) REFERENCES STORMS; 

 ALTER TABLE STORMAC ADD CONSTRAINT FK6d84557bbb8b4853a55fd6965f685429 FOREIGN KEY (Filter_m0) REFERENCES STORMF; 

 ALTER TABLE STORMAC ADD CONSTRAINT FKd1d723d45afb4f2c8dee7f5477494cc1 FOREIGN KEY (Permition_m0) REFERENCES STORMP; 

 ALTER TABLE STORMLO ADD CONSTRAINT FKf947a1f83b664c328436312cd7dac7ae FOREIGN KEY (Class_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLO ADD CONSTRAINT FKae2bd90e6b2e4dd3bcb8a600763f7368 FOREIGN KEY (Operation_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLA ADD CONSTRAINT FK3c0d199053b640d99ddb617a01a55783 FOREIGN KEY (View_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLA ADD CONSTRAINT FK59eb5f11b3534f84931e3798756f5b71 FOREIGN KEY (Attribute_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLV ADD CONSTRAINT FK3ff49480c4994f7cbfae46108b6dc9be FOREIGN KEY (Class_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLV ADD CONSTRAINT FK0ad42f60f8d6486d9fa49d15f8547979 FOREIGN KEY (View_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLR ADD CONSTRAINT FK79a65a2b3f7740c4b97464bde738ca90 FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMLR ADD CONSTRAINT FK564871da808941749713db44c4d7248d FOREIGN KEY (Role_m0) REFERENCES STORMAG; 



 INSERT INTO STORMAG(primaryKey, Name, Login, Pwd, IsUser, IsGroup, IsRole, Enabled)
	VALUES (uuid_in(md5(random()::text)::cstring), 'Administrator', 'admin', 'D033E22AE348AEB5660FC2140AEC35850C4DA997', TRUE, FALSE, FALSE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'admin', null, null, FALSE, FALSE, TRUE, TRUE);

INSERT INTO STORMLR(primaryKey, Agent_m0, Role_m0)
	VALUES (uuid_in(md5(random()::text)::cstring), (SELECT primaryKey FROM STORMAG WHERE Name = 'Administrator'), (SELECT primaryKey FROM STORMAG WHERE Name = 'admin'));

INSERT INTO STORMS(primaryKey, Name, IsAttribute, IsOperation, IsView, IsClass, SharedOper)
	VALUES (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.SendingPermission', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.MessageType', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.Subscription', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.StatisticsCompressionSetting', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.StatisticsSetting', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.SubscriptionStatisticsMonitor', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.Client', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.Message', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.StatisticsMonitor', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.StatisticsRecord', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.Bus', FALSE, FALSE, FALSE, TRUE, TRUE);

INSERT INTO STORMP(primaryKey, Subject_m0, Agent_m0)
	SELECT uuid_in(md5(random()::text)::cstring), primaryKey, (SELECT primaryKey FROM STORMAG WHERE Name = 'admin') FROM STORMS;

INSERT INTO STORMAC(primaryKey, TypeAccess, Permition_m0)
	SELECT uuid_in(md5(random()::text)::cstring), 'Full', primaryKey FROM STORMP;

