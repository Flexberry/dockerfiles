namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Read.Excel
{
    using System.Collections.Specialized;
    using System.Linq;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using NewPlatform.Flexberry.Reports.ExportToExcel;

    /// <summary>
    /// Реализация интерфейса для обработки документа Excel.
    /// </summary>
    public class SpreadsheetCustomizer : ISpreadsheetCustomizer
    {
        /// <summary>
        /// Обработка документа Excel.
        /// </summary>
        /// <param name="document">Документ Excel.</param>
        /// <param name="parameters">Параметры экпорта.</param>
        /// <param name="queryParams">Параметры в запросе HTTP.</param>
        public void Process(ref SpreadsheetDocument document, IExportParams parameters = null, NameValueCollection queryParams = null)
        {
            var worksheetPart = document.WorkbookPart.GetPartsOfType<WorksheetPart>().First();
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            var row = new Row();
            string query = null;
            foreach (var key in queryParams.AllKeys)
            {
                if (query != null)
                {
                    query += "&";
                }

                query += $"{key}={queryParams[key]}";
            }

            var excelDataType = new ExcelTypeDefinition(CellValues.String, CustomStylesheet.StyleIndexTextAllBordersWrapAlignment);
            var cell = CreateCell(query, excelDataType);
            row.AppendChild(cell);
            sheetData.AppendChild(row);
            return;
        }

        /// <summary>
        /// Создание объекта ячейки Excel с указанным значение и форматом.
        /// </summary>
        /// <param name="value">Значение ячейки.</param>
        /// <param name="excelDataType">Формат ячейки.</param>
        /// <returns>Объект Cell.</returns>
        private static Cell CreateCell(string value, ExcelTypeDefinition excelDataType)
        {
            var headerCell = new Cell { CellValue = new CellValue(value), StyleIndex = excelDataType.StyleIndex };

            if (excelDataType.CellValues != CellValues.Date)
            {
                headerCell.DataType = new EnumValue<CellValues>(excelDataType.CellValues);
            }

            return headerCell;
        }
    }
}
