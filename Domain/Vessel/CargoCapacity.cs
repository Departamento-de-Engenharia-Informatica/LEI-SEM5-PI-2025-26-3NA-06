using System;
using System.Collections.Generic;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public class CargoCapacity : ValueObject
    {
        public int Teu { get; private set; }

        private CargoCapacity()
        {
        }

        public CargoCapacity(int teu)
        {
            if (teu <= 0)
                throw new ArgumentException("Cargo capacity must be greater than zero", nameof(teu));

            Teu = teu;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Teu;
        }

        public override string ToString()
        {
            return $"{Teu} TEU";
        }
    }
}
