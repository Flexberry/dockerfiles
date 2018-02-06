CREATE TABLE [Driver] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Name] VARCHAR(255)  NULL,

	 [CarCount] INT  NULL,

	 [Documents] BIT  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Страна] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Название] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Master] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [property] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Лес] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Название] VARCHAR(255)  NULL,

	 [Площадь] INT  NULL,

	 [Заповедник] BIT  NULL,

	 [ДатаПослОсмотра] DATETIME  NULL,

	 [Страна] UNIQUEIDENTIFIER  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [TestDetailWithCicle] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [TestDetailName] VARCHAR(255)  NULL,

	 [Parent] UNIQUEIDENTIFIER  NULL,

	 [TestMaster] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [КлассСМножТипов] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [PropertyGeography] geography  NULL,

	 [PropertyEnum] VARCHAR(6)  NULL,

	 [PropertyBool] BIT  NULL,

	 [PropertyInt] INT  NULL,

	 [PropertyDateTime] DATETIME  NULL,

	 [PropertyString] VARCHAR(255)  NULL,

	 [PropertyFloat] REAL  NULL,

	 [PropertyDouble] FLOAT  NULL,

	 [PropertyDecimal] DECIMAL  NULL,

	 [PropertySystemNullableDateTime] DATETIME  NULL,

	 [PropertySystemNullableInt] INT  NULL,

	 [PropertySystemNullableGuid] UNIQUEIDENTIFIER  NULL,

	 [PropertySystemNullableDecimal] DECIMAL  NULL,

	 [PropStormnetNullableDateTime] DATETIME  NULL,

	 [PropertyStormnetNullableInt] INT  NULL,

	 [PropertyStormnetKeyGuid] UNIQUEIDENTIFIER  NULL,

	 [PropStormnetNullableDecimal] DECIMAL  NULL,

	 [PropertyStormnetPartliedDate] VARCHAR(255)  NULL,

	 [PropertyStormnetContact] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetBlob] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetEvent] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetGeoData] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetImage] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetWebFile] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetFile] NVARCHAR(MAX)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Медведь] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [ПолеБС] VARCHAR(255)  NULL,

	 [ПорядковыйНомер] INT  NULL,

	 [Вес] INT  NULL,

	 [ЦветГлаз] VARCHAR(255)  NULL,

	 [Пол] VARCHAR(9)  NULL,

	 [ДатаРождения] DATETIME  NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 [Страна] UNIQUEIDENTIFIER  NULL,

	 [ЛесОбитания] UNIQUEIDENTIFIER  NULL,

	 [Мама] UNIQUEIDENTIFIER  NULL,

	 [Папа] UNIQUEIDENTIFIER  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Детейл] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [prop1] INT  NULL,

	 [БазовыйКласс_m0] UNIQUEIDENTIFIER  NULL,

	 [БазовыйКласс_m1] UNIQUEIDENTIFIER  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [ДочернийКласс] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [ChildProperty] VARCHAR(255)  NULL,

	 [PropertyGeography] geography  NULL,

	 [PropertyEnum] VARCHAR(6)  NULL,

	 [PropertyBool] BIT  NULL,

	 [PropertyInt] INT  NULL,

	 [PropertyDateTime] DATETIME  NULL,

	 [PropertyString] VARCHAR(255)  NULL,

	 [PropertyFloat] REAL  NULL,

	 [PropertyDouble] FLOAT  NULL,

	 [PropertyDecimal] DECIMAL  NULL,

	 [PropertySystemNullableDateTime] DATETIME  NULL,

	 [PropertySystemNullableInt] INT  NULL,

	 [PropertySystemNullableGuid] UNIQUEIDENTIFIER  NULL,

	 [PropertySystemNullableDecimal] DECIMAL  NULL,

	 [PropStormnetNullableDateTime] DATETIME  NULL,

	 [PropertyStormnetNullableInt] INT  NULL,

	 [PropertyStormnetKeyGuid] UNIQUEIDENTIFIER  NULL,

	 [PropStormnetNullableDecimal] DECIMAL  NULL,

	 [PropertyStormnetPartliedDate] VARCHAR(255)  NULL,

	 [PropertyStormnetContact] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetBlob] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetEvent] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetGeoData] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetImage] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetWebFile] NVARCHAR(MAX)  NULL,

	 [PropertyStormnetFile] NVARCHAR(MAX)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Книга] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Название] VARCHAR(255)  NULL,

	 [Автор1] UNIQUEIDENTIFIER  NOT NULL,

	 [Библиотека1] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [БазовыйКласс] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Свойство1] VARCHAR(255)  NULL,

	 [Свойство2] INT  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [ПоставщикКниг] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Ссылка] UNIQUEIDENTIFIER  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Мастер2] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [свойство2] INT  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Наследник] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Свойство] FLOAT  NULL,

	 [Свойство1] VARCHAR(255)  NULL,

	 [Свойство2] INT  NULL,

	 [Мастер] UNIQUEIDENTIFIER  NULL,

	 [Master] UNIQUEIDENTIFIER  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Car] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Number] VARCHAR(255)  NULL,

	 [Model] VARCHAR(255)  NULL,

	 [TipCar] VARCHAR(9)  NULL,

	 [driver] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Мастер] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [prop] VARCHAR(255)  NULL,

	 [Мастер2] UNIQUEIDENTIFIER  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Блоха] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Кличка] VARCHAR(255)  NULL,

	 [МедведьОбитания] UNIQUEIDENTIFIER  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [КлассСоСтрокКл] (

	 [StoragePrimaryKey] VARCHAR(255)  NOT NULL,

	 PRIMARY KEY ([StoragePrimaryKey]))


CREATE TABLE [Берлога] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [ПолеБС] VARCHAR(255)  NULL,

	 [Наименование] VARCHAR(255)  NULL,

	 [Комфортность] INT  NULL,

	 [Заброшена] BIT  NULL,

	 [ЛесРасположения] UNIQUEIDENTIFIER  NULL,

	 [Медведь] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Автор] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Имя] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [TestMaster] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [TestMasterName] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Библиотека] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Адрес] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Детейл2] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [prop2] VARCHAR(255)  NULL,

	 [Детейл] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Журнал] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Название] VARCHAR(255)  NULL,

	 [Номер] INT  NULL,

	 [Автор2] UNIQUEIDENTIFIER  NOT NULL,

	 [Библиотека2] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [КлассStoredDerived] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [StrAttr2] VARCHAR(255)  NULL,

	 [StrAttr] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMNETLOCKDATA] (

	 [LockKey] VARCHAR(300)  NOT NULL,

	 [UserName] VARCHAR(300)  NOT NULL,

	 [LockDate] DATETIME  NULL,

	 PRIMARY KEY ([LockKey]))


CREATE TABLE [STORMSETTINGS] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Module] varchar(1000)  NULL,

	 [Name] varchar(255)  NULL,

	 [Value] text  NULL,

	 [User] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAdvLimit] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [User] varchar(255)  NULL,

	 [Published] bit  NULL,

	 [Module] varchar(255)  NULL,

	 [Name] varchar(255)  NULL,

	 [Value] text  NULL,

	 [HotKeyData] int  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMFILTERSETTING] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] varchar(255)  NOT NULL,

	 [DataObjectView] varchar(255)  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMWEBSEARCH] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] varchar(255)  NOT NULL,

	 [Order] INT  NOT NULL,

	 [PresentView] varchar(255)  NOT NULL,

	 [DetailedView] varchar(255)  NOT NULL,

	 [FilterSetting_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMFILTERDETAIL] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Caption] varchar(255)  NOT NULL,

	 [DataObjectView] varchar(255)  NOT NULL,

	 [ConnectMasterProp] varchar(255)  NOT NULL,

	 [OwnerConnectProp] varchar(255)  NULL,

	 [FilterSetting_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMFILTERLOOKUP] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [DataObjectType] varchar(255)  NOT NULL,

	 [Container] varchar(255)  NULL,

	 [ContainerTag] varchar(255)  NULL,

	 [FieldsToView] varchar(255)  NULL,

	 [FilterSetting_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [UserSetting] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [AppName] varchar(256)  NULL,

	 [UserName] varchar(512)  NULL,

	 [UserGuid] uniqueidentifier  NULL,

	 [ModuleName] varchar(1024)  NULL,

	 [ModuleGuid] uniqueidentifier  NULL,

	 [SettName] varchar(256)  NULL,

	 [SettGuid] uniqueidentifier  NULL,

	 [SettLastAccessTime] DATETIME  NULL,

	 [StrVal] varchar(256)  NULL,

	 [TxtVal] varchar(max)  NULL,

	 [IntVal] int  NULL,

	 [BoolVal] bit  NULL,

	 [GuidVal] uniqueidentifier  NULL,

	 [DecimalVal] decimal(20,10)  NULL,

	 [DateTimeVal] DATETIME  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [ApplicationLog] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Category] varchar(64)  NULL,

	 [EventId] INT  NULL,

	 [Priority] INT  NULL,

	 [Severity] varchar(32)  NULL,

	 [Title] varchar(256)  NULL,

	 [Timestamp] DATETIME  NULL,

	 [MachineName] varchar(32)  NULL,

	 [AppDomainName] varchar(512)  NULL,

	 [ProcessId] varchar(256)  NULL,

	 [ProcessName] varchar(512)  NULL,

	 [ThreadName] varchar(512)  NULL,

	 [Win32ThreadId] varchar(128)  NULL,

	 [Message] varchar(2500)  NULL,

	 [FormattedMessage] varchar(max)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAG] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] varchar(80)  NOT NULL,

	 [Login] varchar(50)  NULL,

	 [Pwd] varchar(50)  NULL,

	 [IsUser] bit  NOT NULL,

	 [IsGroup] bit  NOT NULL,

	 [IsRole] bit  NOT NULL,

	 [ConnString] varchar(255)  NULL,

	 [Enabled] bit  NULL,

	 [Email] varchar(80)  NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMLG] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Group_m0] uniqueidentifier  NOT NULL,

	 [User_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAuObjType] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] nvarchar(255)  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAuEntity] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [ObjectPrimaryKey] nvarchar(38)  NOT NULL,

	 [OperationTime] DATETIME  NOT NULL,

	 [OperationType] nvarchar(100)  NOT NULL,

	 [ExecutionResult] nvarchar(12)  NOT NULL,

	 [Source] nvarchar(255)  NOT NULL,

	 [SerializedField] nvarchar(max)  NULL,

	 [User_m0] uniqueidentifier  NOT NULL,

	 [ObjectType_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAuField] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Field] nvarchar(100)  NOT NULL,

	 [OldValue] nvarchar(max)  NULL,

	 [NewValue] nvarchar(max)  NULL,

	 [MainChange_m0] uniqueidentifier  NULL,

	 [AuditEntity_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))




 ALTER TABLE [Лес] ADD CONSTRAINT [Лес_FСтрана_0] FOREIGN KEY ([Страна]) REFERENCES [Страна]
CREATE INDEX Лес_IСтрана on [Лес] ([Страна])

 ALTER TABLE [TestDetailWithCicle] ADD CONSTRAINT [TestDetailWithCicle_FTestDetailWithCicle_0] FOREIGN KEY ([Parent]) REFERENCES [TestDetailWithCicle]
CREATE INDEX TestDetailWithCicle_IParent on [TestDetailWithCicle] ([Parent])

 ALTER TABLE [TestDetailWithCicle] ADD CONSTRAINT [TestDetailWithCicle_FTestMaster_0] FOREIGN KEY ([TestMaster]) REFERENCES [TestMaster]
CREATE INDEX TestDetailWithCicle_ITestMaster on [TestDetailWithCicle] ([TestMaster])

 ALTER TABLE [Медведь] ADD CONSTRAINT [Медведь_FСтрана_0] FOREIGN KEY ([Страна]) REFERENCES [Страна]
CREATE INDEX Медведь_IСтрана on [Медведь] ([Страна])

 ALTER TABLE [Медведь] ADD CONSTRAINT [Медведь_FЛес_0] FOREIGN KEY ([ЛесОбитания]) REFERENCES [Лес]
CREATE INDEX Медведь_IЛесОбитания on [Медведь] ([ЛесОбитания])

 ALTER TABLE [Медведь] ADD CONSTRAINT [Медведь_FМедведь_0] FOREIGN KEY ([Мама]) REFERENCES [Медведь]
CREATE INDEX Медведь_IМама on [Медведь] ([Мама])

 ALTER TABLE [Медведь] ADD CONSTRAINT [Медведь_FМедведь_1] FOREIGN KEY ([Папа]) REFERENCES [Медведь]
CREATE INDEX Медведь_IПапа on [Медведь] ([Папа])

 ALTER TABLE [Детейл] ADD CONSTRAINT [Детейл_FБазовыйКласс_0] FOREIGN KEY ([БазовыйКласс_m0]) REFERENCES [БазовыйКласс]
CREATE INDEX Детейл_IБазовыйКласс_m0 on [Детейл] ([БазовыйКласс_m0])

 ALTER TABLE [Детейл] ADD CONSTRAINT [Детейл_FНаследник_0] FOREIGN KEY ([БазовыйКласс_m1]) REFERENCES [Наследник]
CREATE INDEX Детейл_IБазовыйКласс_m1 on [Детейл] ([БазовыйКласс_m1])

 ALTER TABLE [Книга] ADD CONSTRAINT [Книга_FАвтор_0] FOREIGN KEY ([Автор1]) REFERENCES [Автор]
CREATE INDEX Книга_IАвтор1 on [Книга] ([Автор1])

 ALTER TABLE [Книга] ADD CONSTRAINT [Книга_FБиблиотека_0] FOREIGN KEY ([Библиотека1]) REFERENCES [Библиотека]
CREATE INDEX Книга_IБиблиотека1 on [Книга] ([Библиотека1])

 ALTER TABLE [Наследник] ADD CONSTRAINT [Наследник_FМастер_0] FOREIGN KEY ([Мастер]) REFERENCES [Мастер]
CREATE INDEX Наследник_IМастер on [Наследник] ([Мастер])

 ALTER TABLE [Наследник] ADD CONSTRAINT [Наследник_FMaster_0] FOREIGN KEY ([Master]) REFERENCES [Master]
CREATE INDEX Наследник_IMaster on [Наследник] ([Master])

 ALTER TABLE [Car] ADD CONSTRAINT [Car_FDriver_0] FOREIGN KEY ([driver]) REFERENCES [Driver]
CREATE INDEX Car_Idriver on [Car] ([driver])

 ALTER TABLE [Мастер] ADD CONSTRAINT [Мастер_FМастер2_0] FOREIGN KEY ([Мастер2]) REFERENCES [Мастер2]
CREATE INDEX Мастер_IМастер2 on [Мастер] ([Мастер2])

 ALTER TABLE [Блоха] ADD CONSTRAINT [Блоха_FМедведь_0] FOREIGN KEY ([МедведьОбитания]) REFERENCES [Медведь]
CREATE INDEX Блоха_IМедведьОбитания on [Блоха] ([МедведьОбитания])

 ALTER TABLE [Берлога] ADD CONSTRAINT [Берлога_FЛес_0] FOREIGN KEY ([ЛесРасположения]) REFERENCES [Лес]
CREATE INDEX Берлога_IЛесРасположения on [Берлога] ([ЛесРасположения])

 ALTER TABLE [Берлога] ADD CONSTRAINT [Берлога_FМедведь_0] FOREIGN KEY ([Медведь]) REFERENCES [Медведь]
CREATE INDEX Берлога_IМедведь on [Берлога] ([Медведь])

 ALTER TABLE [Детейл2] ADD CONSTRAINT [Детейл2_FДетейл_0] FOREIGN KEY ([Детейл]) REFERENCES [Детейл]
CREATE INDEX Детейл2_IДетейл on [Детейл2] ([Детейл])

 ALTER TABLE [Журнал] ADD CONSTRAINT [Журнал_FАвтор_0] FOREIGN KEY ([Автор2]) REFERENCES [Автор]
CREATE INDEX Журнал_IАвтор2 on [Журнал] ([Автор2])

 ALTER TABLE [Журнал] ADD CONSTRAINT [Журнал_FБиблиотека_0] FOREIGN KEY ([Библиотека2]) REFERENCES [Библиотека]
CREATE INDEX Журнал_IБиблиотека2 on [Журнал] ([Библиотека2])

 ALTER TABLE [STORMWEBSEARCH] ADD CONSTRAINT [STORMWEBSEARCH_FSTORMFILTERSETTING_0] FOREIGN KEY ([FilterSetting_m0]) REFERENCES [STORMFILTERSETTING]

 ALTER TABLE [STORMFILTERDETAIL] ADD CONSTRAINT [STORMFILTERDETAIL_FSTORMFILTERSETTING_0] FOREIGN KEY ([FilterSetting_m0]) REFERENCES [STORMFILTERSETTING]

 ALTER TABLE [STORMFILTERLOOKUP] ADD CONSTRAINT [STORMFILTERLOOKUP_FSTORMFILTERSETTING_0] FOREIGN KEY ([FilterSetting_m0]) REFERENCES [STORMFILTERSETTING]

 ALTER TABLE [STORMLG] ADD CONSTRAINT [STORMLG_FSTORMAG_0] FOREIGN KEY ([Group_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMLG] ADD CONSTRAINT [STORMLG_FSTORMAG_1] FOREIGN KEY ([User_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMAuEntity] ADD CONSTRAINT [STORMAuEntity_FSTORMAG_0] FOREIGN KEY ([User_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMAuEntity] ADD CONSTRAINT [STORMAuEntity_FSTORMAuObjType_0] FOREIGN KEY ([ObjectType_m0]) REFERENCES [STORMAuObjType]

 ALTER TABLE [STORMAuField] ADD CONSTRAINT [STORMAuField_FSTORMAuField_0] FOREIGN KEY ([MainChange_m0]) REFERENCES [STORMAuField]

 ALTER TABLE [STORMAuField] ADD CONSTRAINT [STORMAuField_FSTORMAuEntity_0] FOREIGN KEY ([AuditEntity_m0]) REFERENCES [STORMAuEntity]
