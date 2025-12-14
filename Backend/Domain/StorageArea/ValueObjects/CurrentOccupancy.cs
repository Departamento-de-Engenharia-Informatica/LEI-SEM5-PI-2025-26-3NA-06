using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.StorageAreaAggregate
{
    public class CurrentOccupancy : IValueObject
    {
        public int Value { get; }

        public CurrentOccupancy(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Current occupancy cannot be negative.");

            Value = value;
        }
    }
}