using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using static DVT.Core.Constants;

namespace DVT.Core.FileLoader
{
    /// <summary>
    /// User Story 16005053: 9 - File Load Service - MPN File Loader
    /// </summary>
    public class MPNFileLoader : IFileLoader
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

                var loadedFile = ParseMPNFileDataWithSpans(request);
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

        private IJobFileModel ParseMPNFileDataWithSpans(FileLoadRequest request)
        {
            JobFileModel file = new JobFileModel()
            {
                JobFileId = request.JobFileId,
                FileType = Constants.FileTypes.Mpn,
                FileName = request.FileName,
                DataRows = new List<IDataRow>()
            };

            var parser = new EfficientCsvParser(request.FlatFileContent);
            int rowNumber = 0;

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
                    file.DataRows.Add(new MPNDataRow());
                    rowNumber++;
                    continue;
                }

                //Data Rows
                file.DataRows.Add(new MPNDataRow
                {
                    RowNumber = rowNumber,
                    DivisionID = row.GetFieldOrDefault(MPNColumns.DivisionId).Trim().ToString(),
                    LocalSiteID = row.GetFieldOrDefault(MPNColumns.LocalSiteId).Trim().ToString(),
                    PartNumber = row.GetFieldOrDefault(MPNColumns.PartNumber).Trim().ToString(),
                    LocalManufacturerID = row.GetFieldOrDefault(MPNColumns.LocalManufacturerID).Trim().ToString(),
                    ManufactureID = row.GetFieldOrDefault(MPNColumns.ManufactureID).Trim().ToString(),
                    ManufactureName = row.GetFieldOrDefault(MPNColumns.ManufactureName).Trim().ToString(),
                    ManufacturerPartNumber = row.GetFieldOrDefault(MPNColumns.ManufacturerPartNumber).Trim().ToString(),
                    ObjectID = row.GetFieldOrDefault(MPNColumns.ObjectID).Trim().ToString(),
                    MPNType = row.GetFieldOrDefault(MPNColumns.MPNType).Trim().ToString(),
                    IncorrectColumnCount = row.FieldCount != MPNColumns.TotalColumns
                });
                rowNumber++;
            }
            return file;
        }

        private static class MPNColumns
        {
            public const int DivisionId = 0;
            public const int LocalSiteId = 1;
            public const int PartNumber = 2;
            public const int LocalManufacturerID = 3;
            public const int ManufactureID = 4;
            public const int ManufactureName = 5;
            public const int ManufacturerPartNumber = 6;
            public const int ObjectID = 7;
            public const int MPNType = 8;
            public const int TotalColumns = 9;
        }
    }
}
