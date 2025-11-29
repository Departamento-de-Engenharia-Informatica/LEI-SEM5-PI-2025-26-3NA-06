using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.Container
{
    /// <summary>
    /// Position of container on vessel or in storage area
    /// Defined by Bay (lengthwise), Row (width), and Tier (vertical stack)
    /// </summary>
    public class ContainerPosition : ValueObject
    {
        public int Bay { get; private set; }
        public int Row { get; private set; }
        public int Tier { get; private set; }

        public ContainerPosition(int bay, int row, int tier)
        {
            if (bay < 1)
                throw new BusinessRuleValidationException("Bay must be greater than zero.");

            if (row < 1)
                throw new BusinessRuleValidationException("Row must be greater than zero.");

            if (tier < 1)
                throw new BusinessRuleValidationException("Tier must be greater than zero.");

            Bay = bay;
            Row = row;
            Tier = tier;
        }

        protected ContainerPosition()
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Bay;
            yield return Row;
            yield return Tier;
        }

        public override string ToString()
        {
            return $"Bay {Bay:D2}, Row {Row:D2}, Tier {Tier:D2}";
        }
    }
}
