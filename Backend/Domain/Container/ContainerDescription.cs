using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.Container
{
    /// <summary>
    /// Detailed description of the container contents
    /// </summary>
    public class ContainerDescription : ValueObject
    {
        public string Text { get; private set; }

        public ContainerDescription(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new BusinessRuleValidationException("Container description cannot be empty.");

            if (text.Length > 500)
                throw new BusinessRuleValidationException("Container description cannot exceed 500 characters.");

            Text = text.Trim();
        }

        protected ContainerDescription()
        {
            Text = default!;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
