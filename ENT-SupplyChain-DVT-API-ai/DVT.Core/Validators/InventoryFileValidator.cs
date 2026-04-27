using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using FluentValidation.Results;
using System.Data;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18229942: 1 - Validation Service - Validate Inventory File
    /// </summary>
    public class InventoryFileValidator
    {
        public FileValidationResult ValidateAsync(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData)
        {
            StopwatchLogger logger = new StopwatchLogger("InventoryValidateFilesAsync");
            logger.Start();

            FileValidationResult fileResult = new FileValidationResult(file.JobFileId, file.FileName);
            InventoryDataRowStaticValidator staticValidator = new InventoryDataRowStaticValidator();

            //validate headers
            List<string> headers = file.FileHeader;
            CommonValidation.ValidateHeaders(fileResult, headers, InventoryFileHeaderList);
            logger.StopAndLog("InventoryFileValidator Validate Headers", true);

            object rowObj;

            var dataRows = file.DataRows.Cast<InventoryDataRow>().ToList();
            if (dataRows == null || !dataRows.Any())
            {
                return fileResult;
            }

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

                var dataRow = rowObj as InventoryDataRow;

                //Calculate the uniqueness key that we'll use later when finding duplicates
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

                staticValidator = new InventoryDataRowStaticValidator();
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

            logger.StopAndLog("InventoryFileValidator Static Validation", true);

            ValidateMasterData(fileResult, dataRows, masterData, job.DivisionId);
            logger.StopAndLog("InventoryFileValidator Validate Master Data", true);

            ValidateDuplicateRecords(fileResult, dataRows);
            logger.StopAndLog("InventoryFileValidator Validate Duplicate Records", true);

            ValidateDependentColumns(fileResult, job, file, dataRows, masterData);
            logger.StopAndLog("InventoryFileValidator Validate Dependent Columns", false);

            fileResult.AdditionalInfo = logger.Log.ToString();
            return fileResult;
        }

        private void ValidateMasterData(FileValidationResult result, List<InventoryDataRow> dataRows, IEnumerable<MasterData> masterData, Guid divisionId)
        {
            var currentDivision = masterData.FirstOrDefault(m => m.ItemId == divisionId);

            ValidateDivisionIdAndLocalSiteId(result, dataRows, masterData, currentDivision);

            ValidateDRICode(result, dataRows, masterData);

            ValidateUOM(result, dataRows, masterData);

            ValidateCurrencyCode(result, dataRows, masterData);
        }

        private void ValidateDivisionIdAndLocalSiteId(FileValidationResult result, List<InventoryDataRow> dataRows, IEnumerable<MasterData> masterData, MasterData division)
        {
            var notFoundDivisionIdsRowNums = new List<int>();
            var notFoundLocalSiteIdsRowNums = new List<int>();

            var divisionIds = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Division, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();

            var rowsWithNotFoundDivisionIds = dataRows.Where(r => !string.IsNullOrWhiteSpace(r.DivisionId) && !divisionIds.Any(d => string.Equals(d, r.DivisionId, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotFoundDivisionIds.Any())
            {
                foreach (var notFoundDivisionId in rowsWithNotFoundDivisionIds)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure()
                            {
                                 PropertyName = InventoryFileHeaders.DivisionId,
                                 ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, InventoryFileHeaders.DivisionId),
                                 ErrorCode = DataRowErrorStatus.Critical,
                                 AttemptedValue = notFoundDivisionId.DivisionId
                            }
                        }), notFoundDivisionId.RowNumber));

                    notFoundDivisionIdsRowNums.Add(notFoundDivisionId.RowNumber);
                }
            }

            //Local Site Id
            var localSiteIds = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();

            var rowsWithNotFoundLocalSiteIds = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteId) && !localSiteIds.Any(s => string.Equals(x.LocalSiteId, s, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotFoundLocalSiteIds.Any())
            {
                foreach (var notFoundLocalSiteId in rowsWithNotFoundLocalSiteIds)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = InventoryFileHeaders.LocalSiteID,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, InventoryFileHeaders.LocalSiteID),
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue = notFoundLocalSiteId.LocalSiteId
                        }
                    }), notFoundLocalSiteId.RowNumber));

                    notFoundLocalSiteIdsRowNums.Add(notFoundLocalSiteId.RowNumber);
                }
            }

            //Text 2 is Division Id.
            var localSiteIdList = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToList();

            var rowsWithNotMatchLocalSiteIdOrNotMatchDivIds = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.DivisionId) && !string.IsNullOrWhiteSpace(x.LocalSiteId) && !localSiteIdList.Any(s => string.Equals(x.LocalSiteId, s.TextId, StringComparison.OrdinalIgnoreCase) && string.Equals(x.DivisionId, s.Text2, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotMatchLocalSiteIdOrNotMatchDivIds.Any())
            {
                foreach (var mismatchRow in rowsWithNotMatchLocalSiteIdOrNotMatchDivIds)
                {
                    if (notFoundLocalSiteIdsRowNums.Contains(mismatchRow.RowNumber) || notFoundDivisionIdsRowNums.Contains(mismatchRow.RowNumber))
                    {
                        continue;
                    }

                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = InventoryFileHeaders.LocalSiteID,
                            ErrorMessage =  ValidationMessages.DivisionIdAndLocalSiteIdMismatch,
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue =mismatchRow.DivisionId+"|"+ mismatchRow.LocalSiteId
                        }
                    }), mismatchRow.RowNumber));
                }
            }
        }

        private void ValidateDRICode(FileValidationResult result, List<InventoryDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var driCodes = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.CommodityCode, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidDRICodes = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.DRICode) && !driCodes.Any(s => string.Equals(x.DRICode, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidDRICodes.Any())
            {
                foreach (var invalidDRICode in rowsWithInvalidDRICodes)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = InventoryFileHeaders.DRICode,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, InventoryFileHeaders.DRICode),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidDRICode.DRICode
                        }
                    }), invalidDRICode.RowNumber));
                }
            }
        }

        private void ValidateUOM(FileValidationResult result, List<InventoryDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var uoms = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.UOM, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidUOMs = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.UOM) && !uoms.Any(s => string.Equals(x.UOM, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidUOMs.Any())
            {
                foreach (var invalidUOM in rowsWithInvalidUOMs)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = InventoryFileHeaders.UOM,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, InventoryFileHeaders.UOM),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidUOM.UOM
                        }
                    }), invalidUOM.RowNumber));
                }
            }
        }

        private void ValidateCurrencyCode(FileValidationResult result, List<InventoryDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var currencyCodes = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Currency, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidCurrencyCodes = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.CurrencyCode) && !currencyCodes.Any(s => string.Equals(x.CurrencyCode, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidCurrencyCodes.Any())
            {
                foreach (var invalidCurrencyCode in rowsWithInvalidCurrencyCodes)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = InventoryFileHeaders.CurrencyCode,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, InventoryFileHeaders.CurrencyCode),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidCurrencyCode.CurrencyCode
                        }
                    }), invalidCurrencyCode.RowNumber));
                }
            }
        }

        private void ValidateDuplicateRecords(FileValidationResult result, List<InventoryDataRow> dataRows)
        {
            var duplicateRowNumbers = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            if (duplicateRowNumbers.Count > 0)
            {
                InventoryDataRow dataRowToShow;
                foreach (var duplicateRowNumber in duplicateRowNumbers)
                {
                    dataRowToShow = dataRows.First(x => x.RowNumber == duplicateRowNumber);

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
                                        AttemptedValue = $"DivisionId: {dataRowToShow.DivisionId}, LocalSiteId: {dataRowToShow.LocalSiteId}, PartNumber: {dataRowToShow.PartNumber}, InventoryDate: {dataRowToShow.InventoryDateStr}"
                                    }
                                }
                                ), duplicateRowNumber));
                }
            }
        }

        private void ValidateDependentColumns(FileValidationResult result, JobModel job, IJobFileModel file, List<InventoryDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var itemFile = job.GetJobFileByFileType(Constants.FileTypes.Item);

            //If the item file was not included with the validation, bail out
            if (itemFile == null || itemFile.DataRows == null || !itemFile.DataRows.Any())
            {
                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = InventoryFileHeaders.PartNumber,
                            ErrorMessage = string.Format(ValidationMessages.DependentFileNotFoundOrNoData, DependentFiles.Item),
                            ErrorCode = DataRowErrorStatus.Warning,
                            AttemptedValue = ""
                        }
                    }), -1));

                return;
            }           

            var itemDataRows = itemFile.DataRows.Cast<ItemDataRow>().ToList();

            //Create a lookup dictionary that contains the list of items for each local site
            //This is for optimizing the performance when validating local site id and part number combination
            //by avoiding loop through all item data rows for each inventory data row
            Dictionary<string, HashSet<string>> itemLookupBytSiteId = new Dictionary<string, HashSet<string>>();

            foreach (var item in itemDataRows)
            {
                if(itemLookupBytSiteId.TryGetValue(item.LocalSiteId, out var itemList))
                {                   
                    itemList.Add(item.PartNumber);
                }
                else
                {
                    itemLookupBytSiteId.Add(item.LocalSiteId, new HashSet<string>() { item.PartNumber });
                }
            }

            //Get the list of sites from master data as a dictionary so we can query it quickly
            var siteMasterData = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.TextId, x => x.Text5);

            //search local site id and part number in Inventory file, then get the reference for this local site id from master data site table,
            //and match with Item file's local site id and part number
            var inventoryRowsWithLocalSiteIdAndPartNumbers = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteId) && !string.IsNullOrWhiteSpace(x.PartNumber)).ToList();            

            if (inventoryRowsWithLocalSiteIdAndPartNumbers.Count > 0)
            {
                foreach (var invDataRow in inventoryRowsWithLocalSiteIdAndPartNumbers)
                {
                    //Find the local site reference from master data based on the local site id in inventory file
                    //var localSite = siteMasterData.TryGetValue.FirstOrDefault(x => string.Equals(x.TextId, invDataRow.LocalSiteId, StringComparison.OrdinalIgnoreCase));
                    if (siteMasterData.TryGetValue(invDataRow.LocalSiteId, out var localSiteReference))
                    {
                        if (localSiteReference != null && !string.IsNullOrEmpty(localSiteReference))
                        {
                            //Locate the list based on the SitePartReference(Text5) column in master data
                            if (itemLookupBytSiteId.TryGetValue(localSiteReference, out var itemDataRowsForSite))
                            {
                                //If we found the list for the given site, Look for the part number in the list
                                //If we don't find it, then add the validation error
                                if (!itemDataRowsForSite.Contains(invDataRow.PartNumber))
                                {
                                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                                    {
                                        new ValidationFailure()
                                        {
                                            PropertyName = InventoryFileHeaders.PartNumber,
                                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, InventoryFileHeaders.PartNumber),
                                            ErrorCode = DataRowErrorStatus.Warning,
                                            AttemptedValue = invDataRow.PartNumber
                                        }
                                    }), invDataRow.RowNumber));
                                }
                            }
                        }
                    }
                }
            }
        }

        public FileCalculateStatistics_Inventory GetFileCalculateStatistics(IJobFileModel job, int recordCount)
        {
            var invResult = new FileCalculateStatistics_Inventory();
            invResult.TotalRecords = recordCount;

            var dataRows = job.DataRows.Cast<InventoryDataRow>().ToList();

            var filterOutNullQuantityData = dataRows.Where(x => x.Quantity != null);
            var recordOrderByQuantity = filterOutNullQuantityData.Any() ? filterOutNullQuantityData.OrderBy(r => r.Quantity).ToList() : null;
            invResult.QuantityMin = recordOrderByQuantity == null ? "" : recordOrderByQuantity.First().QuantityOriginalStr;
            invResult.QuantityMax = recordOrderByQuantity == null ? "" : recordOrderByQuantity.Last().QuantityOriginalStr;

            var filterOutNullStandardCostData = dataRows.Where(x => x.StandardCost != null);
            var recordOrderByStandardCostData = filterOutNullStandardCostData.Any() ? filterOutNullStandardCostData.OrderBy(r => r.StandardCost).ToList() : null;
            invResult.StandardCostMin = recordOrderByStandardCostData == null ? "" : recordOrderByStandardCostData.First().StandardCostOriginalStr;
            invResult.StandardCostMax = recordOrderByStandardCostData == null ? "" : recordOrderByStandardCostData.Last().StandardCostOriginalStr;

            var filterOutNullTotalValueData = dataRows.Where(x => x.TotalValue != null);
            var recordOrderByTotalValueData = filterOutNullTotalValueData.Any() ? filterOutNullTotalValueData.OrderBy(r => r.TotalValue).ToList() : null;
            invResult.TotalValueMin = recordOrderByTotalValueData == null ? "" : recordOrderByTotalValueData.First().TotalValueOriginalStr;
            invResult.TotalValueMax = recordOrderByTotalValueData == null ? "" : recordOrderByTotalValueData.Last().TotalValueOriginalStr;

            var filterOutNullInventoryDateData = dataRows.Where(x => x.InventoryDate != null);
            invResult.InventoryDateMin = filterOutNullInventoryDateData.Any() ? filterOutNullInventoryDateData.Min(r => r.InventoryDate).Value.ToString("MM/dd/yyyy") : "";
            invResult.InventoryDateMax = filterOutNullInventoryDateData.Any() ? filterOutNullInventoryDateData.Max(r => r.InventoryDate).Value.ToString("MM/dd/yyyy") : "";

            return invResult;
        }
    }
}
