



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

 PropertyGeography GEOMETRY NULL,

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

 PropertyGeography GEOMETRY NULL,

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

 Сертификат TEXT NULL,

 CertString VARCHAR(255) NULL,

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




 ALTER TABLE Лес ADD CONSTRAINT FK094f14931ec7428783d5475d5b19706a FOREIGN KEY (Страна) REFERENCES Страна; 
CREATE INDEX Index963b8263f4c24768b26844bdf0444550 on Лес (Страна); 

 ALTER TABLE TestDetailWithCicle ADD CONSTRAINT FKde826e643fc549c0b9e1ada3b9e244e8 FOREIGN KEY (Parent) REFERENCES TestDetailWithCicle; 
CREATE INDEX Index6a339d61880d428689a569e3ccc6647b on TestDetailWithCicle (Parent); 

 ALTER TABLE TestDetailWithCicle ADD CONSTRAINT FKd011ab4a131d47f7994be6fab422aabb FOREIGN KEY (TestMaster) REFERENCES TestMaster; 
CREATE INDEX Indexd3225499a90749a2abcde92a5c6ee541 on TestDetailWithCicle (TestMaster); 

 ALTER TABLE Медведь ADD CONSTRAINT FKbe5292b5039144e8afeff77556c4ea48 FOREIGN KEY (Страна) REFERENCES Страна; 
CREATE INDEX Index7305e10e34444ef3a009261241139c44 on Медведь (Страна); 

 ALTER TABLE Медведь ADD CONSTRAINT FKee1efbd3144744119a5a60bb1dc5a059 FOREIGN KEY (ЛесОбитания) REFERENCES Лес; 
CREATE INDEX Index63b2d2125df445ffa1d3ca62e7f5734f on Медведь (ЛесОбитания); 

 ALTER TABLE Медведь ADD CONSTRAINT FK2c1ceb4421294e239e66182b305e681b FOREIGN KEY (Мама) REFERENCES Медведь; 
CREATE INDEX Indexdcbb2008ea5645929f534a51e97668a5 on Медведь (Мама); 

 ALTER TABLE Медведь ADD CONSTRAINT FK293cfde1ed0d4206ba21813343f4f275 FOREIGN KEY (Папа) REFERENCES Медведь; 
CREATE INDEX Index6b616f1c901741c3ba918dd3dc57d2da on Медведь (Папа); 

 ALTER TABLE Детейл ADD CONSTRAINT FKf5ef9351c63947b1a45728e6f5d55d4a FOREIGN KEY (БазовыйКласс_m0) REFERENCES БазовыйКласс; 
CREATE INDEX Index1af1375941f64d83ac96e8283acbd15b on Детейл (БазовыйКласс_m0); 

 ALTER TABLE Детейл ADD CONSTRAINT FK1fe324971cd344f18a41ae8fd9cd8ecc FOREIGN KEY (БазовыйКласс_m1) REFERENCES Наследник; 
CREATE INDEX Index3be594275cbe49a4b7df07457e006cdf on Детейл (БазовыйКласс_m1); 

 ALTER TABLE Книга ADD CONSTRAINT FK38426884f99045938a4357df0d058cdd FOREIGN KEY (Автор1) REFERENCES Автор; 
CREATE INDEX Index9703c34d25554de0b089f585f1d1fa11 on Книга (Автор1); 

 ALTER TABLE Книга ADD CONSTRAINT FK3c751483a96b479892c5853d7dcc1de7 FOREIGN KEY (Библиотека1) REFERENCES Библиотека; 
CREATE INDEX Indexf980823a740d44fb9bb05a1f3e5654f6 on Книга (Библиотека1); 

 ALTER TABLE Наследник ADD CONSTRAINT FK472b1f35ae0d44669f819735962ac2fd FOREIGN KEY (Мастер) REFERENCES Мастер; 
CREATE INDEX Indexedb1da9d9968401f80696f441966ccc0 on Наследник (Мастер); 

 ALTER TABLE Наследник ADD CONSTRAINT FK47a819e26ef64b8eb7aad34a7e99c83e FOREIGN KEY (Master) REFERENCES Master; 
CREATE INDEX Index71c1d3372d464829b20fb4440ecabde1 on Наследник (Master); 

 ALTER TABLE Car ADD CONSTRAINT FKbf9c26b65082485080f9c96ccbef3794 FOREIGN KEY (driver) REFERENCES Driver; 
CREATE INDEX Indexd2c5a72584564755a8afb789ec32c744 on Car (driver); 

 ALTER TABLE Мастер ADD CONSTRAINT FKa1eb7e89e22e4db7a9a31781ad50a276 FOREIGN KEY (Мастер2) REFERENCES Мастер2; 
CREATE INDEX Index59e297eb836b4190935596de39f6a960 on Мастер (Мастер2); 

 ALTER TABLE Блоха ADD CONSTRAINT FK40cf43b39b444a6bb5001d072c3a1e62 FOREIGN KEY (МедведьОбитания) REFERENCES Медведь; 
CREATE INDEX Index82410edac1594c4ea12e81d020a9dfd6 on Блоха (МедведьОбитания); 

 ALTER TABLE Берлога ADD CONSTRAINT FKbe0b722a54e34341af64fd5bc08e1c41 FOREIGN KEY (ЛесРасположения) REFERENCES Лес; 
CREATE INDEX Index129d1c63f3a147e988c398e463f0a188 on Берлога (ЛесРасположения); 

 ALTER TABLE Берлога ADD CONSTRAINT FK10031cfb1a9e40df9ccdde48fd4f8e11 FOREIGN KEY (Медведь) REFERENCES Медведь; 
CREATE INDEX Index50b4c3bedd7f4cbeb0a4e6ddd64bbd58 on Берлога (Медведь); 

 ALTER TABLE Детейл2 ADD CONSTRAINT FK079e0acf499a455da69bfb55eeaa99e2 FOREIGN KEY (Детейл) REFERENCES Детейл; 
CREATE INDEX Indexf7d230117c8c4961811ac0317aa49e8d on Детейл2 (Детейл); 

 ALTER TABLE Журнал ADD CONSTRAINT FK827498636ea241feb2fa4831d7cb42a9 FOREIGN KEY (Автор2) REFERENCES Автор; 
CREATE INDEX Index9b09ac76193d43aabe23e83564af3b5d on Журнал (Автор2); 

 ALTER TABLE Журнал ADD CONSTRAINT FK4c1d362e3fbd4363a7ad75482ba0b0b8 FOREIGN KEY (Библиотека2) REFERENCES Библиотека; 
CREATE INDEX Index19aa4f0281974bd3be6db919cede1771 on Журнал (Библиотека2); 

 ALTER TABLE STORMWEBSEARCH ADD CONSTRAINT FK1e1da8abc8da4bb29d7c841cfcfba676 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERDETAIL ADD CONSTRAINT FK14e2d8cfb3284fafa0887904c95ac153 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERLOOKUP ADD CONSTRAINT FKc7d1f0f598e345ceb100f7dda48765a5 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMAuEntity ADD CONSTRAINT FKa44be23f60824d138fd52117435a5220 FOREIGN KEY (ObjectType_m0) REFERENCES STORMAuObjType; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FKcedf1bc5c5e046668b24a543ac64c39c FOREIGN KEY (MainChange_m0) REFERENCES STORMAuField; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK96361c5fcb8b41569bda60960c843807 FOREIGN KEY (AuditEntity_m0) REFERENCES STORMAuEntity; 

