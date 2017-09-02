namespace NewPlatform.Flexberry
{
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ORM.ODataService.UserSettingsService;

    /// <summary>
    /// Класс для удобной работы с настройками пользователя в клиентском коде.
    /// </summary>
    [ClassStorage("UserSetting")]
    [BusinessServer(typeof(FlexberryUserSettingBS), DataServiceObjectEvents.OnAllEvents)]
    [View(
        "FlexberryUserSettingE",
        new string[]
    {
            "AppName",
            "UserName",
            "UserGuid",
            "ModuleName",
            "ModuleGuid",
            "SettName",
            "SettGuid",
            "StrVal",
            "TxtVal",
            "IntVal",
            "BoolVal",
            "GuidVal",
            "SettLastAccessTime",
            "DecimalVal",
            "DateTimeVal"
    })
        ]
    public class FlexberryUserSetting : ICSSoft.Services.UserSetting
    {
        /// <summary>
        /// Class views container.
        /// </summary>
        public class Views
        {
            /// <summary>
            /// "FlexberryUserSettingE" view.
            /// </summary>
            public static ICSSoft.STORMNET.View FlexberryUserSettingE
            {
                get
                {
                    return ICSSoft.STORMNET.Information.GetView("FlexberryUserSettingE", typeof(FlexberryUserSetting));
                }
            }
        }
    }
}
