using System;
using System.Collections.Generic;
using System.Linq;

namespace Warehouse.Domain.Entities
{
    public class PickingSlot : IEquatable<PickingSlot>
    {
		public Coord Coords { get; private set; }
		public string Adress => $"F-{AlleyNumber:D3}-{PositionNumber:D3}";
		public int AlleyNumber { get; set; }
		public int PositionNumber { get; set; }
		public  long Sku { get; set; }
		public int Units { get; set; }
		public int ReservedUnits { get; set; }

		public PickingSlot(Coord positon, int alleyNumber, int positionNumber)
		{
			Coords = positon;
			AlleyNumber = alleyNumber;
			PositionNumber = positionNumber;
		}

		public bool Equals(PickingSlot other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Adress == other.Adress;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((PickingSlot) obj);
		}

		public override int GetHashCode()
		{
			return (Adress != null ? Adress.GetHashCode() : 0);
		}

		public static bool operator ==(PickingSlot left, PickingSlot right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(PickingSlot left, PickingSlot right)
		{
			return !Equals(left, right);
		}
	}
}
