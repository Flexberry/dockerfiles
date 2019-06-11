// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace NewPlatform.Flexberry.ORM.ODataService.Formatter
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Web.Http;
    using System.Web.OData.Properties;
    using Microsoft.OData.Edm;
    using System;
    using NewPlatform.Flexberry.ORM.ODataService.Expressions;
    using System.Web.OData;

    internal static class CollectionDeserializationHelpers
    {
        private static readonly Type[] _emptyTypeArray = new Type[0];
        private static readonly object[] _emptyObjectArray = new object[0];
        private static readonly MethodInfo _toArrayMethodInfo = typeof(Enumerable).GetMethod("ToArray");

        public static void AddToCollection(this IEnumerable items, IEnumerable collection, Type elementType,
            Type resourceType, string propertyName, Type propertyType)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items), "Contract assertion not met: items != null");
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "Contract assertion not met: collection != null");
            }

            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(elementType), "Contract assertion not met: elementType != null");
            }

            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType), "Contract assertion not met: resourceType != null");
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName), "Contract assertion not met: propertyName != null");
            }

            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType), "Contract assertion not met: propertyType != null");
            }

            MethodInfo addMethod = null;
            IList list = collection as IList;

            if (list == null)
            {
                addMethod = collection.GetType().GetMethod("Add", new Type[] { elementType });
                if (addMethod == null)
                {
                    string message = Error.Format(SRResources.CollectionShouldHaveAddMethod, propertyType.FullName, propertyName, resourceType.FullName);
                    throw new SerializationException(message);
                }
            }
            else if (list.GetType().IsArray)
            {
                string message = Error.Format(SRResources.GetOnlyCollectionCannotBeArray, propertyName, resourceType.FullName);
                throw new SerializationException(message);
            }

            items.AddToCollectionCore(collection, elementType, list, addMethod);
        }

        public static void AddToCollection(this IEnumerable items, IEnumerable collection, Type elementType, string paramName, Type paramType)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items), "Contract assertion not met: items != null");
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "Contract assertion not met: collection != null");
            }

            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(elementType), "Contract assertion not met: elementType != null");
            }

            if (paramType == null)
            {
                throw new ArgumentNullException(nameof(paramType), "Contract assertion not met: paramType != null");
            }

            MethodInfo addMethod = null;
            IList list = collection as IList;

            if (list == null)
            {
                addMethod = collection.GetType().GetMethod("Add", new Type[] { elementType });
                if (addMethod == null)
                {
                    string message = Error.Format(SRResources.CollectionParameterShouldHaveAddMethod, paramType, paramName);
                    throw new SerializationException(message);
                }
            }

            items.AddToCollectionCore(collection, elementType, list, addMethod);
        }

        private static void AddToCollectionCore(this IEnumerable items, IEnumerable collection, Type elementType, IList list, MethodInfo addMethod)
        {
            bool isNonstandardEdmPrimitiveCollection;
            EdmLibHelpers.IsNonstandardEdmPrimitive(elementType, out isNonstandardEdmPrimitiveCollection);

            foreach (object item in items)
            {
                object element = item;

                if (isNonstandardEdmPrimitiveCollection && element != null)
                {
                    // convert non-standard edm primitives if required.
                    element = EdmPrimitiveHelpers.ConvertPrimitiveValue(element, elementType);
                }

                if (list != null)
                {
                    list.Add(element);
                }
                else
                {
                    if (addMethod == null)
                    {
                        throw new ArgumentNullException(nameof(addMethod), "Contract assertion not met: addMethod != null");
                    }

                    addMethod.Invoke(collection, new object[] { element });
                }
            }
        }

        public static void Clear(this IEnumerable collection, string propertyName, Type resourceType)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "Contract assertion not met: collection != null");
            }

            MethodInfo clearMethod = collection.GetType().GetMethod("Clear", _emptyTypeArray);
            if (clearMethod == null)
            {
                string message = Error.Format(SRResources.CollectionShouldHaveClearMethod, collection.GetType().FullName,
                    propertyName, resourceType.FullName);
                throw new SerializationException(message);
            }

            clearMethod.Invoke(collection, _emptyObjectArray);
        }

        public static bool TryCreateInstance(Type collectionType, IEdmCollectionTypeReference edmCollectionType, Type elementType, out IEnumerable instance)
        {
            if (collectionType == null)
            {
                throw new ArgumentNullException(nameof(collectionType), "Contract assertion not met: collectionType != null");
            }

            if (collectionType == typeof(EdmComplexObjectCollection))
            {
                instance = new EdmComplexObjectCollection(edmCollectionType);
                return true;
            }
            else if (collectionType == typeof(EdmEntityObjectCollection))
            {
                instance = new EdmEntityObjectCollection(edmCollectionType);
                return true;
            }
            else if (collectionType == typeof(EdmEnumObjectCollection))
            {
                instance = new EdmEnumObjectCollection(edmCollectionType);
                return true;
            }
            else if (collectionType.IsGenericType)
            {
                Type genericDefinition = collectionType.GetGenericTypeDefinition();
                if (genericDefinition == typeof(IEnumerable<>) ||
                    genericDefinition == typeof(ICollection<>) ||
                    genericDefinition == typeof(IList<>))
                {
                    instance = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IEnumerable;
                    return true;
                }
            }

            if (collectionType.IsArray)
            {
                // We dont know the size of the collection in advance. So, create a list and later call ToArray. 
                instance = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IEnumerable;
                return true;
            }

            if (collectionType.GetConstructor(Type.EmptyTypes) != null && !collectionType.IsAbstract)
            {
                instance = Activator.CreateInstance(collectionType) as IEnumerable;
                return true;
            }

            instance = null;
            return false;
        }

        public static IEnumerable ToArray(IEnumerable value, Type elementType)
        {
            return _toArrayMethodInfo.MakeGenericMethod(elementType).Invoke(null, new object[] { value }) as IEnumerable;
        }
    }
}
