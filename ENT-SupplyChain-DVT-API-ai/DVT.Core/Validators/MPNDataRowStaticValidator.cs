using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18239298: 1 - Validation Service - Validate MPN File
    /// User Story 23751267: Change Validation Rule for Mandatory fields
    /// </summary>
    public class MPNDataRowStaticValidator : AbstractValidator<MPNDataRow>
    {
        string InvalidValue = ValidationMessages.InvalidValue;
        string InvalidFormat = ValidationMessages.InvalidFormat;
        string MandatoryField = ValidationMessages.MandatoryField;
        string CharacterLimitHasBeenExceeded = ValidationMessages.CharacterLimitHasBeenExceeded;

        public MPNDataRowStaticValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.DivisionID)
              .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
              .MaximumLength(256).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
              .WithState(attemptedValue => attemptedValue.DivisionID)
              .OverridePropertyName(MPNFileHeaders.DivisionID);

            RuleFor(v => v.LocalSiteID)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors).WithState(attemptedValue => attemptedValue.LocalSiteID)
                .OverridePropertyName(MPNFileHeaders.LocalSiteID);

            RuleFor(v => v.PartNumber)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(256).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                  .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                  .WithState(attemptedValue => attemptedValue.PartNumber)
                 .OverridePropertyName(MPNFileHeaders.PartNumber);

            RuleFor(v => v.LocalManufacturerID)
                    .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.LocalManufacturerID)
                    .OverridePropertyName(MPNFileHeaders.LocalManufacturerID);

            RuleFor(v => v.ManufactureID)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.ManufactureID)
                .OverridePropertyName(MPNFileHeaders.ManufactureID);

            RuleFor(v => v.ManufactureName)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(128).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .Must(CommonValidation.ValidUTF8Character).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.ManufactureName)
                 .OverridePropertyName(MPNFileHeaders.ManufactureName);

            RuleFor(v => v.ManufacturerPartNumber)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.ManufacturerPartNumber)
                .OverridePropertyName(MPNFileHeaders.ManufacturerPartNumber);

            RuleFor(v => v.ObjectID)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.ObjectID)
                .OverridePropertyName(MPNFileHeaders.ObjectID);

            RuleFor(v => v.MPNType)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(v => !string.IsNullOrWhiteSpace(v) ? MPNMPNTypeList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.MPNType)
                .OverridePropertyName(MPNFileHeaders.MPNType);

            RuleFor(v => v.IncorrectColumnCount)
               .Equal(false).WithMessage(Constants.ValidationMessages.IncorrectNumberOfColumns).WithErrorCode(DataRowErrorStatus.Warning)
               .OverridePropertyName(IDataRowProperties.IncorrectColumnCount);
        }
    }
}
