namespace NewPlatform.Flexberry.ORM.ODataService.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Default implementation of <see cref="IFunctionContainer"/>.
    /// Uses <see cref="ManagementToken"/> for updating current EDM model after registering OData Service function.
    /// </summary>
    /// <seealso cref="IFunctionContainer" />
    internal class FunctionContainer : IFunctionContainer
    {
        /// <summary>
        /// The OData Service token.
        /// </summary>
        private readonly ManagementToken _token;

        /// <summary>
        /// The registered OData Service functions.
        /// </summary>
        private readonly Dictionary<string, Function> _functions = new Dictionary<string, Function>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionContainer"/> class.
        /// </summary>
        /// <param name="token">The OData Service token.</param>
        public FunctionContainer(ManagementToken token)
        {
            _token = token ?? throw new ArgumentNullException(nameof(token), "Contract assertion not met: token != null");
        }

        /// <summary>
        /// Registers the specified OData Service function.
        /// </summary>
        /// <param name="function">The OData Service function.</param>
        public void Register(Function function)
        {
            _functions.Add(function.Name, function);
            _token.Model.AddUserFunction(function);
        }

        /// <summary>
        /// Registers the specified delegate as OData Service function.
        /// </summary>
        /// <param name="function">The function.</param>
        public void Register(Delegate function)
        {
            Register(function, false);
        }

        /// <summary>
        /// Registers the specified delegate as OData Service action.
        /// </summary>
        /// <param name="function">The function.</param>
        public void RegisterAction(Delegate function)
        {
            Register(function, true);
        }

        private void Register(Delegate function, bool createAction)
        {
            var functionName = function.Method.Name;
            var returnType = function.Method.ReturnType;

            var skip = 0;
            var parameters = function.Method.GetParameters();
            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(QueryParameters))
            {
                skip = 1;
            }

            var arguments = parameters.Skip(skip).ToDictionary(i => i.Name, i => i.ParameterType);

            DelegateODataFunction handler = (queryParameters, objects) =>
            {
                var args = new List<object>();
                if (skip == 1)
                {
                    args.Add(queryParameters);
                }

                return function.DynamicInvoke(args.Concat(objects.Values).ToArray());
            };
            if (createAction)
            {
                Register(new Action(functionName, handler, returnType, arguments));
            }
            else
            {
                Register(new Function(functionName, handler, returnType, arguments));
            }
        }

        /// <summary>
        /// Determines whether the specified OData Service function is already registered.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <returns>Returns <c>true</c> if function is registered; otherwise <c>false</c>.</returns>
        public bool IsRegistered(string functionName)
        {
            return _functions.ContainsKey(functionName);
        }

        /// <summary>
        /// Gets the registered OData Service function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>Registered OData Service function with specified name.</returns>
        public Function GetFunction(string functionName)
        {
            return _functions[functionName];
        }

        /// <summary>
        /// Gets all registered functions.
        /// </summary>
        /// <returns>Enumeration of all registered functions.</returns>
        public IEnumerable<Function> GetFunctions()
        {
            return _functions.Values;
        }
    }
}