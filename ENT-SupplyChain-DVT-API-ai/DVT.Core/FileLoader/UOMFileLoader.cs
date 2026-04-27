using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using static DVT.Core.Constants;

namespace DVT.Core.FileLoader
{
    public class UOMFileLoader : IFileLoader
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

                var loadedFile = ParseUOMFileDataWithSpans(request);
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

        private IJobFileModel ParseUOMFileDataWithSpans(FileLoadRequest request)
        {
            JobFileModel file = new JobFileModel()
            {
                JobFileId = request.JobFileId,
                FileType = Constants.FileTypes.Uom,
                FileName = request.FileName,
                DataRows = new List<IDataRow>()
            };

            var parser = new EfficientCsvParser(request.FlatFileContent);
            int rowNumber = 0;
            BigDecimal? conversionRate = null;
            ErrorTypes conversionRateError = ErrorTypes.None;
            string conversionRateOriginalStr = string.Empty;



            while (parser.TryReadRow(out var row))
            {
                if (rowNumber == 0)
                {
                    file.FileHeader.AddRange(row.GetAsSplitStringList());
                    rowNumber++;
                    continue;
                }

                //If the row is empty, create an empty object and move on
                if (row.IsEmptyLine)
                {
                    file.DataRows.Add(new UOMDataRow());
                    rowNumber++;
                    continue;
                }

                conversionRate = null;
                conversionRateError = ErrorTypes.None;
                conversionRateOriginalStr = row.GetFieldOrDefault(UOMColumns.ConversionRate).Trim().ToString();

                conversionRate = DataConverter.ParseNullableBigDecimal(conversionRateOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref conversionRateError);

                //Data Rows
                file.DataRows.Add(new UOMDataRow
                {
                    RowNumber = rowNumber,
                    DivisionID = row.GetFieldOrDefault(UOMColumns.DivisionId).Trim().ToString(),
                    LocalSiteID = row.GetFieldOrDefault(UOMColumns.LocalSiteId).Trim().ToString(),
                    PartNumber = row.GetFieldOrDefault(UOMColumns.PartNumber).Trim().ToString(),
                    LocalUOM = row.GetFieldOrDefault(UOMColumns.LocalUOM).Trim().ToString(),
                    BaseUOM = row.GetFieldOrDefault(UOMColumns.BaseUOM).Trim().ToString(),
                    ConversionRate = conversionRate,
                    ConversionRateError = conversionRateError,
                    ConversionRateOriginalStr = conversionRateOriginalStr,
                    IncorrectColumnCount = row.FieldCount != UOMColumns.TotalColumns
                });
                rowNumber++;

            }

            return file;
        }

        private static class UOMColumns
        {
            public const int DivisionId = 0;
            public const int LocalSiteId = 1;
            public const int PartNumber = 2;
            public const int LocalUOM = 3;
            public const int BaseUOM = 4;
            public const int ConversionRate = 5;
            public const int TotalColumns = 6;
        }
    }
}