using System.Collections.Generic;
using ProjArqsi.Domain.DockAggregate;

namespace ProjArqsi.Domain.StorageArea.ValueObjects
{
	public class ServedDocks
	{
		public List<DockId> Value { get; private set; } = new();

		public ServedDocks(List<DockId> docks)
		{
			Value = docks ?? new List<DockId>();
		}
	}
}