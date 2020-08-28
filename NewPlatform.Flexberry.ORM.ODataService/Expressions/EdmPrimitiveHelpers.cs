// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/Formatter/EdmPrimitiveHelpers.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.AspNet.OData.Common;

#if NETFRAMEWORK
    using System.Data.Linq;
#endif


    using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

    /// <summary>
    /// Вспомогательные методы для работы примитивными значениями.
    /// </summary>
    internal static class EdmPrimitiveHelpers
    {
        /// <summary>
        /// Преобразует значение в заданный тип.
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="type">Заданный тип</param>
        /// <returns>Преобразованное значение</returns>
        public static object ConvertPrimitiveValue(object value, Type type)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Contract assertion not met: value != null");
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "Contract assertion not met: type != null");
            }

            // if value is of the same type nothing to do here.
            if (value.GetType() == type || value.GetType() == Nullable.GetUnderlyingType(type))
            {
                return value;
            }

            string str = value as string;

            if (type == typeof(char))
            {
                if (str == null || str.Length != 1)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeStringLengthOne));
                }

                return str[0];
            }
            else if (type == typeof(char?))
            {
                if (str == null || str.Length > 1)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeStringMaxLengthOne));
                }

                return str.Length > 0 ? str[0] : (char?)null;
            }
            else if (type == typeof(char[]))
            {
                if (str == null)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                }

                return str.ToCharArray();
            }
#if NETFRAMEWORK
            else if (type == typeof(Binary))
            {
                return new Binary((byte[])value);
            }
#endif
            else if (type == typeof(XElement))
            {
                if (str == null)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                }

                return XElement.Parse(str);
            }
            else
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                if (type.IsEnum)
                {
                    if (str == null)
                    {
                        throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                    }

                    return Enum.Parse(type, str);
                }
                else if (type == typeof(DateTime))
                {
                    if (value is DateTimeOffset)
                    {
                        DateTimeOffset dateTimeOffsetValue = (DateTimeOffset)value;
                        TimeZoneInfo timeZone = TimeZoneInfoHelper.TimeZone;
                        dateTimeOffsetValue = TimeZoneInfo.ConvertTime(dateTimeOffsetValue, timeZone);
                        return dateTimeOffsetValue.UtcDateTime;
                    }

                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeDateTimeOffset));
                }
                else
                {
                    if (type != typeof(uint) && type != typeof(ushort) && type != typeof(ulong))
                    {
                        throw new ArgumentException("Contract assertion not met: type == typeof(uint) || type == typeof(ushort) || type == typeof(ulong)", nameof(type));
                    }

                    // Note that we are not casting the return value to nullable<T> as even if we do it
                    // CLR would unbox it back to T.
                    return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
