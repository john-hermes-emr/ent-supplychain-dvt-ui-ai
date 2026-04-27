using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using FluentValidation.Results;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18239248: 1 - Validation Service - Validate POITEM File
    /// User Story 23751267: Change Validation Rule for Mandatory fields
    /// </summary>
    public class POItemDataRowStaticValidator : AbstractValidator<POItemDataRow>
    {
        string InvalidFormat = ValidationMessages.InvalidFormat;
        string InvalidDateFormat = ValidationMessages.InvalidDateFormat;
        string MandatoryField = ValidationMessages.MandatoryField;
        string CharacterLimitHasBeenExceeded = ValidationMessages.CharacterLimitHasBeenExceeded;
        string InvalidValue = ValidationMessages.InvalidValue;

        public POItemDataRowStaticValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.DivisionID)
             .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
             .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
             .WithState(attemptedValue => attemptedValue.DivisionID)
             .OverridePropertyName(POItemFileHeaders.DivisionID);

            RuleFor(v => v.LocalSiteID)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.LocalSiteID)
                .OverridePropertyName(POItemFileHeaders.LocalSiteID);

            RuleFor(v => v.PONumber)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.PONumber)
                 .OverridePropertyName(POItemFileHeaders.PONumber);

            RuleFor(v => v.POLineNumber)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                 .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                 .WithState(attemptedValue => attemptedValue.POLineNumber)
                 .OverridePropertyName(POItemFileHeaders.POLineNumber);

            RuleFor(v => v.PartNumber)
                    .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                    .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.PartNumber)
                    .OverridePropertyName(POItemFileHeaders.PartNumber);

            RuleFor(v => v.SupplierPartNumber)
                    .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.SupplierPartNumber)
                    .OverridePropertyName(POItemFileHeaders.SupplierPartNumber);

            RuleFor(v => v.Description)
                    .MaximumLength(255).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.Description)
                    .OverridePropertyName(POItemFileHeaders.Description);

            RuleFor(v => v.ContractID)
                    .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.ContractID)
                    .OverridePropertyName(POItemFileHeaders.ContractID);

            RuleFor(v => v.PureLoadedCost)
                    .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                    .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .Must(v => !string.IsNullOrWhiteSpace(v) ? POItemPureLoadedCostList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.PureLoadedCost)
                    .OverridePropertyName(POItemFileHeaders.PureLoadedCost);

            RuleFor(v => v.OrderStatus)
                    .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                    .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .Must(v => !string.IsNullOrWhiteSpace(v) ? POItemOrderStatusList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.OrderStatus)
                    .OverridePropertyName(POItemFileHeaders.OrderStatus);

            RuleFor(v => v.CurrencyCode)
                    .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                    .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.CurrencyCode)
                    .OverridePropertyName(POItemFileHeaders.CurrencyCode);

            RuleFor(v => v.UOM)
                    .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                    .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.UOM)
                    .OverridePropertyName(POItemFileHeaders.UOM);

            RuleFor(v => v)
                .Custom((v, context) =>
                {
                    //UnitCost Optional
                    if (v.UnitCost == null)
                    {
                        if (v.UnitCostError == ErrorTypes.CharacterLimitExceeded)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.UnitCost, CharacterLimitHasBeenExceeded);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.UnitCostOriginalStr;
                            context.AddFailure(failure);
                        }
                        else if (v.UnitCostError != ErrorTypes.MandatoryField)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.UnitCost, InvalidValue);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.UnitCostOriginalStr;
                            context.AddFailure(failure);
                        }
                    }

                    //OrderedValue Mandatory
                    if (v.OrderedValue == null)
                    {
                        if (v.OrderedValueError == ErrorTypes.MandatoryField)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.OrderedValue, MandatoryField);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = "";
                            context.AddFailure(failure);
                        }
                        else if (v.OrderedValueError == ErrorTypes.InvalidFormat)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.OrderedValue, InvalidValue);
                            failure.ErrorCode = DataRowErrorStatus.Critical;
                            failure.AttemptedValue = v.OrderedValueOriginalStr;
                            context.AddFailure(failure);
                        }
                        else if (v.OrderedValueError == ErrorTypes.CharacterLimitExceeded)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.OrderedValue, CharacterLimitHasBeenExceeded);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.OrderedValueOriginalStr;
                            context.AddFailure(failure);
                        }
                    }

                    //QuantityOrdered Mandatory
                    if (v.QuantityOrdered == null)
                    {
                        if (v.QuantityOrderedError == ErrorTypes.MandatoryField)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.QuantityOrdered, MandatoryField);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = "";
                            context.AddFailure(failure);
                        }
                        else if (v.QuantityOrderedError == ErrorTypes.InvalidFormat)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.QuantityOrdered, InvalidValue);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.QuantityOrderedOriginalStr;
                            context.AddFailure(failure);
                        }
                        else if (v.QuantityOrderedError == ErrorTypes.CharacterLimitExceeded)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.QuantityOrdered, CharacterLimitHasBeenExceeded);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.QuantityOrderedOriginalStr;
                            context.AddFailure(failure);
                        }
                    }

                    //QuantityReturned Optional
                    if (v.QuantityReturned == null)
                    {
                        if (v.QuantityReturnedError == ErrorTypes.InvalidFormat)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.QuantityReturned, InvalidValue);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.QuantityReturnedOriginalStr;
                            context.AddFailure(failure);
                        }
                        else if (v.QuantityReturnedError == ErrorTypes.CharacterLimitExceeded)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.QuantityReturned, CharacterLimitHasBeenExceeded);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.QuantityReturnedOriginalStr;
                            context.AddFailure(failure);
                        }
                    }

                    //CommittedDate Mandatory
                    if (v.CommittedDate == null)
                    {
                        if (v.CommittedDateError == MandatoryField)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.CommittedDate, MandatoryField);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = "";
                            context.AddFailure(failure);
                        }
                        else if (v.CommittedDateError != MandatoryField)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.CommittedDate, InvalidDateFormat);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.CommittedDateStr;
                            context.AddFailure(failure);
                        }
                    }

                    //RequestedDate Optional
                    if (v.RequestedDate == null)
                    {
                        if (v.RequestedDateError != MandatoryField)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.RequestedDate, InvalidDateFormat);
                            failure.ErrorCode = DataRowErrorStatus.Critical;
                            failure.AttemptedValue = v.RequestedDateStr;
                            context.AddFailure(failure);
                        }
                    }

                    //QtyLeftToReceive Mandatory
                    if (v.QtyLeftToReceive == null)
                    {
                        if (v.QtyLeftToReceiveError == ErrorTypes.MandatoryField)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.QtyLeftToReceive, MandatoryField);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = "";
                            context.AddFailure(failure);
                        }
                        else if (v.QtyLeftToReceiveError == ErrorTypes.InvalidFormat)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.QtyLeftToReceive, InvalidValue);
                            failure.ErrorCode = DataRowErrorStatus.Critical;
                            failure.AttemptedValue = v.QtyLeftToReceiveOriginalStr;
                            context.AddFailure(failure);
                        }
                        else if (v.QtyLeftToReceiveError == ErrorTypes.CharacterLimitExceeded)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.QtyLeftToReceive, CharacterLimitHasBeenExceeded);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.QtyLeftToReceiveOriginalStr;
                            context.AddFailure(failure);
                        }
                    }

                    //ValueLeftToReceive Mandatory
                    if (v.ValueLeftToReceive == null)
                    {
                        if (v.ValueLeftToReceiveError == ErrorTypes.MandatoryField)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.ValueLeftToReceive, MandatoryField);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = "";
                            context.AddFailure(failure);
                        }
                        else if (v.ValueLeftToReceiveError == ErrorTypes.InvalidFormat)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.ValueLeftToReceive, InvalidValue);
                            failure.ErrorCode = DataRowErrorStatus.Critical;
                            failure.AttemptedValue = v.ValueLeftToReceiveOriginalStr;
                            context.AddFailure(failure);
                        }
                        else if (v.ValueLeftToReceiveError == ErrorTypes.CharacterLimitExceeded)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.ValueLeftToReceive, CharacterLimitHasBeenExceeded);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.ValueLeftToReceiveOriginalStr;
                            context.AddFailure(failure);
                        }
                    }

                    //Release Optional
                    if (v.Release == null)
                    {
                        if (v.ReleaseError == ErrorTypes.InvalidFormat)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.Release, InvalidValue);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.ReleaseOriginalStr;
                            context.AddFailure(failure);
                        }
                        else if (v.ReleaseError == ErrorTypes.CharacterLimitExceeded)
                        {
                            var failure = new ValidationFailure(POItemFileHeaders.Release, CharacterLimitHasBeenExceeded);
                            failure.ErrorCode = DataRowErrorStatus.Errors;
                            failure.AttemptedValue = v.ReleaseOriginalStr;
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
