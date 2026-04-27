using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 16006516: 1 - Validation Service - Validate Vir File
    /// </summary>
    public class VirFileValidator
    {
        private StopwatchLogger _logger = new StopwatchLogger("VirFileValidator");

        public FileValidationResult ValidateAsync(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData)
        {
            _logger = new StopwatchLogger("POValidateFilesAsync");
            _logger.Start();

            FileValidationResult fileResult = new FileValidationResult(file.JobFileId, file.FileName);
            VirDataRowStaticValidator staticValidator = new VirDataRowStaticValidator();

            List<string> headers = file.FileHeader;
            CommonValidation.ValidateHeaders(fileResult, headers, VirFileHeaderList);
            _logger.StopAndLog("VirFileValidator Validate Headers", true);

            object rowObj;

            var virDataRows = file.DataRows.Cast<VirDataRow>().ToList();
            if (virDataRows == null || !virDataRows.Any())
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

                var dataRow = rowObj as VirDataRow;

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

                staticValidator = new VirDataRowStaticValidator();
                var rowResult = staticValidator.Validate(dataRow);

                if (rowResult != null && !rowResult.IsValid && rowResult.Errors != null)
                {
                    //Since ReceiptNumber's regular expression "^[\x20-\x7E]+$" have special characters, we need to clear the FormattedMessagePlaceholderValues to avoid serialization issues
                    rowResult.Errors.ToList().ForEach(e =>
                    {
                        e.FormattedMessagePlaceholderValues = null;
                        e.CustomState = null;
                    });
                    fileResult.RowValidationResults.Add(new FileRowValidationResult(rowResult, dataRow.RowNumber));
                }
            }

            _logger.StopAndLog("VirFileValidator Static Validation", true);

            ValidateMasterData(fileResult, virDataRows, masterData, job.DivisionId);
            _logger.StopAndLog("VirFileValidator Validate Master Data", true);

            ValidateCustomField(fileResult, virDataRows);
            _logger.StopAndLog("VirFileValidator Validate Custom Field", true);

            ValidateDuplicateRecords(fileResult, virDataRows);
            _logger.StopAndLog("VirFileValidator Validate Duplicate Records", true);

            ValidateDependentColumns(fileResult, job, file, virDataRows, masterData);
            _logger.StopAndLog("VirFileValidator Validate Dependent Columns", false);

            fileResult.AdditionalInfo = _logger.Log.ToString();
            return fileResult;
        }

        private void ValidateDuplicateRecords(FileValidationResult result, List<VirDataRow> dataRows)
        {
            //These fields cannot be the same for multiple records in the virFile
            //DIVISION ID|| ~|| LOCAL SITE ID|| ~|| RECEIPT_NUMBER || ~|| PO_NUMBER || ~|| PO_LINE_NUMBER || ~|| PART_NUMBER || ~|| DATE_RECEIVED || ~|| COMMITTED_DATE || ~|| RELEASE#

            var duplicateRowNumbers = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            if (duplicateRowNumbers.Count > 0)
            {
                VirDataRow dataRowToShow;
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
                                        AttemptedValue = string.Format("DivisionId: {0}, LocalSiteId: {1}, ReceiptNumber: {2}, PoNumber: {3}, POLineNumber: {4}, PartNumber: {5}, DateReceived: {6}, CommittedDate: {7}, Release: {8}",
                                        dataRowToShow.DivisionId, dataRowToShow.LocalSiteId, dataRowToShow.ReceiptNumber, dataRowToShow.PoNumber, dataRowToShow.POLineNumber, dataRowToShow.PartNumber, dataRowToShow.DateReceivedStr
                                        , dataRowToShow.CommittedDateStr, dataRowToShow.ReleaseOriginalStr)
                                    }
                                }
                                ), duplicateRowNumber));
                }
            }
        }

        /// <summary>
        /// User Story 19001793: File Validation Service (VIR) - API - Enhancement
        /// Task 19555287: Optimize the logic for Supplier, Local site id and Part number, Local site id        
        /// </summary>
        /// <param name="result"></param>
        /// <param name="job"></param>
        /// <param name="file"></param>
        /// <param name="masterData"></param>
        private void ValidateDependentColumns(FileValidationResult result, JobModel job, IJobFileModel file, List<VirDataRow> virDataRows, IEnumerable<MasterData> masterData)
        {
            var supplierResult = ValidateSuppliers(job, file, virDataRows, masterData);
            var itemResult = ValidateItemData(job, file, virDataRows, masterData);

            var combineResult = FileValidationResult.Combine(supplierResult, itemResult);
            if (combineResult.RowValidationResults.Any())
            {
                result.RowValidationResults.AddRange(combineResult.RowValidationResults);
            }
        }

        /// <summary>
        /// User Story 19001793: File Validation Service (VIR) - API - Enhancement
        /// </summary>
        /// <param name="job"></param>
        /// <param name="virFile"></param>
        /// <param name="virDataRows"></param>
        /// <param name="masterData"></param>
        /// <returns></returns>
        private FileValidationResult ValidateSuppliers(JobModel job, IJobFileModel virFile, List<VirDataRow> virDataRows, IEnumerable<MasterData> masterData)
        {
            var result = new FileValidationResult(virFile.JobFileId, virFile.FileName);

            var supplierFile = job.GetJobFileByFileType(Constants.FileTypes.Supplier);

            //Task 19555287: Optimize the logic for Supplier, Local site id and Part number, Local site id --- validation will still proceed as normal even if we only validate VIR file or any other file type, part number and supplier ID field for VIR validation check will return as WARNING since we cannot find item and supplier files
            if (supplierFile == null || supplierFile.DataRows == null || !supplierFile.DataRows.Any())
            {
                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.SupplierId,
                            ErrorMessage = string.Format(ValidationMessages.DependentFileNotFoundOrNoData, DependentFiles.Supplier),
                            ErrorCode = DataRowErrorStatus.Warning,
                        }
                    }), -1));

                return result;
            }

            var supplierDataRows = supplierFile.DataRows.Cast<SupplierDataRow>().ToList();

            //Make a HashSet of the supplier data rows using the LocalSiteId and supplier id as key
            var supplierDataRowHashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var supplierDataRow in supplierDataRows)
            {
                if (!string.IsNullOrWhiteSpace(supplierDataRow.LocalSiteId) && !string.IsNullOrWhiteSpace(supplierDataRow.SupplierId))
                {
                    supplierDataRowHashSet.Add(supplierDataRow.LocalSiteId + supplierDataRow.SupplierId);
                }
            }

            //Task 20626080: Enhancement for Local Site id & Part Number match, Local site id & Supplier_id match - PI26.1.4
            //search local site id and supplier id in vir file, then get the reference for this local site id from master data site table, and match with Supplier file's local site id and supplier id
            var rowsWithLocalSiteIdAndSupplierId = virDataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteId) && !string.IsNullOrWhiteSpace(x.SupplierId)).ToList();

            //Get the list of sites from master data as a dictionary so we can query it quickly. Text4 is supplier site reference
            var siteMasterData = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.TextId, x => x.Text4);

            //Loop through the Vir rows
            if (rowsWithLocalSiteIdAndSupplierId.Any())
            {
                foreach (var dataRow in rowsWithLocalSiteIdAndSupplierId)
                {
                    //Find the local site reference from master data based on the local site id in inventory file
                    if (siteMasterData.TryGetValue(dataRow.LocalSiteId, out var localSiteReference))
                    {
                        if (localSiteReference != null)
                        {
                            //Text4 is Local Site Id For Supplier File
                            if (!supplierDataRowHashSet.Contains(localSiteReference + dataRow.SupplierId))
                            {
                                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                                {
                                     new ValidationFailure()
                                     {
                                        PropertyName = POFileHeaders.SupplierID,
                                        ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POFileHeaders.SupplierID),
                                        ErrorCode = DataRowErrorStatus.Warning,
                                        AttemptedValue = dataRow.SupplierId
                                     }
                                }), dataRow.RowNumber));
                            }
                        }
                    }                    
                }
            }

            return result;
        }

        private FileValidationResult ValidateItemData(JobModel job, IJobFileModel virFile, List<VirDataRow> virDataRows, IEnumerable<MasterData> masterData)
        {
            var result = new FileValidationResult(virFile.JobFileId, virFile.FileName);
            var itemFile = job.GetJobFileByFileType(Constants.FileTypes.Item);

            if (itemFile == null || itemFile.DataRows == null || !itemFile.DataRows.Any())
            {
                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.PartNumber,
                            ErrorMessage = string.Format(ValidationMessages.DependentFileNotFoundOrNoData, DependentFiles.Item),
                            ErrorCode = DataRowErrorStatus.Warning,
                            AttemptedValue = ""
                        }
                    }), -1));

                return result;
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

            //Task 20626080: Enhancement for Local Site id & Part Number match, Local site id & Supplier_id match - PI26.1.4
            //search local site id and part number in vir file, then get the reference for this local site id from master data site table, and match with Item file's local site id and part number
            var rowsWithLocalSiteIdAndPartNumbers = virDataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteId) && !string.IsNullOrWhiteSpace(x.PartNumber)).ToList();

            //Get the list of sites from master data as a dictionary so we can query it quickly. Text5 is site part reference
            var siteMasterData = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.TextId, x => x.Text5);

            if (rowsWithLocalSiteIdAndPartNumbers.Any())
            {
                foreach (var virDataRow in rowsWithLocalSiteIdAndPartNumbers)
                {
                    //Find the local site reference from master data based on the local site id in item file
                    if (siteMasterData.TryGetValue(virDataRow.LocalSiteId, out var localSitePartReference))
                    {
                        if (localSitePartReference != null)
                        {
                            //Look for the part number in the item file HashSet using the local site reference + part number as key. If we don't find it, add a validation error.
                            if (!itemDataRowHashSet.Contains(localSitePartReference + virDataRow.PartNumber))
                            {
                                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                                    {
                                        new ValidationFailure()
                                        {
                                            PropertyName = POItemFileHeaders.PartNumber,
                                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POItemFileHeaders.PartNumber),
                                            ErrorCode = DataRowErrorStatus.Warning,
                                            AttemptedValue = virDataRow.PartNumber
                                        }
                                    }), virDataRow.RowNumber));
                            }
                        }
                    }                   
                }
            }

            return result;
        }

        /// <summary>
        ///Ensure that the Division Id, Local Site Id, UOM, Currency Code, Freight Terms are in the master data table
        /// </summary>
        /// <param name="result"></param>
        /// <param name="virDataRows"></param>
        /// <param name="masterData"></param>
        /// <param name="divisionId"></param>
        private void ValidateMasterData(FileValidationResult result, List<VirDataRow> virDataRows, IEnumerable<MasterData> masterData, Guid divisionId)
        {
            var currentDivision = masterData.FirstOrDefault(m => m.ItemId == divisionId);

            ValidateDivisionIdAndLocalSiteId(result, virDataRows, masterData, currentDivision);

            ValidateUOM(result, virDataRows, masterData);

            ValidateCurrencyCode(result, virDataRows, masterData);

            ValidateFreightTerms(result, virDataRows, masterData);
        }

        /// <summary>
        /// User Story 19836917: VIR - Division ID, Local Site ID must exist based on BU ORG Table - Enhancement
        /// </summary>
        /// <param name="result"></param>
        /// <param name="virDataRows"></param>
        /// <param name="division"></param>
        private void ValidateDivisionIdAndLocalSiteId(FileValidationResult result, List<VirDataRow> virDataRows, IEnumerable<MasterData> masterData, MasterData division)
        {
            var notFoundDivisionIdsRowNums = new List<int>();
            var notFoundLocalSiteIdsRowNums = new List<int>();

            var divisionIds = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Division, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();

            var rowsWithNotFoundDivisionIds = virDataRows.Where(r => !string.IsNullOrWhiteSpace(r.DivisionId) && !divisionIds.Any(d => string.Equals(d, r.DivisionId, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotFoundDivisionIds.Any())
            {
                foreach (var notFoundDivisionId in rowsWithNotFoundDivisionIds)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure()
                            {
                                 PropertyName = VirFileHeaders.DivisionId,
                                 ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, VirFileHeaders.DivisionId),
                                 ErrorCode = DataRowErrorStatus.Critical,
                                 AttemptedValue = notFoundDivisionId.DivisionId
                            }
                        }), notFoundDivisionId.RowNumber));

                    notFoundDivisionIdsRowNums.Add(notFoundDivisionId.RowNumber);
                }
            }

            //Local Site Id
            // User Story 19836917: VIR - Division ID, Local Site ID must exist based on BU ORG Table - Enhancement
            var localSiteIds = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();

            var rowsWithNotFoundLocalSiteIds = virDataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteId) && !localSiteIds.Any(s => string.Equals(x.LocalSiteId, s, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotFoundLocalSiteIds.Any())
            {
                foreach (var notFoundLocalSiteId in rowsWithNotFoundLocalSiteIds)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.LocalSiteId,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, VirFileHeaders.LocalSiteId),
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue = notFoundLocalSiteId.LocalSiteId
                        }
                    }), notFoundLocalSiteId.RowNumber));

                    notFoundLocalSiteIdsRowNums.Add(notFoundLocalSiteId.RowNumber);
                }
            }

            //Text 2 is Division Id.
            var localSiteIdList = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToList();

            var rowsWithNotMatchLocalSiteIdOrNotMatchDivIds = virDataRows.Where(x => !string.IsNullOrWhiteSpace(x.DivisionId) && !string.IsNullOrWhiteSpace(x.LocalSiteId) && !localSiteIdList.Any(s => string.Equals(x.LocalSiteId, s.TextId, StringComparison.OrdinalIgnoreCase) && string.Equals(x.DivisionId, s.Text2, StringComparison.OrdinalIgnoreCase))).ToList();

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
                            PropertyName = VirFileHeaders.LocalSiteId,
                            ErrorMessage =  ValidationMessages.DivisionIdAndLocalSiteIdMismatch,
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue = mismatchRow.LocalSiteId
                        }
                    }), mismatchRow.RowNumber));
                }
            }

            //Bug 19483998: [QA Bug] - Status should be Critical --- cannot to lower if value is null.
            //find the invalid division Ids - not in master data
            //var rowsWithInvalidDivisionIds = virDataRows.Where(r => !string.IsNullOrWhiteSpace(r.DivisionID) && !string.Equals(r.DivisionID, division.TextId, StringComparison.OrdinalIgnoreCase)).ToList();

        }

        private void ValidateUOM(FileValidationResult result, List<VirDataRow> virDataRows, IEnumerable<MasterData> masterData)
        {
            var uoms = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.UOM, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidUOMs = virDataRows.Where(x => !string.IsNullOrWhiteSpace(x.UOM) && !uoms.Any(s => string.Equals(x.UOM, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidUOMs.Any())
            {
                foreach (var invalidUOM in rowsWithInvalidUOMs)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.Uom,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, VirFileHeaders.Uom),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidUOM.UOM
                        }
                    }), invalidUOM.RowNumber));
                }
            }
        }

        private void ValidateCurrencyCode(FileValidationResult result, List<VirDataRow> virDataRows, IEnumerable<MasterData> masterData)
        {
            var currencyCodes = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Currency, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidCurrencyCodes = virDataRows.Where(x => !string.IsNullOrWhiteSpace(x.CurrencyCode) && !currencyCodes.Any(s => string.Equals(x.CurrencyCode, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidCurrencyCodes.Any())
            {
                foreach (var invalidCurrencyCode in rowsWithInvalidCurrencyCodes)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.CurrencyCode,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, VirFileHeaders.CurrencyCode),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidCurrencyCode.CurrencyCode
                        }
                    }), invalidCurrencyCode.RowNumber));
                }
            }
        }

        private void ValidateFreightTerms(FileValidationResult result, List<VirDataRow> virDataRows, IEnumerable<MasterData> masterData)
        {
            var freightTerms = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.FreightTerms, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidFreightTerms = virDataRows.Where(x => !string.IsNullOrWhiteSpace(x.FreightTerms) && !freightTerms.Any(s => string.Equals(x.FreightTerms, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidFreightTerms.Any())
            {
                foreach (var invalidFreightTerm in rowsWithInvalidFreightTerms)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.FreightTerms,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, VirFileHeaders.FreightTerms),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidFreightTerm.FreightTerms
                        }
                    }), invalidFreightTerm.RowNumber));
                }
            }
        }

        private void ValidateCustomField(FileValidationResult fileResult, List<VirDataRow> virDataRows)
        {
            //The value of Pure Uploaded Cost should be P or L           
            var invalidPureLoadedCost = virDataRows.Where(x => !string.IsNullOrEmpty(x.PureLoadedCost) && !(string.Equals(x.PureLoadedCost, "P", StringComparison.OrdinalIgnoreCase) || string.Equals(x.PureLoadedCost, "L", StringComparison.OrdinalIgnoreCase))).ToList();
            if (invalidPureLoadedCost.Any())
            {
                foreach (var invalid in invalidPureLoadedCost)
                {
                    fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.PureLoadedCost,
                            ErrorMessage =  ValidationMessages.InvalidValue,
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalid.PureLoadedCost
                        }
                    }), invalid.RowNumber));
                }
            }

            //The value of Direct Indirect should be D
            var invalidDirectIndirect = virDataRows.Where(x => !string.IsNullOrEmpty(x.DirectIndirect) && !string.Equals(x.DirectIndirect, "D", StringComparison.OrdinalIgnoreCase)).ToList();
            if (invalidDirectIndirect.Any())
            {
                foreach (var invalid in invalidDirectIndirect)
                {
                    fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.DirectIndirect,
                            ErrorMessage =  ValidationMessages.InvalidValue,
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalid.DirectIndirect
                        }
                    }), invalid.RowNumber));
                }
            }

            //The value of Intra Div should be N
            var invalidIntraDiv = virDataRows.Where(x => !string.IsNullOrEmpty(x.IntraDiv) && !string.Equals(x.IntraDiv, "N", StringComparison.OrdinalIgnoreCase)).ToList();
            if (invalidIntraDiv.Any())
            {
                foreach (var invalid in invalidIntraDiv)
                {
                    fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.IntraDiv,
                            ErrorMessage =  ValidationMessages.InvalidValue,
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalid.IntraDiv
                        }
                    }), invalid.RowNumber));
                }
            }

            //The value of Invoice Price Paid Must be equal to (Quantity Received) x (Unit Price)
            var invalidInvoicePricePaid = virDataRows.Where(x => x.QuantityReceived != null && x.UnitPrice != null && x.InvoicePricePaid != null && x.InvoicePricePaid != (x.QuantityReceived * x.UnitPrice)).ToList();
            if (invalidInvoicePricePaid.Any())
            {
                foreach (var invalid in invalidInvoicePricePaid)
                {
                    fileResult.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = VirFileHeaders.InvoicePricePaid,
                            ErrorMessage =  ValidationMessages.InvoicePricePaidMismatch,
                            ErrorCode = DataRowErrorStatus.Warning,
                            AttemptedValue = invalid.InvoicePricePaidOriginalStr
                        }
                    }), invalid.RowNumber));
                }
            }
        }

        /// <summary>
        ///  User Story 16255749: 12 - Job Service - Calculate statistics - Vir/Supplier/Item
        /// </summary>
        /// <param name="job"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public FileCalculateStatistics_Vir GetFileCalculateStatistics(IJobFileModel job, int recordCount)
        {
            var virResult = new FileCalculateStatistics_Vir();
            virResult.TotalRecords = recordCount;

            var dataRows = job.DataRows.Cast<VirDataRow>().ToList();

            var filterOutNullQuantityOrderedData = dataRows.Where(x => x.QuantityOrdered != null);
            var filterOutNullQuantityReceivedData = dataRows.Where(x => x.QuantityReceived != null);
            var filterOutNullInvoicePricePaidData = dataRows.Where(x => x.InvoicePricePaid != null);
            var filterOutNullUnitPriceData = dataRows.Where(x => x.UnitPrice != null);

            var recordOrderByQuantityOrdered = filterOutNullQuantityOrderedData.Any() ? filterOutNullQuantityOrderedData.OrderBy(r => r.QuantityOrdered).ToList() : null;
            virResult.QuantityOrderedMin = recordOrderByQuantityOrdered == null ? "" : recordOrderByQuantityOrdered.First().QuantityOrderedOriginalStr;
            virResult.QuantityOrderedMax = recordOrderByQuantityOrdered == null ? "" : recordOrderByQuantityOrdered.Last().QuantityOrderedOriginalStr;

            var recordOrderByQuantityReceived = filterOutNullQuantityReceivedData.Any() ? filterOutNullQuantityReceivedData.OrderBy(r => r.QuantityReceived).ToList() : null;
            virResult.QuantityReceivedMin = recordOrderByQuantityReceived == null ? "" : recordOrderByQuantityReceived.First().QuantityReceivedOriginalStr;
            virResult.QuantityReceivedMax = recordOrderByQuantityReceived == null ? "" : recordOrderByQuantityReceived.Last().QuantityReceivedOriginalStr;

            virResult.DateReceivedMin = dataRows.Min(r => r.DateReceived)?.ToString("MM/dd/yyyy") ?? null;
            virResult.DateReceivedMax = dataRows.Max(r => r.DateReceived)?.ToString("MM/dd/yyyy") ?? null;

            var recordOrderByInvoicePricePaid = filterOutNullInvoicePricePaidData.Any() ? filterOutNullInvoicePricePaidData.OrderBy(r => r.InvoicePricePaid).ToList() : null;
            virResult.InvoicePricePaidMin = recordOrderByInvoicePricePaid == null ? "" : recordOrderByInvoicePricePaid.First().InvoicePricePaidOriginalStr;
            virResult.InvoicePricePaidMax = recordOrderByInvoicePricePaid == null ? "" : recordOrderByInvoicePricePaid.Last().InvoicePricePaidOriginalStr;

            var recordOrderByUnitPrice = filterOutNullUnitPriceData.Any() ? filterOutNullUnitPriceData.OrderBy(r => r.UnitPrice).ToList() : null;
            virResult.UnitPriceMin = recordOrderByUnitPrice == null ? "" : recordOrderByUnitPrice.First().UnitPriceOriginalStr;
            virResult.UnitPriceMax = recordOrderByUnitPrice == null ? "" : recordOrderByUnitPrice.Last().UnitPriceOriginalStr;

            virResult.CommittedDateMin = dataRows.Min(r => r.CommittedDate)?.ToString("MM/dd/yyyy") ?? null;
            virResult.CommittedDateMax = dataRows.Max(r => r.CommittedDate)?.ToString("MM/dd/yyyy") ?? null;

            return virResult;
        }
    }
}
