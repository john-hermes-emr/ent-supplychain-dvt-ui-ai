using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18239116: 1 - Validation Service - Validate UOM File
    /// User Story 23751267: Change Validation Rule for Mandatory fields
    /// </summary>
    public class UOMDataRowStaticValidator : AbstractValidator<UOMDataRow>
    {
        string InvalidFormat = ValidationMessages.InvalidFormat;
        string MandatoryField = ValidationMessages.MandatoryField;
        string CharacterLimitHasBeenExceeded = ValidationMessages.CharacterLimitHasBeenExceeded;
        string InvalidValue = ValidationMessages.InvalidValue;

        public UOMDataRowStaticValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.DivisionID)
            .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
            .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
            .WithState(attemptedValue => attemptedValue.DivisionID)
            .OverridePropertyName(UOMFileHeaders.DivisionID);

            RuleFor(v => v.LocalSiteID)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.LocalSiteID)
                .OverridePropertyName(UOMFileHeaders.LocalSiteID);

            RuleFor(v => v.PartNumber)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.PartNumber)
                 .OverridePropertyName(UOMFileHeaders.PartNumber);

            RuleFor(v => v.LocalUOM)
                    .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                    .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.LocalUOM)
                    .OverridePropertyName(UOMFileHeaders.LocalUOM);

            RuleFor(v => v.BaseUOM)
                    .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                    .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.BaseUOM)
                    .OverridePropertyName(UOMFileHeaders.BaseUOM);

            RuleFor(v => v)
                .Custom((v, context) =>
                {
                    //ConversionRate Mandatory
                    if (v.ConversionRate == null)
                    {
                        if (v.ConversionRateError == ErrorTypes.MandatoryField)
                        {
                            var failure = new ValidationFailure(UOMFileHeaders.ConversionRate, MandatoryField);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = "";
                            context.AddFailure(failure);
                        }
                        else if (v.ConversionRateError == ErrorTypes.CharacterLimitExceeded)
                        {
                            var failure = new ValidationFailure(UOMFileHeaders.ConversionRate, CharacterLimitHasBeenExceeded);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.ConversionRateOriginalStr;
                            context.AddFailure(failure);
                        }
                        else if (v.ConversionRateError != ErrorTypes.MandatoryField)
                        {
                            var failure = new ValidationFailure(UOMFileHeaders.ConversionRate, InvalidValue);
                            failure.ErrorCode = DataRowErrorStatus.Errors; 
                            failure.AttemptedValue = v.ConversionRateOriginalStr;
                            context.AddFailure(failure);
                        }
                    }
                });

            RuleFor(v => v.IncorrectColumnCount)
               .Equal(false).WithMessage(Constants.ValidationMessages.IncorrectNumberOfColumns).WithErrorCode(DataRowErrorStatus.Warning)
               .OverridePropertyName(IDataRowProperties.IncorrectColumnCount);
        }
    }
}