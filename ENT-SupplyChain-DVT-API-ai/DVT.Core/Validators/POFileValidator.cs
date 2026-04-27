using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18239208: 1 - Validation Service - Validate PO File
    /// </summary>
    public class POFileValidator
    {
        public FileValidationResult ValidateAsync(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData)
        {
            StopwatchLogger logger = new StopwatchLogger("POValidateFilesAsync");
            logger.Start();

            FileValidationResult fileResult = new FileValidationResult(file.JobFileId, file.FileName);
            PODataRowStaticValidator staticValidator = new PODataRowStaticValidator();

            List<string> headers = file.FileHeader;

            CommonValidation.ValidateHeaders(fileResult, headers, POFileHeaderList);
            logger.StopAndLog("POFileValidator Validate Headers", true);

            object rowObj;

            var dataRows = file.DataRows.Cast<PODataRow>().ToList();
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

                var dataRow = rowObj as PODataRow;

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

                staticValidator = new PODataRowStaticValidator();
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
            logger.StopAndLog("POFileValidator Static Validation", true);

            ValidateMasterData(fileResult, dataRows, masterData, job.DivisionId);
            logger.StopAndLog("POFileValidator Validate Master Data", true);

            ValidateDuplicateRecords(fileResult, dataRows);
            logger.StopAndLog("POFileValidator Validate Duplicate Records", true);

            ValidateDependentColumns(fileResult, job, file, dataRows, masterData);
            logger.StopAndLog("POFileValidator Validate Dependent Columns", false);

            fileResult.AdditionalInfo = logger.Log.ToString();
            return fileResult;
        }

        private void ValidateMasterData(FileValidationResult result, List<PODataRow> dataRows, IEnumerable<MasterData> masterData, Guid divisionId)
        {
            //var currentDivision = masterData.FirstOrDefault(m => m.ItemId == divisionId);

            ValidateDivisionIdAndLocalSiteId(result, dataRows, masterData);

            ValidateCurrencyCode(result, dataRows, masterData);

            ValidatePOTerms(result, dataRows, masterData);

            ValidateFreightTerms(result, dataRows, masterData);
        }

        private void ValidateDivisionIdAndLocalSiteId(FileValidationResult result, List<PODataRow> dataRows, IEnumerable<MasterData> masterData)
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
                                 PropertyName = POFileHeaders.DivisionId,
                                 ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POFileHeaders.DivisionId),
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
                            PropertyName = POFileHeaders.LocalSiteID,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POFileHeaders.LocalSiteID),
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
                            PropertyName = POFileHeaders.LocalSiteID,
                            ErrorMessage =  ValidationMessages.DivisionIdAndLocalSiteIdMismatch,
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue =mismatchLocalSiteId.DivisionID+"|"+  mismatchLocalSiteId.LocalSiteID
                        }
                    }), mismatchLocalSiteId.RowNumber));
                }
            }
        }

        private void ValidateCurrencyCode(FileValidationResult result, List<PODataRow> dataRows, IEnumerable<MasterData> masterData)
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
                            PropertyName = POFileHeaders.CurrencyCode,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POFileHeaders.CurrencyCode),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidData.CurrencyCode
                        }
                    }), invalidData.RowNumber));
                }
            }
        }

        private void ValidatePOTerms(FileValidationResult result, List<PODataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var poTerms = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.PaymentTerm, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidPOTerms = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.POTerms) && !poTerms.Any(s => string.Equals(x.POTerms, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidPOTerms.Any())
            {
                foreach (var invalidPOTerm in rowsWithInvalidPOTerms)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = POFileHeaders.POTerms,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POFileHeaders.POTerms),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidPOTerm.POTerms
                        }
                    }), invalidPOTerm.RowNumber));
                }
            }
        }

        private void ValidateFreightTerms(FileValidationResult result, List<PODataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var freightTerms = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.FreightTerms, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidFreightTerms = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.FreightTerms) && !freightTerms.Any(s => string.Equals(x.FreightTerms, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidFreightTerms.Any())
            {
                foreach (var invalidFreightTerm in rowsWithInvalidFreightTerms)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = POFileHeaders.FreightTerms,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POFileHeaders.FreightTerms),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidFreightTerm.FreightTerms
                        }
                    }), invalidFreightTerm.RowNumber));
                }
            }
        }

        private void ValidateDuplicateRecords(FileValidationResult result, List<PODataRow> dataRows)
        {
            //These fields cannot be the same for multiple records in the PO File
            //|DIVISION ID|+|LOCAL SITE ID|+|PO NUMBER|

            var duplicateRowNumbers = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            if (duplicateRowNumbers.Count > 0)
            {
                PODataRow dataRowToShow;
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
                                        AttemptedValue = $"DivisionId: {dataRowToShow.DivisionID}, LocalSiteId: {dataRowToShow.LocalSiteID}, PONumber: {dataRowToShow.PONumber}"
                                    }
                                }
                                ), duplicateRowNumber));
                }
            }
        }

        private void ValidateDependentColumns(FileValidationResult result, JobModel job, IJobFileModel file, List<PODataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var supplierFile = job.GetJobFileByFileType(Constants.FileTypes.Supplier);

            //If the suppliers file was not included with the validation , bail out
            if (supplierFile == null || supplierFile.DataRows == null || !supplierFile.DataRows.Any())
            {
                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                 {
                     new ValidationFailure()
                     {
                         PropertyName = POFileHeaders.SupplierID,
                         ErrorMessage = string.Format(ValidationMessages.DependentFileNotFoundOrNoData, DependentFiles.Supplier),
                         ErrorCode = DataRowErrorStatus.Warning,
                         AttemptedValue = ""

                     }
                 }), -1));

                return;
            }

            var supplierDataRows = supplierFile.DataRows.Cast<SupplierDataRow>().ToList();

            var rowsWithLocalSiteIdAndSupplierId = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteID) && !string.IsNullOrWhiteSpace(x.SupplierID)).ToList();

            //Get the list of sites from master data as a dictionary so we can query it quickly. Text4 is supplier site reference
            var siteMasterData = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.TextId, x => x.Text4);

            if (rowsWithLocalSiteIdAndSupplierId.Any())
            {
                foreach (var dataRow in rowsWithLocalSiteIdAndSupplierId)
                {
                    //Find the local site reference from master data based on the local site id in inventory file
                    if (siteMasterData.TryGetValue(dataRow.LocalSiteID, out var localSiteReference))
                    {
                        if (localSiteReference != null)
                        {
                            //Text4 is Local Site Id For Supplier File
                            var supplierDataRowsMatchesSiteReferenceAndSupplierId = supplierDataRows.Where(x => string.Equals(x.LocalSiteId, localSiteReference, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(x.SupplierId, dataRow.SupplierID, StringComparison.OrdinalIgnoreCase));

                            if (!supplierDataRowsMatchesSiteReferenceAndSupplierId.Any())
                            {
                                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                                {
                                     new ValidationFailure()
                                     {
                                        PropertyName = POFileHeaders.SupplierID,
                                        ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, POFileHeaders.SupplierID),
                                        ErrorCode = DataRowErrorStatus.Warning,
                                        AttemptedValue = dataRow.SupplierID
                                     }
                                }), dataRow.RowNumber));
                            }
                        }
                    }
                }
            }
        }

        public FileCalculateStatistics_PO GetFileCalculateStatistics(IJobFileModel job, int recordCount)
        {
            var statisticsResult = new FileCalculateStatistics_PO();
            statisticsResult.TotalRecords = recordCount;

            var dataRows = job.DataRows.Cast<PODataRow>().ToList();

            statisticsResult.OrderDateMin = dataRows.Min(r => r.OrderDate)?.ToString("MM/dd/yyyy") ?? null;
            statisticsResult.OrderDateMax = dataRows.Max(r => r.OrderDate)?.ToString("MM/dd/yyyy") ?? null;

            statisticsResult.LatestAmendmentMin = dataRows.Min(r => r.LatestAmendment)?.ToString("MM/dd/yyyy") ?? null;
            statisticsResult.LatestAmendmentMax = dataRows.Max(r => r.LatestAmendment)?.ToString("MM/dd/yyyy") ?? null;

            return statisticsResult;
        }
    }
}
