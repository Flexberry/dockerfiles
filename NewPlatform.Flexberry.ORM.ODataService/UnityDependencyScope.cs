namespace NewPlatform.Flexberry.ORM.ODataService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Dependencies;

    using Microsoft.Practices.Unity;

    /// <summary>
    /// WebAPI <see cref="IDependencyScope"/> based on Unity.
    /// </summary>
    /// <seealso cref="System.Web.Http.Dependencies.IDependencyScope" />
    public class UnityDependencyScope : IDependencyScope
    {
        /// <summary>
        /// The Unity DI container.
        /// </summary>
        protected IUnityContainer Container { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityDependencyScope"/> class.
        /// </summary>
        /// <param name="container">The Unity DI container.</param>
        public UnityDependencyScope(IUnityContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null);

            Container = container;
        }

        /// <summary>
        /// Retrieves a service from the scope.
        /// </summary>
        /// <param name="serviceType">The service to be retrieved.</param>
        /// <returns>The retrieved service or <c>null</c>.</returns>
        public object GetService(Type serviceType)
        {
            try
            {
                return Container.Resolve(serviceType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves a collection of services from the scope.
        /// </summary>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        /// <returns>
        /// The retrieved collection of services.
        /// </returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Container.ResolveAll(serviceType);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Container.Dispose();
        }
    }
}