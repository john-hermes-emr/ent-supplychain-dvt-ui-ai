using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DVT.Core.Helper
{
    public static class ExcelHelper
    {
        public static byte[] ExportToExcel(List<string> fileInfos, List<string> headers, List<dynamic> dataObjs)
        {
            byte[] byteResult = null;
            if (headers == null) { return byteResult; }

            using (MemoryStream stream = new MemoryStream())
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();

                    worksheetPart.Worksheet = new Worksheet();

                    SheetData sheetData = new SheetData();

                    var stylesPart = spreadsheetDocument.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                    Stylesheet Stylesheet = new Stylesheet();
                    stylesPart.Stylesheet = Stylesheet;

                    //Append a new worksheet and associate it with the workbook.
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1",
                    };

                    Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                    sheets.Append(sheet);

                    //row start index is 1
                    int rowIndex = 1;

                    if (fileInfos != null && fileInfos.Count > 0)
                    {
                        Row fileInfoRow = new Row() { RowIndex = Convert.ToUInt32(rowIndex) };
                        for (int n = 0; n < fileInfos.Count; n++)
                        {
                            Cell cellHeader = new Cell() { CellValue = new CellValue(fileInfos[n]), DataType = CellValues.String };
                            fileInfoRow.Append(cellHeader);
                        }
                        sheetData.Append(fileInfoRow);
                        rowIndex++;
                    }

                    // set width of columns
                    Columns columns = new Columns();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        Column column = new Column()
                        {
                            Min = (uint)(i + 1),
                            Max = (uint)(i + 1),
                            Width = 30,
                            CustomWidth = true,
                        };
                        columns.Append(column);
                    }

                    worksheetPart.Worksheet.Append(columns);

                    //Write Header
                    Row header = new Row() { RowIndex = Convert.ToUInt32(rowIndex) };
                    var columnsCount = headers.Count;
                    for (int n = 0; n < columnsCount; n++)
                    {
                        Cell cellHeader = new Cell() { CellValue = new CellValue(headers[n]), DataType = CellValues.String };
                        header.Append(cellHeader);
                    }

                    sheetData.Append(header);
                    ++rowIndex;

                    if (dataObjs == null || dataObjs.Count == 0)
                    {
                        SetNoRows(rowIndex, sheetData);
                    }
                    else
                    {
                        //Write Body
                        foreach (var item in dataObjs)
                        {
                            Row Sheetrow = new Row() { RowIndex = Convert.ToUInt32(rowIndex) };
                            var data = (IDictionary<string, object>)item;

                            foreach (var name in data.Keys)
                            {
                                var value = data[name] == null ? "" : data[name].ToString();
                                Cell cell = new Cell() { CellValue = new CellValue(value), DataType = CellValues.String, };

                                Sheetrow.Append(cell);
                            }
                            sheetData.Append(Sheetrow);
                            ++rowIndex;
                        }
                    }

                    worksheetPart.Worksheet.Append(sheetData);

                    workbookpart.Workbook.Save();
                }

                stream.Flush();

                stream.Position = 0;

                byteResult = new byte[stream.Length];
                stream.Read(byteResult, 0, byteResult.Length);
            }

            return byteResult;
        }

        private const string noRows = "No rows";

        private static void SetNoRows(int rowIndexCount, SheetData sheetData)
        {
            Row row = new Row() { RowIndex = Convert.ToUInt32(rowIndexCount) };
            Cell cellHeader = new Cell() { CellValue = new CellValue(noRows), DataType = CellValues.String };

            row.Append(cellHeader);
            sheetData.Append(row);
        }
    }
}
