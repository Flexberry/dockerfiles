// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/TimeZoneInfoHelper.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;

    /// <summary>
    /// Вспомогательные методы для работы с часовой зоной.
    /// </summary>
    internal class TimeZoneInfoHelper
    {
        private static TimeZoneInfo _defaultTimeZoneInfo;

        /// <summary>
        /// Текущая часовая зона по умолчанию.
        /// </summary>
        public static TimeZoneInfo TimeZone
        {
            get
            {
                if (_defaultTimeZoneInfo == null)
                {
                    return TimeZoneInfo.Local;
                }

                return _defaultTimeZoneInfo;
            }

            set
            {
                _defaultTimeZoneInfo = value;
            }
        }
    }
}
