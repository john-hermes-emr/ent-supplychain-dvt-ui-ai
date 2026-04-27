using DVT.Core.Models;
using FluentValidation;

namespace DVT.Core.Validators
{
    public class JobValidator : AbstractValidator<Job>
    {
        public JobValidator()
        {
            RuleFor(b => b.JobId)
                .NotEmpty()
                .NotEqual(Guid.Empty);
            RuleFor(b => b.DivisionId)
               .NotEmpty()
               .NotEqual(Guid.Empty);
            RuleFor(b => b.FeedNumber)
               .NotEmpty();
            RuleFor(b => b.Status)
               .NotEmpty()
               .MaximumLength(20);
            RuleFor(b => b.UserInfoId)
               .NotEmpty()
               .NotEqual(Guid.Empty);
            RuleFor(b => b.ArchiveFilePath)
              .MaximumLength(500);
            RuleFor(b => b.CreateDate)
                .NotEmpty()
                .GreaterThan(new DateTime(1900, 1, 1));
            RuleFor(b => b.CreateBy)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(b => b.UpdateDate)
                .NotEmpty()
                .GreaterThan(new DateTime(1900, 1, 1));
            RuleFor(b => b.UpdateBy)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}