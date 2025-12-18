using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.StorageAreaAggregate
{
    public class MaxCapacity : ValueObject
    {
        public int Value { get; }

        public MaxCapacity(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Max capacity must be greater than zero.");
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}