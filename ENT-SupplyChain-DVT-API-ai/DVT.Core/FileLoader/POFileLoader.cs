using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using static DVT.Core.Constants;

namespace DVT.Core.FileLoader
{
    public class POFileLoader : IFileLoader
    {
        public FileLoadResult LoadFile(FileLoadRequest request)
        {
            var result = new FileLoadResult();

            try
            {
                var loadedFile = ParsePOFileDataWithSpans(request);
                result.JobFileId = request.JobFileId;
                result.Operation = "LoadFile: " + request.FileName;
                result.Success = true;
                result.Message = $"Uploaded {request.FileContent.Count} rows from {request.FileName}";
                result.FileHeader = loadedFile.FileHeader.Where(header => !string.IsNullOrWhiteSpace(header)).ToList();
                result.DataRows = loadedFile.DataRows;
            }
            catch (Exception ex)
            {
                result.Operation = "LoadFile: " + request.FileName;
                result.Success = false;
                result.Message = $"Errors loading file: {ex.Message}";
            }
            return result;
        }

        public async Task<FileLoadResult> LoadFileAsync(FileLoadRequest request, IStorageService storageService)
        {
            var result = new FileLoadResult();
            try
            {
                request.FlatFileContent = await storageService.GetWorkingFileContentsAsync(request.JobId, request.FileName);

                if (string.IsNullOrWhiteSpace(request.FlatFileContent))
                {
                    throw new ArgumentException(StardardMessages.FileContentIsEmpty, nameof(request.FilePath));
                }

                var loadedFile = ParsePOFileDataWithSpans(request);
                result.JobFileId = request.JobFileId;
                result.Operation = "LoadFile: " + request.FileName;
                result.Success = true;
                result.Message = $"Uploaded {loadedFile.DataRows.Count} rows from {request.FileName}";
                result.FileHeader = loadedFile.FileHeader.Where(header => !string.IsNullOrWhiteSpace(header)).ToList();
                result.DataRows = loadedFile.DataRows;
            }
            catch (Exception ex)
            {
                result.Operation = "LoadFile: " + request.FileName;
                result.Success = false;
                result.Message = $"Errors loading file: {ex.Message}";
            }
            return result;
        }

        private IJobFileModel ParsePOFileDataWithSpans(FileLoadRequest request)
        {
            JobFileModel poDataFile = new JobFileModel()
            {
                JobFileId = request.JobFileId,
                FileType = Constants.FileTypes.Po,
                FileName = request.FileName,
                DataRows = new List<IDataRow>()
            };

            var parser = new EfficientCsvParser(request.FlatFileContent);
            int rowNumber = 0;

            DateTime? orderDate = null;
            DateTime? latestAmendment = null;
            string orderDateStr = "";
            string orderDateError = "";
            string latestAmendmentStr = "";
            string latestAmendmentError = "";

            while (parser.TryReadRow(out var row))
            {
                if (rowNumber == 0)
                {
                    poDataFile.FileHeader.AddRange(row.GetAsSplitStringList());
                    rowNumber++;
                    continue;
                }

                //If the row is empty, create an empty object and move on
                if (row.IsEmptyLine)
                {
                    poDataFile.DataRows.Add(new PODataRow());
                    rowNumber++;
                    continue;
                }

                orderDate = null;
                latestAmendment = null;
                orderDateStr = "";
                orderDateError = "";
                latestAmendmentStr = "";
                latestAmendmentError = "";

                orderDate = DataConverter.ParseNullableDate(row.GetFieldOrDefault(POColumns.OrderDate).Trim().ToString(), ref orderDateError, ref orderDateStr);
                latestAmendment = DataConverter.ParseNullableDate(row.GetFieldOrDefault(POColumns.LatestAmendment).Trim().ToString(), ref latestAmendmentError, ref latestAmendmentStr);

                //Data Rows
                poDataFile.DataRows.Add(new PODataRow
                {
                    RowNumber = rowNumber,
                    DivisionID = row.GetFieldOrDefault(POColumns.DivisionId).Trim().ToString(),
                    LocalSiteID = row.GetFieldOrDefault(POColumns.LocalSiteId).Trim().ToString(),
                    PONumber = row.GetFieldOrDefault(POColumns.PONumber).Trim().ToString(),
                    OrderDate = orderDate,
                    OrderDateStr = orderDateStr,
                    OrderDateError = orderDateError,
                    LatestAmendment = latestAmendment,
                    LatestAmendmentStr = latestAmendmentStr,
                    LatestAmendmentError = latestAmendmentError,
                    CommodityMGRId = row.GetFieldOrDefault(POColumns.CommodityMgrId).Trim().ToString(),
                    SupplierID = row.GetFieldOrDefault(POColumns.SupplierId).Trim().ToString(),
                    CurrencyCode = row.GetFieldOrDefault(POColumns.CurrencyCode).Trim().ToString(),
                    POType = row.GetFieldOrDefault(POColumns.POType).Trim().ToString(),
                    IntraDiv = row.GetFieldOrDefault(POColumns.IntraDiv).Trim().ToString(),
                    DirectIndirect = row.GetFieldOrDefault(POColumns.DirectIndirect).Trim().ToString(),
                    POTerms = row.GetFieldOrDefault(POColumns.POTerms).Trim().ToString(),
                    FreightTerms = row.GetFieldOrDefault(POColumns.FreightTerms).Trim().ToString(),
                    EDI = row.GetFieldOrDefault(POColumns.EDI).Trim().ToString(),
                    OrderStatus = row.GetFieldOrDefault(POColumns.OrderStatus).Trim().ToString(),
                    TitleTransfer = row.GetFieldOrDefault(POColumns.TitleTransfer).Trim().ToString(),
                    Port = row.GetFieldOrDefault(POColumns.Port).Trim().ToString(),
                    IncorrectColumnCount = row.FieldCount != POColumns.TotalColumns
                });
                rowNumber++;
            }

            return poDataFile;
        }

        private static class POColumns
        {
            public const int DivisionId = 0;
            public const int LocalSiteId = 1;
            public const int PONumber = 2;
            public const int OrderDate = 3;
            public const int LatestAmendment = 4;
            public const int CommodityMgrId = 5;
            public const int SupplierId = 6;
            public const int CurrencyCode = 7;
            public const int POType = 8;
            public const int IntraDiv = 9;
            public const int DirectIndirect = 10;
            public const int POTerms = 11;
            public const int FreightTerms = 12;
            public const int EDI = 13;
            public const int OrderStatus = 14;
            public const int TitleTransfer = 15;
            public const int Port = 16;
            public const int TotalColumns = 17;
        }
    }
}
