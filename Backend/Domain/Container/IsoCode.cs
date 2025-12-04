using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.Container
{
    /// <summary>
    /// ISO 6346:2022 compliant container identifier
    /// Format: AAAU123456-C where:
    /// - AAA: Owner code (3 letters)
    /// - U: Equipment category identifier (1 letter, typically 'U' for freight containers)
    /// - 123456: Serial number (6 digits)
    /// - C: Check digit (1 digit)
    /// </summary>
    public class IsoCode : EntityId
    {
        public IsoCode(string value) : base(value)
        {
        }

        protected IsoCode() : base(Guid.NewGuid().ToString())
        {
        }

        protected override object createFromString(string text)
        {
            ValidateIsoCode(text);
            // Clean the code and return uppercase version without hyphens
            return text.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        }

        public override string AsString()
        {
            return ObjValue.ToString() ?? string.Empty;
        }

        private void ValidateIsoCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleValidationException("ISO Code cannot be empty.");

            // Remove any hyphens or spaces for validation
            string cleanCode = value.Replace("-", "").Replace(" ", "").ToUpperInvariant();

            if (cleanCode.Length != 11)
                throw new BusinessRuleValidationException(
                    $"ISO Code must be 11 characters long (format: AAAU123456C). Received: {value}");

            // Validate owner code (first 3 characters must be letters)
            if (!char.IsLetter(cleanCode[0]) || !char.IsLetter(cleanCode[1]) || !char.IsLetter(cleanCode[2]))
                throw new BusinessRuleValidationException(
                    "First 3 characters (Owner Code) must be letters.");

            // Validate equipment category identifier (4th character must be a letter)
            if (!char.IsLetter(cleanCode[3]))
                throw new BusinessRuleValidationException(
                    "4th character (Equipment Category) must be a letter.");

            // Validate serial number (characters 5-10 must be digits)
            for (int i = 4; i < 10; i++)
            {
                if (!char.IsDigit(cleanCode[i]))
                    throw new BusinessRuleValidationException(
                        $"Serial number (characters 5-10) must be digits. Invalid character at position {i + 1}.");
            }

            // Validate check digit (11th character must be a digit)
            if (!char.IsDigit(cleanCode[10]))
                throw new BusinessRuleValidationException(
                    "Check digit (11th character) must be a digit.");

            // Validate check digit calculation according to ISO 6346
            int calculatedCheckDigit = CalculateCheckDigit(cleanCode.Substring(0, 10));
            int providedCheckDigit = int.Parse(cleanCode[10].ToString());

            if (calculatedCheckDigit != providedCheckDigit)
                throw new BusinessRuleValidationException(
                    $"Invalid check digit. Expected: {calculatedCheckDigit}, Provided: {providedCheckDigit}");
        }

        private int CalculateCheckDigit(string code)
        {
            // ISO 6346 check digit calculation
            // Each character is assigned a numerical value and multiplied by its position (2^n)
            int sum = 0;
            int[] positionMultipliers = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 };

            for (int i = 0; i < 10; i++)
            {
                int value;
                if (char.IsLetter(code[i]))
                {
                    // A=10, B=12, C=13, ..., Z=38 (skipping multiples of 11: K=11->21, U=11->32, etc.)
                    value = code[i] - 'A' + 10;
                    // Adjust for multiples of 11
                    if (value == 11) value = 21; // K
                    else if (value == 21) value = 23; // U
                    else if (value == 31) value = 33; // Hypothetical, not in alphabet
                    else if (value >= 11) value++; // Shift values after K
                    if (value >= 21 && value != 23) value++; // Shift values after U
                }
                else
                {
                    value = int.Parse(code[i].ToString());
                }

                sum += value * positionMultipliers[i];
            }

            int remainder = sum % 11;
            return remainder == 10 ? 0 : remainder;
        }

        public override string ToString()
        {
            // Format as AAAU123456-C for readability
            string code = AsString();
            if (code.Length == 11 && !code.Contains("-"))
            {
                return $"{code.Substring(0, 10)}-{code[10]}";
            }
            return code;
        }
    }
}
