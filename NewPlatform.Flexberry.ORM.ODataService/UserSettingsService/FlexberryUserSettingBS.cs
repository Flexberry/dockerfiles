namespace NewPlatform.Flexberry.ORM.ODataService.UserSettingsService
{
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    /// <summary>
    /// Бизнес-сервер для класса <see cref="FlexberryUserSetting"/>.
    /// </summary>
    public class FlexberryUserSettingBS : BusinessServer
    {
        /// <summary>
        /// Обработка изменения класса <see cref="FlexberryUserSetting"/>.
        /// В создаваемую запись записывается текущий пользователь.
        /// В редактируемой записи проверяется, что пользователь также соответствует текущему, в противном случае кидается исключение.
        /// </summary>
        /// <param name="updatedObject">Изменяемый объект.</param>
        /// <returns>Дополнительные объекты, которые требуется сохранить.</returns>
        /// <exception cref="System.Exception">Исключение кидается, если у редактируемой записи пользователь не соответствует текущему.</exception>
        public DataObject[] OnUpdateFlexberryUserSetting(FlexberryUserSetting updatedObject)
        {
            ObjectStatus objectStatus = updatedObject.GetStatus();
            string currentUserName = System.Web.HttpContext.Current.User.Identity.Name;

            if (objectStatus == ObjectStatus.Created)
            {
                updatedObject.UserName = currentUserName;
            }

            if (objectStatus == ObjectStatus.Altered && updatedObject.UserName != currentUserName && !(string.IsNullOrEmpty(updatedObject.UserName) && string.IsNullOrEmpty(currentUserName)))
            {
                throw new System.Exception("Altered user setting record doesn't belong to current user.");
            }

            return new DataObject[0];
        }
    }
}
