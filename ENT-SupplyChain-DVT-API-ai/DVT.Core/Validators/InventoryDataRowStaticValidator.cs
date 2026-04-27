using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 23751267: Change Validation Rule for Mandatory fields
    /// </summary>
    public class InventoryDataRowStaticValidator : AbstractValidator<InventoryDataRow>
    {
        string MandatoryField = ValidationMessages.MandatoryField;
        string CharacterLimitHasBeenExceeded = ValidationMessages.CharacterLimitHasBeenExceeded;
        string InvalidValue = ValidationMessages.InvalidValue;
        string ValueIsZeroInvalidValue = ValidationMessages.ValueIsZeroInvalidValue;
        string InvalidDateFormat = ValidationMessages.InvalidDateFormat;
        string FutureDate = ValidationMessages.FutureDate;
        string DateMoreThanOneMonthOld = ValidationMessages.DateMoreThanOneMonthOld;

        public InventoryDataRowStaticValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.DivisionId)
             .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
             .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
             .WithState(attemptedValue => attemptedValue.DivisionId)
             .OverridePropertyName(InventoryFileHeaders.DivisionId);

            RuleFor(v => v.LocalSiteId)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.LocalSiteId)
                .OverridePropertyName(InventoryFileHeaders.LocalSiteID);

            RuleFor(v => v.PartNumber)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.PartNumber)
                .OverridePropertyName(InventoryFileHeaders.PartNumber);

            RuleFor(v => v)
               .Custom((v, context) =>
               {
                   if (v.Quantity == null)
                   {
                       if (v.QuantityError == ErrorTypes.MandatoryField)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.Quantity, MandatoryField);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = "";
                           context.AddFailure(failure);
                       }
                       else if (v.QuantityError == ErrorTypes.InvalidFormat)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.Quantity, InvalidValue);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.QuantityOriginalStr;
                           context.AddFailure(failure);
                       }
                       else if (v.QuantityError == ErrorTypes.CharacterLimitExceeded)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.Quantity, CharacterLimitHasBeenExceeded);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.QuantityOriginalStr;
                           context.AddFailure(failure);
                       }
                   }
                   else
                   {
                       if (v.QuantityError == ErrorTypes.ValueIsZero || v.QuantityError == ErrorTypes.NegativeValue)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.Quantity, InventoryFileHeaders.Quantity + ValueIsZeroInvalidValue);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.QuantityOriginalStr;
                           context.AddFailure(failure);
                       }
                   }

                   if (v.StandardCost == null)
                   {
                       if (v.StandardCostError == ErrorTypes.InvalidFormat)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.StandardCost, InvalidValue);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.StandardCostOriginalStr;
                           context.AddFailure(failure);
                       }
                       else if (v.StandardCostError == ErrorTypes.CharacterLimitExceeded)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.StandardCost, CharacterLimitHasBeenExceeded);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.StandardCostOriginalStr;
                           context.AddFailure(failure);
                       }
                   }

                   if (v.TotalValue == null)
                   {
                       if (v.TotalValueError == ErrorTypes.MandatoryField)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.TotalValue, MandatoryField);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = "";
                           context.AddFailure(failure);
                       }
                       else if (v.TotalValueError == ErrorTypes.InvalidFormat)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.TotalValue, InvalidValue);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.TotalValueOriginalStr;
                           context.AddFailure(failure);
                       }
                       else if (v.TotalValueError == ErrorTypes.CharacterLimitExceeded)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.TotalValue, CharacterLimitHasBeenExceeded);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.TotalValueOriginalStr;
                           context.AddFailure(failure);
                       }
                   }
                   else
                   {
                       if (v.TotalValueError == ErrorTypes.ValueIsZero || v.TotalValueError == ErrorTypes.NegativeValue)
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.TotalValue, InventoryFileHeaders.TotalValue + ValueIsZeroInvalidValue);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.TotalValueOriginalStr;
                           context.AddFailure(failure);
                       }
                   }

                   if (v.InventoryDate == null)
                   {
                       var failure = new ValidationFailure(InventoryFileHeaders.InventoryDate, v.InventoryDateError);
                       if (v.InventoryDateError == MandatoryField)
                       {
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = "";
                       }
                       else
                       {
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.InventoryDateStr;
                       }
                       context.AddFailure(failure);
                   }
                   else
                   {
                       var inventoryDate = v.InventoryDate.Value;

                       // Check if it's the last day of one month
                       bool isLastDayOfMonth = inventoryDate.Day == DateTime.DaysInMonth(inventoryDate.Year, inventoryDate.Month);

                       if (isLastDayOfMonth)
                       {
                           // Check if it's a future date
                           if (inventoryDate.Date > DateTime.UtcNow.Date)
                           {
                               var failure = new ValidationFailure(InventoryFileHeaders.InventoryDate, FutureDate);
                               failure.ErrorCode = DataRowErrorStatus.Errors;
                               failure.AttemptedValue = v.InventoryDateStr;
                               context.AddFailure(failure);
                           }
                           else
                           {
                               // Check if it's more than one month before submission
                               var lastDayOfPreviousMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1);

                               if (inventoryDate.Date < lastDayOfPreviousMonth.Date)
                               {
                                   var failure = new ValidationFailure(InventoryFileHeaders.InventoryDate, DateMoreThanOneMonthOld);
                                   failure.ErrorCode = DataRowErrorStatus.Warning;
                                   failure.AttemptedValue = v.InventoryDateStr;
                                   context.AddFailure(failure);
                               }
                           }
                       }
                       else
                       {
                           var failure = new ValidationFailure(InventoryFileHeaders.InventoryDate, InvalidDateFormat);
                           failure.ErrorCode = DataRowErrorStatus.Errors;
                           failure.AttemptedValue = v.InventoryDateStr;
                           context.AddFailure(failure);
                       }
                   }
               });

            RuleFor(v => v.UOM)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.UOM)
                .OverridePropertyName(InventoryFileHeaders.UOM);

            RuleFor(v => v.CurrencyCode)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors).WithState(attemptedValue => attemptedValue.CurrencyCode)
                .OverridePropertyName(InventoryFileHeaders.CurrencyCode);

            RuleFor(v => v.PartStatus)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(v => !string.IsNullOrWhiteSpace(v) ? InventoryPartStatusList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors).WithState(attemptedValue => attemptedValue.PartStatus)
                .OverridePropertyName(InventoryFileHeaders.PartStatus);

            RuleFor(v => v.Comcode)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors).WithState(attemptedValue => attemptedValue.Comcode)
                .OverridePropertyName(InventoryFileHeaders.Comcode);

            RuleFor(v => v.DRICode)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors).WithState(attemptedValue => attemptedValue.DRICode)
                .OverridePropertyName(InventoryFileHeaders.DRICode);

            RuleFor(v => v.Description)
                .MaximumLength(256).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors).WithState(attemptedValue => attemptedValue.Description)
                .OverridePropertyName(InventoryFileHeaders.Description);

            RuleFor(v => v.IncorrectColumnCount)
               .Equal(false).WithMessage(Constants.ValidationMessages.IncorrectNumberOfColumns).WithErrorCode(DataRowErrorStatus.Warning)
               .OverridePropertyName(IDataRowProperties.IncorrectColumnCount);
        }
    }
}
