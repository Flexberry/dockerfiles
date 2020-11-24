namespace NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers
{
    using System;
    using System.Web.Http;

    using ICSSoft.STORMNET.Business;

    using NewPlatform.Flexberry.ORM.ODataService.Files;

    /// <summary>
    /// WebApi-контроллер, предназначенный для загрузки файлов на сервер и скачивания ранее загруженных файлов.
    /// </summary>
    public partial class FileController : ApiController
    {
        /// <summary>
        /// Получает или задает URL, по которому доступен контроллер.
        /// </summary>
        public static string RouteName { get; internal set; }

        private readonly IDataObjectFileAccessor dataObjectFileAccessor;

        private readonly IDataService dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileController"/> class.
        /// </summary>
        /// <param name="dataObjectFileAccessor">The data object file properties accessor.</param>
        /// <param name="dataService">The data service for all manipulations with data.</param>
        public FileController(IDataObjectFileAccessor dataObjectFileAccessor, IDataService dataService)
        {
            this.dataObjectFileAccessor = dataObjectFileAccessor ?? throw new ArgumentNullException(nameof(dataObjectFileAccessor));
            this.dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }
    }
}