namespace NewPlatform.Flexberry.ORM.ODataService
{
    using System.Web.Http.Dependencies;

    using Microsoft.Practices.Unity;

    /// <summary>
    /// WebAPI <see cref="IDependencyResolver"/> based on Unity.
    /// </summary>
    /// <seealso cref="NewPlatform.Flexberry.ORM.ODataService.UnityDependencyScope" />
    /// <seealso cref="System.Web.Http.Dependencies.IDependencyResolver" />
    public class UnityDependencyResolver : UnityDependencyScope, IDependencyResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The Unity DI container.</param>
        public UnityDependencyResolver(IUnityContainer container)
            : base(container)
        {
        }

        /// <summary>
        /// Starts a resolution scope.
        /// </summary>
        /// <returns>The dependency scope.</returns>
        public IDependencyScope BeginScope()
        {
            var childContainer = Container.CreateChildContainer();

            return new UnityDependencyScope(childContainer);
        }
    }
}