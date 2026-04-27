using DVT.Core.Models;
using DVT.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DVT.Data.Repositories
{
    public class JobFileRepository: Repository<JobFile>, IJobFileRepository
    {
        private DVTContext context;

        public JobFileRepository(DVTContext context) : base(context)
        {
            this.context = context;
        }

        public async ValueTask<JobFile> GetByIdAsync(Guid jobFileId)
        {
            return await context.JobFiles
                .SingleOrDefaultAsync(jobFile => jobFile.JobFileId == jobFileId && !jobFile.Deleted);
        }

        public async ValueTask<JobFile?> GetByIdNoValidationAsync(Guid jobFileId)
        {
            return await context.JobFiles
                .Where(jobFile => jobFile.JobFileId == jobFileId && !jobFile.Deleted)
                .Select(JobFileWithoutValidationProjection)
                .SingleOrDefaultAsync();               
        }

        public async ValueTask<JobFile> GetByIdNoTrackingAsync(Guid jobFileId)
        {
            return await context.JobFiles
                .AsNoTracking()
                .SingleOrDefaultAsync(jobFile => jobFile.JobFileId == jobFileId && !jobFile.Deleted);
        }

        public async ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdAsync(Guid jobId)
        {
            return await context.JobFiles
                .Where(jobFile => jobFile.JobId == jobId && !jobFile.Deleted)
                .OrderByDescending(jobFile => jobFile.SortOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a light-weight List of Job Files in a Job by JobID without the Validation Results and Validation Stats columns.
        /// This method should be used just to get information about the job files in a job without the intention of updating the job files.
        /// </summary>
        /// <param name="jobId">The GUID of the job</param>
        /// <returns>A list of JobFile objects for the given JobID</returns>
        public async ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdNoValidationNoTrackingAsync(Guid jobId)
        {            
            //Get the JobFile without the validation messages and stats and no tracking.

            return await context.JobFiles
                .AsNoTracking()
                .Where(jobFile => jobFile.JobId == jobId && !jobFile.Deleted)
                .Select(JobFileWithoutValidationProjection)
                .OrderByDescending(jobFile => jobFile.SortOrder)                
                .ToListAsync();
        }

        /// <summary>
        /// Gets a light-weight list of Job Files in a Job by JobID without the Validation Results and Validation Stats columns.
        /// This method should be used to get the JobFiles in a job where we don't need the validation information 
        /// but still need the EF tracking to update the Job File in the database.
        /// </summary>
        /// <param name="jobId">The GUID of the job</param>
        /// <returns>A list of JobFile objects for the given JobID</returns>
        public async ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdNoValidationAsync(Guid jobId)
        {
            //Get the JobFile without the validation messages and stats and no tracking.

            return await context.JobFiles                
                .Where(jobFile => jobFile.JobId == jobId && !jobFile.Deleted)
                .Select(JobFileWithoutValidationProjection)
                .OrderByDescending(jobFile => jobFile.SortOrder)
                .ToListAsync();
        }

        public async ValueTask<IEnumerable<JobFile>> GetJobFilesByJobFileIdsAsync(List<Guid> jobFileIds)
        {
            return await context.JobFiles
                .Where(jobFile => jobFileIds.Contains(jobFile.JobFileId) && !jobFile.Deleted)
                .OrderByDescending(jobFile => jobFile.SortOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a light-weight list of Job Files by a list of JobFileId without the Validation Results and Validation Stats columns.
        /// This method should be used to get the JobFiles where we don't need the validation information 
        /// but still need the EF tracking to update the Job File in the database.
        /// </summary>
        /// <param name="jobFileIds">List of GUIDs of JobFile records</param>
        /// <returns>List of JobFile</returns>
        public async ValueTask<IEnumerable<JobFile>> GetJobFilesByJobFileIdsNoValidationAsync(List<Guid> jobFileIds)
        {
            return await context.JobFiles
                .Where(jobFile => jobFileIds.Contains(jobFile.JobFileId) && !jobFile.Deleted)
                .Select(JobFileWithoutValidationProjection)
                .OrderByDescending(jobFile => jobFile.SortOrder)
                .ToListAsync();
        }

        private static readonly Expression<Func<JobFile, JobFile>> JobFileWithoutValidationProjection = j => new JobFile()
        {
            JobFileId = j.JobFileId,
            JobId = j.JobId,
            FileName = j.FileName,
            FilePath = j.FilePath,
            TableName = j.TableName,
            FileType = j.FileType,
            SortOrder = j.SortOrder,
            DependsOnFileType = j.DependsOnFileType,
            Status = j.Status,
            FileCreationTimestamp = j.FileCreationTimestamp,
            FileLastModifiedTimestamp = j.FileLastModifiedTimestamp,
            RecordCount = j.RecordCount,
            LoadDate = j.LoadDate,
            UpdateBy = j.UpdateBy,
            UpdateDate = j.UpdateDate,
            Deleted = j.Deleted
        };
    }
}
