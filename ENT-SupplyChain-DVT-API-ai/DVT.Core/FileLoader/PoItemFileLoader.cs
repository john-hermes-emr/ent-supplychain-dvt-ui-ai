using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using static DVT.Core.Constants;

namespace DVT.Core.FileLoader
{
    public class POItemFileLoader : IFileLoader
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

                var loadedFile = ParsePoItemFileDataWithSpans(request);
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

        private IJobFileModel ParsePoItemFileDataWithSpans(FileLoadRequest request)
        {
            JobFileModel file = new JobFileModel()
            {
                JobFileId = request.JobFileId,
                FileType = Constants.FileTypes.PoItem,
                FileName = request.FileName,
                DataRows = new List<IDataRow>()
            };

            var parser = new EfficientCsvParser(request.FlatFileContent);
            int rowNumber = 0;

            BigDecimal? unitCost = null;
            ErrorTypes unitCostError = ErrorTypes.None;
            string unitCostOriginalStr = "";
            BigDecimal? ordered = null;
            ErrorTypes orderedError = ErrorTypes.None;
            string orderedOriginalStr = "";
            BigDecimal? quantityOrdered = null;
            ErrorTypes quantityOrderedError = ErrorTypes.None;
            string quantityOrderedOriginalStr = "";
            BigDecimal? quantityReturned = null;
            ErrorTypes quantityReturnedError = ErrorTypes.None;
            string quantityReturnedOriginalStr = "";
            DateTime? committedDate = null;
            DateTime? requestedDate = null;
            BigDecimal? qtyLeftToReceive = null;
            ErrorTypes qtyLeftToReceiveError = ErrorTypes.None;
            string qtyLeftToReceiveOriginalStr = "";
            BigDecimal? valueLeftToReceive = null;
            ErrorTypes valueLeftToReceiveError = ErrorTypes.None;
            string valueLeftToReceiveOriginalStr = "";
            BigDecimal? release = null;
            ErrorTypes releaseError = ErrorTypes.None;
            string releaseOriginalStr = "";
            string committedDateStr = "";
            string committedDateError = "";
            string requestedDateStr = "";
            string requestedDateError = "";            

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
                    file.DataRows.Add(new POItemDataRow());
                    rowNumber++;
                    continue;
                }               

                unitCost = null;
                unitCostError = ErrorTypes.None;
                ordered = null;
                orderedError = ErrorTypes.None;
                quantityOrdered = null;
                quantityOrderedError = ErrorTypes.None;
                quantityReturned = null;
                quantityReturnedError = ErrorTypes.None;
                committedDate = null;
                requestedDate = null;
                qtyLeftToReceive = null;
                qtyLeftToReceiveError = ErrorTypes.None;
                valueLeftToReceive = null;
                valueLeftToReceiveError = ErrorTypes.None;
                release = null;
                releaseError = ErrorTypes.None;
                committedDateStr = "";
                committedDateError = "";
                requestedDateStr = "";
                requestedDateError = "";

                unitCostOriginalStr = row.GetFieldOrDefault(PoItemColumns.UnitCost).Trim().ToString();
                orderedOriginalStr = row.GetFieldOrDefault(PoItemColumns.OrderedValue).Trim().ToString();
                quantityOrderedOriginalStr = row.GetFieldOrDefault(PoItemColumns.QuantityOrdered).Trim().ToString();
                quantityReturnedOriginalStr = row.GetFieldOrDefault(PoItemColumns.QuantityReturned).Trim().ToString();
                qtyLeftToReceiveOriginalStr = row.GetFieldOrDefault(PoItemColumns.QtyLeftToReceive).Trim().ToString();
                valueLeftToReceiveOriginalStr = row.GetFieldOrDefault(PoItemColumns.ValueLeftToReceive).Trim().ToString();
                releaseOriginalStr = row.GetFieldOrDefault(PoItemColumns.Release).Trim().ToString();
                committedDateStr = row.GetFieldOrDefault(PoItemColumns.CommittedDate).Trim().ToString();
                requestedDateStr = row.GetFieldOrDefault(PoItemColumns.RequestedDate).Trim().ToString();

                unitCost = DataConverter.ParseNullableBigDecimal(unitCostOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref unitCostError);
                ordered = DataConverter.ParseNullableBigDecimal(orderedOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref orderedError);
                quantityOrdered = DataConverter.ParseNullableBigDecimal(quantityOrderedOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref quantityOrderedError);
                quantityReturned = DataConverter.ParseNullableBigDecimal(quantityReturnedOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref quantityReturnedError);
                qtyLeftToReceive = DataConverter.ParseNullableBigDecimal(qtyLeftToReceiveOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref qtyLeftToReceiveError);
                valueLeftToReceive = DataConverter.ParseNullableBigDecimal(valueLeftToReceiveOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref valueLeftToReceiveError);
                release = DataConverter.ParseNullableBigDecimal(releaseOriginalStr, NumberTypeCharacterLengthLimit.FiftyCharacters, ref releaseError);
                committedDate = DataConverter.ParseNullableDate(committedDateStr, ref committedDateError, ref committedDateStr);
                requestedDate = DataConverter.ParseNullableDate(requestedDateStr, ref requestedDateError, ref requestedDateStr);

                //Data Rows
                file.DataRows.Add(new POItemDataRow
                {
                    RowNumber = rowNumber,
                    DivisionID = row.GetFieldOrDefault(PoItemColumns.DivisionID).Trim().ToString(),
                    LocalSiteID = row.GetFieldOrDefault(PoItemColumns.LocalSiteID).Trim().ToString(),
                    PONumber = row.GetFieldOrDefault(PoItemColumns.PONumber).Trim().ToString(),
                    POLineNumber = row.GetFieldOrDefault(PoItemColumns.POLineNumber).Trim().ToString(),
                    PartNumber = row.GetFieldOrDefault(PoItemColumns.PartNumber).Trim().ToString(),
                    SupplierPartNumber = row.GetFieldOrDefault(PoItemColumns.SupplierPartNumber).Trim().ToString(),
                    Description = row.GetFieldOrDefault(PoItemColumns.Description).Trim().ToString(),
                    ContractID = row.GetFieldOrDefault(PoItemColumns.ContractId).Trim().ToString(),
                    UnitCost = unitCost,
                    UnitCostError = unitCostError,
                    UnitCostOriginalStr = unitCostOriginalStr,
                    PureLoadedCost = row.GetFieldOrDefault(PoItemColumns.PureLoadedCost).Trim().ToString(),
                    OrderedValue = ordered,
                    OrderedValueError = orderedError,
                    OrderedValueOriginalStr = orderedOriginalStr,
                    QuantityOrdered = quantityOrdered,
                    QuantityOrderedError = quantityOrderedError,
                    QuantityOrderedOriginalStr = quantityOrderedOriginalStr,
                    QuantityReturned = quantityReturned,
                    QuantityReturnedError = quantityReturnedError,
                    QuantityReturnedOriginalStr = quantityReturnedOriginalStr,
                    CommittedDate = committedDate,
                    CommittedDateError = committedDateError,
                    CommittedDateStr = committedDateStr,
                    RequestedDate = requestedDate,
                    RequestedDateError = requestedDateError,
                    RequestedDateStr = requestedDateStr,
                    OrderStatus = row.GetFieldOrDefault(PoItemColumns.OrderStatus).Trim().ToString(),
                    CurrencyCode = row.GetFieldOrDefault(PoItemColumns.CurrencyCode).Trim().ToString(),
                    UOM = row.GetFieldOrDefault(PoItemColumns.UOM).Trim().ToString(),
                    QtyLeftToReceive = qtyLeftToReceive,
                    QtyLeftToReceiveError = qtyLeftToReceiveError,
                    QtyLeftToReceiveOriginalStr = qtyLeftToReceiveOriginalStr,
                    ValueLeftToReceive = valueLeftToReceive,
                    ValueLeftToReceiveError = valueLeftToReceiveError,
                    ValueLeftToReceiveOriginalStr = valueLeftToReceiveOriginalStr,
                    Release = release,
                    ReleaseError = releaseError,
                    ReleaseOriginalStr = releaseOriginalStr,      
                    IncorrectColumnCount = row.FieldCount != PoItemColumns.TotalColumns
                });
                rowNumber++;
            }

            return file;

        }

        private static class PoItemColumns
        {
            public const int DivisionID = 0;
            public const int LocalSiteID = 1;
            public const int PONumber = 2;
            public const int POLineNumber = 3;
            public const int PartNumber = 4;
            public const int SupplierPartNumber = 5;
            public const int Description = 6;
            public const int ContractId = 7;
            public const int UnitCost = 8;
            public const int PureLoadedCost = 9;
            public const int OrderedValue = 10;
            public const int QuantityOrdered = 11;
            public const int QuantityReturned = 12;
            public const int CommittedDate = 13;
            public const int RequestedDate = 14;
            public const int OrderStatus = 15;
            public const int CurrencyCode = 16;
            public const int UOM = 17;
            public const int QtyLeftToReceive = 18;
            public const int ValueLeftToReceive = 19;
            public const int Release = 20;
            public const int TotalColumns = 21;
        }
    }
}
