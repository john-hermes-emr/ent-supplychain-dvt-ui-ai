using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using static DVT.Core.Constants;

namespace DVT.Core.FileLoader
{
    public class VirFileLoader : IFileLoader
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

                var loadedFile = ParseVirFileDataWithSpans(request);
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

        private IJobFileModel ParseVirFileDataWithSpans(FileLoadRequest request)
        {
            JobFileModel virDataFile = new JobFileModel()
            {
                JobFileId = request.JobFileId,
                FileType = Constants.FileTypes.Vir,
                FileName = request.FileName,
                DataRows = new List<IDataRow>()
            };

            var parser = new EfficientCsvParser(request.FlatFileContent);
            int rowNumber = 0;

            DateTime? dateReceived = null;
            DateTime? committedDate = null;
            string dateReceivedStr = "";
            string dateReceivedError = "";
            string committedDateStr = "";
            string committedDateError = "";
            BigDecimal? quantityOrdered = null;
            ErrorTypes quantityOrderedError = ErrorTypes.None;
            string quantityOrderedOriginalStr = "";
            BigDecimal? quantityReceived = null;
            ErrorTypes quantityReceivedError = ErrorTypes.None;
            string quantityReceivedOriginalStr = "";
            BigDecimal? release = null;
            ErrorTypes releaseError = ErrorTypes.None;
            string releaseOriginalStr = "";
            BigDecimal? invoicePricePaid = null;
            ErrorTypes invoicePricePaidError = ErrorTypes.None;
            string invoicePricePaidOriginalStr = "";
            BigDecimal? unitPrice = null;
            ErrorTypes unitPriceError = ErrorTypes.None;
            string unitPriceOriginalStr = "";

            while (parser.TryReadRow(out var row))
            {
                if (rowNumber == 0)
                {
                    virDataFile.FileHeader.AddRange(row.GetAsSplitStringList());
                    rowNumber++;
                    continue;
                }

                //If the row is empty, create an empty object and move on
                if (row.IsEmptyLine)
                {
                    virDataFile.DataRows.Add(new VirDataRow());
                    rowNumber++;
                    continue;
                }

                dateReceived = null;
                committedDate = null;
                dateReceivedStr = "";
                committedDateStr = "";
                quantityOrdered = null;
                quantityReceived = null;
                release = null;
                releaseError = ErrorTypes.None;
                dateReceivedError = "";
                committedDateError = "";
                quantityOrderedError = ErrorTypes.None;
                quantityReceivedError = ErrorTypes.None;
                invoicePricePaid = null;
                unitPrice = null;
                invoicePricePaidError = ErrorTypes.None;
                unitPriceError = ErrorTypes.None;

                quantityOrderedOriginalStr = row.GetFieldOrDefault(VirColumns.QuantityOrdered).Trim().ToString();
                quantityReceivedOriginalStr = row.GetFieldOrDefault(VirColumns.QuantityReceived).Trim().ToString();
                releaseOriginalStr = row.GetFieldOrDefault(VirColumns.Release).Trim().ToString();
                invoicePricePaidOriginalStr = row.GetFieldOrDefault(VirColumns.InvoicePricePaid).Trim().ToString();
                unitPriceOriginalStr = row.GetFieldOrDefault(VirColumns.UnitPrice).Trim().ToString();
                dateReceived = DataConverter.ParseNullableDate(row.GetFieldOrDefault(VirColumns.DateReceived).Trim().ToString(), ref dateReceivedError, ref dateReceivedStr);
                committedDate = DataConverter.ParseNullableDate(row.GetFieldOrDefault(VirColumns.CommittedDate).Trim().ToString(), ref committedDateError, ref committedDateStr);
                quantityOrdered = DataConverter.ParseNullableBigDecimal(quantityOrderedOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref quantityOrderedError);
                quantityReceived = DataConverter.ParseNullableBigDecimal(quantityReceivedOriginalStr, NumberTypeCharacterLengthLimit.FifteenCharacters, ref quantityReceivedError);
                invoicePricePaid = DataConverter.ParseNullableBigDecimal(invoicePricePaidOriginalStr, NumberTypeCharacterLengthLimit.ThirtyEightCharacters, ref invoicePricePaidError);
                unitPrice = DataConverter.ParseNullableBigDecimal(unitPriceOriginalStr, NumberTypeCharacterLengthLimit.ThirtyEightCharacters, ref unitPriceError);
                release = DataConverter.ParseNullableBigDecimal(releaseOriginalStr, NumberTypeCharacterLengthLimit.FiftyCharacters, ref releaseError);

                //Data Rows
                virDataFile.DataRows.Add(new VirDataRow
                {
                    RowNumber = rowNumber,
                    DivisionId = row.GetFieldOrDefault(VirColumns.DivisionId).Trim().ToString(),
                    LocalSiteId = row.GetFieldOrDefault(VirColumns.LocalSiteId).Trim().ToString(),
                    ReceiptNumber = row.GetFieldOrDefault(VirColumns.ReceiptNumber).Trim().ToString(),
                    PoNumber = row.GetFieldOrDefault(VirColumns.PoNumber).Trim().ToString(),
                    POLineNumber = row.GetFieldOrDefault(VirColumns.POLineNumber).Trim().ToString(),
                    SupplierId = row.GetFieldOrDefault(VirColumns.SupplierId).Trim().ToString(),
                    PartNumber = row.GetFieldOrDefault(VirColumns.PartNumber).Trim().ToString(),
                    SupplierPartNumber = row.GetFieldOrDefault(VirColumns.SupplierPartNumber).Trim().ToString(),
                    QuantityOrderedOriginalStr = quantityOrderedOriginalStr,
                    QuantityOrdered = quantityOrdered,
                    QuantityOrderedError = quantityOrderedError,
                    QuantityReceivedOriginalStr = quantityReceivedOriginalStr,
                    QuantityReceived = quantityReceived,
                    QuantityReceivedError = quantityReceivedError,
                    DateReceived = dateReceived,
                    DateReceivedError = dateReceivedError,
                    DateReceivedStr = dateReceivedStr,
                    InvoicePricePaidOriginalStr = invoicePricePaidOriginalStr,
                    InvoicePricePaid = invoicePricePaid,
                    InvoicePricePaidError = invoicePricePaidError,
                    UnitPriceOriginalStr = unitPriceOriginalStr,
                    UnitPrice = unitPrice,
                    UnitPriceError = unitPriceError,
                    PureLoadedCost = row.GetFieldOrDefault(VirColumns.PureLoadedCost).Trim().ToString(),
                    CurrencyCode = row.GetFieldOrDefault(VirColumns.CurrencyCode).Trim().ToString(),
                    IntraDiv = row.GetFieldOrDefault(VirColumns.IntraDiv).Trim().ToString(),
                    DirectIndirect = row.GetFieldOrDefault(VirColumns.DirectIndirect).Trim().ToString(),
                    POTerms = row.GetFieldOrDefault(VirColumns.POTerms).Trim().ToString(),
                    FreightTerms = row.GetFieldOrDefault(VirColumns.FreightTerms).Trim().ToString(),
                    UOM = row.GetFieldOrDefault(VirColumns.UOM).Trim().ToString(),
                    TitleTransfer = row.GetFieldOrDefault(VirColumns.TitleTransfer).Trim().ToString(),
                    Port = row.GetFieldOrDefault(VirColumns.Port).Trim().ToString(),
                    ReleaseOriginalStr = releaseOriginalStr,
                    Release = release,
                    ReleaseError = releaseError,
                    CommittedDate = committedDate,
                    CommittedDateError = committedDateError,
                    CommittedDateStr = committedDateStr,
                    IncorrectColumnCount = row.FieldCount != VirColumns.TotalColumns
                });
                rowNumber++;

            }

            return virDataFile;
        }

        private static class VirColumns
        {
            public const int DivisionId = 0;
            public const int LocalSiteId = 1;
            public const int ReceiptNumber = 2;
            public const int PoNumber = 3;
            public const int POLineNumber = 4;
            public const int SupplierId = 5;
            public const int PartNumber = 6;
            public const int SupplierPartNumber = 7;
            public const int QuantityOrdered = 8;
            public const int QuantityReceived = 9;
            public const int DateReceived = 10;
            public const int InvoicePricePaid = 11;
            public const int UnitPrice = 12;
            public const int PureLoadedCost = 13;
            public const int CurrencyCode = 14;
            public const int IntraDiv = 15;
            public const int DirectIndirect = 16;
            public const int POTerms = 17;
            public const int FreightTerms = 18;
            public const int UOM = 19;
            public const int TitleTransfer = 20;
            public const int Port = 21;
            public const int Release = 22;
            public const int CommittedDate = 23;
            public const int TotalColumns = 24;
        }
    }
}