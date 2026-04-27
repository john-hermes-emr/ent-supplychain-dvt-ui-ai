using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18239248: 1 - Validation Service - Validate POITEM File
    /// </summary>
    public class POItemFileValidator
    {
        private StopwatchLogger logger = new StopwatchLogger("POItemFileValidator");

        public FileValidationResult ValidateAsync(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData)
        {
            logger = new StopwatchLogger("POItemFileValidator", file.DataRows.Count);
            logger.Start();

            FileValidationResult fileResult = new FileValidationResult(file.JobFileId, file.FileName);
            POItemDataRowStaticValidator staticValidator = new POItemDataRowStaticValidator();

            List<string> headers = file.FileHeader;

            CommonValidation.ValidateHeaders(fileResult, headers, POItemFileHeaderList);
            logger.StopAndLog("POItemFileValidator Validate Headers", true);

            object rowObj;

            var dataRows = file.DataRows.Cast<POItemDataRow>().ToList();
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

                var dataRow = rowObj as POItemDataRow;

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

                staticValidator = new POItemDataRowStaticValidator();
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

            logger.StopAndLog("POItemFileValidator Static Validation", true);

            ValidateMasterData(fileResult, dataRows, masterData, job.DivisionId);
            logger.StopAndLog("POItemFileValidator Validate Master Data", true);

            ValidateDuplicateRecords(fileResult, dataRows);
            logger.StopAndLog("POItemFileValidator Validate Duplicate Records", true);

            ValidateDependentColumns(fileResult, job, file, dataRows, masterData);
            logger.StopAndLog("POItemFileValidator Validate Dependent Columns", false);

            fileResult.AdditionalInfo = logger.Log.ToString();
            return fileResult;
        }

        private void ValidateMasterData(FileValidationResult result, List<POItemDataRow> dataRows, IEnumerable<MasterData> masterData, Guid divisionId)
        {
            ValidateDivisionIdAndLocalSiteId(result, dataRows, masterData);

            ValidateUOM(result, dataRows, masterData);

            ValidateCurrencyCode(result, dataRows, masterData);
        }

        private void ValidateDivisionIdAndLocalSiteId(FileValidationResult result, List<POItemDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var notFoundDivisionIdsRowNums = new List<int>();
            var notFoundLocalSiteIdsRowNums = new List<int>();

            var divisionIds = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Division, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();

            var rowsWithNotFoundDivisionIds = dataRows.Where(r => !string.IsNullOrWhiteSpace(r.DivisionID) && !divisionIds.Any(d => string.Equals(d, r.DivisionID, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotFoundDivisionIds.Any())
            {
                foreach (var notFoundDivisionId in rowsWithNotFoundDivisionIds)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure()
                            {
                                 PropertyName = POItemFileHeaders.DivisionID,
                                 ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POItemFileHeaders.DivisionID),
                                 ErrorCode = DataRowErrorStatus.Critical,
                                 AttemptedValue = notFoundDivisionId.DivisionID
                            }
                        }), notFoundDivisionId.RowNumber));

                    notFoundDivisionIdsRowNums.Add(notFoundDivisionId.RowNumber);
                }
            }

            //Local Site Id             
            var localSiteIds = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();

            var rowsWithNotFoundLocalSiteIds = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteID) && !localSiteIds.Any(s => string.Equals(x.LocalSiteID, s, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotFoundLocalSiteIds.Any())
            {
                foreach (var notFoundLocalSiteId in rowsWithNotFoundLocalSiteIds)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = POItemFileHeaders.LocalSiteID,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POItemFileHeaders.LocalSiteID),
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue = notFoundLocalSiteId.LocalSiteID
                        }
                    }), notFoundLocalSiteId.RowNumber));

                    notFoundLocalSiteIdsRowNums.Add(notFoundLocalSiteId.RowNumber);
                }
            }

            //Text 2 is Division Id.
            var localSiteIdList = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToList();

            var rowsWithNotMatchLocalSiteIdOrNotMatchDivIds = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.DivisionID) && !string.IsNullOrWhiteSpace(x.LocalSiteID) && !localSiteIdList.Any(s => string.Equals(x.LocalSiteID, s.TextId, StringComparison.OrdinalIgnoreCase) && string.Equals(x.DivisionID, s.Text2, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotMatchLocalSiteIdOrNotMatchDivIds.Any())
            {
                foreach (var mismatchLocalSiteId in rowsWithNotMatchLocalSiteIdOrNotMatchDivIds)
                {
                    if (notFoundLocalSiteIdsRowNums.Contains(mismatchLocalSiteId.RowNumber) || notFoundDivisionIdsRowNums.Contains(mismatchLocalSiteId.RowNumber))
                    {
                        continue;
                    }

                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = POItemFileHeaders.LocalSiteID,
                            ErrorMessage =  ValidationMessages.DivisionIdAndLocalSiteIdMismatch,
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue = mismatchLocalSiteId.LocalSiteID
                        }
                    }), mismatchLocalSiteId.RowNumber));
                }
            }
        }
        private void ValidateUOM(FileValidationResult result, List<POItemDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var UOMs = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.UOM, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidUOMs = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.UOM) && !UOMs.Any(s => string.Equals(x.UOM, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidUOMs.Any())
            {
                foreach (var invalidData in rowsWithInvalidUOMs)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = POItemFileHeaders.UOM,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POItemFileHeaders.UOM),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidData.UOM
                        }
                    }), invalidData.RowNumber));
                }
            }
        }

        private void ValidateCurrencyCode(FileValidationResult result, List<POItemDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var currencies = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Currency, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidCurrencies = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.CurrencyCode) && !currencies.Any(s => string.Equals(x.CurrencyCode, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidCurrencies.Any())
            {
                foreach (var invalidData in rowsWithInvalidCurrencies)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = POItemFileHeaders.CurrencyCode,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POItemFileHeaders.CurrencyCode),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidData.CurrencyCode
                        }
                    }), invalidData.RowNumber));
                }
            }
        }

        private void ValidateDuplicateRecords(FileValidationResult result, List<POItemDataRow> dataRows)
        {
            //These fields cannot be the same for multiple records in the POItem File
            //|DIVISION ID|+|LOCAL SITE ID|+|PO NUMBER|+|PO LINE NUMBER|+|PART NUMBER|+|COMMITTED DATE|+|REQUESTED DATE|+|RELEASE#|

            var duplicateRowNumbers = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            if (duplicateRowNumbers.Count > 0)
            {
                POItemDataRow dataRowToShow;
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
                                        AttemptedValue = $"DivisionID: {dataRowToShow.DivisionID}, LocalSiteID: {dataRowToShow.LocalSiteID}, PONumber: {dataRowToShow.PONumber}, POLineNumber: {dataRowToShow.POLineNumber}, PartNumber: {dataRowToShow.PartNumber}, CommittedDate: {dataRowToShow.CommittedDateStr}, RequestedDate: {dataRowToShow.RequestedDate}, Release: {dataRowToShow.ReleaseOriginalStr}"
                                    }
                                }
                                ), duplicateRowNumber));
                }
            }
        }

        private void ValidateDependentColumns(FileValidationResult result, JobModel job, IJobFileModel file, List<POItemDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            //Validate the PO Number in the POItem file
            //Get the PO File that is uploaded together with the POItem file and verify it's not empty.
            var POFile = job.GetJobFileByFileType(Constants.FileTypes.Po);

            if (POFile == null || POFile.DataRows == null || !POFile.DataRows.Any())
            {
                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = POItemFileHeaders.PONumber,
                            ErrorMessage = string.Format(ValidationMessages.DependentFileNotFoundOrNoData, DependentFiles.PO),
                            ErrorCode = DataRowErrorStatus.Warning,
                            AttemptedValue = ""
                        }
                    }), -1));

                return;
            }


            var PODataRows = POFile.DataRows.Cast<PODataRow>().ToList();

            var rowsWithPONumber = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.PONumber)).ToList();

            //Build a HashSet of PO numbers from the PO file for efficient lookup. 
            var uniquePoNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var poRow in PODataRows)
            {
                if (!string.IsNullOrWhiteSpace(poRow.PONumber))
                {
                    uniquePoNumbers.Add(poRow.PONumber);
                }
            }

            //If we found PO Numbers and we have POItems with PO Numbers, validate that the PO Numbers in the POItem file exist in the PO file.
            if (uniquePoNumbers != null && uniquePoNumbers.Any() && rowsWithPONumber.Any())
            {
                foreach (var dataRow in rowsWithPONumber)
                {
                    if (!uniquePoNumbers.Contains(dataRow.PONumber))
                    {
                        result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                                {
                                    new ValidationFailure()
                                    {
                                       PropertyName = POItemFileHeaders.PONumber,
                                       ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POItemFileHeaders.PONumber),
                                       ErrorCode = DataRowErrorStatus.Warning,
                                        AttemptedValue = dataRow.PONumber
                                    }
                                }), dataRow.RowNumber));
                    }
                }
            }

            //User Story 23772169: POITEM - Part number must be included in the Item master file (item_o.txt)
            var itemFile = job.GetJobFileByFileType(Constants.FileTypes.Item);

            if (itemFile == null || itemFile.DataRows == null || !itemFile.DataRows.Any())
            {
                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = POItemFileHeaders.PartNumber,
                            ErrorMessage = string.Format(ValidationMessages.DependentFileNotFoundOrNoData, DependentFiles.Item),
                            ErrorCode = DataRowErrorStatus.Warning,
                            AttemptedValue = ""
                        }
                    }), -1));

                return;
            }

            var itemDataRows = itemFile.DataRows.Cast<ItemDataRow>().ToList();

            //Make a HashSet of the item data rows using the LocalSiteId and PartNumber as key
            var itemDataRowHashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var itemDataRow in itemDataRows)
            {
                if (!string.IsNullOrWhiteSpace(itemDataRow.LocalSiteId) && !string.IsNullOrWhiteSpace(itemDataRow.PartNumber))
                {
                    itemDataRowHashSet.Add(itemDataRow.LocalSiteId + itemDataRow.PartNumber);
                }
            }

            var rowsWithLocalSiteIdAndPartNumbers = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteID) && !string.IsNullOrWhiteSpace(x.PartNumber)).ToList();

            //Get the list of sites from master data as a dictionary so we can query it quickly. Text5 is site part reference
            var siteMasterData = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.TextId, x => x.Text5);

            if (rowsWithLocalSiteIdAndPartNumbers.Any())
            {
                foreach (var dataRow in rowsWithLocalSiteIdAndPartNumbers)
                {
                    //Find the local site reference from master data based on the local site id in item file
                    if (siteMasterData.TryGetValue(dataRow.LocalSiteID, out var localSitePartReference))
                    {
                        if (localSitePartReference != null)
                        {
                            //Look for the part number in the item file HashSet using the local site reference + part number as key. If we don't find it, add a validation error.
                            if (!itemDataRowHashSet.Contains(localSitePartReference + dataRow.PartNumber))
                            {
                                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                                    {
                                        new ValidationFailure()
                                        {
                                            PropertyName = POItemFileHeaders.PartNumber,
                                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POItemFileHeaders.PartNumber),
                                            ErrorCode = DataRowErrorStatus.Warning,
                                            AttemptedValue =dataRow.PartNumber
                                        }
                                    }), dataRow.RowNumber));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// User Story 18516331: Validation Service - Generate statistics Report for POITEM - API Controller
        /// </summary>
        /// <param name="job"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public FileCalculateStatistics_POItem GetFileCalculateStatistics(IJobFileModel job, int recordCount)
        {
            var statisticsResult = new FileCalculateStatistics_POItem();
            statisticsResult.TotalRecords = recordCount;

            var dataRows = job.DataRows.Cast<POItemDataRow>().ToList();

            var filterOutNullUnitCostData = dataRows.Where(x => x.UnitCost != null);
            var recordOrderByStandardCostData = filterOutNullUnitCostData.Any() ? filterOutNullUnitCostData.OrderBy(r => r.UnitCost).ToList() : null;
            statisticsResult.UnitCostMin = recordOrderByStandardCostData == null ? "" : recordOrderByStandardCostData.First().UnitCostOriginalStr;
            statisticsResult.UnitCostMax = recordOrderByStandardCostData == null ? "" : recordOrderByStandardCostData.Last().UnitCostOriginalStr;

            var filterOutNullOrderedValueData = dataRows.Where(x => x.OrderedValue != null);
            var recordOrderByOrderedValueData = filterOutNullOrderedValueData.Any() ? filterOutNullOrderedValueData.OrderBy(r => r.OrderedValue).ToList() : null;
            statisticsResult.OrderedValueMin = recordOrderByOrderedValueData == null ? "" : recordOrderByOrderedValueData.First().OrderedValueOriginalStr;
            statisticsResult.OrderedValueMax = recordOrderByOrderedValueData == null ? "" : recordOrderByOrderedValueData.Last().OrderedValueOriginalStr;

            var filterOutNullQuantityOrderedData = dataRows.Where(x => x.QuantityOrdered != null);
            var recordOrderByQuantityOrderedData = filterOutNullQuantityOrderedData.Any() ? filterOutNullQuantityOrderedData.OrderBy(r => r.QuantityOrdered).ToList() : null;
            statisticsResult.QuantityOrderedMin = recordOrderByQuantityOrderedData == null ? "" : recordOrderByQuantityOrderedData.First().QuantityOrderedOriginalStr;
            statisticsResult.QuantityOrderedMax = recordOrderByQuantityOrderedData == null ? "" : recordOrderByQuantityOrderedData.Last().QuantityOrderedOriginalStr;

            var filterOutNullQuantityReturnedData = dataRows.Where(x => x.QuantityReturned != null);
            var recordOrderByQuantityReturnedData = filterOutNullQuantityReturnedData.Any() ? filterOutNullQuantityReturnedData.OrderBy(r => r.QuantityReturned).ToList() : null;
            statisticsResult.QuantityReturnedMin = recordOrderByQuantityReturnedData == null ? "" : recordOrderByQuantityReturnedData.First().QuantityReturnedOriginalStr;
            statisticsResult.QuantityReturnedMax = recordOrderByQuantityReturnedData == null ? "" : recordOrderByQuantityReturnedData.Last().QuantityReturnedOriginalStr;

            statisticsResult.CommittedDateMin = dataRows.Min(r => r.CommittedDate)?.ToString("MM/dd/yyyy") ?? null;
            statisticsResult.CommittedDateMax = dataRows.Max(r => r.CommittedDate)?.ToString("MM/dd/yyyy") ?? null;

            statisticsResult.RequestedDateMin = dataRows.Min(r => r.RequestedDate)?.ToString("MM/dd/yyyy") ?? null;
            statisticsResult.RequestedDateMax = dataRows.Max(r => r.RequestedDate)?.ToString("MM/dd/yyyy") ?? null;

            var filterOutNullQtyLeftToReceiveData = dataRows.Where(x => x.QtyLeftToReceive != null);
            var recordOrderByQtyLeftToReceiveData = filterOutNullQtyLeftToReceiveData.Any() ? filterOutNullQtyLeftToReceiveData.OrderBy(r => r.QtyLeftToReceive).ToList() : null;
            statisticsResult.QtyLeftToReceiveMin = recordOrderByQtyLeftToReceiveData == null ? "" : recordOrderByQtyLeftToReceiveData.First().QtyLeftToReceiveOriginalStr;
            statisticsResult.QtyLeftToReceiveMax = recordOrderByQtyLeftToReceiveData == null ? "" : recordOrderByQtyLeftToReceiveData.Last().QtyLeftToReceiveOriginalStr;

            var filterOutNullValueLeftToReceiveData = dataRows.Where(x => x.ValueLeftToReceive != null);
            var recordOrderByValueLeftToReceiveData = filterOutNullValueLeftToReceiveData.Any() ? filterOutNullValueLeftToReceiveData.OrderBy(r => r.ValueLeftToReceive).ToList() : null;
            statisticsResult.ValueLeftToReceiveMin = recordOrderByValueLeftToReceiveData == null ? "" : recordOrderByValueLeftToReceiveData.First().ValueLeftToReceiveOriginalStr;
            statisticsResult.ValueLeftToReceiveMax = recordOrderByValueLeftToReceiveData == null ? "" : recordOrderByValueLeftToReceiveData.Last().ValueLeftToReceiveOriginalStr;

            return statisticsResult;
        }
    }
}
