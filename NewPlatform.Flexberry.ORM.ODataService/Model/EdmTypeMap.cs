namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using Microsoft.OData.Edm;
    using Microsoft.Spatial;

    using FileType = ICSSoft.STORMNET.FileType;
    using KeyGen = ICSSoft.STORMNET.KeyGen;
    using UserDataTypes = ICSSoft.STORMNET.UserDataTypes;

#if NETFRAMEWORK
    using System.Data.Linq;
#endif

    /// <summary>
    /// Карта соответствия типов CLR и EDM.
    /// </summary>
    /// <remarks>
    /// Основная часть кода взята из internal класса System.Web.OData.EdmLibHelpers.
    /// </remarks>
    internal static class EdmTypeMap
    {
        /// <summary>
        /// Модель, содержащая стандартные сопоставления для типов CLR и EDM.
        /// </summary>
        private static readonly EdmCoreModel _coreModel = EdmCoreModel.Instance;

        /// <summary>
        /// Расширенные сопоставления для типов CLR и EDM.
        /// </summary>
        private static readonly Dictionary<Type, IEdmPrimitiveType> _typesMap = new Dictionary<Type, IEdmPrimitiveType>
        {
            // Стандартные CLR-типы (соответствия взяты из internal класса System.Web.OData.EdmLibHelpers).
            { typeof(object), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(string), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(bool), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Boolean) },
            { typeof(bool?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Boolean) },
            { typeof(byte), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Byte) },
            { typeof(byte?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Byte) },
            { typeof(decimal), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Decimal) },
            { typeof(decimal?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Decimal) },
            { typeof(double), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Double) },
            { typeof(double?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Double) },
            { typeof(Guid), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Guid) },
            { typeof(Guid?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Guid) },
            { typeof(short), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int16) },
            { typeof(short?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int16) },
            { typeof(int), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int32) },
            { typeof(int?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int32) },
            { typeof(long), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int64) },
            { typeof(long?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int64) },
            { typeof(sbyte), GetEdmPrimitiveType(EdmPrimitiveTypeKind.SByte) },
            { typeof(sbyte?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.SByte) },
            { typeof(float), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Single) },
            { typeof(float?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Single) },
            { typeof(byte[]), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Binary) },
            { typeof(Stream), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Stream) },
            { typeof(Geography), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Geography) },
            { typeof(GeographyPoint), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint) },
            { typeof(GeographyLineString), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString) },
            { typeof(GeographyPolygon), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon) },
            { typeof(GeographyCollection), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection) },
            { typeof(GeographyMultiLineString), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString) },
            { typeof(GeographyMultiPoint), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint) },
            { typeof(GeographyMultiPolygon), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon) },
            { typeof(Geometry), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Geometry) },
            { typeof(GeometryPoint), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint) },
            { typeof(GeometryLineString), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString) },
            { typeof(GeometryPolygon), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon) },
            { typeof(GeometryCollection), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection) },
            { typeof(GeometryMultiLineString), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString) },
            { typeof(GeometryMultiPoint), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint) },
            { typeof(GeometryMultiPolygon), GetEdmPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon) },
            { typeof(DateTimeOffset), GetEdmPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset) },
            { typeof(DateTimeOffset?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset) },
            { typeof(TimeSpan), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Duration) },
            { typeof(TimeSpan?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Duration) },
            { typeof(Date), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Date) },
            { typeof(Date?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Date) },
            { typeof(TimeOfDay), GetEdmPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay) },
            { typeof(TimeOfDay?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay) },
            { typeof(XElement), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
#if NETFRAMEWORK
            { typeof(Binary), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Binary) },
#endif
            { typeof(ushort), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int32) },
            { typeof(ushort?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int32) },
            { typeof(uint), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int64) },
            { typeof(uint?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int64) },
            { typeof(ulong), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int64) },
            { typeof(ulong?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int64) },
            { typeof(char[]), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(char), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(char?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(DateTime), GetEdmPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset) },
            { typeof(DateTime?), GetEdmPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset) },

            // Кастомные типы из Newplatform.Flexberry.ORM.
            { typeof(UserDataTypes.NullableDateTime), GetEdmPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset) },
            { typeof(UserDataTypes.NullableInt), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Int32) },
            { typeof(UserDataTypes.NullableDecimal), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Decimal) },
            { typeof(UserDataTypes.Blob), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Binary) },
            { typeof(UserDataTypes.PartliedDate), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(UserDataTypes.Contact), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(UserDataTypes.Event), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(UserDataTypes.WebFile), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(UserDataTypes.GeoData), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(UserDataTypes.Image), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(FileType.File), GetEdmPrimitiveType(EdmPrimitiveTypeKind.String) },
            { typeof(KeyGen.KeyGuid), GetEdmPrimitiveType(EdmPrimitiveTypeKind.Guid) }
        };

        /// <summary>
        /// Осуществляет получение EDM-типа, по соответствующему ему CLR-типу.
        /// </summary>
        /// <param name="clrType">CLR-тип, для которого требуется получить соответствующий ему EDM-тип.</param>
        /// <param name="additionalMapping">Дополнительный маппинг типов.</param>
        /// <returns>EDM-тип, соответствующий заданному CRL-типу.</returns>
        public static IEdmPrimitiveType GetEdmPrimitiveType(Type clrType, Dictionary<Type, IEdmPrimitiveType> additionalMapping)
        {
            var typesMap = _typesMap;
            additionalMapping?.ToList().ForEach(x =>
            {
                if (!typesMap.ContainsKey(x.Key))
                {
                    typesMap.Add(x.Key, x.Value);
                }
            });

            _typesMap.TryGetValue(clrType, out var edmPrimitiveType);

            return edmPrimitiveType;
        }

        /// <summary>
        /// Осуществляет получение EDM-типа, по соответствующему ему значению перечисления <see cref="EdmPrimitiveTypeKind"/>.
        /// </summary>
        /// <param name="primitiveKind">Значение перечисления <see cref="EdmPrimitiveTypeKind"/>, соответствующее искомому EDM-типу.</param>
        /// <returns>EDM-тип, соответствующий заданному значению перечисления <see cref="EdmPrimitiveTypeKind"/>.</returns>
        private static IEdmPrimitiveType GetEdmPrimitiveType(EdmPrimitiveTypeKind primitiveKind)
        {
            return _coreModel.GetPrimitiveType(primitiveKind);
        }
    }
}
