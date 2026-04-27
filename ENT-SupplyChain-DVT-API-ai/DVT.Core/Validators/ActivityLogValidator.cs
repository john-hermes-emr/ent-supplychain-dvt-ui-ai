using DVT.Core.Models;
using FluentValidation;

namespace DVT.Core.Validators
{
    public class ActivityLogValidator : AbstractValidator<ActivityLog>
    {
        public ActivityLogValidator()
        {
            RuleFor(b => b.LogId).NotEqual(Guid.Empty);
            RuleFor(b => b.Entity)
               .NotEmpty()
               .MaximumLength(100);
            RuleFor(b => b.EntityId)
               .NotEmpty();
            RuleFor(b => b.MessageType)
               .NotEmpty()
               .MaximumLength(50);
            RuleFor(b => b.Message)
               .NotNull()
               .MaximumLength(1000);
            RuleFor(b => b.CreateDate)
                .NotEmpty()
                .GreaterThan(new DateTime(1900, 1, 1));
            RuleFor(b => b.CreateBy)
                .NotNull()
                .MaximumLength(200);
        }
    }
}
