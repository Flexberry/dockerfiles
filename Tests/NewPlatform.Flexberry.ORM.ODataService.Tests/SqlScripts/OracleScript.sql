



CREATE TABLE "Driver"
(

	"primaryKey" RAW(16) NOT NULL,

	"Name" NVARCHAR2(255) NULL,

	"CarCount" NUMBER(10) NULL,

	"Documents" NUMBER(1) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Страна"
(

	"primaryKey" RAW(16) NOT NULL,

	"Название" NVARCHAR2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Master"
(

	"primaryKey" RAW(16) NOT NULL,

	"property" NVARCHAR2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Лес"
(

	"primaryKey" RAW(16) NOT NULL,

	"Название" NVARCHAR2(255) NULL,

	"Площадь" NUMBER(10) NULL,

	"Заповедник" NUMBER(1) NULL,

	"ДатаПослОсмотра" DATE NULL,

	"Страна" RAW(16) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "TestDetailWithCicle"
(

	"primaryKey" RAW(16) NOT NULL,

	"TestDetailName" NVARCHAR2(255) NULL,

	"Parent" RAW(16) NULL,

	"TestMaster" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "КлассСМножТипов"
(

	"primaryKey" RAW(16) NOT NULL,

	"PropertyGeography" CLOB NULL,

	"PropertyEnum" NVARCHAR2(6) NULL,

	"PropertyBool" NUMBER(1) NULL,

	"PropertyInt" NUMBER(10) NULL,

	"PropertyDateTime" DATE NULL,

	"PropertyString" NVARCHAR2(255) NULL,

	"PropertyFloat" FLOAT(53) NULL,

	"PropertyDouble" FLOAT(126) NULL,

	"PropertyDecimal" NUMBER(38) NULL,

	"PropertySystemNullableDateTime" DATE NULL,

	"PropertySystemNullableInt" NUMBER(10) NULL,

	"PropertySystemNullableGuid" RAW(16) NULL,

	"PropertySystemNullableDecimal" NUMBER(38) NULL,

	"PropStormnetNullableDateTime" DATE NULL,

	"PropertyStormnetNullableInt" NUMBER(10) NULL,

	"PropertyStormnetKeyGuid" RAW(16) NULL,

	"PropStormnetNullableDecimal" NUMBER(38) NULL,

	"PropertyStormnetPartliedDate" NVARCHAR2(255) NULL,

	"PropertyStormnetContact" CLOB NULL,

	"PropertyStormnetBlob" CLOB NULL,

	"PropertyStormnetEvent" CLOB NULL,

	"PropertyStormnetGeoData" CLOB NULL,

	"PropertyStormnetImage" CLOB NULL,

	"PropertyStormnetWebFile" CLOB NULL,

	"PropertyStormnetFile" CLOB NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Медведь"
(

	"primaryKey" RAW(16) NOT NULL,

	"ПолеБС" NVARCHAR2(255) NULL,

	"ПорядковыйНомер" NUMBER(10) NULL,

	"Вес" NUMBER(10) NULL,

	"ЦветГлаз" NVARCHAR2(255) NULL,

	"Пол" NVARCHAR2(9) NULL,

	"ДатаРождения" DATE NULL,

	"CreateTime" DATE NULL,

	"Creator" NVARCHAR2(255) NULL,

	"EditTime" DATE NULL,

	"Editor" NVARCHAR2(255) NULL,

	"Страна" RAW(16) NULL,

	"ЛесОбитания" RAW(16) NULL,

	"Мама" RAW(16) NULL,

	"Папа" RAW(16) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Детейл"
(

	"primaryKey" RAW(16) NOT NULL,

	"prop1" NUMBER(10) NULL,

	"БазовыйКласс_m0" RAW(16) NULL,

	"БазовыйКласс_m1" RAW(16) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "ДочернийКласс"
(

	"primaryKey" RAW(16) NOT NULL,

	"ChildProperty" NVARCHAR2(255) NULL,

	"PropertyGeography" CLOB NULL,

	"PropertyEnum" NVARCHAR2(6) NULL,

	"PropertyBool" NUMBER(1) NULL,

	"PropertyInt" NUMBER(10) NULL,

	"PropertyDateTime" DATE NULL,

	"PropertyString" NVARCHAR2(255) NULL,

	"PropertyFloat" FLOAT(53) NULL,

	"PropertyDouble" FLOAT(126) NULL,

	"PropertyDecimal" NUMBER(38) NULL,

	"PropertySystemNullableDateTime" DATE NULL,

	"PropertySystemNullableInt" NUMBER(10) NULL,

	"PropertySystemNullableGuid" RAW(16) NULL,

	"PropertySystemNullableDecimal" NUMBER(38) NULL,

	"PropStormnetNullableDateTime" DATE NULL,

	"PropertyStormnetNullableInt" NUMBER(10) NULL,

	"PropertyStormnetKeyGuid" RAW(16) NULL,

	"PropStormnetNullableDecimal" NUMBER(38) NULL,

	"PropertyStormnetPartliedDate" NVARCHAR2(255) NULL,

	"PropertyStormnetContact" CLOB NULL,

	"PropertyStormnetBlob" CLOB NULL,

	"PropertyStormnetEvent" CLOB NULL,

	"PropertyStormnetGeoData" CLOB NULL,

	"PropertyStormnetImage" CLOB NULL,

	"PropertyStormnetWebFile" CLOB NULL,

	"PropertyStormnetFile" CLOB NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Книга"
(

	"primaryKey" RAW(16) NOT NULL,

	"Название" NVARCHAR2(255) NULL,

	"Автор1" RAW(16) NOT NULL,

	"Библиотека1" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "БазовыйКласс"
(

	"primaryKey" RAW(16) NOT NULL,

	"Свойство1" NVARCHAR2(255) NULL,

	"Свойство2" NUMBER(10) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "ПоставщикКниг"
(

	"primaryKey" RAW(16) NOT NULL,

	"Ссылка" RAW(16) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Мастер2"
(

	"primaryKey" RAW(16) NOT NULL,

	"свойство2" NUMBER(10) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Наследник"
(

	"primaryKey" RAW(16) NOT NULL,

	"Свойство" FLOAT(126) NULL,

	"Свойство1" NVARCHAR2(255) NULL,

	"Свойство2" NUMBER(10) NULL,

	"Мастер" RAW(16) NULL,

	"Master" RAW(16) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Car"
(

	"primaryKey" RAW(16) NOT NULL,

	"Number" NVARCHAR2(255) NULL,

	"Model" NVARCHAR2(255) NULL,

	"TipCar" NVARCHAR2(9) NULL,

	"driver" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Мастер"
(

	"primaryKey" RAW(16) NOT NULL,

	"prop" NVARCHAR2(255) NULL,

	"Мастер2" RAW(16) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Блоха"
(

	"primaryKey" RAW(16) NOT NULL,

	"Кличка" NVARCHAR2(255) NULL,

	"МедведьОбитания" RAW(16) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "КлассСоСтрокКл"
(

	"StoragePrimaryKey" NVARCHAR2(255) NOT NULL,

	 PRIMARY KEY ("StoragePrimaryKey")
) ;


CREATE TABLE "Берлога"
(

	"primaryKey" RAW(16) NOT NULL,

	"ПолеБС" NVARCHAR2(255) NULL,

	"Наименование" NVARCHAR2(255) NULL,

	"Комфортность" NUMBER(10) NULL,

	"Заброшена" NUMBER(1) NULL,

	"Сертификат" CLOB NULL,

	"CertString" CLOB NULL,

	"ЛесРасположения" RAW(16) NULL,

	"Медведь" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Автор"
(

	"primaryKey" RAW(16) NOT NULL,

	"Имя" NVARCHAR2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "TestMaster"
(

	"primaryKey" RAW(16) NOT NULL,

	"TestMasterName" NVARCHAR2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Библиотека"
(

	"primaryKey" RAW(16) NOT NULL,

	"Адрес" NVARCHAR2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Детейл2"
(

	"primaryKey" RAW(16) NOT NULL,

	"prop2" NVARCHAR2(255) NULL,

	"Детейл" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "Журнал"
(

	"primaryKey" RAW(16) NOT NULL,

	"Название" NVARCHAR2(255) NULL,

	"Номер" NUMBER(10) NULL,

	"Автор2" RAW(16) NOT NULL,

	"Библиотека2" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "КлассStoredDerived"
(

	"primaryKey" RAW(16) NOT NULL,

	"StrAttr2" NVARCHAR2(255) NULL,

	"StrAttr" NVARCHAR2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMNETLOCKDATA"
(

	"LockKey" NVARCHAR2(300) NOT NULL,

	"UserName" NVARCHAR2(300) NOT NULL,

	"LockDate" DATE NULL,

	 PRIMARY KEY ("LockKey")
) ;


CREATE TABLE "STORMSETTINGS"
(

	"primaryKey" RAW(16) NOT NULL,

	"Module" nvarchar2(1000) NULL,

	"Name" nvarchar2(255) NULL,

	"Value" CLOB NULL,

	"User" nvarchar2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMAdvLimit"
(

	"primaryKey" RAW(16) NOT NULL,

	"User" nvarchar2(255) NULL,

	"Published" NUMBER(1) NULL,

	"Module" nvarchar2(255) NULL,

	"Name" nvarchar2(255) NULL,

	"Value" CLOB NULL,

	"HotKeyData" NUMBER(10) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMFILTERSETTING"
(

	"primaryKey" RAW(16) NOT NULL,

	"Name" nvarchar2(255) NOT NULL,

	"DataObjectView" nvarchar2(255) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMWEBSEARCH"
(

	"primaryKey" RAW(16) NOT NULL,

	"Name" nvarchar2(255) NOT NULL,

	"Order" NUMBER(10) NOT NULL,

	"PresentView" nvarchar2(255) NOT NULL,

	"DetailedView" nvarchar2(255) NOT NULL,

	"FilterSetting_m0" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMFILTERDETAIL"
(

	"primaryKey" RAW(16) NOT NULL,

	"Caption" nvarchar2(255) NOT NULL,

	"DataObjectView" nvarchar2(255) NOT NULL,

	"ConnectMasterProp" nvarchar2(255) NOT NULL,

	"OwnerConnectProp" nvarchar2(255) NULL,

	"FilterSetting_m0" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMFILTERLOOKUP"
(

	"primaryKey" RAW(16) NOT NULL,

	"DataObjectType" nvarchar2(255) NOT NULL,

	"Container" nvarchar2(255) NULL,

	"ContainerTag" nvarchar2(255) NULL,

	"FieldsToView" nvarchar2(255) NULL,

	"FilterSetting_m0" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "UserSetting"
(

	"primaryKey" RAW(16) NOT NULL,

	"AppName" nvarchar2(256) NULL,

	"UserName" nvarchar2(512) NULL,

	"UserGuid" RAW(16) NULL,

	"ModuleName" nvarchar2(1024) NULL,

	"ModuleGuid" RAW(16) NULL,

	"SettName" nvarchar2(256) NULL,

	"SettGuid" RAW(16) NULL,

	"SettLastAccessTime" DATE NULL,

	"StrVal" nvarchar2(256) NULL,

	"TxtVal" CLOB NULL,

	"IntVal" NUMBER(10) NULL,

	"BoolVal" NUMBER(1) NULL,

	"GuidVal" RAW(16) NULL,

	"DecimalVal" NUMBER(20,10) NULL,

	"DateTimeVal" DATE NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "ApplicationLog"
(

	"primaryKey" RAW(16) NOT NULL,

	"Category" nvarchar2(64) NULL,

	"EventId" NUMBER(10) NULL,

	"Priority" NUMBER(10) NULL,

	"Severity" nvarchar2(32) NULL,

	"Title" nvarchar2(256) NULL,

	"Timestamp" DATE NULL,

	"MachineName" nvarchar2(32) NULL,

	"AppDomainName" nvarchar2(512) NULL,

	"ProcessId" nvarchar2(256) NULL,

	"ProcessName" nvarchar2(512) NULL,

	"ThreadName" nvarchar2(512) NULL,

	"Win32ThreadId" nvarchar2(128) NULL,

	"Message" nvarchar2(2000) NULL,

	"FormattedMessage" nvarchar2(2000) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMAG"
(

	"primaryKey" RAW(16) NOT NULL,

	"Name" nvarchar2(80) NOT NULL,

	"Login" nvarchar2(50) NULL,

	"Pwd" nvarchar2(50) NULL,

	"IsUser" NUMBER(1) NOT NULL,

	"IsGroup" NUMBER(1) NOT NULL,

	"IsRole" NUMBER(1) NOT NULL,

	"ConnString" nvarchar2(255) NULL,

	"Enabled" NUMBER(1) NULL,

	"Email" nvarchar2(80) NULL,

	"Comment" CLOB NULL,

	"CreateTime" DATE NULL,

	"Creator" nvarchar2(255) NULL,

	"EditTime" DATE NULL,

	"Editor" nvarchar2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMLG"
(

	"primaryKey" RAW(16) NOT NULL,

	"Group_m0" RAW(16) NOT NULL,

	"User_m0" RAW(16) NOT NULL,

	"CreateTime" DATE NULL,

	"Creator" nvarchar2(255) NULL,

	"EditTime" DATE NULL,

	"Editor" nvarchar2(255) NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMAuObjType"
(

	"primaryKey" RAW(16) NOT NULL,

	"Name" nvarchar2(255) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMAuEntity"
(

	"primaryKey" RAW(16) NOT NULL,

	"ObjectPrimaryKey" nvarchar2(38) NOT NULL,

	"OperationTime" DATE NOT NULL,

	"OperationType" nvarchar2(100) NOT NULL,

	"ExecutionResult" nvarchar2(12) NOT NULL,

	"Source" nvarchar2(255) NOT NULL,

	"SerializedField" nvarchar2(2000) NULL,

	"User_m0" RAW(16) NOT NULL,

	"ObjectType_m0" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;


CREATE TABLE "STORMAuField"
(

	"primaryKey" RAW(16) NOT NULL,

	"Field" nvarchar2(100) NOT NULL,

	"OldValue" nvarchar2(2000) NULL,

	"NewValue" nvarchar2(2000) NULL,

	"MainChange_m0" RAW(16) NULL,

	"AuditEntity_m0" RAW(16) NOT NULL,

	 PRIMARY KEY ("primaryKey")
) ;



ALTER TABLE "Лес"
	ADD CONSTRAINT "Лес_FСтрана_0" FOREIGN KEY ("Страна") REFERENCES "Страна" ("primaryKey");

CREATE INDEX "Лес_IСтрана" on "Лес" ("Страна");

ALTER TABLE "TestDetailWithCicle"
	ADD CONSTRAINT "TestDetailWithCicle_FTest_5294" FOREIGN KEY ("Parent") REFERENCES "TestDetailWithCicle" ("primaryKey");

CREATE INDEX "TestDetailWithCicle_IParent" on "TestDetailWithCicle" ("Parent");

ALTER TABLE "TestDetailWithCicle"
	ADD CONSTRAINT "TestDetailWithCicle_FTest_5002" FOREIGN KEY ("TestMaster") REFERENCES "TestMaster" ("primaryKey");

CREATE INDEX "TestDetailWithCicle_ITest_3425" on "TestDetailWithCicle" ("TestMaster");

ALTER TABLE "Медведь"
	ADD CONSTRAINT "Медведь_FСтрана_0" FOREIGN KEY ("Страна") REFERENCES "Страна" ("primaryKey");

CREATE INDEX "Медведь_IСтрана" on "Медведь" ("Страна");

ALTER TABLE "Медведь"
	ADD CONSTRAINT "Медведь_FЛес_0" FOREIGN KEY ("ЛесОбитания") REFERENCES "Лес" ("primaryKey");

CREATE INDEX "Медведь_IЛесО_5757" on "Медведь" ("ЛесОбитания");

ALTER TABLE "Медведь"
	ADD CONSTRAINT "Медведь_FМедв_4334" FOREIGN KEY ("Мама") REFERENCES "Медведь" ("primaryKey");

CREATE INDEX "Медведь_IМама" on "Медведь" ("Мама");

ALTER TABLE "Медведь"
	ADD CONSTRAINT "Медведь_FМедв_4335" FOREIGN KEY ("Папа") REFERENCES "Медведь" ("primaryKey");

CREATE INDEX "Медведь_IПапа" on "Медведь" ("Папа");

ALTER TABLE "Детейл"
	ADD CONSTRAINT "Детейл_FБазов_7676" FOREIGN KEY ("БазовыйКласс_m0") REFERENCES "БазовыйКласс" ("primaryKey");

CREATE INDEX "Детейл_IБазов_4616" on "Детейл" ("БазовыйКласс_m0");

ALTER TABLE "Детейл"
	ADD CONSTRAINT "Детейл_FНасле_6543" FOREIGN KEY ("БазовыйКласс_m1") REFERENCES "Наследник" ("primaryKey");

CREATE INDEX "Детейл_IБазов_4617" on "Детейл" ("БазовыйКласс_m1");

ALTER TABLE "Книга"
	ADD CONSTRAINT "Книга_FАвтор_0" FOREIGN KEY ("Автор1") REFERENCES "Автор" ("primaryKey");

CREATE INDEX "Книга_IАвтор1" on "Книга" ("Автор1");

ALTER TABLE "Книга"
	ADD CONSTRAINT "Книга_FБиблио_6449" FOREIGN KEY ("Библиотека1") REFERENCES "Библиотека" ("primaryKey");

CREATE INDEX "Книга_IБиблио_4875" on "Книга" ("Библиотека1");

ALTER TABLE "Наследник"
	ADD CONSTRAINT "Наследник_FМас_278" FOREIGN KEY ("Мастер") REFERENCES "Мастер" ("primaryKey");

CREATE INDEX "Наследник_IМа_7239" on "Наследник" ("Мастер");

ALTER TABLE "Наследник"
	ADD CONSTRAINT "Наследник_FMaster_0" FOREIGN KEY ("Master") REFERENCES "Master" ("primaryKey");

CREATE INDEX "Наследник_IMaster" on "Наследник" ("Master");

ALTER TABLE "Car"
	ADD CONSTRAINT "Car_FDriver_0" FOREIGN KEY ("driver") REFERENCES "Driver" ("primaryKey");

CREATE INDEX "Car_Idriver" on "Car" ("driver");

ALTER TABLE "Мастер"
	ADD CONSTRAINT "Мастер_FМастер2_0" FOREIGN KEY ("Мастер2") REFERENCES "Мастер2" ("primaryKey");

CREATE INDEX "Мастер_IМастер2" on "Мастер" ("Мастер2");

ALTER TABLE "Блоха"
	ADD CONSTRAINT "Блоха_FМедведь_0" FOREIGN KEY ("МедведьОбитания") REFERENCES "Медведь" ("primaryKey");

CREATE INDEX "Блоха_IМедвед_6073" on "Блоха" ("МедведьОбитания");

ALTER TABLE "Берлога"
	ADD CONSTRAINT "Берлога_FЛес_0" FOREIGN KEY ("ЛесРасположения") REFERENCES "Лес" ("primaryKey");

CREATE INDEX "Берлога_IЛесР_1411" on "Берлога" ("ЛесРасположения");

ALTER TABLE "Берлога"
	ADD CONSTRAINT "Берлога_FМедв_5600" FOREIGN KEY ("Медведь") REFERENCES "Медведь" ("primaryKey");

CREATE INDEX "Берлога_IМедведь" on "Берлога" ("Медведь");

ALTER TABLE "Детейл2"
	ADD CONSTRAINT "Детейл2_FДетейл_0" FOREIGN KEY ("Детейл") REFERENCES "Детейл" ("primaryKey");

CREATE INDEX "Детейл2_IДетейл" on "Детейл2" ("Детейл");

ALTER TABLE "Журнал"
	ADD CONSTRAINT "Журнал_FАвтор_0" FOREIGN KEY ("Автор2") REFERENCES "Автор" ("primaryKey");

CREATE INDEX "Журнал_IАвтор2" on "Журнал" ("Автор2");

ALTER TABLE "Журнал"
	ADD CONSTRAINT "Журнал_FБибли_9226" FOREIGN KEY ("Библиотека2") REFERENCES "Библиотека" ("primaryKey");

CREATE INDEX "Журнал_IБибли_1176" on "Журнал" ("Библиотека2");

ALTER TABLE "STORMWEBSEARCH"
	ADD CONSTRAINT "STORMWEBSEARCH_FSTORMFILT_6521" FOREIGN KEY ("FilterSetting_m0") REFERENCES "STORMFILTERSETTING" ("primaryKey");

ALTER TABLE "STORMFILTERDETAIL"
	ADD CONSTRAINT "STORMFILTERDETAIL_FSTORMF_2900" FOREIGN KEY ("FilterSetting_m0") REFERENCES "STORMFILTERSETTING" ("primaryKey");

ALTER TABLE "STORMFILTERLOOKUP"
	ADD CONSTRAINT "STORMFILTERLOOKUP_FSTORMF_1583" FOREIGN KEY ("FilterSetting_m0") REFERENCES "STORMFILTERSETTING" ("primaryKey");

ALTER TABLE "STORMLG"
	ADD CONSTRAINT "STORMLG_FSTORMAG_0" FOREIGN KEY ("Group_m0") REFERENCES "STORMAG" ("primaryKey");

ALTER TABLE "STORMLG"
	ADD CONSTRAINT "STORMLG_FSTORMAG_1" FOREIGN KEY ("User_m0") REFERENCES "STORMAG" ("primaryKey");

ALTER TABLE "STORMAuEntity"
	ADD CONSTRAINT "STORMAuEntity_FSTORMAG_0" FOREIGN KEY ("User_m0") REFERENCES "STORMAG" ("primaryKey");

ALTER TABLE "STORMAuEntity"
	ADD CONSTRAINT "STORMAuEntity_FSTORMAuObj_3287" FOREIGN KEY ("ObjectType_m0") REFERENCES "STORMAuObjType" ("primaryKey");

ALTER TABLE "STORMAuField"
	ADD CONSTRAINT "STORMAuField_FSTORMAuField_0" FOREIGN KEY ("MainChange_m0") REFERENCES "STORMAuField" ("primaryKey");

ALTER TABLE "STORMAuField"
	ADD CONSTRAINT "STORMAuField_FSTORMAuEntity_0" FOREIGN KEY ("AuditEntity_m0") REFERENCES "STORMAuEntity" ("primaryKey");


