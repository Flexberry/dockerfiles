namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers
{
    using ICSSoft.Services;
    using ICSSoft.STORMNET.Business;
    using Microsoft.AspNetCore.Mvc;
    using NewPlatform.Flexberry.ORM.ODataService.Files;

    /// <summary>
    /// WebApi-контроллер, предназначенный для загрузки файлов на сервер и скачивания ранее загруженных файлов.
    /// </summary>
    [Produces("application/json")]
    public partial class FileController : ControllerBase
    {
        private readonly IDataObjectFileAccessor dataObjectFileAccessor;

        private readonly IDataService dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileController"/> class.
        /// </summary>
        /// <param name="dataObjectFileAccessor">The data object file properties accessor.</param>
        /// <param name="dataService">The data service for all manipulations with data.</param>
        public FileController(IDataObjectFileAccessor dataObjectFileAccessor, IDataService dataService = null)
            : base ()
        {
            this.dataObjectFileAccessor = dataObjectFileAccessor;
    
            this.dataService = UnityFactoryHelper.ResolveRequiredIfNull(dataService);
        }
    }
}
