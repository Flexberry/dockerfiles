// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
// Branch of https://github.com/OData/WebApi/blob/v5.7.0/OData/src/System.Web.OData/OData/Formatter/EdmLibHelpers.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Expressions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.OData;
    using System.Web.OData.Builder;
    using System.Web.OData.Properties;
    using System.Web.OData.Query.Expressions;
    using System.Xml.Linq;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Annotations;
    using Microsoft.OData.Edm.Expressions;
    using Microsoft.OData.Edm.Library;
    using Microsoft.OData.Edm.Vocabularies.V1;
    using Microsoft.Spatial;

    /// <summary>
    /// Класс содержит вспомогательные методы для работы с EDM-моделью.
    /// </summary>
    internal static class EdmLibHelpers
    {
        private static readonly EdmCoreModel _coreModel = EdmCoreModel.Instance;

        private static readonly IAssembliesResolver _defaultAssemblyResolver = new DefaultAssembliesResolver();

        private static readonly Dictionary<Type, IEdmPrimitiveType> _builtInTypesMapping =
            new[]
            {
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(string), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool?), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte?), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(decimal), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(decimal?), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(double), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(double?), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid?), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(short), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(short?), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(int), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(int?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(long), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(long?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(sbyte), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(sbyte?), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(float), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(float?), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte[]), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Stream), GetPrimitiveType(EdmPrimitiveTypeKind.Stream)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geography), GetPrimitiveType(EdmPrimitiveTypeKind.Geography)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geometry), GetPrimitiveType(EdmPrimitiveTypeKind.Geometry)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan?), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Date), GetPrimitiveType(EdmPrimitiveTypeKind.Date)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Date?), GetPrimitiveType(EdmPrimitiveTypeKind.Date)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeOfDay), GetPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeOfDay?), GetPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay)),

                // Keep the Binary and XElement in the end, since there are not the default mappings for Edm.Binary and Edm.String.
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(XElement), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Binary), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ushort), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ushort?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(uint), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(uint?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ulong), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ulong?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char[]), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char?), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ICSSoft.STORMNET.UserDataTypes.NullableDateTime), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ICSSoft.STORMNET.UserDataTypes.NullableDecimal), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ICSSoft.STORMNET.UserDataTypes.NullableInt), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
            }
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Преобразует IEdmType в IEdmTypeReference.
        /// </summary>
        /// <param name="edmType">Тип IEdmType.</param>
        /// <param name="isNullable">Возможно ли сущности данного типа присвоить значение null.</param>
        /// <returns>Ссылку на IEdmType.</returns>
        public static IEdmTypeReference ToEdmTypeReference(this IEdmType edmType, bool isNullable)
        {
            if (edmType == null)
            {
                throw new ArgumentNullException(nameof(edmType), "Contract assertion not met: edmType != null");
            }

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Collection:
                    return new EdmCollectionTypeReference(edmType as IEdmCollectionType);
                case EdmTypeKind.Complex:
                    return new EdmComplexTypeReference(edmType as IEdmComplexType, isNullable);
                case EdmTypeKind.Entity:
                    return new EdmEntityTypeReference(edmType as IEdmEntityType, isNullable);
                case EdmTypeKind.EntityReference:
                    return new EdmEntityReferenceTypeReference(edmType as IEdmEntityReferenceType, isNullable);
                case EdmTypeKind.Enum:
                    return new EdmEnumTypeReference(edmType as IEdmEnumType, isNullable);
                case EdmTypeKind.Primitive:
                    return _coreModel.GetPrimitive((edmType as IEdmPrimitiveType).PrimitiveKind, isNullable);
                default:
                    throw Error.NotSupported(SRResources.EdmTypeNotSupported, edmType.ToTraceString());
            }
        }

        /// <summary>
        /// Получает тип C# для данной ссылки на IEdmType в представленной модели.
        /// </summary>
        /// <param name="edmTypeReference">Ссылка на IEdmType.</param>
        /// <param name="edmModel">Модель.</param>
        /// <returns>Тип C#.</returns>
        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel)
        {
            return GetClrType(edmTypeReference, edmModel, _defaultAssemblyResolver);
        }

        /// <summary>
        /// Получает тип C# для данной ссылки на IEdmType в представленной модели.
        /// </summary>
        /// <param name="edmTypeReference">Ссылка на IEdmType.</param>
        /// <param name="edmModel">Модель.</param>
        /// <param name="assembliesResolver">Реализация интерфейса IAssembliesResolver.</param>
        /// <returns>Тип C#.</returns>
        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel, IAssembliesResolver assembliesResolver)
        {
            if (edmTypeReference == null)
            {
                throw Error.ArgumentNull("edmTypeReference");
            }

            Type primitiveClrType = _builtInTypesMapping
                .Where(kvp => edmTypeReference.Definition.IsEquivalentTo(kvp.Value) && (!edmTypeReference.IsNullable || IsNullable(kvp.Key)))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (primitiveClrType != null)
            {
                return primitiveClrType;
            }
            else
            {
                Type clrType = GetClrType(edmTypeReference.Definition, edmModel, assembliesResolver);
                if (clrType != null && clrType.IsEnum && edmTypeReference.IsNullable)
                {
                    return clrType.ToNullable();
                }

                return clrType;
            }
        }

        /// <summary>
        /// Получает тип C# для данной ссылки на IEdmType в представленной модели.
        /// </summary>
        /// <param name="edmType">Тип.</param>
        /// <param name="edmModel">Модель.</param>
        /// <param name="assembliesResolver">Реализация интерфейса IAssembliesResolver.</param>
        /// <returns>Тип C#.</returns>
        public static Type GetClrType(IEdmType edmType, IEdmModel edmModel, IAssembliesResolver assembliesResolver)
        {
            IEdmSchemaType edmSchemaType = edmType as IEdmSchemaType;

            if (edmSchemaType == null)
            {
                throw new ArgumentException("Contract assertion not met: edmSchemaType != null", "value");
            }

            ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmSchemaType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            string typeName = edmSchemaType.FullName();
            IEnumerable<Type> matchingTypes = GetMatchingTypes(typeName, assembliesResolver);

            if (matchingTypes.Count() > 1)
            {
                throw Error.Argument(
                    "edmTypeReference",
                    SRResources.MultipleMatchingClrTypesForEdmType,
                    typeName,
                    string.Join(",", matchingTypes.Select(type => type.AssemblyQualifiedName)));
            }

            edmModel.SetAnnotationValue<ClrTypeAnnotation>(edmSchemaType, new ClrTypeAnnotation(matchingTypes.SingleOrDefault()));

            return matchingTypes.SingleOrDefault();
        }

        /// <summary>
        /// Имя свойства у класса C#.
        /// </summary>
        /// <param name="edmProperty">EDM-свойство.</param>
        /// <param name="edmModel">EDM-модель.</param>
        /// <returns>Свойство.</returns>
        public static string GetClrPropertyName(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            string propertyName = edmProperty.Name;
            ClrPropertyInfoAnnotation annotation = edmModel.GetAnnotationValue<ClrPropertyInfoAnnotation>(edmProperty);
            if (annotation != null)
            {
                PropertyInfo propertyInfo = annotation.ClrPropertyInfo;
                if (propertyInfo != null)
                {
                    propertyName = propertyInfo.Name;
                }
            }

            return propertyName;
        }

        /// <summary>
        /// Возвращает свойство, в котором хранятся динамические свойства для EDM-типа.
        /// </summary>
        /// <param name="edmType">EDM-тип.</param>
        /// <param name="edmModel">EDM-модель.</param>
        /// <returns>Свойство.</returns>
        public static PropertyInfo GetDynamicPropertyDictionary(IEdmStructuredType edmType, IEdmModel edmModel)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            DynamicPropertyDictionaryAnnotation annotation =
                edmModel.GetAnnotationValue<DynamicPropertyDictionaryAnnotation>(edmType);
            if (annotation != null)
            {
                return annotation.PropertyInfo;
            }

            return null;
        }

        /// <summary>
        /// Возвращает EDM-тип для данного примитивного типа.
        /// </summary>
        /// <param name="clrType">Примитивный тип.</param>
        /// <returns>EDM-тип.</returns>
        public static IEdmPrimitiveType GetEdmPrimitiveTypeOrNull(Type clrType)
        {
            IEdmPrimitiveType primitiveType;
            return _builtInTypesMapping.TryGetValue(clrType, out primitiveType) ? primitiveType : null;
        }

        /// <summary>
        /// Возвращает ссылку на примитивный EDM-тип.
        /// </summary>
        /// <param name="clrType">Примитивный тип.</param>
        /// <returns>Ссылка на примитивный EDM-тип.</returns>
        public static IEdmPrimitiveTypeReference GetEdmPrimitiveTypeReferenceOrNull(Type clrType)
        {
            IEdmPrimitiveType primitiveType = GetEdmPrimitiveTypeOrNull(clrType);
            return primitiveType != null ? _coreModel.GetPrimitive(primitiveType.PrimitiveKind, IsNullable(clrType)) : null;
        }

        // figures out if the given clr type is nonstandard edm primitive like uint, ushort, char[] etc.
        // and returns the corresponding clr type to which we map like uint => long.

        /// <summary>
        /// Определяет, является ли тип нестандартным.
        /// </summary>
        /// <param name="type">Примитивный тип.</param>
        /// <param name="isNonstandardEdmPrimitive">Результат проверки.</param>
        /// <returns>Тип.</returns>
        public static Type IsNonstandardEdmPrimitive(Type type, out bool isNonstandardEdmPrimitive)
        {
            IEdmPrimitiveTypeReference edmType = GetEdmPrimitiveTypeReferenceOrNull(type);
            if (edmType == null)
            {
                isNonstandardEdmPrimitive = false;
                return type;
            }

            Type reverseLookupClrType = GetClrType(edmType, EdmCoreModel.Instance);
            isNonstandardEdmPrimitive = type != reverseLookupClrType;

            return reverseLookupClrType;
        }

        public static bool IsNotNavigable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotNavigable;
        }


        // Mangle the invalid EDM literal Type.FullName (System.Collections.Generic.IEnumerable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
        // to a valid EDM literal (the C# type name IEnumerable<int>).
        private static string EdmName(this Type clrType)
        {
            // We cannot use just Type.Name here as it doesn't work for generic types.
            return MangleClrTypeName(clrType);
        }

        private static string EdmFullName(this Type clrType)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", clrType.Namespace, clrType.EdmName());
        }

        private static IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind primitiveKind)
        {
            return _coreModel.GetPrimitiveType(primitiveKind);
        }

        private static bool IsNullable(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static IEnumerable<Type> GetMatchingTypes(string edmFullName, IAssembliesResolver assembliesResolver)
        {
            return GetLoadedTypes(assembliesResolver).Where(t => t.IsPublic && t.EdmFullName() == edmFullName);
        }

        // TODO (workitem 336): Support nested types and anonymous types.
        private static string MangleClrTypeName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "Contract assertion not met: type != null");
            }

            if (!type.IsGenericType)
            {
                return type.Name;
            }
            else
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}Of{1}",
                    type.Name.Replace('`', '_'),
                    string.Join("_", type.GetGenericArguments().Select(t => MangleClrTypeName(t))));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching all exceptions in this case is the right to do.")]

        private static IEnumerable<Type> GetLoadedTypes(IAssembliesResolver assembliesResolver)
        {
            List<Type> result = new List<Type>();

            // Go through all assemblies referenced by the application and search for types matching a predicate
            ICollection<Assembly> assemblies = assembliesResolver.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] exportedTypes = null;
                if (assembly == null || assembly.IsDynamic)
                {
                    // can't call GetTypes on a null (or dynamic?) assembly
                    continue;
                }

                try
                {
                    exportedTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    continue;
                }

                if (exportedTypes != null)
                {
                    result.AddRange(exportedTypes.Where(t => t != null && t.IsVisible));
                }
            }

            return result;
        }

        private static QueryableRestrictionsAnnotation GetPropertyRestrictions(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            if (edmProperty == null)
            {
                throw new ArgumentNullException(nameof(edmProperty), "Contract assertion not met: edmProperty != null");
            }

            if (edmModel == null)
            {
                throw new ArgumentNullException(nameof(edmModel), "Contract assertion not met: edmModel != null");
            }

            return edmModel.GetAnnotationValue<QueryableRestrictionsAnnotation>(edmProperty);
        }

    }
}
