export const DvtApiPaths = {
        Home: {
                "GetDivisions": "/master-data/divisions",
                "GetStatusProgress": "/status-report",
                "CreateActiveJob": "/jobs/create",
                "GetActiveJobs": "/jobs/get-active/user-id/{id}",
                "ValidateJobFiles": "/validations/validate-files",
                "LoadJobFiles": "/jobs/load-files",
                "AnalyzeFilesErrors": "/analysis/errors",
                "AnalyzeFilesStatistics": "/analysis/stats/job/{id}",
                "AnalyzeFilesStatisticsSingle": "/analysis/stats",
                "AnalyzeFilesErrorsReportExport": "/analysis/error-report",
                "AcceptValidatedData": "/validations/accept-validation",
                "CompleteJob": "/jobs/complete",
                "GetJobStatus": "/jobs/status/{id}"
        },
        ChangePath: {
                "GetFolderList": "/storage/folder-list/email-address/{id}",
                "GetUserDefaultPaths": "/user-info/email-address/{id}",
                "UpdateUserLoadFolderPaths": "/user-info/load-folder",
                "UpdateUserLogFolderPaths": "/user-info/log-folder",
                "UpdateUserProductionFolderPaths": "/user-info/production-folder"
        },
        OptionList: {
                "GetByCategoryName": "/optionList/category/{id}",
                "GetAll": "/optionlist",
                "GetOptionListItem": "/optionlist/category/{id}",
                "CreateListItem": "/optionlist",
                "UpdateListItem": "/optionlist/{id}",
                "DeleteListItem": "/optionlist/{id}"
        },
        Users: {
                "GetUserFromAd": "/ad-users/search",
                "GetAllUserGroups": "/okta-api/groups",
                "UpdateUserGroups": "/okta-api/groups/{id}/{id}",
                "DeleteUserGroup": "/okta-api/groups/{id}/{id}",
                "GetAssignUserGroup": "/okta-api/user-groups/{id}",
                "GetUserRole": "/okta-api/admin-check/{id}",
        },
        HelpDocuments: {
                "GetHelpDocuments": "/config-setting/help-documents",
                "DownloadHelpDocument": "/storage/download-help-doc"
        }
}
