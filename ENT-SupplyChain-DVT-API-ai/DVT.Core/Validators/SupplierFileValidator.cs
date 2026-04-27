using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using FluentValidation.Results;
using System.Text;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18238976: 1 - Validation Service - Validate Supplier File
    /// </summary>
    public class SupplierFileValidator
    {
        public FileValidationResult ValidateAsync(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData)
        {
            StopwatchLogger logger = new StopwatchLogger("SupplierValidateFilesAsync");
            logger.Start();

            FileValidationResult fileResult = new FileValidationResult(file.JobFileId, file.FileName);
            List<string> headers = file.FileHeader;

            CommonValidation.ValidateHeaders(fileResult, headers, SupplierFileHeaderList);
            logger.StopAndLog("SupplierValidator Validate Headers", true);

            object rowObj;

            var dataRows = file.DataRows.Cast<SupplierDataRow>().ToList();
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

                var dataRow = rowObj as SupplierDataRow;

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

                SupplierDataRowStaticValidator staticValidator = new SupplierDataRowStaticValidator();
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

            logger.StopAndLog("SupplierFileValidator Static Validation", true);

            ValidateMasterData(fileResult, dataRows, masterData, job.DivisionId);
            logger.StopAndLog("SupplierFileValidator Validate Master Data", true);

            ValidateDuplicateRecords(fileResult, dataRows);
            logger.StopAndLog("SupplierFileValidator Validate Duplicate Records", false);

            fileResult.AdditionalInfo = logger.Log.ToString();
            return fileResult;
        }

        private void ValidateMasterData(FileValidationResult result, List<SupplierDataRow> dataRows, IEnumerable<MasterData> masterData, Guid divisionId)
        {
            //var currentDivision = masterData.FirstOrDefault(m => m.ItemId == divisionId);

            ValidateDivisionIdAndLocalSiteId(result, dataRows, masterData);

            ValidateCountryInfo(result, dataRows, masterData);
        }

        private void ValidateDivisionIdAndLocalSiteId(FileValidationResult result, List<SupplierDataRow> dataRows, IEnumerable<MasterData> masterData)
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
                                 PropertyName = SupplierFileHeaders.DivisionId,
                                 ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, SupplierFileHeaders.DivisionId),
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
                            PropertyName = SupplierFileHeaders.LocalSiteId,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, SupplierFileHeaders.LocalSiteId),
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
                            PropertyName = SupplierFileHeaders.LocalSiteId,
                            ErrorMessage =  ValidationMessages.DivisionIdAndLocalSiteIdMismatch,
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue =  mismatchLocalSiteId.LocalSiteId
                        }
                    }), mismatchLocalSiteId.RowNumber));
                }
            }
        }

        private void ValidateCountryInfo(FileValidationResult result, List<SupplierDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var countries = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.Country, StringComparison.OrdinalIgnoreCase)).ToList();

            //Country Code
            var rowsWithInvalidCountryCodes = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.CountryCode) && !countries.Any(s => string.Equals(x.CountryCode, s.TextId, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithInvalidCountryCodes.Any())
            {
                foreach (var invalidCountry in rowsWithInvalidCountryCodes)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = SupplierFileHeaders.CountryCode,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, SupplierFileHeaders.CountryCode),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidCountry.CountryCode
                        }
                    }), invalidCountry.RowNumber));
                }
            }

            //Country and Country Code mismatch
            var rowsWithNotMatchCountryAndCountryCodes = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.Country) && !string.IsNullOrWhiteSpace(x.CountryCode) && !countries.Any(s => string.Equals(x.Country, s.ItemName, StringComparison.OrdinalIgnoreCase) && string.Equals(x.CountryCode, s.TextId, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rowsWithNotMatchCountryAndCountryCodes.Any())
            {
                foreach (var invalidRow in rowsWithNotMatchCountryAndCountryCodes)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = SupplierFileHeaders.Country,
                            ErrorMessage = string.Format(ValidationMessages.XDoesNotMatch, SupplierFileHeaders.Country),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidRow.Country
                        }
                    }), invalidRow.RowNumber));
                }
            }
        }

        private void ValidateDuplicateRecords(FileValidationResult result, List<SupplierDataRow> dataRows)
        {
            //These fields cannot be the same for multiple records in the SupplierFile
            //|DIVISION ID|+|LOCAL SITE ID|+|SUPPLIER ID|

            var duplicateRowNumbers = IDataRowDuplicateFinder.FindDuplicatesRowNumbers(dataRows);

            if (duplicateRowNumbers.Count > 0)
            {
                SupplierDataRow dataRowToShow;
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
                                        AttemptedValue = $"DivisionId: {dataRowToShow.DivisionId}, LocalSiteId: {dataRowToShow.LocalSiteId}, SupplierId: {dataRowToShow.SupplierId}"
                                    }
                                }
                                ), duplicateRowNumber));
                }
            }

        }

        public FileCalculateStatistics_Supplier GetFileCalculateStatistics(IJobFileModel job, int recordCount)
        {
            var result = new FileCalculateStatistics_Supplier();

            var dataRows = job.DataRows.Cast<SupplierDataRow>().ToList();

            result.TotalRecords = recordCount;

            return result;
        }
    }
}
