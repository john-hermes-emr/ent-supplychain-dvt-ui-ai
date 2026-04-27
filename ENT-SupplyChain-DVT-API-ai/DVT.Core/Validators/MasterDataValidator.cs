using DVT.Core.Models;
using FluentValidation;

namespace DVT.Core.Validators
{
    public class MasterDataValidator : AbstractValidator<MasterData>
    {
        public MasterDataValidator()
        {
            RuleFor(b => b.ItemId)
                .NotEmpty()
                .NotEqual(Guid.Empty);
            RuleFor(b => b.TableName)
               .NotEmpty()
               .MaximumLength(50);
            RuleFor(b => b.TextId)
               .NotEmpty()
               .MaximumLength(200);
            RuleFor(b => b.ItemNameAbbrev)
               .NotEmpty()
               .MaximumLength(100);
            RuleFor(b => b.Text1)
               .MaximumLength(100);
            RuleFor(b => b.Text2)
               .MaximumLength(100);
            RuleFor(b => b.Text3)
               .MaximumLength(100);
            RuleFor(b => b.Text4)
               .MaximumLength(100);
            RuleFor(b => b.Text5)
               .MaximumLength(100);
            RuleFor(b => b.Text6)
              .MaximumLength(100);
            RuleFor(b => b.UpdateDate)
                .NotEmpty()
                .GreaterThan(new DateTime(1900, 1, 1));
            RuleFor(b => b.UpdateBy)
                .NotNull()
                .MaximumLength(50);
        }
    }
}