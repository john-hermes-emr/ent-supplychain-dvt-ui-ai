using DVT.Core.Models;

namespace DVT.Core.FileLoader
{
    public class FileDependencyLoader
    {
        public void LoadDependentFiles(Job job, JobModel jobModel, JobLoadResult jobLoadResult, JobFile file, string updateBy)
        {
            var files = job.JobFiles;
            FileLoadResult fileLoadResult = null;
            if (!string.IsNullOrEmpty(file.DependsOnFileType))
            {

                var dependentFileTypes = file.DependsOnFileType.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var dependentFileType in dependentFileTypes)
                {
                    var dependentFile = files.FirstOrDefault(f => f.FileType.Equals(dependentFileType, StringComparison.OrdinalIgnoreCase));
                    if (dependentFile != null)
                    {
                        fileLoadResult = jobLoadResult.GetFileLoadResultByJobFileId(dependentFile.JobFileId);

                        if (fileLoadResult == null)
                        {
                            continue;
                        }

                        if (fileLoadResult.DataRows == null || !fileLoadResult.DataRows.Any())
                        {

                            continue;
                        }

                        if (fileLoadResult.FileHeader == null || !fileLoadResult.FileHeader.Any())
                        {

                            continue;
                        }

                        var dependentDataRows = fileLoadResult.DataRows;
                        var dependentFileHeader = fileLoadResult.FileHeader;
                        var dependentFileModel = new JobFileModel
                        {
                            JobFileId = dependentFile.JobFileId,
                            FileType = dependentFile.FileType,
                            FileName = dependentFile.FileName,
                            FileHeader = dependentFileHeader,
                            DataRows = dependentDataRows
                        };
                        jobModel.JobFiles.Add(dependentFileModel);
                    }
                }
            }
        }

    }
}
