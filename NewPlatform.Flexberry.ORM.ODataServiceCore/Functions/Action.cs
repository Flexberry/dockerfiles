namespace NewPlatform.Flexberry.ORM.ODataService.Functions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class of user-defined OData action.
    /// </summary>
    public class Action : Function
    {
        /// <inheritdoc /> 
        public Action(string actionName, DelegateODataFunction handler, Type returnType, Dictionary<string, Type> parametersTypes = null)
            : base(actionName, handler, returnType, parametersTypes)
        {
        }

        /// <inheritdoc /> 
        public Action(string actionName, DelegateODataFunction handler, Dictionary<string, Type> parametersTypes = null)
            : this(actionName, handler, typeof(void), parametersTypes)
        {
        }

    }
}
