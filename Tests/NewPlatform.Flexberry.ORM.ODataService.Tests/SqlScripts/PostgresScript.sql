CREATE TABLE Driver (

 primaryKey UUID NOT NULL,

 Name VARCHAR(255) NULL,

 CarCount INT NULL,

 Documents BOOLEAN NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Страна (

 primaryKey UUID NOT NULL,

 Название VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Master (

 primaryKey UUID NOT NULL,

 property VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Лес (

 primaryKey UUID NOT NULL,

 Название VARCHAR(255) NULL,

 Площадь INT NULL,

 Заповедник BOOLEAN NULL,

 ДатаПослОсмотра TIMESTAMP(3) NULL,

 Страна UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE TestDetailWithCicle (

 primaryKey UUID NOT NULL,

 TestDetailName VARCHAR(255) NULL,

 Parent UUID NULL,

 TestMaster UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE КлассСМножТипов (

 primaryKey UUID NOT NULL,

 PropertyGeography geometry NULL,

 PropertyEnum VARCHAR(6) NULL,

 PropertyBool BOOLEAN NULL,

 PropertyInt INT NULL,

 PropertyDateTime TIMESTAMP(3) NULL,

 PropertyString VARCHAR(255) NULL,

 PropertyFloat REAL NULL,

 PropertyDouble DOUBLE PRECISION NULL,

 PropertyDecimal DECIMAL NULL,

 PropertySystemNullableDateTime TIMESTAMP(3) NULL,

 PropertySystemNullableInt INT NULL,

 PropertySystemNullableGuid UUID NULL,

 PropertySystemNullableDecimal DECIMAL NULL,

 PropStormnetNullableDateTime TIMESTAMP(3) NULL,

 PropertyStormnetNullableInt INT NULL,

 PropertyStormnetKeyGuid UUID NULL,

 PropStormnetNullableDecimal DECIMAL NULL,

 PropertyStormnetPartliedDate VARCHAR(255) NULL,

 PropertyStormnetContact TEXT NULL,

 PropertyStormnetBlob TEXT NULL,

 PropertyStormnetEvent TEXT NULL,

 PropertyStormnetGeoData TEXT NULL,

 PropertyStormnetImage TEXT NULL,

 PropertyStormnetWebFile TEXT NULL,

 PropertyStormnetFile TEXT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Медведь (

 primaryKey UUID NOT NULL,

 ПолеБС VARCHAR(255) NULL,

 ПорядковыйНомер INT NULL,

 Вес INT NULL,

 ЦветГлаз VARCHAR(255) NULL,

 Пол VARCHAR(9) NULL,

 ДатаРождения TIMESTAMP(3) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Страна UUID NULL,

 ЛесОбитания UUID NULL,

 Мама UUID NULL,

 Папа UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Детейл (

 primaryKey UUID NOT NULL,

 prop1 INT NULL,

 БазовыйКласс_m0 UUID NULL,

 БазовыйКласс_m1 UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE ДочернийКласс (

 primaryKey UUID NOT NULL,

 ChildProperty VARCHAR(255) NULL,

 PropertyGeography GEOGRAPHY NULL,

 PropertyEnum VARCHAR(6) NULL,

 PropertyBool BOOLEAN NULL,

 PropertyInt INT NULL,

 PropertyDateTime TIMESTAMP(3) NULL,

 PropertyString VARCHAR(255) NULL,

 PropertyFloat REAL NULL,

 PropertyDouble DOUBLE PRECISION NULL,

 PropertyDecimal DECIMAL NULL,

 PropertySystemNullableDateTime TIMESTAMP(3) NULL,

 PropertySystemNullableInt INT NULL,

 PropertySystemNullableGuid UUID NULL,

 PropertySystemNullableDecimal DECIMAL NULL,

 PropStormnetNullableDateTime TIMESTAMP(3) NULL,

 PropertyStormnetNullableInt INT NULL,

 PropertyStormnetKeyGuid UUID NULL,

 PropStormnetNullableDecimal DECIMAL NULL,

 PropertyStormnetPartliedDate VARCHAR(255) NULL,

 PropertyStormnetContact TEXT NULL,

 PropertyStormnetBlob TEXT NULL,

 PropertyStormnetEvent TEXT NULL,

 PropertyStormnetGeoData TEXT NULL,

 PropertyStormnetImage TEXT NULL,

 PropertyStormnetWebFile TEXT NULL,

 PropertyStormnetFile TEXT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Книга (

 primaryKey UUID NOT NULL,

 Название VARCHAR(255) NULL,

 Автор1 UUID NOT NULL,

 Библиотека1 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE БазовыйКласс (

 primaryKey UUID NOT NULL,

 Свойство1 VARCHAR(255) NULL,

 Свойство2 INT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE ПоставщикКниг (

 primaryKey UUID NOT NULL,

 Ссылка UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Мастер2 (

 primaryKey UUID NOT NULL,

 свойство2 INT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Наследник (

 primaryKey UUID NOT NULL,

 Свойство DOUBLE PRECISION NULL,

 Свойство1 VARCHAR(255) NULL,

 Свойство2 INT NULL,

 Мастер UUID NULL,

 Master UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Car (

 primaryKey UUID NOT NULL,

 Number VARCHAR(255) NULL,

 Model VARCHAR(255) NULL,

 TipCar VARCHAR(9) NULL,

 driver UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Мастер (

 primaryKey UUID NOT NULL,

 prop VARCHAR(255) NULL,

 Мастер2 UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Блоха (

 primaryKey UUID NOT NULL,

 Кличка VARCHAR(255) NULL,

 МедведьОбитания UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE КлассСоСтрокКл (

 StoragePrimaryKey VARCHAR(255) NOT NULL,

 PRIMARY KEY (StoragePrimaryKey));


CREATE TABLE Берлога (

 primaryKey UUID NOT NULL,

 ПолеБС VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 Комфортность INT NULL,

 Заброшена BOOLEAN NULL,

 ЛесРасположения UUID NULL,

 Медведь UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Автор (

 primaryKey UUID NOT NULL,

 Имя VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE TestMaster (

 primaryKey UUID NOT NULL,

 TestMasterName VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Библиотека (

 primaryKey UUID NOT NULL,

 Адрес VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Детейл2 (

 primaryKey UUID NOT NULL,

 prop2 VARCHAR(255) NULL,

 Детейл UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Журнал (

 primaryKey UUID NOT NULL,

 Название VARCHAR(255) NULL,

 Номер INT NULL,

 Автор2 UUID NOT NULL,

 Библиотека2 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE КлассStoredDerived (

 primaryKey UUID NOT NULL,

 StrAttr2 VARCHAR(255) NULL,

 StrAttr VARCHAR(255) NULL,

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




 ALTER TABLE Лес ADD CONSTRAINT FKfaf589c5667744e3bf41fa05bc106d38 FOREIGN KEY (Страна) REFERENCES Страна; 
CREATE INDEX Indexa867fa1bf0834e1692b7aee6f566dae9 on Лес (Страна); 

 ALTER TABLE TestDetailWithCicle ADD CONSTRAINT FKa9718e123623494b849f1a28431d51b0 FOREIGN KEY (Parent) REFERENCES TestDetailWithCicle; 
CREATE INDEX Index3105ccaeda724fe4a668eb2a2bd78e36 on TestDetailWithCicle (Parent); 

 ALTER TABLE TestDetailWithCicle ADD CONSTRAINT FKa7eb94bdbaca41f690bf9f50e0a2f02c FOREIGN KEY (TestMaster) REFERENCES TestMaster; 
CREATE INDEX Index954f552afe66425494194fa1870f1807 on TestDetailWithCicle (TestMaster); 

 ALTER TABLE Медведь ADD CONSTRAINT FKef020db7e3564701a8272248cf1868bf FOREIGN KEY (Страна) REFERENCES Страна; 
CREATE INDEX Index331bb607e15846edb36e198b01f70a3a on Медведь (Страна); 

 ALTER TABLE Медведь ADD CONSTRAINT FKcca6e7d8d7114d44b9d22788a3657139 FOREIGN KEY (ЛесОбитания) REFERENCES Лес; 
CREATE INDEX Index799da30ac7c84e5ca4242b4e34da7898 on Медведь (ЛесОбитания); 

 ALTER TABLE Медведь ADD CONSTRAINT FK45f1e853ab4a49e5aae894d3f471953a FOREIGN KEY (Мама) REFERENCES Медведь; 
CREATE INDEX Index7fdfd5f41c4a40bcad6a35cc9e5c547e on Медведь (Мама); 

 ALTER TABLE Медведь ADD CONSTRAINT FK0ef1ec9fac484652b62d929dcbaaf8df FOREIGN KEY (Папа) REFERENCES Медведь; 
CREATE INDEX Index37dfa0309fd24ca8a829aa7eea0b9a17 on Медведь (Папа); 

 ALTER TABLE Детейл ADD CONSTRAINT FK768e7100b32e4a50ba0a580695689d4a FOREIGN KEY (БазовыйКласс_m0) REFERENCES БазовыйКласс; 
CREATE INDEX Indexb0098fd6ecc54aa7885544ac327e0bcc on Детейл (БазовыйКласс_m0); 

 ALTER TABLE Детейл ADD CONSTRAINT FKbb29f99fd929457e85cb2b1312c282dd FOREIGN KEY (БазовыйКласс_m1) REFERENCES Наследник; 
CREATE INDEX Index9930bcc0eda54dcb9cff9e388ff1e65b on Детейл (БазовыйКласс_m1); 

 ALTER TABLE Книга ADD CONSTRAINT FK7c7ab4e788f5497984749706b57a847e FOREIGN KEY (Автор1) REFERENCES Автор; 
CREATE INDEX Index7fef61c828664a0f93f07e9176e1250f on Книга (Автор1); 

 ALTER TABLE Книга ADD CONSTRAINT FK10f0b0536cd84d64b7c49ebf2541e1f9 FOREIGN KEY (Библиотека1) REFERENCES Библиотека; 
CREATE INDEX Index1b45728b8d244198bf4a03483d872cb7 on Книга (Библиотека1); 

 ALTER TABLE Наследник ADD CONSTRAINT FK6c32a88d2fa7455c9881bb9eec3a90bc FOREIGN KEY (Мастер) REFERENCES Мастер; 
CREATE INDEX Index4fe7489bc1c040d49107a8d7d03eddc7 on Наследник (Мастер); 

 ALTER TABLE Наследник ADD CONSTRAINT FK4327315ad1fd423e90404d53a6771f19 FOREIGN KEY (Master) REFERENCES Master; 
CREATE INDEX Indexa1ac56fb24184ae2aae721b7e602991c on Наследник (Master); 

 ALTER TABLE Car ADD CONSTRAINT FKf8be8dc5903a419d9d018477683b529c FOREIGN KEY (driver) REFERENCES Driver; 
CREATE INDEX Index039cb71dc2ac4cd3ad22592cb4faa5c3 on Car (driver); 

 ALTER TABLE Мастер ADD CONSTRAINT FKf2339bff8e0d487daf37296aa6ad4846 FOREIGN KEY (Мастер2) REFERENCES Мастер2; 
CREATE INDEX Index138d0f32b4174b61828d08345693d6e2 on Мастер (Мастер2); 

 ALTER TABLE Блоха ADD CONSTRAINT FK998bbb9e825441e59b6daa799026fde9 FOREIGN KEY (МедведьОбитания) REFERENCES Медведь; 
CREATE INDEX Index8edaafd7f46c46409aeb50fe70e1e355 on Блоха (МедведьОбитания); 

 ALTER TABLE Берлога ADD CONSTRAINT FK212b440453874e6685bbffc61a51fab0 FOREIGN KEY (ЛесРасположения) REFERENCES Лес; 
CREATE INDEX Index784a9e4098354f83a15b7321ef4436ce on Берлога (ЛесРасположения); 

 ALTER TABLE Берлога ADD CONSTRAINT FK3a57e092569a408ab068c3c9752dd57b FOREIGN KEY (Медведь) REFERENCES Медведь; 
CREATE INDEX Index87d78c8f8f49457c864dc0553c5774d1 on Берлога (Медведь); 

 ALTER TABLE Детейл2 ADD CONSTRAINT FK732d4544409447659786580da4a31f91 FOREIGN KEY (Детейл) REFERENCES Детейл; 
CREATE INDEX Index59e4da9bf31844f18df2f4f02b4702d4 on Детейл2 (Детейл); 

 ALTER TABLE Журнал ADD CONSTRAINT FKc9adffe57d4449e583ba8707ebf410d4 FOREIGN KEY (Автор2) REFERENCES Автор; 
CREATE INDEX Indexf2423de6948444aeb87d133691c1e367 on Журнал (Автор2); 

 ALTER TABLE Журнал ADD CONSTRAINT FK59e8c4535ded4588acf9ab0e38b951c6 FOREIGN KEY (Библиотека2) REFERENCES Библиотека; 
CREATE INDEX Index68b584151d9b4b278b5bdb65d830724e on Журнал (Библиотека2); 

 ALTER TABLE STORMWEBSEARCH ADD CONSTRAINT FK6be5b0cd7f1346369359911f90ba8f98 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERDETAIL ADD CONSTRAINT FK74b0afd39c6e4935886dfbc7bd4409fb FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERLOOKUP ADD CONSTRAINT FKbcf22a64d31f40d58117c237fe391202 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMAuEntity ADD CONSTRAINT FK6bdc6132d97748d0bef391edb83baa6e FOREIGN KEY (ObjectType_m0) REFERENCES STORMAuObjType; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK01a54f416a8f43c59266265f1e18172a FOREIGN KEY (MainChange_m0) REFERENCES STORMAuField; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK7f523aa364574d3fa4a4f09b8a104a9e FOREIGN KEY (AuditEntity_m0) REFERENCES STORMAuEntity; 
