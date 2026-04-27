namespace DVT.Api.Contracts
{
    public static class ApiRoutes
    {
        public const string Root = "";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version;

        public static class StatusReport
        {
            public const string HubUrl = Base + "/status-report";
        }

        public static class About
        {
            public const string Get = Base + "/about";
            public const string GetStartupInfo = Base + "/about/startup-info";
            public const string TestDatabaseConnection = Base + "/about/test-db-connection";
            public const string GetDatabaseTables = Base + "/about/db-tables";
        }

        public static class SecurityDecorations
        {
            public const string CorsAllowAll = "AllowAll";
        }

        public static class MasterData
        {
            public const string GetAllDivisions = Base + "/master-data/divisions";
            public const string GetAllMasterData = Base + "/master-data";
            public const string GetAllTableNames = Base + "/master-data/table-names";
            public const string GetMasterDataByTableName = Base + "/master-data/table-name/{id}";

        }

        public static class Storages
        {
            public const string GetFoldersFromUserShareByEmailAddress = Base + "/storage/folder-list/email-address/{id}";
            public const string DownloadFileFromMainShareDocs = Base + "/storage/download-help-doc";
        }

        public static class OptionListItems
        {
            public const string GetByCategoryName = Base + "/option-list-items/category/{id}";
            public const string GetById = Base + "/option-list-items/{id}";
        }

        public static class UserInfos
        {
            public const string GetById = Base + "/user-info/{id}";
            public const string GetByEmailAddress = Base + "/user-info/email-address/{id}";
            public const string SetFolders = Base + "/user-info";
            public const string SetLoadFolder = Base + "/user-info/load-folder";
            public const string SetLogFolder = Base + "/user-info/log-folder";
            public const string SetProdFolder = Base + "/user-info/production-folder";

        }

        public static class Jobs
        {
            public const string CreateJob = Base + "/jobs/create";
            public const string LoadExtractFiles = Base + "/jobs/load-files";
            public const string GetById = Base + "/jobs/{id}";
            public const string DeleteJob = Base + "/jobs/delete/{id}";
            public const string GetUserActiveJob = Base + "/jobs/get-active/user-id/{id}";
            public const string GetJobStatus = Base + "/jobs/status/{id}";
            public const string CompleteJob = Base + "/jobs/complete";
        }

        public static class Validations
        {
            public const string ValidateFiles = Base + "/validations/validate-files";
            public const string AcceptValidation = Base + "/validations/accept-validation";

        }

        public static class Analysis
        {
            public const string GetAnalysisErrorsByJobIdAndJobFileId = Base + "/analysis/errors";
            public const string GenerateAnalysisErrorReportByJobIdAndJobFileId = Base + "/analysis/error-report";

            public const string GetAnalysisStatsByJobId = Base + "/analysis/stats/job/{id}";
            public const string GetAnalysisStatsByJobIdAndJobFileId = Base + "/analysis/stats";
            public const string GenerateAnalysisStatsReportByJobIdAndJobFileId = Base + "/analysis/stats-report";
        }

        public static class ConfigSettings
        {
            public const string GetHelpDocuments = Base + "/config-setting/help-documents";
        }
    }
}
