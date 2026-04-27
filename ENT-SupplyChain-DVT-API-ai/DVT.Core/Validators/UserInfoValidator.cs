using DVT.Core.Models;
using FluentValidation;

namespace DVT.Core.Validators
{
    public class UserInfoValidator : AbstractValidator<UserInfo>
    {
        public UserInfoValidator()
        {
            RuleFor(b => b.UserInfoId).NotEqual(Guid.Empty);

            RuleFor(b => b.UserInfoId)
                .NotEmpty()
                .NotEqual(Guid.Empty);
            RuleFor(b => b.FirstName)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(b => b.LastName)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(b => b.EmailAddress)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(b => b.LoadFolder)
               .MaximumLength(1000);
            RuleFor(b => b.LogFolder)
               .MaximumLength(1000);
            RuleFor(b => b.ProductionFolder)
             .MaximumLength(1000);
            RuleFor(b => b.UpdateDate)
                .GreaterThan(new DateTime(1900, 1, 1));
            RuleFor(b => b.UpdateBy)
                .NotNull()
                .MaximumLength(200);
        }
    }
}