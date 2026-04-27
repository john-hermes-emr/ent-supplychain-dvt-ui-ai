using static DVT.Core.Constants;

namespace DVT.Core.Helper
{
    public static class DataConverter
    {
        public static double? ParseNullableDouble(string input, ref ErrorTypes errorType)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                errorType = ErrorTypes.MandatoryField;
                return null;
            }

            if (double.TryParse(input, out double result))
            {
                if (result == 0)
                {
                    errorType = ErrorTypes.ValueIsZero;
                }
                else if (result < 0)
                {
                    errorType = ErrorTypes.NegativeValue;
                }
                return result;
            }

            errorType = ErrorTypes.InvalidFormat;
            return null;
        }

        public static DateTime? ParseNullableDate(string input, ref string error, ref string dateStr)
        {
            dateStr = input;

            if (string.IsNullOrWhiteSpace(input))
            {
                dateStr = "";
                error = ValidationMessages.MandatoryField;
                return null;
            }

            if (input.Length != 8)
            {
                error = ValidationMessages.InvalidFormat;
                return null;
            }

            string format = "yyyyMMdd";
            if (DateTime.TryParseExact(input, format, null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            error = ValidationMessages.InvalidFormat;
            return null;
        }

        public static BigDecimal? ParseNullableBigDecimal(string input, int lengthLimit, ref ErrorTypes errorType)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                errorType = ErrorTypes.MandatoryField;
                return null;
            }
            else if (input.TrimStart('-').Length > lengthLimit)
            {
                errorType = ErrorTypes.CharacterLimitExceeded;
                return null;
            }
            //mutiple decimal points check
            else if (input.Split('.').Length > 2)
            {
                errorType = ErrorTypes.InvalidFormat;
                return null;
            }

            try
            {
                BigDecimal bigDecimal = input;

                if (bigDecimal == 0)
                {
                    errorType = ErrorTypes.ValueIsZero;
                }
                else if (bigDecimal < 0)
                {
                    errorType = ErrorTypes.NegativeValue;
                }

                return bigDecimal;
            }
            catch (Exception ex)
            {
                errorType = ErrorTypes.InvalidFormat;
                return null;
            }
        }
    }
}
