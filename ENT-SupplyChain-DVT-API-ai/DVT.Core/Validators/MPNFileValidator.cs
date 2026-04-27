using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18239298: 1 - Validation Service - Validate MPN File
    /// </summary>
    internal class MPNFileValidator
    {
        private StopwatchLogger _logger = new StopwatchLogger("MPNFileValidator");

        public FileValidationResult ValidateAsync(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData)
        {
            _logger = new StopwatchLogger("MPNFileValidator");
            _logger.Start();

            FileValidationResult fileResult = new FileValidationResult(file.JobFileId, file.FileName);
            MPNDataRowStaticValidator staticValidator = new MPNDataRowStaticValidator();

            List<string> headers = file.FileHeader;

            CommonValidation.ValidateHeaders(fileResult, headers, MPNFileHeaderList);
            _logger.StopAndLog("MPNFileValidator Validate Headers", true);

            object rowObj;

            var dataRows = file.DataRows.Cast<MPNDataRow>().ToList();
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

                var dataRow = rowObj as MPNDataRow;

                //Calculate the uniqueness key that we'll use later when finding duplicates
                //Comment it out for now since the files are so small that we don't need special optimization
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

                staticValidator = new MPNDataRowStaticValidator();
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

            _logger.StopAndLog("MPNFileValidator Static Validation", true);

            ValidateMasterData(fileResult, dataRows, masterData, job.DivisionId);
            _logger.StopAndLog("MPNFileValidator Validate Master Data", true);

            ValidateDuplicateRecords(fileResult, dataRows);
            _logger.StopAndLog("MPNFileValidator Validate Duplicate Records", true);

            ValidateDependentColumns(fileResult, job, file, dataRows, masterData);
            _logger.StopAndLog("MPNFileValidator Validate Dependent Columns", false);

            fileResult.AdditionalInfo = _logger.Log.ToString();
            return fileResult;
        }

        private void ValidateMasterData(FileValidationResult result, List<MPNDataRow> dataRows, IEnumerable<MasterData> masterData, Guid divisionId)
        {
            ValidateDivisionIdAndLocalSiteId(result, dataRows, masterData);
        }

        private void ValidateDivisionIdAndLocalSiteId(FileValidationResult result, List<MPNDataRow> dataRows, IEnumerable<MasterData> masterData)
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
                                 PropertyName = MPNFileHeaders.DivisionID,
                                 ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, MPNFileHeaders.DivisionID),
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
                            PropertyName = MPNFileHeaders.LocalSiteID,
                            ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, MPNFileHeaders.LocalSiteID),
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
                            PropertyName = MPNFileHeaders.LocalSiteID,
                            ErrorMessage =  ValidationMessages.DivisionIdAndLocalSiteIdMismatch,
                            ErrorCode = DataRowErrorStatus.Critical,
                            AttemptedValue = $"{mismatchLocalSiteId.LocalSiteID} - {mismatchLocalSiteId.DivisionID}"
                        }
                    }), mismatchLocalSiteId.RowNumber));
                }
            }
        }

        private void ValidateDuplicateRecords(FileValidationResult result, List<MPNDataRow> dataRows)
        {
            //These fields cannot be the same for multiple records in the MPN File
            //|DIVISION ID|+|LOCAL SITE ID|+|PART NUMBER|+|MANUFACTURER PART NUMBER|+|LOCAL MANUFACTURER ID|+|MANUFACTURER NAME|

            // Check for duplicate records in the MPN File
            //Get a strongely typed version of the list of data rows
            var dataRowsCopy = new List<MPNDataRow>();

            //ObjectID fields cannot be the same in the MPN File
            var objIDRowsCopy = new List<MPNDataRow>();

            dataRows.ForEach(d =>
            {
                dataRowsCopy.Add(new MPNDataRow()
                {
                    DivisionID = d.DivisionID?.ToLower(),
                    LocalSiteID = d.LocalSiteID?.ToLower(),
                    PartNumber = d.PartNumber?.ToLower(),
                    ManufactureName = d.ManufactureName?.ToLower(),
                    ManufacturerPartNumber = d.ManufacturerPartNumber?.ToLower(),
                    LocalManufacturerID = d.LocalManufacturerID?.ToLower(),
                    RowNumber = d.RowNumber
                });

                if (!string.IsNullOrWhiteSpace(d.ObjectID))
                {
                    objIDRowsCopy.Add(new MPNDataRow()
                    {
                        ObjectID = d.ObjectID?.ToLower(),
                        RowNumber = d.RowNumber
                    });
                }
            });

            var duplicates = dataRowsCopy.GroupBy(x => new
            {
                x.DivisionID,
                x.LocalSiteID,
                x.PartNumber,
                x.ManufactureName,
                x.ManufacturerPartNumber,
                x.LocalManufacturerID,
            }).Where(g => g.Count() > 1)
              .Select(g => new { Key = g.Key, RowNumbers = g.Select(a => a.RowNumber).ToList() })
              .ToList();

            if (duplicates.Any())
            {
                MPNDataRow dataRowToShow;
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
                                AttemptedValue = $"DivisionID: {dataRowToShow.DivisionID}, LocalSiteID: {dataRowToShow.LocalSiteID}, PartNumber: {dataRowToShow.PartNumber}, ManufactureName: {dataRowToShow.ManufactureName}, ManufacturerPartNumber: {dataRowToShow.ManufacturerPartNumber}, LocalManufacturerID: {dataRowToShow.LocalManufacturerID}"
                            }
                        }), rowNum));
                    }
                }
            }

            var objIDDuplicateData = objIDRowsCopy.GroupBy(x => new
            {
                x.ObjectID,
            }).Where(g => g.Count() > 1)
              .Select(g => new { Key = g.Key, RowNumbers = g.Select(a => a.RowNumber).ToList() })
              .ToList();

            if (objIDDuplicateData.Any())
            {
                MPNDataRow dataRowToShow;
                foreach (var duplicate in objIDDuplicateData)
                {
                    foreach (var rowNum in duplicate.RowNumbers)
                    {
                        dataRowToShow = dataRows.First(x => x.RowNumber == rowNum);
                        result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure()
                            {
                                 PropertyName = MPNFileHeaders.ObjectID,
                                ErrorMessage = string.Format(ValidationMessages.DuplicateXFound, MPNFileHeaders.ObjectID),
                                ErrorCode = DataRowErrorStatus.Errors,
                                AttemptedValue = dataRowToShow.ObjectID
                            }
                        }), rowNum));
                    }
                }
            }
        }

        private void ValidateDependentColumns(FileValidationResult result, JobModel job, IJobFileModel file, List<MPNDataRow> dataRows, IEnumerable<MasterData> masterData)
        {
            var itemFile = job.GetJobFileByFileType(Constants.FileTypes.Item);

            if (itemFile == null || itemFile.DataRows == null || !itemFile.DataRows.Any())
            {
                result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure()
                        {
                            PropertyName = MPNFileHeaders.PartNumber,
                            ErrorMessage = string.Format(ValidationMessages.DependentFileNotFoundOrNoData, DependentFiles.Item),
                            ErrorCode = DataRowErrorStatus.Warning,
                            AttemptedValue = ""
                        }
                    }), -1));

                return;
            }

            var itemDataRows = itemFile.DataRows.Cast<ItemDataRow>().ToList();

            var rowsWithPartNumber = dataRows.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).ToList();

            var itemPartNumbers = itemDataRows.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).Select(p => p.PartNumber).ToList();

            if (itemPartNumbers != null && itemPartNumbers.Any() && rowsWithPartNumber.Any())
            {
                foreach (var dataRow in rowsWithPartNumber)
                {
                    if (!itemPartNumbers.Any(s => string.Equals(s, dataRow.PartNumber, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.RowValidationResults.Add(new FileRowValidationResult(new ValidationResult(new List<ValidationFailure>
                                {
                                    new ValidationFailure()
                                    {
                                       PropertyName = MPNFileHeaders.PartNumber,
                                       ErrorMessage = string.Format(ValidationMessages.XColumnNotFound, MPNFileHeaders.PartNumber),
                                       ErrorCode = DataRowErrorStatus.Warning,
                                       AttemptedValue = dataRow.PartNumber
                                    }
                                }), dataRow.RowNumber));
                    }
                }
            }
        }

        public FileCalculateStatistics_MPN GetFileCalculateStatistics(IJobFileModel job, int recordCount)
        {
            var statisticsResult = new FileCalculateStatistics_MPN();
            statisticsResult.TotalRecords = recordCount;

            return statisticsResult;
        }
    }
}
