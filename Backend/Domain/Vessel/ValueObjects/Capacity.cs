using System;
using System.Collections.Generic;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class Capacity : ValueObject
    {
        public int Value { get; private set; }

        private Capacity()
        {
        }

        public Capacity(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Capacity must be greater than zero", nameof(value));

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return $"{Value} TEU";
        }
    }
}
