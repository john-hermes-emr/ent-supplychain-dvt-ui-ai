using DocumentFormat.OpenXml.ExtendedProperties;
using DVT.Core.Helper;
using DVT.Core.Models.DataRowEntities;
using FluentValidation;
using System.Text.RegularExpressions;
using static DVT.Core.Constants;

namespace DVT.Core.Validators
{
    /// <summary>
    /// User Story 18238976: 1 - Validation Service - Validate Supplier File
    /// User Story 23751267: Change Validation Rule for Mandatory fields
    /// </summary>
    public class SupplierDataRowStaticValidator : AbstractValidator<SupplierDataRow>
    {
        string InvalidFormat = ValidationMessages.InvalidFormat;
        string InvalidValue = ValidationMessages.InvalidValue;
        string MandatoryField = ValidationMessages.MandatoryField;
        string CharacterLimitHasBeenExceeded = ValidationMessages.CharacterLimitHasBeenExceeded;

        public SupplierDataRowStaticValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.DivisionId)
               .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.DivisionId)
               .OverridePropertyName(SupplierFileHeaders.DivisionId);

            RuleFor(v => v.LocalSiteId)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.LocalSiteId)
                .OverridePropertyName(SupplierFileHeaders.LocalSiteId);

            RuleFor(v => v.SupplierId)
               .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .Must(CommonValidation.ValidASCIIEnglishCharacter).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.SupplierId)
               .OverridePropertyName(SupplierFileHeaders.SupplierId);

            RuleFor(v => v.SupplierName)
               .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(120).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(CommonValidation.ValidUTF8Character).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.SupplierName)
               .OverridePropertyName(SupplierFileHeaders.SupplierName);

            RuleFor(v => v.DUNS)
              .MaximumLength(100).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
              .WithState(attemptedValue => attemptedValue.DUNS)
              .OverridePropertyName(SupplierFileHeaders.DUNS);

            RuleFor(v => v.ActiveInactive)
               .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .Must(v => !string.IsNullOrWhiteSpace(v) ? SupplierActiveInactiveList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.ActiveInactive)
               .OverridePropertyName(SupplierFileHeaders.ActiveInactive);

            RuleFor(v => v.DirectIndirect)
               .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .Must(v => !string.IsNullOrWhiteSpace(v) ? SupplierDirectIndirectList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
               .WithState(attemptedValue => attemptedValue.DirectIndirect)
               .OverridePropertyName(SupplierFileHeaders.DirectIndirect);

            RuleFor(v => v.AddressDescr)
             .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
             .WithState(attemptedValue => attemptedValue.AddressDescr)
             .OverridePropertyName(SupplierFileHeaders.AddressDescr);

            RuleFor(v => v.Street)
                .MaximumLength(80).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.Street)
                .OverridePropertyName(SupplierFileHeaders.Street);

            RuleFor(v => v.Suite)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.Suite)
                .OverridePropertyName(SupplierFileHeaders.Suite);

            //Bug 21317889: [QA BUG] - no character limit for 'city'
            RuleFor(v => v.City)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.City)
               .OverridePropertyName(SupplierFileHeaders.City);

            RuleFor(v => v.State)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.State)
                .OverridePropertyName(SupplierFileHeaders.State);

            RuleFor(v => v.PostalCode)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.PostalCode)
               .OverridePropertyName(SupplierFileHeaders.PostalCode);

            RuleFor(v => v.County)
                .MaximumLength(30).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.County)
                .OverridePropertyName(SupplierFileHeaders.County);

            RuleFor(v => v.Country)
                 .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.Country)
                .OverridePropertyName(SupplierFileHeaders.Country);

            RuleFor(v => v.Addr1)
                .MaximumLength(128).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .Must(CommonValidation.ValidUTF8Character).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.Addr1)
                .OverridePropertyName(SupplierFileHeaders.Addr1);

            RuleFor(v => v.Addr2)
                .MaximumLength(128).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .Must(CommonValidation.ValidUTF8Character).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.Addr2)
                .OverridePropertyName(SupplierFileHeaders.Addr2);

            RuleFor(v => v.Addr3)
                .MaximumLength(128).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                 .Must(CommonValidation.ValidUTF8Character).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.Addr3)
                 .OverridePropertyName(SupplierFileHeaders.Addr3);

            RuleFor(v => v.Addr4)
                .MaximumLength(128).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(CommonValidation.ValidUTF8Character).WithMessage(InvalidFormat).WithErrorCode(DataRowErrorStatus.Errors)
                    .WithState(attemptedValue => attemptedValue.Addr4)
                .OverridePropertyName(SupplierFileHeaders.Addr4);

            RuleFor(v => v.CountryCode)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.CountryCode)
               .OverridePropertyName(SupplierFileHeaders.CountryCode);

            RuleFor(v => v.GlobalFlag)
                .MaximumLength(10).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .Must(v => !string.IsNullOrWhiteSpace(v) ? (SupplierGlobalFlagList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase))) : true).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.GlobalFlag)
                .OverridePropertyName(SupplierFileHeaders.GlobalFlag);

            RuleFor(v => v.MainTelephone)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .Must(CommonValidation.ValidMainTelephone).WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.MainTelephone)
               .OverridePropertyName(SupplierFileHeaders.MainTelephone);

            RuleFor(v => v.TollFree)
                .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.TollFree)
                .OverridePropertyName(SupplierFileHeaders.TollFree);

            RuleFor(v => v.Fax)
                .MaximumLength(20).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.Fax)
                .OverridePropertyName(SupplierFileHeaders.Fax);

            RuleFor(v => v.WebSite)
                .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.WebSite)
                .OverridePropertyName(SupplierFileHeaders.WebSite);

            RuleFor(v => v.SupplierType)
                .NotEmpty().WithMessage(MandatoryField).WithErrorCode(DataRowErrorStatus.Errors)
               .MaximumLength(50).WithMessage(CharacterLimitHasBeenExceeded).WithErrorCode(DataRowErrorStatus.Errors)
               .Must(v => !string.IsNullOrWhiteSpace(v) ? SupplierSupplierTypeList.Any(x => v.Equals(x, StringComparison.OrdinalIgnoreCase)) : true)
               .WithMessage(InvalidValue).WithErrorCode(DataRowErrorStatus.Errors)
                .WithState(attemptedValue => attemptedValue.SupplierType)
               .OverridePropertyName(SupplierFileHeaders.SupplierType);

            RuleFor(v => v.IncorrectColumnCount)
               .Equal(false).WithMessage(Constants.ValidationMessages.IncorrectNumberOfColumns).WithErrorCode(DataRowErrorStatus.Warning)
               .OverridePropertyName(IDataRowProperties.IncorrectColumnCount);
        }
    }
}
