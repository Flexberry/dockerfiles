﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System;
    using System.Xml;
    using ICSSoft.STORMNET;
    
    
    // *** Start programmer edit section *** (Using statements)

    // *** End programmer edit section *** (Using statements)


    /// <summary>
    /// Блоха.
    /// </summary>
    // *** Start programmer edit section *** (Блоха CustomAttributes)

    // *** End programmer edit section *** (Блоха CustomAttributes)
    [AutoAltered()]
    [AccessType(ICSSoft.STORMNET.AccessType.none)]
    [View("PseudoDetailView", new string[] {
            "Кличка"})]
    public class Блоха : ICSSoft.STORMNET.DataObject
    {
        
        private string fКличка;
        
        private NewPlatform.Flexberry.ORM.ODataService.Tests.Медведь fМедведьОбитания;
        
        // *** Start programmer edit section *** (Блоха CustomMembers)

        // *** End programmer edit section *** (Блоха CustomMembers)

        
        /// <summary>
        /// Кличка.
        /// </summary>
        // *** Start programmer edit section *** (Блоха.Кличка CustomAttributes)

        // *** End programmer edit section *** (Блоха.Кличка CustomAttributes)
        [StrLen(255)]
        public virtual string Кличка
        {
            get
            {
                // *** Start programmer edit section *** (Блоха.Кличка Get start)

                // *** End programmer edit section *** (Блоха.Кличка Get start)
                string result = this.fКличка;
                // *** Start programmer edit section *** (Блоха.Кличка Get end)

                // *** End programmer edit section *** (Блоха.Кличка Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Блоха.Кличка Set start)

                // *** End programmer edit section *** (Блоха.Кличка Set start)
                this.fКличка = value;
                // *** Start programmer edit section *** (Блоха.Кличка Set end)

                // *** End programmer edit section *** (Блоха.Кличка Set end)
            }
        }
        
        /// <summary>
        /// Блоха.
        /// </summary>
        // *** Start programmer edit section *** (Блоха.МедведьОбитания CustomAttributes)

        // *** End programmer edit section *** (Блоха.МедведьОбитания CustomAttributes)
        [PropertyStorage(new string[] {
                "МедведьОбитания"})]
        public virtual NewPlatform.Flexberry.ORM.ODataService.Tests.Медведь МедведьОбитания
        {
            get
            {
                // *** Start programmer edit section *** (Блоха.МедведьОбитания Get start)

                // *** End programmer edit section *** (Блоха.МедведьОбитания Get start)
                NewPlatform.Flexberry.ORM.ODataService.Tests.Медведь result = this.fМедведьОбитания;
                // *** Start programmer edit section *** (Блоха.МедведьОбитания Get end)

                // *** End programmer edit section *** (Блоха.МедведьОбитания Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Блоха.МедведьОбитания Set start)

                // *** End programmer edit section *** (Блоха.МедведьОбитания Set start)
                this.fМедведьОбитания = value;
                // *** Start programmer edit section *** (Блоха.МедведьОбитания Set end)

                // *** End programmer edit section *** (Блоха.МедведьОбитания Set end)
            }
        }
        
        /// <summary>
        /// Class views container.
        /// </summary>
        public class Views
        {
            
            /// <summary>
            /// Представление для работы тестов на фильтрацию с использованием псевдодетейла.
            /// </summary>
            public static ICSSoft.STORMNET.View PseudoDetailView
            {
                get
                {
                    return ICSSoft.STORMNET.Information.GetView("PseudoDetailView", typeof(NewPlatform.Flexberry.ORM.ODataService.Tests.Блоха));
                }
            }
        }
    }
}
