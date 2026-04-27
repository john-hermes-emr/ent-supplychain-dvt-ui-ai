using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using FluentValidation.Results;
using System.Text.RegularExpressions;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 23751267: Change Validation Rule for Mandatory fields
    /// </summary>
    public class VirDataRowStaticValidator : AbstractValidator<VirDataRow>
    {
        string InvalidFormat = ValidationMessages.InvalidFormat;
        string MandatoryField = ValidationMessages.MandatoryField;
        string IncorrectDateFormat = ValidationMessages.IncorrectDateFormat;
        string ValueIsZeroInvalidValue = ValidationMessages.ValueIsZeroInvalidValue;
        //Task 19785141: VIR - Validation message update and enhancement update for the status and warning message to Mandatory fields
        string CharacterLimitHasBeenExceeded = ValidationMessages.CharacterLimitHasBeenExceeded;
        string InvalidValue = ValidationMessages.InvalidValue;

        /// <summary>
        /// Bug 23714652: [QA Bug] - VIR file - Blank data errors for multiple fields
        /// </summary>
        public VirDataRowStaticValidator()
        {
            // Set the default cascade mode for all rules in this validator
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.DivisionId)
               //User Story 19782923: VIR - Validation message update and enhancement update for the status and warning message to Mandatory fields --- MandatoryField status is Critical
               .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.DivisionId)
               .OverridePropertyName(VirFileHeaders.DivisionId);

            RuleFor(v => v.LocalSiteId)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.LocalSiteId)
                .OverridePropertyName(VirFileHeaders.LocalSiteId);

            RuleFor(v => v.PoNumber)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.PoNumber)
                 .OverridePropertyName(VirFileHeaders.PoNumber);

            RuleFor(v => v.POLineNumber)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.POLineNumber)
                .OverridePropertyName(VirFileHeaders.PoLineNumber);

            RuleFor(v => v.SupplierId)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.SupplierId)
                .OverridePropertyName(VirFileHeaders.SupplierId);

            RuleFor(v => v.PartNumber)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.PartNumber)
                .OverridePropertyName(VirFileHeaders.PartNumber);

            RuleFor(v => v.SupplierPartNumber)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.SupplierPartNumber)
                .OverridePropertyName(VirFileHeaders.SupplierPartNumber);

            RuleFor(v => v)
                 .Custom((v, context) =>
                 {
                     //User Story 12788304: VIR - Field values must contain ASCII English characters
                     if (string.IsNullOrWhiteSpace(v.ReceiptNumber))
                     {
                         var failure = new ValidationFailure(VirFileHeaders.ReceiptNumber, MandatoryField);
                         failure.ErrorCode = DataRowErrorStatus.Errors;
                         failure.AttemptedValue = "";
                         context.AddFailure(failure);
                     }
                     else
                     {
                         if (v.ReceiptNumber.Length > 50)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.ReceiptNumber, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.ReceiptNumber;
                             context.AddFailure(failure);
                         }
                         else
                         {
                             if (!Regex.IsMatch(v.ReceiptNumber, SpecialStringRegularExpression.ASCII))
                             {
                                 var failure = new ValidationFailure(VirFileHeaders.ReceiptNumber, InvalidFormat);
                                 failure.ErrorCode = DataRowErrorStatus.Errors;
                                 failure.AttemptedValue = v.ReceiptNumber;
                                 context.AddFailure(failure);
                             }
                         }
                     }

                     //Bug 19934645: [QA Bug] - Validation message must be INVALID VALUE
                     //QuantityOrdered Optional
                     if (v.QuantityOrdered == null)
                     {
                         if (v.QuantityOrderedError == ErrorTypes.CharacterLimitExceeded)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.QuantityOrdered, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.QuantityOrderedOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.QuantityOrderedError != ErrorTypes.MandatoryField)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.QuantityOrdered, InvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.QuantityOrderedOriginalStr;
                             context.AddFailure(failure);
                         }
                     }

                     //QuantityReceived Mandatory
                     if (v.QuantityReceived == null)
                     {
                         if (v.QuantityReceivedError == ErrorTypes.MandatoryField)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.QuantityReceived, MandatoryField);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = "";
                             context.AddFailure(failure);
                         }
                         else if (v.QuantityReceivedError == ErrorTypes.InvalidFormat)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.QuantityReceived, VirFileHeaders.QuantityReceived + ValueIsZeroInvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.QuantityReceivedOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.QuantityReceivedError == ErrorTypes.CharacterLimitExceeded)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.QuantityReceived, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.QuantityReceivedOriginalStr;
                             context.AddFailure(failure);
                         }
                     }
                     else
                     {
                         if (v.QuantityReceivedError == ErrorTypes.ValueIsZero)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.QuantityReceived, VirFileHeaders.QuantityReceived + ValueIsZeroInvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.QuantityReceivedOriginalStr;
                             context.AddFailure(failure);
                         }
                     }

                     //Bug 19519775: [QA Bug] - Date Received is a Mandatory field
                     //Bug 19518786: [QA Bug] - Invalid future date includes +1 future day and +1 future month and Day 32
                     //DateReceived Mandatory
                     if (v.DateReceived == null)
                     {
                         var failure = new ValidationFailure(VirFileHeaders.DateReceived, v.DateReceivedError);
                         if (v.DateReceivedError == MandatoryField)
                         {
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = "";
                         }
                         else
                         {
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.DateReceivedStr;
                         }
                         context.AddFailure(failure);
                     }
                     else
                     {
                         //if (v.DateReceived < new DateTime(1900, 1, 1))
                         //{
                         //    var failure = new ValidationFailure(VirFileHeaders.DateReceived, IncorrectDateFormat);
                         //    failure.ErrorCode = DataRowErrorStatus.Errors;
                         //}
                         //date value falls on current month and future months: if today is 6/15/2024, then 6/1/2024 and any date in future months are invalid
                         if (v.DateReceived >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1))
                         {
                             var failure = new ValidationFailure(VirFileHeaders.DateReceived, IncorrectDateFormat);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.DateReceivedStr;
                             context.AddFailure(failure);
                         }
                     }

                     //CommittedDate Mandatory
                     if (v.CommittedDate == null)
                     {
                         var failure = new ValidationFailure(VirFileHeaders.CommittedDate, v.CommittedDateError);
                         if (v.CommittedDateError == MandatoryField)
                         {
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = "";
                         }
                         else
                         {
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.CommittedDateStr;
                         }
                         context.AddFailure(failure);
                     }
                     //else
                     //{
                     //    if (v.CommittedDate < new DateTime(1900, 1, 1))
                     //    {
                     //        context.AddFailure(VirFileHeaders.CommittedDate, IncorrectDateFormat);
                     //    }
                     //}

                     //InvoicePricePaid Mandatory
                     if (v.InvoicePricePaid == null)
                     {
                         if (v.InvoicePricePaidError == ErrorTypes.MandatoryField)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.InvoicePricePaid, MandatoryField);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = "";
                             context.AddFailure(failure);
                         }
                         else if (v.InvoicePricePaidError == ErrorTypes.InvalidFormat)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.InvoicePricePaid, VirFileHeaders.InvoicePricePaid + ValueIsZeroInvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.InvoicePricePaidOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.InvoicePricePaidError == ErrorTypes.CharacterLimitExceeded)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.InvoicePricePaid, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.InvoicePricePaidOriginalStr;
                             context.AddFailure(failure);
                         }
                     }
                     else
                     {
                         //Task 20173058: VIR - Character limits validation - Enhancement - API --- Invoice Price Paid max value check
                         if (v.InvoicePricePaidError == ErrorTypes.ValueIsZero)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.InvoicePricePaid, VirFileHeaders.InvoicePricePaid + ValueIsZeroInvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.InvoicePricePaidOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.InvoicePricePaid.Value > NumberValueRanges.VirInvoicePricePaidMaxValue)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.InvoicePricePaid, ValidationMessages.InvoicePricePaidIsOverMaximum);
                             failure.ErrorCode = DataRowErrorStatus.Warning;
                             failure.AttemptedValue = v.InvoicePricePaidOriginalStr;
                             context.AddFailure(failure);
                         }
                     }

                     //UnitPrice Mandatory
                     if (v.UnitPrice == null)
                     {
                         if (v.UnitPriceError == ErrorTypes.MandatoryField)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.UnitPrice, MandatoryField);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = "";
                             context.AddFailure(failure);
                         }
                         else if (v.UnitPriceError == ErrorTypes.InvalidFormat)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.UnitPrice, VirFileHeaders.UnitPrice + ValueIsZeroInvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.UnitPriceOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.UnitPriceError == ErrorTypes.CharacterLimitExceeded)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.UnitPrice, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.UnitPriceOriginalStr;
                             context.AddFailure(failure);
                         }
                     }
                     else
                     {
                         if (v.UnitPriceError == ErrorTypes.ValueIsZero || v.UnitPriceError == ErrorTypes.NegativeValue)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.UnitPrice, VirFileHeaders.UnitPrice + ValueIsZeroInvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.UnitPriceOriginalStr;
                             context.AddFailure(failure);
                         }
                     }

                     //Release Optional
                     if (v.Release == null)
                     {
                         if (v.ReleaseError == ErrorTypes.CharacterLimitExceeded)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.ReleaseNumber, CharacterLimitHasBeenExceeded);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.ReleaseOriginalStr;
                             context.AddFailure(failure);
                         }
                         else if (v.ReleaseError == ErrorTypes.InvalidFormat)
                         {
                             var failure = new ValidationFailure(VirFileHeaders.ReleaseNumber, VirFileHeaders.ReleaseNumber + ValueIsZeroInvalidValue);
                             failure.ErrorCode = DataRowErrorStatus.Errors;
                             failure.AttemptedValue = v.ReleaseOriginalStr;
                             context.AddFailure(failure);
                         }
                     }
                 });

            RuleFor(v => v.PureLoadedCost)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.PureLoadedCost)
                .OverridePropertyName(VirFileHeaders.PureLoadedCost);

            RuleFor(v => v.CurrencyCode)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.CurrencyCode)
                .OverridePropertyName(VirFileHeaders.CurrencyCode);

            RuleFor(v => v.IntraDiv)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.IntraDiv)
                .OverridePropertyName(VirFileHeaders.IntraDiv);

            RuleFor(v => v.DirectIndirect)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.DirectIndirect)
                .OverridePropertyName(VirFileHeaders.DirectIndirect);

            RuleFor(v => v.POTerms)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(128).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.POTerms)
                .OverridePropertyName(VirFileHeaders.PoTerms);

            RuleFor(v => v.FreightTerms)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.FreightTerms)
                .OverridePropertyName(VirFileHeaders.FreightTerms);

            RuleFor(v => v.UOM)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.UOM)
                .OverridePropertyName(VirFileHeaders.Uom);

            RuleFor(v => v.TitleTransfer)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.TitleTransfer)
                .OverridePropertyName(VirFileHeaders.TitleTransfer);

            RuleFor(v => v.Port)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.Port)
                .OverridePropertyName(VirFileHeaders.Port);

            RuleFor(v => v.IncorrectColumnCount)
               .Equal(false).WithMessage(Constants.ValidationMessages.IncorrectNumberOfColumns).WithErrorCode(DataRowErrorStatus.Warning)
               .OverridePropertyName(IDataRowProperties.IncorrectColumnCount);
        }
    }
}
