using ProjArqsi.Domain.Shared;


// Pay attention: IMO number follows the official format (seven digits with a check digit)
namespace ProjArqsi.Domain.VesselAggregate
{
    public class IMOnumber : EntityId
    {
        public string Number { get; private set; }

        public IMOnumber(string number) : base(number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new InvalidOperationException("IMO number cannot be empty");

            if (!IsValidIMO(number, out string errorMessage))
                throw new InvalidOperationException(errorMessage);

            Number = number;
        }

        public override object CreateFromString(string text)
        {
            // For IMO numbers, just return the string itself (do not parse as Guid)
            return text;
        }

        private static bool IsValidIMO(string imoNumber, out string errorMessage)
        {
            // IMO must be exactly 7 digits
            if (imoNumber.Length != 7)
            {
                errorMessage = "IMO number must be exactly 7 digits";
                return false;
            }

            // All characters must be digits
            if (!imoNumber.All(char.IsDigit))
            {
                errorMessage = "IMO number must contain only numeric digits";
                return false;
            }

            // Calculate check digit using the first 6 digits
            int sum = 0;
            int[] multipliers = { 7, 6, 5, 4, 3, 2 };

            for (int i = 0; i < 6; i++)
            {
                sum += (imoNumber[i] - '0') * multipliers[i];
            }

            // The check digit is the last digit of the sum (mod 10)
            int checkDigit = sum % 10;
            int lastDigit = imoNumber[6] - '0';

            if (checkDigit != lastDigit)
            {
                errorMessage = $"Invalid IMO check digit: expected {checkDigit}, but got {lastDigit}";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
