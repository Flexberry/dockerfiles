namespace NewPlatform.Flexberry.ORM.ODataService.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Class of user-defined OData function.
    /// </summary>
    public class Function
    {
        /// <summary>
        /// The name of the function.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The handler for the function.
        /// </summary>
        public DelegateODataFunction Handler { get; set; }

        /// <summary>
        /// The return type of the function.
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        /// The arguments of the function.
        /// </summary>
        public Dictionary<string, Type> ParametersTypes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Function"/> class.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="handler">The handler for the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <param name="parametersTypes">The arguments of the function.</param>
        public Function(string functionName, DelegateODataFunction handler, Type returnType, Dictionary<string, Type> parametersTypes = null)
        {
            Contract.Requires<ArgumentNullException>(functionName != null);
            Contract.Requires<ArgumentException>(functionName != string.Empty);
            Contract.Requires<ArgumentNullException>(handler != null);
            Contract.Requires<ArgumentNullException>(returnType != null);

            Name = functionName;
            Handler = handler;
            ReturnType = returnType;

            if (parametersTypes == null)
                parametersTypes = new Dictionary<string, Type>();

            ParametersTypes = parametersTypes;
        }
    }
}