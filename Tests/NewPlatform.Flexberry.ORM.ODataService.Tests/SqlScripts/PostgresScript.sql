



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


CREATE TABLE ТипПороды (

 primaryKey UUID NOT NULL,

 Название VARCHAR(255) NULL,

 ДатаРегистрации TIMESTAMP(3) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Порода (

 primaryKey UUID NOT NULL,

 Название VARCHAR(255) NULL,

 ТипПороды_m0 UUID NULL,

 Иерархия_m0 UUID NULL,

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


CREATE TABLE Перелом (

 primaryKey UUID NOT NULL,

 Дата TIMESTAMP(3) NULL,

 Тип VARCHAR(8) NULL,

 Лапа_m0 UUID NOT NULL,

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


CREATE TABLE Лапа (

 primaryKey UUID NOT NULL,

 Цвет VARCHAR(255) NULL,

 Размер INT NULL,

 ДатаРождения TIMESTAMP(3) NULL,

 БылиЛиПереломы BOOLEAN NULL,

 Сторона VARCHAR(11) NULL,

 Номер INT NULL,

 РазмерDouble DOUBLE PRECISION NULL,

 РазмерFloat REAL NULL,

 РазмерDecimal DECIMAL NULL,

 ТипЛапы_m0 UUID NULL,

 Кошка_m0 UUID NOT NULL,

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


CREATE TABLE Котенок (

 primaryKey UUID NOT NULL,

 КличкаКотенка VARCHAR(255) NULL,

 Глупость INT NULL,

 Кошка_m0 UUID NOT NULL,

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


CREATE TABLE ТипЛапы (

 primaryKey UUID NOT NULL,

 Название VARCHAR(255) NULL,

 Актуально BOOLEAN NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE КлассStoredDerived (

 primaryKey UUID NOT NULL,

 StrAttr2 VARCHAR(255) NULL,

 StrAttr VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Кошка (

 primaryKey UUID NOT NULL,

 Кличка VARCHAR(255) NULL,

 ДатаРождения TIMESTAMP(3) NULL,

 Тип VARCHAR(11) NULL,

 ПородаСтрокой VARCHAR(255) NULL,

 Агрессивная BOOLEAN NULL,

 УсыСлева INT NULL,

 УсыСправа INT NULL,

 Порода_m0 UUID NOT NULL,

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




 ALTER TABLE Лес ADD CONSTRAINT FK4da24bbb5b38442d825480a84098f7ec FOREIGN KEY (Страна) REFERENCES Страна; 
CREATE INDEX Index72569b2046d54858bcf72761570686dd on Лес (Страна); 

 ALTER TABLE Порода ADD CONSTRAINT FK77c6fe16cdb04a4f86223532810961f8 FOREIGN KEY (ТипПороды_m0) REFERENCES ТипПороды; 
CREATE INDEX Index8136adc2c49c477ea0408552980d0015 on Порода (ТипПороды_m0); 

 ALTER TABLE Порода ADD CONSTRAINT FK163f2689520e4012bde8f8fc9e6b25b2 FOREIGN KEY (Иерархия_m0) REFERENCES Порода; 
CREATE INDEX Index2dc361dcaabc421e8f62bdd5beea62dc on Порода (Иерархия_m0); 

 ALTER TABLE TestDetailWithCicle ADD CONSTRAINT FKd66b1280585c4fe99df7ae5724a4168a FOREIGN KEY (Parent) REFERENCES TestDetailWithCicle; 
CREATE INDEX Indexe28f9633f98a465e8e583502b63350d4 on TestDetailWithCicle (Parent); 

 ALTER TABLE TestDetailWithCicle ADD CONSTRAINT FKba0687c86be74a6284b51993a644fe65 FOREIGN KEY (TestMaster) REFERENCES TestMaster; 
CREATE INDEX Index30d2404cc9e746dea53110e212563b9a on TestDetailWithCicle (TestMaster); 

 ALTER TABLE Медведь ADD CONSTRAINT FK07623ec1badd46df92950633a2e1cfa4 FOREIGN KEY (Страна) REFERENCES Страна; 
CREATE INDEX Indexc7c86a0b21ed46e7903d19f9b4fd1bbb on Медведь (Страна); 

 ALTER TABLE Медведь ADD CONSTRAINT FK2bb7da89de9f4b8fa2c458139ae0b02c FOREIGN KEY (ЛесОбитания) REFERENCES Лес; 
CREATE INDEX Index349fe19fbefe4809920d58cea698a959 on Медведь (ЛесОбитания); 

 ALTER TABLE Медведь ADD CONSTRAINT FKf746ff2af5e0405f82b89838bfa08b9c FOREIGN KEY (Мама) REFERENCES Медведь; 
CREATE INDEX Index434397ba87784392b3ccf9403b278fa1 on Медведь (Мама); 

 ALTER TABLE Медведь ADD CONSTRAINT FKa08a22d20e2a47d79d504545d997243a FOREIGN KEY (Папа) REFERENCES Медведь; 
CREATE INDEX Index0c64660225ce46ffbf702caca951a852 on Медведь (Папа); 

 ALTER TABLE Детейл ADD CONSTRAINT FKc4ca2f302d5c444ca0ef54ddbf48b280 FOREIGN KEY (БазовыйКласс_m0) REFERENCES БазовыйКласс; 
CREATE INDEX Indexcf605f3317314f5d9f8ede16909eaa2f on Детейл (БазовыйКласс_m0); 

 ALTER TABLE Детейл ADD CONSTRAINT FK90282e34d7a04e30a22856ead30729b7 FOREIGN KEY (БазовыйКласс_m1) REFERENCES Наследник; 
CREATE INDEX Index591ab09451e644bea3de54d92875d679 on Детейл (БазовыйКласс_m1); 

 ALTER TABLE Книга ADD CONSTRAINT FK6ab4a749b42d4156b21d50d1550a889d FOREIGN KEY (Автор1) REFERENCES Автор; 
CREATE INDEX Index87d2f389dddb4c90879cca877e93c46a on Книга (Автор1); 

 ALTER TABLE Книга ADD CONSTRAINT FKd7fa44624b4d46bd874215f0901d8885 FOREIGN KEY (Библиотека1) REFERENCES Библиотека; 
CREATE INDEX Indexe775dae77f6d4863b9f2dbe1c3462466 on Книга (Библиотека1); 

 ALTER TABLE Перелом ADD CONSTRAINT FK98812f4509f54b07965492bf7eb261d9 FOREIGN KEY (Лапа_m0) REFERENCES Лапа; 
CREATE INDEX Indexb200082f1ec24c03a9db8ea2158589bc on Перелом (Лапа_m0); 

 ALTER TABLE Наследник ADD CONSTRAINT FKee2a88c9856e4a3ca73d3de622227d99 FOREIGN KEY (Мастер) REFERENCES Мастер; 
CREATE INDEX Index8acd8b00d11148f7b6b45babfcd52cff on Наследник (Мастер); 

 ALTER TABLE Наследник ADD CONSTRAINT FKff62560444e7427f9cc8d9b1a29fc09e FOREIGN KEY (Master) REFERENCES Master; 
CREATE INDEX Index5c4d2ece8e1448d99e3d66833b5f8ee2 on Наследник (Master); 

 ALTER TABLE Car ADD CONSTRAINT FKff9365be79084af4a04b4f8474018fb4 FOREIGN KEY (driver) REFERENCES Driver; 
CREATE INDEX Indexae894f86e82443909180062f782bd992 on Car (driver); 

 ALTER TABLE Мастер ADD CONSTRAINT FKb71735bc3e6f49ffaecee348ba8c8d7f FOREIGN KEY (Мастер2) REFERENCES Мастер2; 
CREATE INDEX Index10b437048ca64677af4e0881c6b91b10 on Мастер (Мастер2); 

 ALTER TABLE Блоха ADD CONSTRAINT FK4bee02061156472599f3c9a9c1bd9f8a FOREIGN KEY (МедведьОбитания) REFERENCES Медведь; 
CREATE INDEX Index74d7077783774d21bf48c077c268290e on Блоха (МедведьОбитания); 

 ALTER TABLE Лапа ADD CONSTRAINT FK63f23c14474f4c81a30420a740c11d04 FOREIGN KEY (ТипЛапы_m0) REFERENCES ТипЛапы; 
CREATE INDEX Indexcca4fd3396744230969127b18eac55ca on Лапа (ТипЛапы_m0); 

 ALTER TABLE Лапа ADD CONSTRAINT FKbca1ee847fb84cc79b3a120850abd0e5 FOREIGN KEY (Кошка_m0) REFERENCES Кошка; 
CREATE INDEX Index07c53f58959d4135b121d54ae4d93552 on Лапа (Кошка_m0); 

 ALTER TABLE Берлога ADD CONSTRAINT FK0be1b0c03ce44dd7bc9da8bd2ee85932 FOREIGN KEY (ЛесРасположения) REFERENCES Лес; 
CREATE INDEX Index246fde73dc354b4c8060dc273f74dfa0 on Берлога (ЛесРасположения); 

 ALTER TABLE Берлога ADD CONSTRAINT FKa40ec62909484dbe82a9d17b0f3988ca FOREIGN KEY (Медведь) REFERENCES Медведь; 
CREATE INDEX Index11ee66d9ca864dd1bae8c3c13a3a4e25 on Берлога (Медведь); 

 ALTER TABLE Котенок ADD CONSTRAINT FK239ec541c45146bf960215232bee375a FOREIGN KEY (Кошка_m0) REFERENCES Кошка; 
CREATE INDEX Indexff61f752a19743899b4352e5ad5bde23 on Котенок (Кошка_m0); 

 ALTER TABLE Детейл2 ADD CONSTRAINT FK30a932c9a4a243ad9887bc448ae881ee FOREIGN KEY (Детейл_m0) REFERENCES Детейл; 
CREATE INDEX Indexf64ed07aa8bd4d14ab7602bbec39873e on Детейл2 (Детейл_m0); 

 ALTER TABLE Детейл2 ADD CONSTRAINT FKf339385b8b7f42dfb42b7597a586df5c FOREIGN KEY (Детейл_m1) REFERENCES ДетейлНаследник; 
CREATE INDEX Indexd98a815802774a899ffc542504a52b44 on Детейл2 (Детейл_m1); 

 ALTER TABLE Журнал ADD CONSTRAINT FKac2e4bda701b40738c2b8e1ddcd118d7 FOREIGN KEY (Автор2) REFERENCES Автор; 
CREATE INDEX Indexe273cbeca581404c985de7e5835fa160 on Журнал (Автор2); 

 ALTER TABLE Журнал ADD CONSTRAINT FKfd405010f62843e7b814b38ff533d7b7 FOREIGN KEY (Библиотека2) REFERENCES Библиотека; 
CREATE INDEX Indexe267229a2c59487cb329d23afe175d8e on Журнал (Библиотека2); 

 ALTER TABLE Кошка ADD CONSTRAINT FKdc5b85b998e94db4a60bbee5aa19aec2 FOREIGN KEY (Порода_m0) REFERENCES Порода; 
CREATE INDEX Indexe7b50c7fd95d46e8b2fdd3ed1108add7 on Кошка (Порода_m0); 

 ALTER TABLE ДетейлНаследник ADD CONSTRAINT FK7dd68e3e24544a019c18f7cb6757ef4d FOREIGN KEY (БазовыйКласс_m0) REFERENCES БазовыйКласс; 
CREATE INDEX Index85a196bb63944249aae52511a75b47aa on ДетейлНаследник (БазовыйКласс_m0); 

 ALTER TABLE ДетейлНаследник ADD CONSTRAINT FK972130f9811a4f6ebfd1bcae2da440d5 FOREIGN KEY (БазовыйКласс_m1) REFERENCES Наследник; 
CREATE INDEX Index7f49f0c246684445800f2cd47253ef62 on ДетейлНаследник (БазовыйКласс_m1); 

 ALTER TABLE STORMWEBSEARCH ADD CONSTRAINT FKbf4186317c44448b9130f832473af3bd FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERDETAIL ADD CONSTRAINT FK6271258ea9464251a14e004f4e287ba0 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERLOOKUP ADD CONSTRAINT FKbb3680bb62454a4f8af432b8357c6764 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMAuEntity ADD CONSTRAINT FKd57747db77b14114beb64e1064031565 FOREIGN KEY (ObjectType_m0) REFERENCES STORMAuObjType; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK451e5346b00146a7a2da8250c964bab3 FOREIGN KEY (MainChange_m0) REFERENCES STORMAuField; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK45d4013bd04f4732bb0b5c994b490a2c FOREIGN KEY (AuditEntity_m0) REFERENCES STORMAuEntity; 

