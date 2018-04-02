namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read.Excel
{
    using System.Collections.Specialized;
    using System.IO;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using NewPlatform.Flexberry.Reports.ExportToExcel;

    /// <summary>
    /// Реализация интерфейса для экспорта данных из ODataService.
    /// </summary>
    public class ExportExcel : IODataExportService
    {
        /// <summary>
        /// Создаёт файл экспорта  данных из ORM.
        /// </summary>
        /// <param name="dataService">Сервис данных ORM.</param>
        /// <param name="parameters">Параметры экпорта.</param>
        /// <param name="objs">Объекты для экспорта.</param>
        /// <param name="queryParams">Параметры в запросе HTTP.</param>
        /// <returns>Возвращает файл экспорта в виде MemoryStream.</returns>
        public MemoryStream CreateExportStream(IDataService dataService, IExportParams parameters, DataObject[] objs, NameValueCollection queryParams)
        {
            var exportService = new DataExportLimitedService(parameters, dataService);
            var dataExport = exportService.GetDataExport(objs);
            var excelService = new ExcelExportService(parameters, queryParams);
            excelService.SpreadsheetCustomizer = new SpreadsheetCustomizer();
            MemoryStream result = excelService.GetExportStream(dataExport);
            return result;
        }
    }

}
