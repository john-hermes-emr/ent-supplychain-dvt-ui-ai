using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18239208: 1 - Validation Service - Validate PO File
    /// User Story 23751267: Change Validation Rule for Mandatory fields
    /// </summary>
    public class PODataRowStaticValidator : AbstractValidator<PODataRow>
    {
        string InvalidFormat = ValidationMessages.InvalidFormat;
        string InvalidDateFormat = ValidationMessages.InvalidDateFormat;
        string MandatoryField = ValidationMessages.MandatoryField;
        string CharacterLimitHasBeenExceeded = ValidationMessages.CharacterLimitHasBeenExceeded;
        string InvalidValue = ValidationMessages.InvalidValue;

        public PODataRowStaticValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.DivisionID)
              .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
              .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
              .WithState(attemptedValue => attemptedValue.DivisionID)
              .OverridePropertyName(POFileHeaders.DivisionId);

            RuleFor(v => v.LocalSiteID)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.LocalSiteID)
                .OverridePropertyName(POFileHeaders.LocalSiteID);

            RuleFor(v => v.PONumber)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Critical)
                 .WithState(attemptedValue => attemptedValue.PONumber)
                 .OverridePropertyName(POFileHeaders.PONumber);

            RuleFor(v => v)
                 .Custom((v, context) =>
                 {
                     //Order Date Must contain valid date format and should be less than 5 years from submission month (Submission = current month)
                     if (v.OrderDate == null)
                     {
                         if (v.OrderDateError == MandatoryField)
                         {
                             var failure = new ValidationFailure(POFileHeaders.OrderDate, MandatoryField);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = "";
                             context.AddFailure(failure);
                         }
                         else if (v.OrderDateError != MandatoryField)
                         {
                             var failure = new ValidationFailure(POFileHeaders.OrderDate, InvalidDateFormat);
                             failure.ErrorCode = DataRowErrorStatus.Critical;
                             failure.AttemptedValue = v.OrderDateStr;
                             context.AddFailure(failure);
                         }
                     }
                     else
                     {
                         var OrderDateTime = new DateTime(v.OrderDate.Value.Year, v.OrderDate.Value.Month, v.OrderDate.Value.Day);
                         var currentDateTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
                         //if is future date, invalid.
                         if (OrderDateTime > currentDateTime)
                         {
                             var failure = new ValidationFailure(POFileHeaders.OrderDate, InvalidDateFormat);
                             failure.ErrorCode = DataRowErrorStatus.Critical;
                             failure.AttemptedValue = v.OrderDateStr;
                             context.AddFailure(failure);
                         }
                         else
                         {
                             var orderDateYearMonthDate = new DateTime(v.OrderDate.Value.Year, v.OrderDate.Value.Month, 1);

                             var currentYearMonthDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

                             var years = currentYearMonthDate.Year - orderDateYearMonthDate.Year;
                             if (years > 5)
                             {
                                 var failure = new ValidationFailure(POFileHeaders.OrderDate, InvalidDateFormat);
                                 failure.ErrorCode = DataRowErrorStatus.Critical;
                                 failure.AttemptedValue = v.OrderDateStr;
                                 context.AddFailure(failure);
                             }
                             else
                             {
                                 if (currentYearMonthDate.Month < orderDateYearMonthDate.Month)
                                 {
                                     years--;
                                 }
                                 else if (currentYearMonthDate.Month > orderDateYearMonthDate.Month)
                                 {
                                     years++;
                                 }

                                 if (years > 5)
                                 {
                                     var failure = new ValidationFailure(POFileHeaders.OrderDate, InvalidDateFormat);
                                     failure.ErrorCode = DataRowErrorStatus.Critical;
                                     failure.AttemptedValue = v.OrderDateStr;
                                     context.AddFailure(failure);
                                 }
                             }
                         }
                     }

                     //LatestAmendment Optional
                     if (v.LatestAmendment == null)
                     {
                         if (v.LatestAmendmentError != MandatoryField)
                         {
                             var failure = new ValidationFailure(POFileHeaders.LatestAmendment, InvalidFormat);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.LatestAmendmentStr;
                             context.AddFailure(failure);
                         }
                     }
                 });

            RuleFor(v => v.CommodityMGRId)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.CommodityMGRId)
                .OverridePropertyName(POFileHeaders.CommodityMGRId);

            RuleFor(v => v.SupplierID)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.SupplierID)
                .OverridePropertyName(POFileHeaders.SupplierID);

            RuleFor(v => v.CurrencyCode)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.CurrencyCode)
                .OverridePropertyName(POFileHeaders.CurrencyCode);

            RuleFor(v => v.POType)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(v => !string.IsNullOrWhiteSpace(v) ? POPOTypeList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.POType)
                .OverridePropertyName(POFileHeaders.POType);

            RuleFor(v => v.IntraDiv)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(v => !string.IsNullOrWhiteSpace(v) ? POIntraDivList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.IntraDiv)
                .OverridePropertyName(POFileHeaders.IntraDiv);

            RuleFor(v => v.DirectIndirect)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(v => !string.IsNullOrWhiteSpace(v) ? PODirectIndirectList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.DirectIndirect)
                .OverridePropertyName(POFileHeaders.DirectIndirect);

            RuleFor(v => v.POTerms)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(128).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.POTerms)
                .OverridePropertyName(POFileHeaders.POTerms);

            RuleFor(v => v.FreightTerms)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.FreightTerms)
                .OverridePropertyName(POFileHeaders.FreightTerms);

            RuleFor(v => v.EDI)
               .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .Must(v => !string.IsNullOrWhiteSpace(v) ? POEDIList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.EDI)
                .OverridePropertyName(POFileHeaders.EDI);

            RuleFor(v => v.OrderStatus)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(v => !string.IsNullOrWhiteSpace(v) ? POOrderStatusList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.OrderStatus)
                .OverridePropertyName(POFileHeaders.OrderStatus);

            RuleFor(v => v.TitleTransfer)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.TitleTransfer)
                .OverridePropertyName(POFileHeaders.TitleTransfer);

            RuleFor(v => v.Port)
                 .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.Port)
                .OverridePropertyName(POFileHeaders.Port);

            RuleFor(v => v.IncorrectColumnCount)
               .Equal(false).WithMessage(Constants.ValidationMessages.IncorrectNumberOfColumns).WithErrorCode(DataRowErrorStatus.Warning)
               .OverridePropertyName(IDataRowProperties.IncorrectColumnCount);
        }
    }
}
