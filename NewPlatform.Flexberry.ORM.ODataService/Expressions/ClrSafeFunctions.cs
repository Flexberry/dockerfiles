// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/Query/Expressions/ClrSafeFunctions.cs
namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;

    /// <summary>
    /// This class contains safe equivalents of CLR functions that
    /// could throw exceptions at runtime.
    /// </summary>
    internal class ClrSafeFunctions
    {
        /// <summary>
        /// Возвращает подстроку из данной строки.
        /// </summary>
        /// <param name="str">Строка</param>
        /// <param name="startIndex">Индекс начала подстроки.</param>
        /// <returns>Подстрока.</returns>
        public static string SubstringStart(string str, int startIndex)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str), "Contract assertion not met: str != null");
            }

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            // String.Substring(int) accepts startIndex==length
            return startIndex <= str.Length
                    ? str.Substring(startIndex)
                    : string.Empty;
        }

        /// <summary>
        /// Возвращает подстроку из данной строки.
        /// </summary>
        /// <param name="str">Строка</param>
        /// <param name="startIndex">Индекс начала подстроки.</param>
        /// <param name="length">Длина подстроки.</param>
        /// <returns>Подстрока.</returns>
        public static string SubstringStartAndLength(string str, int startIndex, int length)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str), "Contract assertion not met: str != null");
            }

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            int strLength = str.Length;

            // String.Substring(int, int) accepts startIndex==length
            if (startIndex > strLength)
            {
                return string.Empty;
            }

            length = Math.Min(length, strLength - startIndex);
            return length >= 0
                    ? str.Substring(startIndex, length)
                    : string.Empty;
        }
    }
}
