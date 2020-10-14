namespace NewPlatform.Flexberry.ORM.ODataService.Functions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface of container with OData Service functions.
    /// </summary>
    public interface IFunctionContainer
    {
        /// <summary>
        /// Registers the specified OData Service function.
        /// </summary>
        /// <param name="function">The function.</param>
        void Register(Function function);

        /// <summary>
        /// Registers the specified delegate as OData Service function.
        /// </summary>
        /// <param name="function">The function.</param>
        void Register(Delegate function);

        /// <summary>
        /// Registers the specified delegate as OData Service action.
        /// </summary>
        /// <param name="function">The function.</param>
        void RegisterAction(Delegate function);

        /// <summary>
        /// Determines whether the specified OData Service function is already registered.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <returns>Returns <c>true</c> if function is registered; otherwise <c>false</c>.</returns>
        bool IsRegistered(string functionName);

        /// <summary>
        /// Gets the registered OData Service function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>Registered OData Service function with specified name.</returns>
        Function GetFunction(string functionName);

        /// <summary>
        /// Gets all registered functions.
        /// </summary>
        /// <returns>Enumeration of all registered functions.</returns>
        IEnumerable<Function> GetFunctions();
    }
}