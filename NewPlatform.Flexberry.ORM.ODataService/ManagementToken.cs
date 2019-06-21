namespace NewPlatform.Flexberry.ORM.ODataService
{
    using System;
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
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Contract assertion not met: value != null");
                }

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
            Route = route ?? throw new ArgumentNullException(nameof(route), "Contract assertion not met: route != null");
            _model = model ?? throw new ArgumentNullException(nameof(model), "Contract assertion not met: model != null");
            Functions = new FunctionContainer(this);
        }
    }
}
