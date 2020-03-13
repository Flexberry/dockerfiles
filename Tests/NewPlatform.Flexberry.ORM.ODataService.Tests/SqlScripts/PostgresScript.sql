



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

 CertString TEXT NULL,

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

 Детейл_m0 UUID NULL,

 Детейл_m1 UUID NULL,

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


CREATE TABLE ДетейлНаследник (

 primaryKey UUID NOT NULL,

 prop3 VARCHAR(255) NULL,

 prop1 INT NULL,

 БазовыйКласс_m0 UUID NULL,

 БазовыйКласс_m1 UUID NULL,

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




 ALTER TABLE Лес ADD CONSTRAINT FK4bf8a1bfdbdd41eb84ca90f98834f704 FOREIGN KEY (Страна) REFERENCES Страна; 
CREATE INDEX Index63b43364df1e416ca5bc151756c0c100 on Лес (Страна); 

 ALTER TABLE TestDetailWithCicle ADD CONSTRAINT FK3e70b3c90253482c8c0914f899a581b4 FOREIGN KEY (Parent) REFERENCES TestDetailWithCicle; 
CREATE INDEX Index228edc4821b644099cac58a725dbc02c on TestDetailWithCicle (Parent); 

 ALTER TABLE TestDetailWithCicle ADD CONSTRAINT FKf3a5ee14a6264022840ed58db91fa1c2 FOREIGN KEY (TestMaster) REFERENCES TestMaster; 
CREATE INDEX Index51e9edabe5fc44bd8bbd533ad527ff68 on TestDetailWithCicle (TestMaster); 

 ALTER TABLE Медведь ADD CONSTRAINT FK7d11d3ad9071421ca2c0cf39270ad999 FOREIGN KEY (Страна) REFERENCES Страна; 
CREATE INDEX Index58b48d828c7e4cc0952ba31327f6f99b on Медведь (Страна); 

 ALTER TABLE Медведь ADD CONSTRAINT FK1375e7e830f94b9c96beb3a3234e0e3c FOREIGN KEY (ЛесОбитания) REFERENCES Лес; 
CREATE INDEX Indexd9df71b7cd534de4984df530acbb79b1 on Медведь (ЛесОбитания); 

 ALTER TABLE Медведь ADD CONSTRAINT FKba63f74112e149558384380e5371e2a5 FOREIGN KEY (Мама) REFERENCES Медведь; 
CREATE INDEX Indexdafc614a77cf45c58f3aa469f5743f8e on Медведь (Мама); 

 ALTER TABLE Медведь ADD CONSTRAINT FKbd92bbe365404188bf88b91ba130ef97 FOREIGN KEY (Папа) REFERENCES Медведь; 
CREATE INDEX Index510601b89de0491193d24de21a8c6198 on Медведь (Папа); 

 ALTER TABLE Детейл ADD CONSTRAINT FK01dac57904d0435597d5f6b2feb57268 FOREIGN KEY (БазовыйКласс_m0) REFERENCES БазовыйКласс; 
CREATE INDEX Index9980cb74d02746f183a4eb30fa23f5b4 on Детейл (БазовыйКласс_m0); 

 ALTER TABLE Детейл ADD CONSTRAINT FKdc5908eb8c4349ed8c3c087830d51469 FOREIGN KEY (БазовыйКласс_m1) REFERENCES Наследник; 
CREATE INDEX Index6d95ee551d5c445c948660e3c9b59144 on Детейл (БазовыйКласс_m1); 

 ALTER TABLE Книга ADD CONSTRAINT FK824616d270fc4d5db9512f61851e9d68 FOREIGN KEY (Автор1) REFERENCES Автор; 
CREATE INDEX Indexf8a28bae2309454782dcc3b5376b88e4 on Книга (Автор1); 

 ALTER TABLE Книга ADD CONSTRAINT FKf15a192d8ae34ae1a05e3b339bd98968 FOREIGN KEY (Библиотека1) REFERENCES Библиотека; 
CREATE INDEX Indexefacb479486141df8a7b15106c9b28d3 on Книга (Библиотека1); 

 ALTER TABLE Наследник ADD CONSTRAINT FK0d1509d228c34e429d9cf55f584cfe2f FOREIGN KEY (Мастер) REFERENCES Мастер; 
CREATE INDEX Indexa18c471553a140cfaf7cdb941eb1249a on Наследник (Мастер); 

 ALTER TABLE Наследник ADD CONSTRAINT FK2ec79246a0194efdb2896410bdeacf7b FOREIGN KEY (Master) REFERENCES Master; 
CREATE INDEX Indexa316ebc795f9481093cf1d701d99c6bd on Наследник (Master); 

 ALTER TABLE Car ADD CONSTRAINT FK4aaad52880724b98a5411700db9b2174 FOREIGN KEY (driver) REFERENCES Driver; 
CREATE INDEX Index13aef4336cba45ae8b40e6c6f528fb43 on Car (driver); 

 ALTER TABLE Мастер ADD CONSTRAINT FK642fec7a0afc4abc93f5c086b0fc36ba FOREIGN KEY (Мастер2) REFERENCES Мастер2; 
CREATE INDEX Indexadc2a64ae4a24d80a9ce88d932cab387 on Мастер (Мастер2); 

 ALTER TABLE Блоха ADD CONSTRAINT FK76bf15631ab14e1680350db24cbbfb66 FOREIGN KEY (МедведьОбитания) REFERENCES Медведь; 
CREATE INDEX Indexe09c5b581ad54f21a76d704aaa6817a7 on Блоха (МедведьОбитания); 

 ALTER TABLE Берлога ADD CONSTRAINT FKc8ed058e9cfb432b9ec6151d0b74962b FOREIGN KEY (ЛесРасположения) REFERENCES Лес; 
CREATE INDEX Index68ac1a3ac5664d7084213a7c5dd575f5 on Берлога (ЛесРасположения); 

 ALTER TABLE Берлога ADD CONSTRAINT FK5fae9578467d4a53893a42ac26be5894 FOREIGN KEY (Медведь) REFERENCES Медведь; 
CREATE INDEX Index696a9436d8de428fbb3e4e510cb86dbd on Берлога (Медведь); 

 ALTER TABLE Детейл2 ADD CONSTRAINT FKe5bf3f68853c4842a00dc306f67cf677 FOREIGN KEY (Детейл_m0) REFERENCES Детейл; 
CREATE INDEX Indexa26dc2e4a94e4c89b6444f55eb05253f on Детейл2 (Детейл_m0); 

 ALTER TABLE Детейл2 ADD CONSTRAINT FK8e61973eb581431fa8ece7a8128dde61 FOREIGN KEY (Детейл_m1) REFERENCES ДетейлНаследник; 
CREATE INDEX Index6050b1577cc14aaa89b3a57170962bd7 on Детейл2 (Детейл_m1); 

 ALTER TABLE Журнал ADD CONSTRAINT FKb85e9c6f1aab4896a2aab5796a4d4220 FOREIGN KEY (Автор2) REFERENCES Автор; 
CREATE INDEX Indexa8e2ef8e9bc84c3e8691eac94ab61b1f on Журнал (Автор2); 

 ALTER TABLE Журнал ADD CONSTRAINT FK879eae30ad104392b472ed723704acfa FOREIGN KEY (Библиотека2) REFERENCES Библиотека; 
CREATE INDEX Index3c95cc52096e4547b1ef29a5e3ef4919 on Журнал (Библиотека2); 

 ALTER TABLE ДетейлНаследник ADD CONSTRAINT FK6ce8beb5efdb482cb942d9588543973f FOREIGN KEY (БазовыйКласс_m0) REFERENCES БазовыйКласс; 
CREATE INDEX Indexda5afe6351b342d2ac76657d754f6de3 on ДетейлНаследник (БазовыйКласс_m0); 

 ALTER TABLE ДетейлНаследник ADD CONSTRAINT FKd486951d923149c79a1eed9f801cbdc0 FOREIGN KEY (БазовыйКласс_m1) REFERENCES Наследник; 
CREATE INDEX Indexa1525268c27642678d95efd574aa652a on ДетейлНаследник (БазовыйКласс_m1); 

 ALTER TABLE STORMWEBSEARCH ADD CONSTRAINT FK2d8205373fd74080a651d16a284603eb FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERDETAIL ADD CONSTRAINT FKe00fa012d84b4178ad44fd06ba1d36c4 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERLOOKUP ADD CONSTRAINT FKc222f3b72aa14cce8eca97296f79e25f FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMAuEntity ADD CONSTRAINT FK103b38b652af489e92f3e6273191717f FOREIGN KEY (ObjectType_m0) REFERENCES STORMAuObjType; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK6a9f87ed684e422282eb5a9a0a0b26b6 FOREIGN KEY (MainChange_m0) REFERENCES STORMAuField; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FKa2606e49136d43ca8efe44e928e79c06 FOREIGN KEY (AuditEntity_m0) REFERENCES STORMAuEntity; 

