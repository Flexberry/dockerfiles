// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/Query/Expressions/ClrCanonicalFunctions.cs
namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using Microsoft.Spatial;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.OData.Edm.Library;
    using ICSSoft.STORMNET.Business.LINQProvider.Extensions;

    /// <summary>
    /// Класс содержит определения функций описанных в стандарте OData в главе 11.2.5.1 System Query Option $filter:
    /// http://docs.oasis-open.org/odata/odata/v4.0/odata-v4.0-part1-protocol.html
    /// </summary>
    internal class ClrCanonicalFunctions
    {

        /// <summary>
        /// Определение reflection для пользовательской функции GisExtensions.GeoIntersects
        /// </summary>
        public static readonly MethodInfo GeoIntersects;

        /// <summary>
        /// Определение reflection для пользовательской функции GisExtensions.GeoIntersects для типа Geometry
        /// </summary>
        public static readonly MethodInfo GeomIntersects;

        // string functions

        /// <summary>
        /// Определение reflection для default(string).StartsWith
        /// </summary>
        public static readonly MethodInfo StartsWith;

        /// <summary>
        /// Определение reflection для default(string).EndsWith
        /// </summary>
        public static readonly MethodInfo EndsWith;

        /// <summary>
        /// Определение reflection для default(string).Contains
        /// </summary>
        public static readonly MethodInfo Contains;

        /// <summary>
        /// Определение reflection для default(string).Substring
        /// </summary>
        public static readonly MethodInfo SubstringStart;

        /// <summary>
        /// Определение reflection для default(string).Substring
        /// </summary>
        public static readonly MethodInfo SubstringStartAndLength;

        /// <summary>
        /// Определение reflection для default(string).Substring
        /// </summary>
        public static readonly MethodInfo SubstringStartNoThrow;

        /// <summary>
        /// Определение reflection для default(string).Substring
        /// </summary>
        public static readonly MethodInfo SubstringStartAndLengthNoThrow;

        /// <summary>
        /// Определение reflection для default(string).IndexOf
        /// </summary>
        public static readonly MethodInfo IndexOf;

        /// <summary>
        /// Определение reflection для default(string).ToLower
        /// </summary>
        public static readonly MethodInfo ToLower;

        /// <summary>
        /// Определение reflection для default(string).ToUpper
        /// </summary>
        public static readonly MethodInfo ToUpper;

        /// <summary>
        /// Определение reflection для default(string).Trim
        /// </summary>
        public static readonly MethodInfo Trim;

        /// <summary>
        /// Определение reflection для default(string).Concat
        /// </summary>
        public static readonly MethodInfo Concat;

        // math functions

        /// <summary>
        /// Определение reflection для Math.Ceiling
        /// </summary>
        public static readonly MethodInfo CeilingOfDouble;

        /// <summary>
        /// Определение reflection для Math.Round
        /// </summary>
        public static readonly MethodInfo RoundOfDouble;

        /// <summary>
        /// Определение reflection для Math.Floor
        /// </summary>
        public static readonly MethodInfo FloorOfDouble;

        /// <summary>
        /// Определение reflection для Math.Ceiling
        /// </summary>
        public static readonly MethodInfo CeilingOfDecimal;

        /// <summary>
        /// Определение reflection для Math.Round
        /// </summary>
        public static readonly MethodInfo RoundOfDecimal;

        /// <summary>
        /// Определение reflection для Math.Floor
        /// </summary>
        public static readonly MethodInfo FloorOfDecimal;

        /// <summary>
        /// Определение reflection для DateTime.Now
        /// </summary>
        public static readonly PropertyInfo Now;

        // enum functions

        /// <summary>
        /// Определение reflection для default(Enum).HasFlag
        /// </summary>
        public static readonly MethodInfo HasFlag;

        // Date properties

        /// <summary>
        /// Определения reflection для Date.Year, Date.Month, Date.Day.
        /// В качестве ключей применяются строковые констаны функций OData: YearFunctionName, MonthFunctionName, DayFunctionName.
        /// </summary>
        public static readonly Dictionary<string, PropertyInfo> DateProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(YearFunctionName, typeof(Date).GetProperty("Year")),
            new KeyValuePair<string, PropertyInfo>(MonthFunctionName, typeof(Date).GetProperty("Month")),
            new KeyValuePair<string, PropertyInfo>(DayFunctionName, typeof(Date).GetProperty("Day")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // DateTime properties

        /// <summary>
        /// Определения reflection для DateTime.Year, DateTime.Month, DateTime.Day, DateTime.Hour,
        /// DateTime.Minute, DateTime.Second, DateTime.Millisecond.
        /// В качестве ключей применяются строковые констаны функций OData: YearFunctionName, MonthFunctionName, DayFunctionName,
        /// HourFunctionName, MinuteFunctionName, SecondFunctionName, MillisecondFunctionName.
        /// </summary>
        public static readonly Dictionary<string, PropertyInfo> DateTimeProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(YearFunctionName, typeof(DateTime).GetProperty("Year")),
            new KeyValuePair<string, PropertyInfo>(MonthFunctionName, typeof(DateTime).GetProperty("Month")),
            new KeyValuePair<string, PropertyInfo>(DayFunctionName, typeof(DateTime).GetProperty("Day")),
            new KeyValuePair<string, PropertyInfo>(HourFunctionName, typeof(DateTime).GetProperty("Hour")),
            new KeyValuePair<string, PropertyInfo>(MinuteFunctionName, typeof(DateTime).GetProperty("Minute")),
            new KeyValuePair<string, PropertyInfo>(SecondFunctionName, typeof(DateTime).GetProperty("Second")),
            new KeyValuePair<string, PropertyInfo>(MillisecondFunctionName, typeof(DateTime).GetProperty("Millisecond")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // DateTimeOffset properties

        /// <summary>
        /// Определения reflection для DateTimeOffset.Year, DateTimeOffset.Month, DateTimeOffset.Day, DateTimeOffset.Hour,
        /// DateTimeOffset.Minute, DateTimeOffset.Second, TimeOfDay.Millisecond.
        /// В качестве ключей применяются строковые констаны функций OData: YearFunctionName, MonthFunctionName, DayFunctionName,
        /// HourFunctionName, MinuteFunctionName, SecondFunctionName, MillisecondFunctionName.
        /// </summary>
        public static readonly Dictionary<string, PropertyInfo> DateTimeOffsetProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(YearFunctionName, typeof(DateTimeOffset).GetProperty("Year")),
            new KeyValuePair<string, PropertyInfo>(MonthFunctionName, typeof(DateTimeOffset).GetProperty("Month")),
            new KeyValuePair<string, PropertyInfo>(DayFunctionName, typeof(DateTimeOffset).GetProperty("Day")),
            new KeyValuePair<string, PropertyInfo>(HourFunctionName, typeof(DateTimeOffset).GetProperty("Hour")),
            new KeyValuePair<string, PropertyInfo>(MinuteFunctionName, typeof(DateTimeOffset).GetProperty("Minute")),
            new KeyValuePair<string, PropertyInfo>(SecondFunctionName, typeof(DateTimeOffset).GetProperty("Second")),
            new KeyValuePair<string, PropertyInfo>(MillisecondFunctionName, typeof(DateTimeOffset).GetProperty("Millisecond")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // TimeOfDay properties
        // ODL uses the Hour(s), Minute(s), Second(s), It's the wrong property name. It should be Hour, Minute, Second.

        /// <summary>
        /// Определения reflection для TimeOfDay.Hours, TimeOfDay.Minutes, TimeOfDay.Seconds, TimeOfDay.Milliseconds.
        /// В качестве ключей применяются строковые констаны функций OData: HourFunctionName, MinuteFunctionName, SecondFunctionName, MillisecondFunctionName
        /// </summary>
        public static readonly Dictionary<string, PropertyInfo> TimeOfDayProperties = new[]
        {
            new KeyValuePair<string, PropertyInfo>(HourFunctionName, typeof(TimeOfDay).GetProperty("Hours")),
            new KeyValuePair<string, PropertyInfo>(MinuteFunctionName, typeof(TimeOfDay).GetProperty("Minutes")),
            new KeyValuePair<string, PropertyInfo>(SecondFunctionName, typeof(TimeOfDay).GetProperty("Seconds")),
            new KeyValuePair<string, PropertyInfo>(MillisecondFunctionName, typeof(TimeOfDay).GetProperty("Milliseconds")),
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // String Properties

        /// <summary>
        /// Определение reflection для string.Length
        /// </summary>
        public static readonly PropertyInfo Length = typeof(string).GetProperty("Length");

        // PropertyInfo and MethodInfo of DateTime & DateTimeOffset related.

        /// <summary>
        /// Определение reflection для DateTime.Kind
        /// </summary>
        public static readonly PropertyInfo DateTimeKindPropertyInfo = typeof(DateTime).GetProperty("Kind");

        /// <summary>
        /// Определение reflection для DateTime.ToUniversalTime
        /// </summary>
        public static readonly MethodInfo ToUniversalTimeDateTime = typeof(DateTime).GetMethod("ToUniversalTime", BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Определение reflection для DateTimeOffset.ToUniversalTime
        /// </summary>
        public static readonly MethodInfo ToUniversalTimeDateTimeOffset = typeof(DateTimeOffset).GetMethod("ToUniversalTime", BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Определение reflection для DateTimeOffset.ToOffset
        /// </summary>
        public static readonly MethodInfo ToOffsetFunction = typeof(DateTimeOffset).GetMethod("ToOffset", BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Определение reflection для TimeZoneInfo.GetUtcOffset
        /// </summary>
        public static readonly MethodInfo GetUtcOffset = typeof(TimeZoneInfo).GetMethod("GetUtcOffset", new[] { typeof(DateTime) });

        // function names

        /// <summary>
        /// Строковая константа для функции startswith
        /// </summary>
        internal const string StartswithFunctionName = "startswith";

        /// <summary>
        /// Строковая константа для функции endswith
        /// </summary>
        internal const string EndswithFunctionName = "endswith";

        /// <summary>
        /// Строковая константа для функции contains
        /// </summary>
        internal const string ContainsFunctionName = "contains";

        /// <summary>
        /// Строковая константа для функции substring
        /// </summary>
        internal const string SubstringFunctionName = "substring";

        /// <summary>
        /// Строковая константа для функции length
        /// </summary>
        internal const string LengthFunctionName = "length";

        /// <summary>
        /// Строковая константа для функции indexof
        /// </summary>
        internal const string IndexofFunctionName = "indexof";

        /// <summary>
        /// Строковая константа для функции tolower
        /// </summary>
        internal const string TolowerFunctionName = "tolower";

        /// <summary>
        /// Строковая константа для функции toupper
        /// </summary>
        internal const string ToupperFunctionName = "toupper";

        /// <summary>
        /// Строковая константа для функции trim
        /// </summary>
        internal const string TrimFunctionName = "trim";

        /// <summary>
        /// Строковая константа для функции concat
        /// </summary>
        internal const string ConcatFunctionName = "concat";

        /// <summary>
        /// Строковая константа для функции year
        /// </summary>
        internal const string YearFunctionName = "year";

        /// <summary>
        /// Строковая константа для функции month
        /// </summary>
        internal const string MonthFunctionName = "month";

        /// <summary>
        /// Строковая константа для функции day
        /// </summary>
        internal const string DayFunctionName = "day";

        /// <summary>
        /// Строковая константа для функции hour
        /// </summary>
        internal const string HourFunctionName = "hour";

        /// <summary>
        /// Строковая константа для функции minute
        /// </summary>
        internal const string MinuteFunctionName = "minute";

        /// <summary>
        /// Строковая константа для функции second
        /// </summary>
        internal const string SecondFunctionName = "second";

        /// <summary>
        /// Строковая константа для функции millisecond
        /// </summary>
        internal const string MillisecondFunctionName = "millisecond";

        /// <summary>
        /// Строковая константа для функции fractionalseconds
        /// </summary>
        internal const string FractionalSecondsFunctionName = "fractionalseconds";

        /// <summary>
        /// Строковая константа для функции round
        /// </summary>
        internal const string RoundFunctionName = "round";

        /// <summary>
        /// Строковая константа для функции floor
        /// </summary>
        internal const string FloorFunctionName = "floor";

        /// <summary>
        /// Строковая константа для функции ceiling
        /// </summary>
        internal const string CeilingFunctionName = "ceiling";

        /// <summary>
        /// Строковая константа для функции cast
        /// </summary>
        internal const string CastFunctionName = "cast";

        /// <summary>
        /// Строковая константа для функции date
        /// </summary>
        internal const string DateFunctionName = "date";

        /// <summary>
        /// Строковая константа для функции time
        /// </summary>
        internal const string TimeFunctionName = "time";

        private static string _defaultString = default(string);
        private static Enum _defaultEnum = default(Enum);

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Initialization is order dependent")]
        static ClrCanonicalFunctions()
        {
            GeoIntersects = MethodOf(_ => GisExtensions.GeoIntersects(default(Geography), default(Geography)));
            GeomIntersects = MethodOf(_ => GisExtensions.GeomIntersects(default(Geometry), default(Geometry)));

            StartsWith = MethodOf(_ => _defaultString.StartsWith(default(string)));
            EndsWith = MethodOf(_ => _defaultString.EndsWith(default(string)));
            IndexOf = MethodOf(_ => _defaultString.IndexOf(default(string)));
            SubstringStart = MethodOf(_ => _defaultString.Substring(default(int)));
            SubstringStartAndLength = MethodOf(_ => _defaultString.Substring(default(int), default(int)));
            SubstringStartNoThrow = MethodOf(_ => ClrSafeFunctions.SubstringStart(default(string), default(int)));
            SubstringStartAndLengthNoThrow = MethodOf(_ => ClrSafeFunctions.SubstringStartAndLength(default(string), default(int), default(int)));
            Contains = MethodOf(_ => _defaultString.Contains(default(string)));
            ToLower = MethodOf(_ => _defaultString.ToLower());
            ToUpper = MethodOf(_ => _defaultString.ToUpper());
            Trim = MethodOf(_ => _defaultString.Trim());
            Concat = MethodOf(_ => string.Concat(default(string), default(string)));

            CeilingOfDecimal = MethodOf(_ => Math.Ceiling(default(decimal)));
            RoundOfDecimal = MethodOf(_ => Math.Round(default(decimal)));
            FloorOfDecimal = MethodOf(_ => Math.Floor(default(decimal)));

            CeilingOfDouble = MethodOf(_ => Math.Ceiling(default(double)));
            RoundOfDouble = MethodOf(_ => Math.Round(default(double)));
            FloorOfDouble = MethodOf(_ => Math.Floor(default(double)));

            Now = typeof(DateTime).GetProperty("Now");

            HasFlag = MethodOf(_ => _defaultEnum.HasFlag(default(Enum)));
        }

        private static MethodInfo MethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return MethodOf(expression as Expression);
        }

        private static MethodInfo MethodOf(Expression expression)
        {
            LambdaExpression lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression == null)
            {
                throw new ArgumentException("Contract assertion not met: lambdaExpression != null", "value");
            }

            if (expression.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException("Contract assertion not met: expression.NodeType == ExpressionType.Lambda", nameof(expression));
            }

            if (lambdaExpression.Body.NodeType != ExpressionType.Call)
            {
                throw new ArgumentException("Contract assertion not met: lambdaExpression.Body.NodeType == ExpressionType.Call", "value");
            }

            return (lambdaExpression.Body as MethodCallExpression).Method;
        }
    }
}
