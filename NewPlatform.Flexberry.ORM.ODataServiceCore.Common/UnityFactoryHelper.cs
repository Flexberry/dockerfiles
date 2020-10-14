namespace ICSSoft.Services
{
    using System;
    using Unity;

    /// <summary>
    /// Helper class for <see cref="UnityFactory"/>.
    /// </summary>
    public static class UnityFactoryHelper
    {
        /// <summary>
        /// Resolves the specified type in a singleton instance of Unity container, if an instance in the 'value' parameter is null.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="value">The pre-resolved instance.</param>
        /// <returns>
        /// A resolved instance of the specified type, if an instance in the 'value' parameter is null,
        /// otherwise an instance in the 'value' parameter.
        /// </returns>
        public static T ResolveIfNull<T>(T value) where T : class
        {
            T result = value;
            if (result != null)
            {
                return result;
            }

            IUnityContainer unityContainer = UnityFactory.GetContainer();
            if (unityContainer != null)
            {
                if (unityContainer.IsRegistered<T>())
                {
                    result = unityContainer.Resolve<T>();
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves the specified type in a singleton instance of Unity container, if an instance in the 'value' parameter is null.
        /// If no instance to return, throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="value">The pre-resolved instance.</param>
        /// <returns>
        /// A resolved instance of the specified type, if an instance in the 'value' parameter is null,
        /// otherwise an instance in the 'value' parameter.
        /// If no instance to return, throws <see cref="InvalidOperationException"/>.
        /// </returns>
        public static T ResolveRequiredIfNull<T>(T value) where T : class
        {
            return ResolveIfNull(value) ?? throw new InvalidOperationException($"The '{typeof(T).Name}' type is not resolved by Unity.");
        }
    }
}
