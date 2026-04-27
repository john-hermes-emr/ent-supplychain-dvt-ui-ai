using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18239116: 1 - Validation Service - Validate UOM File
    /// </summary>
    internal class UOMFileValidator
    {
        private StopwatchLogger _logger = new StopwatchLogger("UOMFileValidator");

        public FileValidationResult ValidateAsync(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData)
        {
            _logger = new StopwatchLogger("UOMFileValidator");
            _logger.Start();

            FileValidationResult fileResult = new FileValidationResult(file.JobFileId, file.FileName);
            UOMDataRowStaticValidator staticValidator = new UOMDataRowStaticValidator();

            List<string> headers = file.FileHeader;
            CommonValidation.ValidateHeaders(fileResult, headers, UOMFileHeaderList);
            _logger.StopAndLog("UOMFileValidator Validate Headers", true);

            object rowObj;

            var dataRows = file.DataRows.Cast<UOMDataRow>().ToList();
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

                var dataRow = rowObj as UOMDataRow;

                //Calculate the uniqueness key that we'll use later when finding duplicates
                //Comment it out for now since the files are small enough that we don't need optimization
                //dataRow?.GenerateUniquenessKey();

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

                staticValidator = new UOMDataRowStaticValidator();
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

            _logger.StopAndLog("UOMFileValidator Static Validation", true);

            ValidateMasterData(fileResult, dataRows, masterData, job.DivisionId);
            _logger.StopAndLog("UOMFileValidator Validate Master Data", true);

            ValidateDuplicateRecords(fileResult, dataRows);
            _logger.StopAndLog("UOMFileValidator Validate Duplicate Records", true);

            ValidateDependentColumns(fileResult, job, file, dataRows, masterData);
            _logger.StopAndLog("UOMFileValidator Validate Dependent Columns", false);

            fileResult.AdditionalInfo = _logger.Log.ToString();
            return fileResult;
        }

        private void ValidateMasterData(FileValidationResult result, List<UOMDataRow> dataRows, IEnumerable<MasterData> masterData, Guid divisionId)
        {
            ValidateDivisionIdAndLocalSiteId(result, dataRows, masterData);

            ValidateLocalUOM(result, dataRows, masterData);

            ValidateBaseUOM(result, dataRows, masterData);
        }

        private void ValidateDivisionIdAndLocalSiteId(FileValidationResult result, List<UOMDataRow> dataRows, IEnumerable<MasterData> masterData)
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
                                 PropertyName = UOMFileHeaders.DivisionID,
                                 ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, UOMFileHeaders.DivisionID),
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
                            PropertyName = UOMFileHeaders.LocalSiteID,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, UOMFileHeaders.LocalSiteID),
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
                            PropertyName = UOMFileHeaders.LocalSiteID,
                            ErrorMessage =  ValidationMessages.DivisionIdAndLocalSiteIdMismatch,
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue = mismatchLocalSiteId.LocalSiteID
                        }
                    }), mismatchLocalSiteId.RowNumber));
                }
            }
        }

        private void ValidateLocalUOM(FileValidationResult result, List<UOMDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var UOMs = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.UOM, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidUOMs = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalUOM) && !UOMs.Any(s => string.Equals(x.LocalUOM, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidUOMs.Any())
            {
                foreach (var invalidData in rowsWithInvalidUOMs)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = UOMFileHeaders.LocalUOM,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, UOMFileHeaders.LocalUOM),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidData.LocalUOM
                        }
                    }), invalidData.RowNumber));
                }
            }
        }

        private void ValidateBaseUOM(FileValidationResult result, List<UOMDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var UOMs = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.UOM, StringComparison.OrdinalIgnoreCase)).Select(x => x.TextId).ToList();
            var rowsWithInvalidUOMs = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.BaseUOM) && !UOMs.Any(s => string.Equals(x.BaseUOM, s, StringComparison.OrdinalIgnoreCase))).ToList();
            if (rowsWithInvalidUOMs.Any())
            {
                foreach (var invalidData in rowsWithInvalidUOMs)
                {
                    result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = UOMFileHeaders.BaseUOM,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, UOMFileHeaders.BaseUOM),
                            ErrorCode = DataRowErrorStatus.Errors,
                            AttemptedValue = invalidData.BaseUOM
                        }
                    }), invalidData.RowNumber));
                }
            }
        }

        private void ValidateDuplicateRecords(FileValidationResult result, List<UOMDataRow> dataRows)
        {
            //These fields cannot be the same for multiple records in the UOM File
            //|DIVISION ID|+|LOCAL SITE ID|+|PART NUMBER|+|LOCAL UOM|+|BASE UOM|

            // Check for duplicate records in the UOM File
            //Get a strongely typed version of the list of data rows
            var dataRowsCopy = new List<UOMDataRow>();
            dataRows.ForEach(d => dataRowsCopy.Add(new UOMDataRow()
            {
                DivisionID = d.DivisionID?.ToLower(),
                LocalSiteID = d.LocalSiteID?.ToLower(),
                PartNumber = d.PartNumber?.ToLower(),
                LocalUOM = d.LocalUOM?.ToLower(),
                BaseUOM = d.BaseUOM?.ToLower(),
                RowNumber = d.RowNumber
            }));

            var duplicates = dataRowsCopy.GroupBy(x => new
            {
                x.DivisionID,
                x.LocalSiteID,
                x.PartNumber,
                x.LocalUOM,
                x.BaseUOM
            }).Where(g => g.Count() > 1)
              .Select(g => new { Key = g.Key, RowNumbers = g.Select(a => a.RowNumber).ToList() })
              .ToList();

            if (duplicates.Any())
            {
                UOMDataRow dataRowToShow;
                foreach (var duplicate in duplicates)
                {
                    foreach (var rowNum in duplicate.RowNumbers)
                    {
                        dataRowToShow = dataRows.First(x => x.RowNumber == rowNum);
                        result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure()
                            {
                                PropertyName = CustomFileHeaders.SourceRecordIDfields,
                                ErrorMessage =ValidationMessages.DuplicateRecordFound,
                                ErrorCode = DataRowErrorStatus.Critical,
                                AttemptedValue = $"DivisionID: {dataRowToShow.DivisionID}, LocalSiteID: {dataRowToShow.LocalSiteID}, PartNumber: {dataRowToShow.PartNumber}, LocalUOM: {dataRowToShow.LocalUOM}, BaseUOM: {dataRowToShow.BaseUOM}"
                            }
                        }), rowNum));
                    }
                }
            }
        }

        private void ValidateDependentColumns(FileValidationResult result, JobModel job, IJobFileModel file, List<UOMDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var itemFile = job.GetJobFileByFileType(Constants.FileTypes.Item);

            if (itemFile == null || itemFile.DataRows == null || !itemFile.DataRows.Any())
            {
                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = UOMFileHeaders.PartNumber,
                            ErrorMessage = string.Format(ValidationMessages.DependentFileNotFoundOrNoData, DependentFiles.Item),
                            ErrorCode = DataRowErrorStatus.Warning,
                            AttemptedValue = ""
                        }
                    }), -1));

                return;
            }

            var itemDataRows = itemFile.DataRows.Cast<ItemDataRow>().ToList();

            //Task 23780441: UOM - Part number must be included in the Item master file (item_o.txt)
            var rowsWithLocalSiteIdAndPartNumbers = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.LocalSiteID) && !string.IsNullOrWhiteSpace(x.PartNumber)).ToList();

            var siteMasterData = masterData.Where(x => string.Equals(x.TableName, MasterDataTableNames.SiteMaster, StringComparison.OrdinalIgnoreCase)).ToList();

            if (rowsWithLocalSiteIdAndPartNumbers.Any())
            {
                foreach (var dataRow in rowsWithLocalSiteIdAndPartNumbers)
                {
                    var localSite = siteMasterData.FirstOrDefault(x => string.Equals(x.TextId, dataRow.LocalSiteID, StringComparison.OrdinalIgnoreCase));

                    if (localSite != null)
                    {
                        //Text5 is Local Site Id For Item File
                        var itemDataRowsMatchesSiteReferenceAndPartNumber = itemDataRows.Where(x => string.Equals(x.LocalSiteId, localSite.Text5, StringComparison.OrdinalIgnoreCase) 
                        && string.Equals(x.PartNumber, dataRow.PartNumber, StringComparison.OrdinalIgnoreCase));

                        if (!itemDataRowsMatchesSiteReferenceAndPartNumber.Any())
                        {
                            result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                            {
                                new ValidationFailure()
                                {
                                    PropertyName = UOMFileHeaders.PartNumber,
                                    ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, UOMFileHeaders.PartNumber),
                                    ErrorCode = DataRowErrorStatus.Warning,
                                    AttemptedValue =dataRow.PartNumber
                                }
                            }), dataRow.RowNumber));
                        }
                    }
                }
            }
        }

        public FileCalculateStatistics_UOM GetFileCalculateStatistics(IJobFileModel job, int recordCount)
        {
            var statisticsResult = new FileCalculateStatistics_UOM();
            statisticsResult.TotalRecords = recordCount;

            var dataRows = job.DataRows.Cast<UOMDataRow>().ToList();

            var filterOutNullConversionRateData = dataRows.Where(x => x.ConversionRate != null);

            var recordOrderByConversionRate = filterOutNullConversionRateData.Any() ? filterOutNullConversionRateData.OrderBy(r => r.ConversionRate).ToList() : null;
            statisticsResult.ConversionRateMin = recordOrderByConversionRate == null ? "" : recordOrderByConversionRate.First().ConversionRateOriginalStr;
            statisticsResult.ConversionRateMax = recordOrderByConversionRate == null ? "" : recordOrderByConversionRate.Last().ConversionRateOriginalStr;

            return statisticsResult;
        }
    }
}
