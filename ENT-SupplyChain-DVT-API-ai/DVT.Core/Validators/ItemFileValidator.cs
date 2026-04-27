using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using FluentValidation.Results;
using System.Data;
using System.Diagnostics;
using System.Text;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18238793: 1 - Validation Service - Validate ITEM File
    /// </summary>
    public class ItemFileValidator
    {
        public FileValidationResult ValidateAsync(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData)
        {
            StopwatchLogger logger = new StopwatchLogger("ValidateFilesAsync");
            logger.Start();

            FileValidationResult fileResult = new FileValidationResult(file.JobFileId, file.FileName);
            ItemDataRowStaticValidator staticValidator = new ItemDataRowStaticValidator();

            List<string> headers = file.FileHeader;

            //validate headers
            CommonValidation.ValidateHeaders(fileResult, headers, ItemFileHeaderList);
            logger.StopAndLog("ItemFileValidator ValidateHeaders", true);

            object rowObj;

            var itemDataRows = file.DataRows.Cast<ItemDataRow>().ToList();
            if (itemDataRows == null || !itemDataRows.Any())
            {
                return fileResult;
            }

            logger.StopAndLog("ItemFileValidator CastDataRows", true);

            for (int i = 0; i < file.DataRows.Count; i++)
            {
                rowObj = file.DataRows[i];
                if (rowObj == null)
                {
                    fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                           PropertyName = ValidationMessages.FullRecordError,
                           ErrorMessage = ValidationMessages.NullRow + $" in row {i + 1}.",
                           ErrorCode = DataRowErrorStatus.Critical
                        }
                    }), i + 1));
                    continue;
                }

                var dataRow = rowObj as ItemDataRow;

                //Calculate uniqueness key for the data row for later use in duplicate record validation
                dataRow?.GenerateUniquenessKey();

                if (dataRow == null)
                {
                    fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                        {
                             new ValidationFailure()
                             {
                               PropertyName = ValidationMessages.FullRecordError,
                               ErrorMessage = ValidationMessages.InvalidRowType + $" in row {i + 1}.",
                               ErrorCode = DataRowErrorStatus.Critical
                             }
                        }), i + 1));
                    continue;
                }

                staticValidator = new ItemDataRowStaticValidator();
                var rowResult = staticValidator.Validate(dataRow);

                if (rowResult != null && !rowResult.IsValid && rowResult.Errors != null)
                {
                    rowResult.Errors.ToList().ForEach(e =>
                    {
                        e.FormattedMessagePlaceholderValues = null;
                        e.CustomState = null;
                    });
                    fileResult.RowValidationResults.Add(new FileRowValidationResult(rowResult, dataRow.RowNumber));
                }
            }

            logger.StopAndLog("ItemFileValidator Static Validator", true);

            ValidateMasterData(fileResult, itemDataRows, masterData);
            logger.StopAndLog("ItemFileValidator Master Data validation", true);

            ValidateCustomField(fileResult, itemDataRows);
            logger.StopAndLog("ItemFileValidator Custom Field validation", true);

            ValidateDuplicateRecords2(fileResult, itemDataRows);
            logger.StopAndLog("ItemFileValidator Duplicate Records validation with Dictionary", true);

            //ValidateDuplicateRecords(fileResult, itemDataRows);
           //logger.StopAndLog("ItemFileValidator Duplicate Records validation", true);

            fileResult.AdditionalInfo = logger.Log.ToString();
            return fileResult;
        }

        private void ValidateMasterData(FileValidationResult result, List<ItemDataRow> itemDataRows, IEnumerable<MasterData> masterData)
        {
            //Ensure that the Division Id, Local Site Id, UOM, Currency Code, DRI Code are in the master data table
            ValidateDivisionId(result, itemDataRows, masterData);

            ValidateLocalSiteId(result, itemDataRows, masterData);

            ValidateUOM(result, itemDataRows, masterData);

            ValidateItemWeightUOM(result, itemDataRows, masterData);

            ValidateCurrencyCode(result, itemDataRows, masterData);

            ValidateDRICode(result, itemDataRows, masterData);
        }

        private void ValidateDivisionId(FileValidationResult result, List<ItemDataRow> itemDataRows, IEnumerable<MasterData> masterData)
        {
            var divisionIds = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Division, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();

            //Bug 19483998: [QA Bug] - Status should be Critical --- cannot to lower if value is null.
            //find the invalid division Ids - not in master data
            var rowsWithInvalidDivisionIds = itemDataRows.Where(r => !string.IsNullOrWhiteSpace(r.DivisionId) && !divisionIds.Any(d => string.Equals(d, r.DivisionId, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithInvalidDivisionIds.Any())
            {
                foreach (var invalidDivisionId in rowsWithInvalidDivisionIds)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                             PropertyName = ItemFileHeaders.DivisionId,
                             ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, ItemFileHeaders.DivisionId),
                             ErrorCode = DataRowErrorStatus.Critical,
                             AttemptedValue = invalidDivisionId.DivisionId
                        }
                    }), invalidDivisionId.RowNumber));
                }
            }
        }

        private void ValidateLocalSiteId(FileValidationResult result, List<ItemDataRow> itemDataRows, IEnumerable<MasterData> masterData)
        {
            var localSiteIds = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();

            var rowsWithInvalidLocalSiteIds = itemDataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteId) && !localSiteIds.Any(s => string.Equals(x.LocalSiteId, s, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithInvalidLocalSiteIds.Any())
            {
                foreach (var invalidLocalSiteId in rowsWithInvalidLocalSiteIds)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = ItemFileHeaders.LocalSiteId,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, ItemFileHeaders.LocalSiteId),
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue = invalidLocalSiteId.LocalSiteId
                        }
                    }), invalidLocalSiteId.RowNumber));
                }
            }
        }

        private void ValidateUOM(FileValidationResult result, List<ItemDataRow> itemDataRows, IEnumerable<MasterData> masterData)
        {
            var uoms = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.UOM, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidUOMs = itemDataRows.Where(x => !string.IsNullOrWhiteSpace(x.UOM) && !uoms.Any(s => string.Equals(x.UOM, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidUOMs.Any())
            {
                foreach (var invalidUOM in rowsWithInvalidUOMs)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = ItemFileHeaders.UOM,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, ItemFileHeaders.UOM),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidUOM.UOM
                        }
                    }), invalidUOM.RowNumber));
                }
            }
        }

        /// <summary>
        /// User Story 19854626 - ITEM - ITEM Weight UOM must exist in database
        /// </summary>
        /// <param name="result"></param>
        /// <param name="itemDataRows"></param>
        /// <param name="masterData"></param>
        private void ValidateItemWeightUOM(FileValidationResult result, List<ItemDataRow> itemDataRows, IEnumerable<MasterData> masterData)
        {
            var itemWeightUOMs = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.ItemWeightUOM, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidItemWeightUOMs = itemDataRows.Where(x => !string.IsNullOrWhiteSpace(x.ItemWeightUOM) && !itemWeightUOMs.Any(s => string.Equals(x.ItemWeightUOM, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidItemWeightUOMs.Any())
            {
                foreach (var invalidItemWeightUOM in rowsWithInvalidItemWeightUOMs)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = ItemFileHeaders.ItemWeightUOM,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, ItemFileHeaders.ItemWeightUOM),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidItemWeightUOM.ItemWeightUOM
                        }
                    }), invalidItemWeightUOM.RowNumber));
                }
            }
        }

        private void ValidateCurrencyCode(FileValidationResult result, List<ItemDataRow> itemDataRows, IEnumerable<MasterData> masterData)
        {
            var currencyCodes = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Currency, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidCurrencyCodes = itemDataRows.Where(x => !string.IsNullOrWhiteSpace(x.CurrencyCode) && !currencyCodes.Any(s => string.Equals(x.CurrencyCode, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidCurrencyCodes.Any())
            {
                foreach (var invalidCurrencyCode in rowsWithInvalidCurrencyCodes)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = ItemFileHeaders.CurrencyCode,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, ItemFileHeaders.CurrencyCode),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidCurrencyCode.CurrencyCode
                        }
                    }), invalidCurrencyCode.RowNumber));
                }
            }
        }

        private void ValidateDRICode(FileValidationResult result, List<ItemDataRow> itemDataRows, IEnumerable<MasterData> masterData)
        {
            var driCodes = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.CommodityCode, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidDRICodes = itemDataRows.Where(x => !string.IsNullOrWhiteSpace(x.DRICode) && !driCodes.Any(s => string.Equals(x.DRICode, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidDRICodes.Any())
            {
                foreach (var invalidFreightTerm in rowsWithInvalidDRICodes)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = ItemFileHeaders.DRICode,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, ItemFileHeaders.DRICode),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidFreightTerm.DRICode
                        }
                    }), invalidFreightTerm.RowNumber));
                }
            }
        }

        /// <summary>
        /// User Story 13375691: ITEM - Part Description cannot be equal to Part Number --- using Invalid Format for it.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="dataRows"></param>
        private void ValidateCustomField(FileValidationResult result, List<ItemDataRow> dataRows)
        {
            //Part Description Must not be equal to Part Number
            var invalidRows = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber) && !string.IsNullOrWhiteSpace(x.Description) && string.Equals(x.PartNumber, x.Description, StringComparison.OrdinalIgnoreCase)).ToList();
            {
                foreach (var invalidRow in invalidRows)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = ItemFileHeaders.Description,
                            ErrorMessage = ValidationMessages.InvalidFormat,
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidRow.Description
                        }
                    }), invalidRow.RowNumber));
                }
            }
        }

        private void ValidateDuplicateRecords2(FileValidationResult result, List<ItemDataRow> itemDataRows)
        {
            var duplicateRowNumbers = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(itemDataRows);

            if (duplicateRowNumbers.Count > 0)
            {
                ItemDataRow dataRowToShow;
                foreach (var duplicateRowNumber in duplicateRowNumbers)
                {
                    dataRowToShow = itemDataRows.First(x => x.RowNumber == duplicateRowNumber);

                    result.RowValidationResults.Add(
                        new FileRowValidationResult(
                            new ValidationResult(
                                new List<ValidationFailure>
                                {
                                    new ValidationFailure()
                                    {
                                        PropertyName = CustomFileHeaders.SourceRecordIDfields,
                                        ErrorMessage =ValidationMessages.DuplicateRecordFound,
                                        ErrorCode = DataRowErrorStatus.Critical,
                                        AttemptedValue = $"Division ID: {dataRowToShow.DivisionId}, Local Site ID: {dataRowToShow.LocalSiteId}, Part Number: {dataRowToShow.PartNumber}"
                                    }
                                }), duplicateRowNumber));
                }
            }
        }

        private void ValidateDuplicateRecords(FileValidationResult result, List<ItemDataRow> itemDataRows)
        {
            //These fields cannot be the same for multiple records in the ItemFile
            //| DIVISION ID | +| LOCAL SITE ID| +| PART NUMBER |

            // Check for duplicate records in the virFile
            //Get a strongely typed version of the list of data rows
            var itemDataRowsCopy = new List<ItemDataRow>();
            itemDataRows.ForEach(d => itemDataRowsCopy.Add(new ItemDataRow()
            {
                DivisionId = d.DivisionId?.ToLower(),
                LocalSiteId = d.LocalSiteId?.ToLower(),
                PartNumber = d.PartNumber?.ToLower(),
                RowNumber = d.RowNumber
            }));

            var duplicates = itemDataRowsCopy.GroupBy(x => new
            {
                x.DivisionId,
                x.LocalSiteId,
                x.PartNumber,
            }).Where(g => g.Count() > 1)
              .Select(g => new { Key = g.Key, RowNumbers = g.Select(a => a.RowNumber).ToList() })
              .ToList();

            if (duplicates.Any())
            {
                ItemDataRow dataRowToShow;
                foreach (var duplicate in duplicates)
                {
                    foreach (var rowNum in duplicate.RowNumbers)
                    {
                        dataRowToShow = itemDataRows.First(x => x.RowNumber == rowNum);
                        result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure()
                            {
                                PropertyName = CustomFileHeaders.SourceRecordIDfields,
                                ErrorMessage =ValidationMessages.DuplicateRecordFound,
                                ErrorCode = DataRowErrorStatus.Critical,
                                AttemptedValue = $"Division ID: {dataRowToShow.DivisionId}, Local Site ID: {dataRowToShow.LocalSiteId}, Part Number: {dataRowToShow.PartNumber}"
                            }
                            //string.Format(ValidationMessages.DuplicateSourceRecordFound,dataRowToShow.DivisionID,dataRowToShow.LocalSiteID,dataRowToShow.ReceiptNumber,dataRowToShow.PONumber,dataRowToShow.POLineNumber,dataRowToShow.PartNumber,dataRowToShow.DateReceivedStr,dataRowToShow.Release,dataRowToShow.CommittedDateStr))
                        }), rowNum));
                    }
                }
            }
        }

        public FileCalculateStatistics_Item GetFileCalculateStatistics(IJobFileModel job, int recordCount)
        {
            var result = new FileCalculateStatistics_Item();
            result.TotalRecords = recordCount;

            var dataRows = job.DataRows.Cast<ItemDataRow>().ToList();

            var filterOutNullStandardCostData = dataRows.Where(x => x.StandardCost != null);

            var recordOrderByStandardCost = filterOutNullStandardCostData.Any() ? filterOutNullStandardCostData.OrderBy(r => r.StandardCost).ToList() : null;
            result.StandardCostMin = recordOrderByStandardCost == null ? "" : recordOrderByStandardCost.First().StandardCostOriginalStr;
            result.StandardCostMax = recordOrderByStandardCost == null ? "" : recordOrderByStandardCost.Last().StandardCostOriginalStr;

            return result;
        }
    }
}
