using DVT.Core.Helper;
using DVT.Core.Models;

namespace DVT.Core
{
    public static class Constants
    {
        public static class StardardMessages
        {
            public static string BadRequest = "Bad Request";
            public static string ObjectCannotBeNull = "Object cannot be null";
            public static string UserInfoIdCannotBeNull = "User Info Id cannot be null or empty";
            public static string DivisionIdCannotBeNull = "Division Id cannot be null or empty";
            public static string FeedNumberCannotBeNull = "Feed Number cannot be null or empty";
            public static string JobIdCannotBeNull = "Job Id cannot be null or empty";
            public static string JobFileIdCannotBeNull = "Job File Id cannot be null or empty";
            public static string UpdateByBeNull = "Update By cannot be null or empty";
            public static string ObjectNotFound = "Object was not found";
            public static string InternalServerError = "Internal Server Errors";
            public static string ErrorPersistingToDb = "Errors persisting to database. Errors: ";
            public static string ItemNotFound = "Item was not found.";
            public static string NoFileFound = "Not found job file with ID:";
            public static string FieldCannotBeNull = "Field cannot be null or empty.";
            public static string ProtectedCannotBeDeleted = "This entity is protected and cannot be deleted.";
            public static string DuplicateValue = "Duplicate value found.";
            public static string InvalidOperation = "Invalid operation.";
            public static string UserNotFound = "User not found in system.";
            public static string NoJobFilesFound = "No Job Files found.";

            public static string ValidationError = "Validation Errors";

            public static string FileShareDoesNotExist = "File Share folder does not exist.";
            public static string UserHasNotSetLoadDirectory = "User hasn't set Load Folder.";
            public static string UserHasNotSetLogDirectory = "User hasn't set Log Folder.";
            public static string UserHasNotSetProductionDirectory = "User hasn't set Production Folder.";
            public static string LoadDirectoryCannotBeEmpty = "Load Folder path cannot be empty.";

            public static string LogDirectoryCannotBeEmpty = "Log Folder path cannot be empty.";
            public static string ProductionDirectoryCannotBeEmpty = "Production Folder path cannot be empty.";
            public static string ProductionDirectoryIsEmpty = "User hasn't set Production Folder path.";
            public static string UserDirectoryDoesNotExist = "User's folder does not exist.";
            public static string LogDirectoryIsEmpty = "User hasn't set Log Folder path.";

            public static string DirectoryDoesNotExist = "Folder '{0}' does not exist.";
            public static string FileDoesNotExist = "File '{0}' does not exist.";
            public static string GetUserFolderError = "Get user folder list error.";

            public static string UserSetFolder = "User '{0}' set Load Folder to '{1}', Log Folder to '{2}', Production Folder to '{3}'.";

            public static string UserSetLoadFolder = "User '{0}' set Load Folder to '{1}'.";

            public static string UserSetLogFolder = "User '{0}' set Log Folder to '{1}'.";

            public static string UserSetProductionFolder = "User '{0}' set Production Folder to '{1}'.";

            public static string NoFilesFoundInDirectory = "No matching files found in user's folder '{0}'";

            public static string IncorrectFileFormat = "Incorrect file name format '{0}'. ";
            public static string LoadFileError = "Load file '{0}' error: {1}";
            public static string JobCreatedSuccessfully = "Job created successfully.";

            public static string ActiveJobAlreadyExists = "There is an active job for the user.";
            public static string ExistingJobDoesNotMatchSelections = "The existing job does not match the user's selections";
            public static string ValidationCompletedSuccessfully = "Validation completed successfully.";

            public static string ValidationFailed = "Validation completed failed";
            public static string ValidationFailedMsg = "Validation failed: ";

            public static string JobDirectoryDoesNotExist = "Job directory does not exist";
            public static string JobWorkingFolderDoesNotExist = "Job working folder does not exist";

            public static string FileContentIsEmpty = "File content is empty.";
            public static string InvalidDivision = "Division not found.";
            public static string NoActiveJobFound = "There is no active job for the user.";
            public static string ActiveJobFound = "Active job found.";
            public static string JobFileStatusUpdatedSuccessfully = "Update job file Status successfully.";
            public static string JobFileStatusUpdatedFailed = "Update job file Status failed.";

            public static string SetJobFileValidationResultSuccessfully = "Update job file validation Result successfully.";
            public static string JobStatusUpdatedSuccessfully = "Update job status successfully.";
            public static string JobStatusUpdatedFailed = "Update job status Failed.";
            public static string JobUpdatedSuccessfully = "Job updated successfully.";
            public static string JobUpdatedFailed = "Job updated Failed.";
            public static string JobFilesLoadedSuccessfully = "Job files loaded successfully.";
            public static string JobFilesLoadedFailed = "Job files loaded failed.";
            public static string JobFilesLoadedAlready = "Job files loaded already.";

            public static string CleanupJobFilesSuccessfully = "Cleanup job files successfully.";
            public static string CleanupJobFilesFailed = "Cleanup job files failed.";
            public static string JobFileStatusAcceptedSuccessfully = "Job file status accepted successfully.";

            public static string IncorrectJobStatus = "Incorrect job Status.";
            public static string IncorrectJobFileStatus = "Incorrect job file Status.";

            public static string JobDeletedSuccessfully = "Job deleted successfully.";
            public static string JobDeletedFailed = "Job deleted Failed.";

            public static string JobFilesDeletedSuccessfully = "Job files deleted successfully.";
            public static string JobIsNotCompleted = "Job is not completed.";
            public static string CreateOutputFileSuccessful = "Create output file successfully.";
            public static string CreateOutputFileFailed = "Create output file failed.";
            public static string RefreshJobFailed = "Refresh job failed.";
            public static string AcceptValidationFailed = "Failed to accept validation result.";
            public static string AcceptValidationSuccessfully = "Validation result accepted successfully.";
            public static string NoFilesSelectedForValidation = "No files selected for validation";
            public static string NoFilesUnderThisJob = "No files under this job";
            public static string NoFilesSelectedForComplete = "No files selected for complete";
            public static string NoAcceptedFilesSelectedForComplete = "No accepted files selected for complete";

            public static string NoFilesLoadedForValidation = "No files loaded for validation";
            public static string NoMasterData = "No Master Data found.";
            public static string UserStartTheFileValidationSuccessfully = "User start the file validation successfully.";
            public static string CannotStartTheFileValidation = "Cannot start the file validation.";
            public static string CreateZipFileFailed = "Create zip file failed.";
            public static string ErrorOccurredWhileProcessingValidationErrors = "Errors occurred while get validation errors.";
            public static string NoErrorFound = "No errors found.";
            public static string ErrorOccurredWhileProcessingValidationStatistics = "Errors occurred while get validation Statistics.";
            public static string FileMarkedDeleted = "File {0} has been marked as deleted.";
            public static string NoNewJobFilesToLoad = "No new job files to load.";
            public static string AcceptValidationNotAllowed = "Accepting validation is not allowed.";
            public static string CompleteJobAndArchiveZipFilesFailed = "Complete job and archive zip files failed.";

            public static string CompleteJobAndArchiveZipFilesSuccessfully = "Complete job and archive zip files successfully.";
            public static string FileVerified = "The file '{0}' has been verified, status changed to {1}";
        }

        public static class WellKnownJobStatuses
        {
            public static string New = "NEW";
            public static string Uploaded = "UPLOADED";
            public static string Validated = "VALIDATED";
            public static string Failed = "FAILED";
            public static string InProgress = "IN_PROGRESS";
            public static string Completed = "COMPLETED";
        }

        public static class WellKnownFileStatuses
        {
            public static string New = "NEW";
            public static string Uploaded = "UPLOADED";
            public static string InProgress = "IN_PROGRESS";
            public static string Validated = "VALIDATED";
            public static string Failed = "FAILED";
            public static string Critical = "CRITICAL";
            public static string Errors = "ERRORS";
            public static string Warning = "WARNING";
            public static string Accepted = "ACCEPTED";
            //public static string Completed = "COMPLETED";
        }

        public static List<string> WellKnownFileValidatedStatusList = new List<string>
        {
            WellKnownFileStatuses.Validated,
            WellKnownFileStatuses.Critical,
            WellKnownFileStatuses.Errors,
            WellKnownFileStatuses.Warning
        };

        public static class WellKnownJobTemplates
        {
            public static string StandardTemplate = "Standard DVT Job FileType";
        }

        public static class DVTEntities
        {
            public static string ActivityLog = "ActivityLog";
            public static string Division = "Division";
            public static string UserInfo = "UserInfo";
            public static string Job = "Job";
            public static string JobFile = "JobFile";
            public static string JobLogFile = "JobLogFile";

        }

        public static class Operations
        {
            public static string CreateJob = "CreateJob";
            public static string LoadJobFiles = "LoadJobFiles";
            public static string UpdateJob = "UpdateJob";
            public static string DeleteJob = "DeleteJob";
            public static string PrepareJob = "PrepareJob";
            public static string UploadFiles = "UploadFiles";
            public static string ValidateFiles = "ValidateFiles";
            public static string GetActiveJob = "GetActiveJob";
            public static string GetJob = "GetJob";
            public static string UpdateJobStatus = "UpdateJobStatus";
            public static string AddJobFile = "AddJobFile";
            public static string DeleteJobFile = "DeleteJobFile";
            public static string DeleteExistingJobWorkingFolder = "DeleteExistingJobWorkingFolder";
            public static string SetJobFileValidationResult = "SetJobFileValidationResult";
            public static string UpdateJobFileStatus = "UpdateJobFileStatus";
            public static string AcceptValidationResult = "AcceptValidationResult";
            public static string AcceptJobFileStatus = "AcceptJobFileStatus";
            public static string CreateOutputFiles = "CreateOutputFiles";
            public static string RefreshJob = "RefreshJob";
            public static string RefreshAndDeleteJob = "RefreshAndDeleteJob";
            public static string RefreshAndDeleteJobFile = "RefreshAndDeleteJobFile";
            public static string CleanupJobWorkingDirectory = "CleanupJobWorkingDirectory";
            public static string GetJobFileValidationErrors = "GetJobFileValidationErrors";
            public static string GetJobStatistics = "GetJobStatistics";
            public static string GetJobFileStatistics = "GetJobFileStatistics";
            public static string GenerateJobStatistics = "GenerateJobStatistics";
            public static string CreateLogFiles = "CreateLogFiles";
            public static string CompleteJobAndArchiveZipFiles = "CompleteJobAndArchiveZipFiles";

        }

        public static class FileNameFormats
        {
            public static string DivAbbrev = "[div abbrev]";
            public static string FeedNumber = "[feed number]";
        }

        public static class FileTypes
        {
            public static string Supplier = "Supplier";
            public static string Item = "Item";
            public static string Inventory = "Inventory";
            public static string Vir = "Vir";
            public static string Po = "Po";
            public static string PoItem = "PoItem";
            public static string Mpn = "Mpn";
            public static string Uom = "Uom";
        }

        public static class DataRowErrorStatus
        {
            public static string Critical = "CRITICAL";
            public static string Warning = "WARNING";
            public static string Errors = "ERRORS";
        }

        public static List<string> VirFileHeaderList = new List<string>
        {
            "Division ID",
            "Local Site ID",
            "Receipt Number",
            "PO Number",
            "PO Line Number",
            "Supplier ID",
            "Part Number",
            "Supplier Part Number",
            "Quantity Ordered",
            "Quantity Received",
            "Date Received",
            "Invoice Price Paid",
            "Unit Price",
            "Pure_Loaded Cost",
            "Currency Code",
            "Intra-div",
            "Direct_indirect",
            "PO Terms",
            "Freight Terms",
            "UOM",
            "Title Transfer",
            "Port",
            "Release#",
            "Committed Date"
        };

        public class VirFileHeaders
        {
            public const string DivisionId = "Division ID";
            public const string LocalSiteId = "Local Site ID";
            public const string ReceiptNumber = "Receipt Number";
            public const string PoNumber = "PO Number";
            public const string PoLineNumber = "PO Line Number";
            public const string SupplierId = "Supplier ID";
            public const string PartNumber = "Part Number";
            public const string SupplierPartNumber = "Supplier Part Number";
            public const string QuantityOrdered = "Quantity Ordered";
            public const string QuantityReceived = "Quantity Received";
            public const string DateReceived = "Date Received";
            public const string InvoicePricePaid = "Invoice Price Paid";
            public const string UnitPrice = "Unit Price";
            public const string PureLoadedCost = "Pure_Loaded Cost";
            public const string CurrencyCode = "Currency Code";
            public const string IntraDiv = "Intra-div";
            public const string DirectIndirect = "Direct_indirect";
            public const string PoTerms = "PO Terms";
            public const string FreightTerms = "Freight Terms";
            public const string Uom = "UOM";
            public const string TitleTransfer = "Title Transfer";
            public const string Port = "Port";
            public const string ReleaseNumber = "Release#";
            public const string CommittedDate = "Committed Date";
        }

        public static List<string> ItemFileHeaderList = new List<string>
        {
            "Division ID",
            "Local Site ID",
            "Part Number",
            "Description",
            "Comcode",
            "DRI Code",
            "Part Status",
            "Direct_Indirect",
            "Purch_mfrd",
            "Lead Time",
            "Standard Cost",
            "Pure_loaded Cost",
            "Currency Code",
            "UOM",
            "ABC Category",
            "Item Weight",
            "Item Weight UOM",
            "Item HTS Code",
            "Item HS Code"
        };

        public class ItemFileHeaders
        {
            public const string DivisionId = "Division ID";
            public const string LocalSiteId = "Local Site ID";
            public const string PartNumber = "Part Number";
            public const string Description = "Description";
            public const string Comcode = "Comcode";
            public const string DRICode = "DRI Code";
            public const string PartStatus = "Part Status";
            public const string Direct_Indirect = "Direct_Indirect";
            public const string Purch_mfrd = "Purch_mfrd";
            public const string LeadTime = "Lead Time";
            public const string StandardCost = "Standard Cost";
            public const string Pure_loadedCost = "Pure_loaded Cost";
            public const string CurrencyCode = "Currency Code";
            public const string UOM = "UOM";
            public const string ABCCategory = "ABC Category";
            public const string ItemWeight = "Item Weight";
            public const string ItemWeightUOM = "Item Weight UOM";
            public const string ItemHTSCode = "Item HTS Code";
            public const string ItemHSCode = "Item HS Code";
        };

        public static HashSet<string> ItemPartStatusList = new HashSet<string>
        {
            "A", "I", "O"
        };

        public static HashSet<string> ItemDirectIndirectList = new HashSet<string>
        {
            "D"
        };

        public static HashSet<string> ItemPurchMfrdsList = new HashSet<string>
        {
            "P", "M", "B"
        };

        public static HashSet<string> ItemPureLoadedCostsList = new HashSet<string>
        {
            "P", "L"
        };

        public static HashSet<string> ItemABCCategoryList = new HashSet<string>
        {
            "A","AA","B","C","D","D USE","D NEW","D E&O","U"
        };

        public class SupplierFileHeaders
        {
            public const string DivisionId = "Division ID";
            public const string LocalSiteId = "Local Site ID";
            public const string SupplierId = "Supplier ID";
            public const string SupplierName = "Supplier Name";
            public const string DUNS = "DUNS";
            public const string ActiveInactive = "Active_inactive";
            public const string DirectIndirect = "Direct_indirect";
            public const string AddressDescr = "Address Descr";
            public const string Street = "Street";
            public const string Suite = "Suite";
            public const string City = "City";
            public const string State = "State";
            public const string PostalCode = "Postal Code";
            public const string County = "County";
            public const string Country = "Country";
            public const string Addr1 = "Addr1";
            public const string Addr2 = "Addr2";
            public const string Addr3 = "Addr3";
            public const string Addr4 = "Addr4";
            public const string CountryCode = "Country Code";
            public const string GlobalFlag = "Global Flag";
            public const string MainTelephone = "Main Telephone";
            public const string TollFree = "Toll Free";
            public const string Fax = "Fax";
            public const string WebSite = "Web Site";
            public const string SupplierType = "Supplier Type";
        }

        public static List<string> SupplierFileHeaderList = new List<string>
        {
           "Division ID",
           "Local Site ID",
           "Supplier ID",
           "Supplier Name",
           "DUNS",
           "Active_inactive",
           "Direct_indirect",
           "Address Descr",
           "Street",
           "Suite",
           "City",
           "State",
           "Postal Code",
           "County",
           "Country",
           "Addr1",
           "Addr2",
           "Addr3",
           "Addr4",
           "Country Code",
           "Global Flag",
           "Main Telephone",
           "Toll Free",
           "Fax",
           "Web Site",
           "Supplier Type"
         };

        public static List<string> SupplierActiveInactiveList = new List<string>
        {
            "A","I","U"
        };

        public static List<string> SupplierDirectIndirectList = new List<string>
        {
            "D","I"
        };

        public static List<string> SupplierGlobalFlagList = new List<string>
        {
            "G","R","U"
        };

        public static List<string> SupplierSupplierTypeList = new List<string>
        {
            "D","M","B"
        };

        public static List<string> InventoryFileHeaderList = new List<string>
        {
            "Division ID",
            "Local Site ID",
            "Part Number",
            "Quantity",
            "Standard cost",
            "Total value",
            "UOM",
            "Currency code",
            "Part status",
            "Comcode",
            "DRI code",
            "Description",
            "Inventory date"
        };

        public class InventoryFileHeaders
        {
            public const string DivisionId = "Division ID";
            public const string LocalSiteID = "Local Site ID";
            public const string PartNumber = "Part Number";
            public const string Quantity = "Quantity";
            public const string StandardCost = "Standard cost";
            public const string TotalValue = "Total value";
            public const string UOM = "UOM";
            public const string CurrencyCode = "Currency code";
            public const string PartStatus = "Part status";
            public const string Comcode = "Comcode";
            public const string DRICode = "DRI code";
            public const string Description = "Description";
            public const string InventoryDate = "Inventory date";
        }

        public static List<string> InventoryPartStatusList = new List<string>
        {
            "A","I","O","U"
        };

        /// <summary>
        /// for error message.
        /// </summary>
        public class CustomFileHeaders
        {
            public const string SourceRecordIDfields = "Source Record ID fields";
            public const string AllHeaderFields = "All header fields";
        }

        public static List<FileTemplate> FileTemplateList = new List<FileTemplate>
        {
            new FileTemplate()
            {
                Table = "SupplierTable",
                FileType = FileTypes.Supplier,
                FileNameFormat = "[div abbrev]_[feed number]_supplier_o.txt",
                SortOrder = 1,
                DependsOnFileTypes = ""
            },
            new FileTemplate()
            {
                Table = "ItemTable",
                FileType = FileTypes.Item,
                FileNameFormat = "[div abbrev]_[feed number]_item_o.txt",
                SortOrder = 2,
                DependsOnFileTypes = ""
            },
            new FileTemplate()
            {
                Table = "InventoryTable",
                FileType = FileTypes.Inventory,
                FileNameFormat = "[div abbrev]_[feed number]_inv_o.txt",
                SortOrder = 3,
                DependsOnFileTypes = "Item"
            },
            new FileTemplate()
            {
                Table = "VIRTable",
                FileType = FileTypes.Vir,
                FileNameFormat = "[div abbrev]_[feed number]_vir_o.txt",
                SortOrder = 4,
                DependsOnFileTypes = "Supplier,Item"
            },
            new FileTemplate()
            {
                Table = "POTable",
                FileType = FileTypes.Po,
                FileNameFormat = "[div abbrev]_[feed number]_po_o.txt",
                SortOrder = 5,
                DependsOnFileTypes = "Supplier"
            },
            new FileTemplate()
            {
                Table = "POItemTable",
                FileType = FileTypes.PoItem,
                FileNameFormat = "[div abbrev]_[feed number]_poitem_o.txt",
                SortOrder = 6,
                DependsOnFileTypes = "Po"
            },
            new FileTemplate()
            {
                Table = "MPNTable",
                FileType = FileTypes.Mpn,
                FileNameFormat = "[div abbrev]_[feed number]_mpn_o.txt",
                SortOrder = 7,
                DependsOnFileTypes = "Item"
            },
            new FileTemplate()
            {
                Table = "UOMTable",
                FileType = FileTypes.Uom,
                FileNameFormat = "[div abbrev]_[feed number]_uom_o.txt",
                SortOrder = 8,
                DependsOnFileTypes = "Item"
            }
        };

        public static class WellKnownConfigSettingModules
        {
            public static string MainShareFolderPaths = "MainShareFolderPaths";
        }

        public static class WellKnownPathNames
        {
            public static string LoadFolder = "Load Folder";
            public static string LogFolder = "Log Folder";
            public static string ProductionFolder = "Production Folder";
            public static string SupplyChainTargetFolder = "SupplyChainTargetFolder";
        }

        public static class WellKnownStorageAccountDirectoryNames
        {
            public static string JobWorkingFolder = "JobWorkingFolder";
            public static string JobArchives = "JobArchives";
            public static string Documents = "Documents";
        }

        public static class MasterDataTableNames
        {
            public static string Division = "Division";
            public static string SiteMaster = "SiteMaster";
            public static string UOM = "UOM";
            public static string Country = "Country";
            public static string Currency = "Currency";
            public static string CommodityCode = "CommodityCode";
            public static string FreightTerms = "FreightTerms";
            public static string PaymentTerm = "PaymentTerm";
            public static string ItemWeightUOM = "ItemWeightUOM";

        }

        public static class LogMessageTypes
        {
            public static string Info = "INFO";
            public static string Warning = "WARNING";
            public static string Error = "ERROR";
        }

        public static class ValidationMessages
        {
            public static string ValidateFile = "Start validate '{0}' file {1}";
            public static string ValidateFileError = "Validate '{0}' file {1}, error: {2}";

            public static string CharacterLimitHasBeenExceeded = "CHARACTER LIMIT HAS BEEN EXCEEDED";
            public static string InvalidFormat = "INVALID FORMAT";
            public static string InvalidDateFormat = "INVALID DATE FORMAT";
            public static string InvalidValue = "INVALID VALUE";
            public static string ValueIsZeroInvalidValue = " IS ZERO, INVALID VALUE";
            public static string TheFileNoDataRow = "THE FILE HAS NO DATA ROWS";
            public static string NullRow = "The data row is null";
            public static string InvalidRowType = "Invalid data row type";
            public static string HeaderDoesNotMatchRequiredFormat = "HEADER DOES NOT MATCH REQUIRED FORMAT";
            public static string FullRecordError = "ERROR FULL RECORD";
            public static string MandatoryField = "MANDATORY FIELD, VALUE REQUIRED";
            public static string HeaderCountMismatch = "HeaderCountMismatch";
            public static string HeaderCountDoesNotMatch = "The file has {0} columns in header, but {1} were expected.";
            public static string MissingHeaders = "The file missing headers";
            public static string HeaderMisMatchDetail = "Header mismatch at position {0}: expected '{1}', found '{2}'.";
            public static string DuplicateRecordFound = "DUPLICATE SOURCE RECORD FOUND";
            public static string DuplicateXFound = "DUPLICATE {0} FOUND";

            public static string DuplicateSourceRecordFound = "DUPLICATE SOURCE RECORD FOUND: Division ID = {0}, Local Site ID = {1}, Receipt Number = {2}, PO Number = {3}, PO Line Number = {4}, Part Number = {5}, Date Received = {6}, Release# = {7}, Committed Date = {8}";
            public static string SupplierIdNotFound = "Supplier ID NOT FOUND: {0}";
            public static string LocalSiteIdNotFound = "Local Site ID NOT FOUND: {0}";
            public static string XColumnNotFound = "{0} NOT FOUND";
            public static string InvoicePricePaidMismatch = "INVOICE PRICE PAID MISMATCH";
            public static string NotFoundDependendFile = "Not found dependent '{0}' file for file: {1}";
            public static string ChangedJobStatus = "Changed job status to {0}.";
            public static string ChangedFileStatus = "Changed file '{0}''s status to {1}.";
            public static string LoadingDependentFiles = "Loading dependent files for file: {0}";
            public static string NotFoundDependentFile = "Not found dependent files '{0}' for file: {1}";
            public static string SupplierIdMustBeIncludedInSupplierFile = "Supplier ID must be included in the supplier master file (supplier_o.txt).";
            public static string LocalSiteIdMustBeIncludedInSupplierFile = "Local Site ID must be included in the supplier master file (supplier_o.txt).";
            public static string PartNumberMustBeIncludedInItemFile = "Part Number must be included in the Item master file (item_o.txt).";
            public static string LocalSiteIdMustBeIncludedInItemFile = "Local Site ID must be included in the Item master file (item_o.txt).";
            public static string SupplierIdAndLocalSiteIdMustBeIncludedInSupplierFile = "Supplier ID and Local Site ID must be included in the supplier master file (supplier_o.txt).";
            public static string PartNumberAndLocalSiteIdMustBeIncludedInItemFile = "Part Number and Local Site ID must be included in the Item master file (item_o.txt).";
            public static string DependentFileNoData = "Dependent files '{0}' no data row";
            public static string DependentFileNoHeader = "Dependent files '{0}' no header";
            public static string DependentFileNotFoundOrNoData = "Dependent file '{0}' NOT FOUND OR NO DATA";
            public static string IncorrectDateFormat = "INCORRECT DATE FORMAT";
            public static string MustContainNumericValueAndCannotBeEqualToZero = "Must contain numeric value and cannot be equal to 0";
            public static string DivisionIdMismatch = "DIVISION ID DOES NOT MATCH";
            public static string DivisionIdAndLocalSiteIdMismatch = "Division ID and Local Site ID DOES NOT MATCH";
            public static string InvoicePricePaidIsOverMaximum = "Invoice Price Paid reached maximum of 5,000,000";
            public static string XDoesNotMatch = "{0} DOES NOT MATCH";
            public static string FutureDate = "FUTURE DATE";
            public static string DateMoreThanOneMonthOld = "DATE MORE THAN ONE MONTH OLD";
            public static string IncorrectNumberOfColumns = "INCORRECT NUMBER OF COLUMNS";
        }

        public static class NumberValueRanges
        {
            public static BigDecimal VirInvoicePricePaidMaxValue = 5000000;

        }

        public static class DependentFiles
        {
            public static string Supplier = "supplier_o.txt";
            public static string Item = "item_o.txt";
            public static string PO = "po_o.txt";
        }

        public enum ErrorTypes
        {
            None = -2,
            NegativeValue = -1,
            ValueIsZero = 0,
            MandatoryField = 1,
            InvalidValue = 2,
            InvalidFormat = 3,
            CharacterLimitExceeded = 4
        }

        public static class ReportNames
        {
            public const string ErrorReportName = "{0}_error_report_{1}.xlsx";
            public const string StatisticsReportName = "{0}_stats_report_{1}.xlsx";
        }

        public static List<string> ErrorReportHeaderList = new List<string>
        {
            "Row Number",
            "Problem",
            "Validation Message ",
            "Data",
            //"Reference",
        };

        public static class ErrorReportHeaders
        {
            public const string RowNumber = "Row Number";
            public const string Problem = "Problem";
            public const string ValidationMessage = "Validation Message";
            public const string Data = "Data";
            //public const string Reference = "Reference";
        };

        public static class VirStatisticsReportFieldNames
        {
            public const string TotalRecords = "Total Records";
            public const string QuantityOrdered = "Quantity Ordered";
            public const string QuantityReceived = "Quantity Received";
            public const string DateReceived = "Date Received";
            public const string InvoicePricePaid = "Invoice Price Paid";
            public const string UnitPrice = "Unit Price";
            public const string CommittedDate = "Committed Date";
        }

        public static List<string> StatisticsReportHeadersList = new List<string>
        {
            "Field Name",
            "Min",
            "Max",
        };

        public static class StatisticsReportHeaders
        {
            public const string FieldName = "Field Name";
            public const string Min = "Min";
            public const string Max = "Max";
        }

        public static class ItemStatisticsReportFieldNames
        {
            public const string TotalRecords = "Total Records";
            public const string StandardCost = "Standard Cost ";
        }

        public static class SupplierStatisticsReportFieldNames
        {
            public const string TotalRecords = "Total Records";
        }

        public static class NumberTypeCharacterLengthLimit
        {
            public const int FifteenCharacters = 15;
            public const int ThirtyEightCharacters = 38;
            public const int FiftyCharacters = 50;
        }

        public static class InventoryStatisticsReportFieldNames
        {
            public const string TotalRecords = "Total Records";
            public const string Quantity = "Quantity";
            public const string StandardCost = "Standard Cost";
            public const string TotalValue = "Total Value";
            public const string InventoryDate = "Inventory Date";

        }

        public static List<string> POFileHeaderList = new List<string>
        {
            "Division ID",
            "Local Site ID",
            "PO Number",
            "Order date",
            "Latest Amendment",
            "Commodity Mgr Id",
            "Supplier ID",
            "Currency code",
            "PO Type",
            "Intra-div",
            "Direct_indirect",
            "PO Terms",
            "Freight Terms",
            "EDI",
            "Order Status",
            "Title transfer",
            "Port"
        };

        public class POFileHeaders
        {
            public const string DivisionId = "Division ID";
            public const string LocalSiteID = "Local site ID";
            public const string PONumber = "PO Number";
            public const string OrderDate = "Order date";
            public const string LatestAmendment = "Latest Amendment";
            public const string CommodityMGRId = "Commodity Mgr ID";
            public const string SupplierID = "Supplier ID";
            public const string CurrencyCode = "Currency code";
            public const string POType = "PO Type";
            public const string IntraDiv = "Intra-div";
            public const string DirectIndirect = "Direct_indirect";
            public const string POTerms = "PO Terms";
            public const string FreightTerms = "Freight Terms";
            public const string EDI = "EDI";
            public const string OrderStatus = "Order Status";
            public const string TitleTransfer = "Title transfer";
            public const string Port = "Port";
        }

        public static List<string> POItemFileHeaderList = new List<string>
        {
            "Division ID",
            "Local Site ID",
            "PO Number",
            "PO Line Number",
            "Part Number",
            "Supplier Part Number",
            "Description",
            "Contract ID",
            "Unit Cost",
            "Pure_Loaded Cost",
            "Ordered Value",
            "Quantity Ordered",
            "Quantity Returned",
            "Committed Date",
            "Requested Date",
            "Order Status",
            "Currency Code",
            "UOM",
            "Qty Left to Receive",
            "Value Left to Receive",
            "Release#"
        };

        public class POItemFileHeaders
        {
            public const string DivisionID = "Division ID";
            public const string LocalSiteID = "Local Site ID";
            public const string PONumber = "PO Number";
            public const string POLineNumber = "PO Line Number";
            public const string PartNumber = "Part Number";
            public const string SupplierPartNumber = "Supplier Part Number";
            public const string Description = "Description";
            public const string ContractID = "Contract ID";
            public const string UnitCost = "Unit Cost";
            public const string PureLoadedCost = "Pure_Loaded Cost";
            public const string OrderedValue = "Ordered Value";
            public const string QuantityOrdered = "Quantity Ordered";
            public const string QuantityReturned = "Quantity Returned";
            public const string CommittedDate = "Committed Date";
            public const string RequestedDate = "Requested Date";
            public const string OrderStatus = "Order Status";
            public const string CurrencyCode = "Currency Code";
            public const string UOM = "UOM";
            public const string QtyLeftToReceive = "Qty Left to Receive";
            public const string ValueLeftToReceive = "Value Left to Receive";
            public const string Release = "Release#";
        }

        public static List<string> UOMFileHeaderList = new List<string>
        {
            "Division ID",
            "Local Site ID",
            "Part Number",
            "Local UOM",
            "Base UOM",
            "Conversion Rate"
        };

        public class UOMFileHeaders
        {
            public const string DivisionID = "Division ID";
            public const string LocalSiteID = "Local Site ID";
            public const string PartNumber = "Part Number";
            public const string LocalUOM = "Local UOM";
            public const string BaseUOM = "Base UOM";
            public const string ConversionRate = "Conversion Rate";
        }

        public static List<string> POPOTypeList = new List<string>
        {
            "B","P"
        };

        public static List<string> POIntraDivList = new List<string>
        {
            "N"
        };

        public static List<string> PODirectIndirectList = new List<string>
        {
            "D"
        };

        public static List<string> POEDIList = new List<string>
        {
            "Y","N","U"
        };

        public static List<string> POOrderStatusList = new List<string>
        {
            "O","C"
        };

        public static class POStatisticsReportFieldNames
        {
            public const string TotalRecords = "Total Records";
            public const string OrderDateCost = "Order date Cost";
            public const string LatestAmendment = "Latest Amendment";
        }

        public static List<string> POItemPureLoadedCostList = new List<string>
        {
            "P","L"
        };

        public static List<string> POItemOrderStatusList = new List<string>
        {
            "O","C"
        };

        public static class POItemStatisticsReportFieldNames
        {
            public const string TotalRecords = "Total Records";
            public const string UnitCost = "Unit Cost";
            public const string OrderedValue = "Ordered Value";
            public const string QuantityOrdered = "Quantity Ordered";
            public const string QuantityReturned = "Quantity Returned";
            public const string CommittedDate = "Committed Date";
            public const string RequestedDate = "Requested Date";
            public const string QtyLeftToReceive = "Qty Left to Receive";
            public const string ValueLeftToReceive = "Value Left to Receive";
        }

        public static class UOMStatisticsReportFieldNames
        {
            public const string TotalRecords = "Total Records";
            public const string ConversionRate = "Conversion Rate";
        }

        public static List<string> MPNFileHeaderList = new List<string>
        {
            "Division ID",
            "Local Site ID",
            "Part Number",
            "Local Manufacturer ID",
            "Manufacture ID",
            "Manufacture Name",
            "Manufacturer Part Number",
            "Object ID",
            "MPN Type"
        };

        public class MPNFileHeaders
        {
            public const string DivisionID = "Division ID";
            public const string LocalSiteID = "Local Site ID";
            public const string PartNumber = "Part Number";
            public const string LocalManufacturerID = "Local Manufacturer ID";
            public const string ManufactureID = "Manufacture ID";
            public const string ManufactureName = "Manufacture Name";
            public const string ManufacturerPartNumber = "Manufacturer Part Number";
            public const string ObjectID = "Object ID";
            public const string MPNType = "MPN Type";
        }

        public static List<string> MPNMPNTypeList = new List<string>
        {
            "P","S"
        };

        public static class MPNStatisticsReportFieldNames
        {
            public const string TotalRecords = "Total Records";
        }

        public static class SpecialStringRegularExpression
        {
            public const string ASCII = "^[\u0000-\u007F]*$";
            public const string NumericWithDashes = @"^\d+-\d+(-\d+)*$";
        }

        public static class ConfigSettingModules
        {
            public const string HelpDocuments = "HelpDocuments";
        }

        public static class IDataRowProperties
        {
            public const string RowNumber = "Row Number";
            public const string IncorrectColumnCount = "Incorrect ColumnCount";  
            public const string UniquenessKey = "Uniqueness Key";
        }
    }
}