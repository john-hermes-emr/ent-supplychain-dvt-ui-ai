using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using static DVT.Core.Constants;

namespace DVT.Core.FileLoader
{
    public class ItemFileLoader : IFileLoader
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

                var loadedFile = ParseItemFileDataWithSpans(request);
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

        private IJobFileModel ParseItemFileDataWithSpans(FileLoadRequest request)
        {
            JobFileModel itemDataFile = new JobFileModel()
            {
                JobFileId = request.JobFileId,
                FileType = Constants.FileTypes.Item,
                FileName = request.FileName,
                DataRows = new List<IDataRow>()
            };

            var parser = new EfficientCsvParser(request.FlatFileContent);
            int rowNumber = 0;

            BigDecimal? leadTime = null;
            string leadTimeOriginalStr = "";
            BigDecimal? standardCost = null;
            string standardCostOriginalStr = "";
            BigDecimal? itemWeight = null;
            string itemWeightOriginalStr = "";
            ErrorTypes leadTimeError = ErrorTypes.None;
            ErrorTypes standardCostError = ErrorTypes.None;
            ErrorTypes itemWeightError = ErrorTypes.None;

            while (parser.TryReadRow(out var row))
            {
                if (rowNumber == 0)
                {
                    itemDataFile.FileHeader.AddRange(row.GetAsSplitStringList());
                    rowNumber++;
                    continue;
                }

                //If the row is empty, create an empty object and move on
                if (row.IsEmptyLine)
                {
                    itemDataFile.DataRows.Add(new ItemDataRow());
                    rowNumber++;
                    continue;
                }

                leadTimeError = ErrorTypes.None;
                standardCostError = ErrorTypes.None;
                itemWeightError = ErrorTypes.None;
                standardCostOriginalStr = row.GetFieldOrDefault(ItemColumns.StandardCost).Trim().ToString();
                leadTimeOriginalStr = row.GetFieldOrDefault(ItemColumns.LeadTime).Trim().ToString();
                itemWeightOriginalStr = row.GetFieldOrDefault(ItemColumns.ItemWeight).Trim().ToString();

                leadTime = DataConverter.ParseNullableBigDecimal(leadTimeOriginalStr, NumberTypeCharacterLengthLimit.FiftyCharacters, ref leadTimeError);
                standardCost = DataConverter.ParseNullableBigDecimal(standardCostOriginalStr, NumberTypeCharacterLengthLimit.FiftyCharacters, ref standardCostError);
                itemWeight = DataConverter.ParseNullableBigDecimal(itemWeightOriginalStr, NumberTypeCharacterLengthLimit.FiftyCharacters, ref itemWeightError);

                itemDataFile.DataRows.Add(new ItemDataRow
                {
                    RowNumber = rowNumber,
                    DivisionId = row.GetFieldOrDefault(ItemColumns.DivisionId).Trim().ToString(),
                    LocalSiteId = row.GetFieldOrDefault(ItemColumns.LocalSiteId).Trim().ToString(),
                    PartNumber = row.GetFieldOrDefault(ItemColumns.PartNumber).Trim().ToString(),
                    Description = row.GetFieldOrDefault(ItemColumns.Description).Trim().ToString(),
                    Comcode = row.GetFieldOrDefault(ItemColumns.Comcode).Trim().ToString(),
                    DRICode = row.GetFieldOrDefault(ItemColumns.DRICode).Trim().ToString(),
                    PartStatus = row.GetFieldOrDefault(ItemColumns.PartStatus).Trim().ToString(),
                    DirectIndirect = row.GetFieldOrDefault(ItemColumns.DirectIndirect).Trim().ToString(),
                    PurchMfrd = row.GetFieldOrDefault(ItemColumns.PurchMfrd).Trim().ToString(),
                    LeadTime = leadTime,
                    LeadTimeError = leadTimeError,
                    LeadTimeOriginalStr = leadTimeOriginalStr,
                    StandardCostOriginalStr = standardCostOriginalStr,
                    StandardCost = standardCost,
                    StandardCostError = standardCostError,
                    ItemWeightOriginalStr = itemWeightOriginalStr,
                    ItemWeight = itemWeight,
                    ItemWeightError = itemWeightError,
                    PureLoadedCost = row.GetFieldOrDefault(ItemColumns.PureLoadedCost).Trim().ToString(),
                    CurrencyCode = row.GetFieldOrDefault(ItemColumns.CurrencyCode).Trim().ToString(),
                    UOM = row.GetFieldOrDefault(ItemColumns.UOM).Trim().ToString(),
                    ABCCategory = row.GetFieldOrDefault(ItemColumns.ABCCategory).Trim().ToString(),
                    ItemWeightUOM = row.GetFieldOrDefault(ItemColumns.ItemWeightUOM).Trim().ToString(),
                    ItemHtsCode = row.GetFieldOrDefault(ItemColumns.ItemHtsCode).Trim().ToString(),
                    ItemHsCode = row.GetFieldOrDefault(ItemColumns.ItemHsCode).Trim().ToString(),
                    IncorrectColumnCount = row.FieldCount != ItemColumns.TotalColumns
                });
                rowNumber++;
            }

            return itemDataFile;
        }

        private static class ItemColumns
        {
            public const int DivisionId = 0;
            public const int LocalSiteId = 1;
            public const int PartNumber = 2;
            public const int Description = 3;
            public const int Comcode = 4;
            public const int DRICode = 5;
            public const int PartStatus = 6;
            public const int DirectIndirect = 7;
            public const int PurchMfrd = 8;
            public const int LeadTime = 9;
            public const int StandardCost = 10;
            public const int PureLoadedCost = 11;
            public const int CurrencyCode = 12;
            public const int UOM = 13;
            public const int ABCCategory = 14;
            public const int ItemWeight = 15;
            public const int ItemWeightUOM = 16;
            public const int ItemHtsCode = 17;
            public const int ItemHsCode = 18;
            public const int TotalColumns = 19;
        }
    }
}