namespace NewPlatform.Flexberry.ORM.ODataService
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Routing;

    using NewPlatform.Flexberry.ORM.ODataService.Events;
    using NewPlatform.Flexberry.ORM.ODataService.Functions;
    using NewPlatform.Flexberry.ORM.ODataService.Model;

    public sealed class ManagementToken
    {
        private DataObjectEdmModel _model;

        public DataObjectEdmModel Model
        {
            get
            {
                return _model;
            }

            set
            {
                Contract.Requires<ArgumentNullException>(value != null);

                if (_model == value)
                    return;

                _model = value;

                foreach (var userFunction in Functions.GetFunctions())
                {
                    _model.AddUserFunction(userFunction);
                }
            }
        }

        public ODataRoute Route { get; }

        public IEventHandlerContainer Events { get; } = new EventHandlerContainer();

        public IFunctionContainer Functions { get; }

        public ManagementToken(ODataRoute route, DataObjectEdmModel model)
        {
            Contract.Requires<ArgumentNullException>(route != null);
            Contract.Requires<ArgumentNullException>(model != null);

            Route = route;
            _model = model;
            Functions = new FunctionContainer(this);
        }
    }
}
