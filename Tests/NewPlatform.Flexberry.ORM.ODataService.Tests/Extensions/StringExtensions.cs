namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Класс, содержащий вспомогательные методы для работы со строками.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Осуществляет приведение строки, содержащей unicode коды символов и спецсимволы, к читабельному виду.
        /// </summary>
        /// <param name="uglyString">Строка, содержащая unicode коды символов и спецсимволы.</param>
        /// <returns>Строка, содержащая читабельные слова вместо unicode кодов и спецсимволов.</returns>
        public static string Beautify(this string uglyString)
        {
            return Uri.UnescapeDataString(Regex.Unescape(uglyString));
        }

        /// <summary>
        /// Осуществляет приведение строки, из читабельного вида к содержащему unicode коды символов и спецсимволы.
        /// </summary>
        /// <param name="uglyString">Строка, в читабельном виде.</param>
        /// <returns>Строка, содержащая unicode кодов и спецсимволов.</returns>
        public static string Unicodify(this string uglyString)
        {
            return Uri.EscapeDataString(Regex.Unescape(uglyString));
        }
    }
}
