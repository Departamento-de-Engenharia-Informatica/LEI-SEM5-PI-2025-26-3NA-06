using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselTypeAggregate
{
    public class TypeCapacity : ValueObject
    {
        public int Value { get; private set; }

        private TypeCapacity()
        {
        }

        public TypeCapacity(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Type capacity must be greater than zero", nameof(value));

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
