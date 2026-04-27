using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using static DVT.Core.Constants;

namespace DVT.Core.FileLoader
{
    public class InventoryFileLoader : IFileLoader
    {
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

                var loadedFile = ParseInventoryFileDataWithSpans(request);
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

        private IJobFileModel ParseInventoryFileDataWithSpans(FileLoadRequest request)
        {
            JobFileModel inventoryDataFile = new JobFileModel()
            {
                JobFileId = request.JobFileId,
                FileType = Constants.FileTypes.Inventory,
                FileName = request.FileName,
                DataRows = new List<IDataRow>()
            };

            var parser = new EfficientCsvParser(request.FlatFileContent);
            int rowNumber = 0;
            
            BigDecimal? quantity = null;
            ErrorTypes quantityError = ErrorTypes.None;
            string quantityOriginalStr = "";
            BigDecimal? standardCost = null;
            ErrorTypes standardCostError = ErrorTypes.None;
            string standardCostOriginalStr = "";
            BigDecimal? totalValue = null;
            ErrorTypes totalValueError = ErrorTypes.None;
            string totalValueOriginalStr = "";
            DateTime? inventoryDate = null;
            string inventoryDateStr = "";
            string inventoryDateError = "";

            while (parser.TryReadRow(out var row))
            {
                if (rowNumber == 0)
                {
                    inventoryDataFile.FileHeader.AddRange(row.GetAsSplitStringList());
                    rowNumber++;
                    continue;
                }

                //If the row is empty, create an empty object and move on
                if (row.IsEmptyLine)
                {
                    inventoryDataFile.DataRows.Add(new InventoryDataRow());
                    rowNumber++;
                    continue;
                }

                quantityError = ErrorTypes.None;
                standardCostError = ErrorTypes.None;
                totalValueError = ErrorTypes.None;
                inventoryDateStr = "";
                inventoryDateError = "";
                quantityOriginalStr = row.GetFieldOrDefault(InventoryColumns.Quantity).Trim().ToString();
                standardCostOriginalStr = row.GetFieldOrDefault(InventoryColumns.StandardCost).Trim().ToString();
                totalValueOriginalStr = row.GetFieldOrDefault(InventoryColumns.TotalValue).Trim().ToString();

                quantity = DataConverter.ParseNullableBigDecimal(quantityOriginalStr, NumberTypeCharacterLengthLimit.ThirtyEightCharacters, ref quantityError);
                standardCost = DataConverter.ParseNullableBigDecimal(standardCostOriginalStr, NumberTypeCharacterLengthLimit.ThirtyEightCharacters, ref standardCostError);
                totalValue = DataConverter.ParseNullableBigDecimal(totalValueOriginalStr, NumberTypeCharacterLengthLimit.ThirtyEightCharacters, ref totalValueError);
                inventoryDate = DataConverter.ParseNullableDate(row.GetFieldOrDefault(InventoryColumns.InventoryDate).Trim().ToString(), ref inventoryDateError, ref inventoryDateStr);

                inventoryDataFile.DataRows.Add(new InventoryDataRow
                {
                    RowNumber = rowNumber,
                    DivisionId = row.GetFieldOrDefault(InventoryColumns.DivisionId).Trim().ToString(),
                    LocalSiteId = row.GetFieldOrDefault(InventoryColumns.LocalSiteId).Trim().ToString(),
                    PartNumber = row.GetFieldOrDefault(InventoryColumns.PartNumber).Trim().ToString(),
                    QuantityOriginalStr = quantityOriginalStr,
                    Quantity = quantity,
                    QuantityError = quantityError,
                    StandardCostOriginalStr = standardCostOriginalStr,
                    StandardCost = standardCost,
                    StandardCostError = standardCostError,
                    TotalValueOriginalStr = totalValueOriginalStr,
                    TotalValue = totalValue,
                    TotalValueError = totalValueError,
                    UOM = row.GetFieldOrDefault(InventoryColumns.UOM).Trim().ToString(),
                    CurrencyCode = row.GetFieldOrDefault(InventoryColumns.CurrencyCode).Trim().ToString(),
                    PartStatus = row.GetFieldOrDefault(InventoryColumns.PartStatus).Trim().ToString(),
                    Comcode = row.GetFieldOrDefault(InventoryColumns.Comcode).Trim().ToString(),
                    DRICode = row.GetFieldOrDefault(InventoryColumns.DRICode).Trim().ToString(),
                    Description = row.GetFieldOrDefault(InventoryColumns.Description).Trim().ToString(),
                    InventoryDate = inventoryDate,
                    InventoryDateStr = inventoryDateStr,
                    InventoryDateError = inventoryDateError,
                    IncorrectColumnCount = row.FieldCount != InventoryColumns.TotalColumns
                });
                rowNumber++;
            }
            return inventoryDataFile;
        }

        private static class InventoryColumns
        {
            public const int DivisionId = 0;
            public const int LocalSiteId = 1;
            public const int PartNumber = 2;
            public const int Quantity = 3;
            public const int StandardCost = 4;
            public const int TotalValue = 5;
            public const int UOM = 6;
            public const int CurrencyCode = 7;
            public const int PartStatus = 8;
            public const int Comcode = 9;
            public const int DRICode = 10;
            public const int Description = 11;
            public const int InventoryDate = 12;
            public const int TotalColumns = 13;
        }
    }
}
