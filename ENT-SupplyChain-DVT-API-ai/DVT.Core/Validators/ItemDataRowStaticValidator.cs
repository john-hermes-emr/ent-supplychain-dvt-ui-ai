using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 23751267: Change Validation Rule for Mandatory fields
    /// </summary>
    public class ItemDataRowStaticValidator : AbstractValidator<ItemDataRow>
    {
        string InvalidFormat = ValidationMessages.InvalidFormat;
        string InvalidValue = ValidationMessages.InvalidValue;
        string MandatoryField = ValidationMessages.MandatoryField;
        string CharacterLimitHasBeenExceeded = ValidationMessages.CharacterLimitHasBeenExceeded;

        //User Story 18238793: 1 - Validation Service - Validate ITEM File
        public ItemDataRowStaticValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.DivisionId)
               .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.DivisionId)
               .OverridePropertyName(ItemFileHeaders.DivisionId);

            RuleFor(v => v.LocalSiteId)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.LocalSiteId)
                 .OverridePropertyName(ItemFileHeaders.LocalSiteId);

            RuleFor(v => v.Comcode)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.Comcode)
                .OverridePropertyName(ItemFileHeaders.Comcode);

            RuleFor(v => v.DRICode)
              .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
              .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
              .WithState(attemptedValue => attemptedValue.DRICode)
              .OverridePropertyName(ItemFileHeaders.DRICode);

            //PartStatus Mandatory, Value must only be A, I, O
            RuleFor(v => v.PartStatus)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                  .Must(v => ItemPartStatusList.Contains(v)).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.PartStatus)
                 .OverridePropertyName(ItemFileHeaders.PartStatus);

            //DirectIndirect Mandatory, Value must only be D
            RuleFor(v => v.DirectIndirect)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                  .Must(v => ItemDirectIndirectList.Contains(v)).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.DirectIndirect)
                 .OverridePropertyName(ItemFileHeaders.Direct_Indirect);

            //PurchMfrd Mandatory, Value must only be P, M or B
            RuleFor(v => v.PurchMfrd)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                  .Must(v => ItemPurchMfrdsList.Contains(v)).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.PurchMfrd)
                 .OverridePropertyName(ItemFileHeaders.Purch_mfrd);

            //PureLoadedCost Mandatory, Value must only be P, L
            RuleFor(v => v.PureLoadedCost)
                            .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                            .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                             .Must(v => ItemPureLoadedCostsList.Contains(v)).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                            .WithState(attemptedValue => attemptedValue.PureLoadedCost)
                            .OverridePropertyName(ItemFileHeaders.Pure_loadedCost);

            //ABCCategory Mandatory, Value must only be A, AA, B, C, D USE, D NEW, D E&O, U
            RuleFor(v => v.ABCCategory)
                            .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                            .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                             .Must(v => ItemABCCategoryList.Contains(v)).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                            .WithState(attemptedValue => attemptedValue.ABCCategory)
                            .OverridePropertyName(ItemFileHeaders.ABCCategory);

            //PartNumber Mandatory
            RuleFor(v => v.PartNumber)
                            .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                            .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                             .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                            .WithState(attemptedValue => attemptedValue.PartNumber)
                            .OverridePropertyName(ItemFileHeaders.PartNumber);

            //Description Mandatory
            RuleFor(v => v.Description)
                            .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                            .MaximumLength(255).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                             .Must(CommonValidation.ValidUTF8Character).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                            .WithState(attemptedValue => attemptedValue.Description)
                            .OverridePropertyName(ItemFileHeaders.Description);


            RuleFor(v => v)
                 .Custom((v, context) =>
                 {
                     //LeadTime Mandatory
                     if (v.LeadTime == null)
                     {
                         if (v.LeadTimeError == ErrorTypes.MandatoryField)
                         {
                             var failure = new ValidationFailure(ItemFileHeaders.LeadTime, MandatoryField);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = "";
                             context.AddFailure(failure);
                         }
                         else if (v.LeadTimeError == ErrorTypes.InvalidFormat)
                         {
                             var failure = new ValidationFailure(ItemFileHeaders.LeadTime, InvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.LeadTimeOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.LeadTimeError == ErrorTypes.CharacterLimitExceeded)
                         {
                             var failure = new ValidationFailure(ItemFileHeaders.LeadTime, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.LeadTimeOriginalStr;
                             context.AddFailure(failure);
                         }
                     }

                     //StandardCost Mandatory
                     if (v.StandardCost == null)
                     {
                         if (v.StandardCostError == ErrorTypes.MandatoryField)
                         {
                             var failure = new ValidationFailure(ItemFileHeaders.StandardCost, MandatoryField);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = "";
                             context.AddFailure(failure);
                         }
                         else if (v.StandardCostError == ErrorTypes.InvalidFormat)
                         {
                             var failure = new ValidationFailure(ItemFileHeaders.StandardCost, InvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.StandardCostOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.StandardCostError == ErrorTypes.CharacterLimitExceeded)
                         {
                             var failure = new ValidationFailure(ItemFileHeaders.StandardCost, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.StandardCostOriginalStr;
                             context.AddFailure(failure);
                         }
                     }
                     else if (v.StandardCostError == ErrorTypes.ValueIsZero)
                     {
                         var failure = new ValidationFailure(ItemFileHeaders.StandardCost, InvalidValue);
                         failure.ErrorCode = DataRowErrorStatus.Errors;
                         failure.AttemptedValue = v.StandardCostOriginalStr;
                         context.AddFailure(failure);
                     }

                     //ItemWeight Optional
                     if (v.ItemWeight == null)
                     {
                         if (v.ItemWeightError == ErrorTypes.CharacterLimitExceeded)
                         {
                             var failure = new ValidationFailure(ItemFileHeaders.ItemWeight, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.ItemWeightOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.ItemWeightError != ErrorTypes.MandatoryField)
                         {
                             var failure = new ValidationFailure(ItemFileHeaders.ItemWeight, InvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.ItemWeightOriginalStr;
                             context.AddFailure(failure);
                         }
                     }
                 });

            RuleFor(v => v.CurrencyCode)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.CurrencyCode)
                .OverridePropertyName(ItemFileHeaders.CurrencyCode);

            RuleFor(v => v.UOM)
               .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.UOM)
               .OverridePropertyName(ItemFileHeaders.UOM);

            RuleFor(v => v.ItemWeightUOM)
               .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.ItemWeightUOM)
               .OverridePropertyName(ItemFileHeaders.ItemWeightUOM);

            RuleFor(v => v.ItemHtsCode)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.ItemHtsCode)
                 .OverridePropertyName(ItemFileHeaders.ItemHTSCode);

            RuleFor(v => v.ItemHsCode)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.ItemHsCode)
                 .OverridePropertyName(ItemFileHeaders.ItemHSCode);

            RuleFor(v => v.IncorrectColumnCount)
               .Equal(false).WithMessage(Constants.ValidationMessages.IncorrectNumberOfColumns).WithErrorCode(DataRowErrorStatus.Warning)
               .OverridePropertyName(IDataRowProperties.IncorrectColumnCount);
        }
    }
}