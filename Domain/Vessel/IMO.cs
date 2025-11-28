using ProjArqsi.Domain.Shared;


// Pay attention: IMO number follows the official format (seven digits with a check digit)
namespace ProjArqsi.Domain.VesselAggregate
{
    public class IMO : EntityId
    {
        public string Number { get; private set; }

        public IMO(string number) : base(number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("IMO number cannot be empty", nameof(number));

            Number = number;
        }

        protected override Object createFromString(String text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("IMO number cannot be empty", nameof(text));
            return text;
        }

        public override String AsString()
        {
            return (string)ObjValue;
        }
    }
}
