using System;
using System.Collections.Generic;
using System.Linq;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Domain.Entities
{
    public class PickingSlot : IEquatable<PickingSlot>, ILayoutElement
	{
		public Coord Position { get; private set; }

        public Area Area => new Area(1,1);
        public string Address => $"F-{AlleyNumber:D3}-{PositionNumber:D3}";
		public int AlleyNumber { get; set; }
		public int PositionNumber { get; set; }
		public  long Sku { get; set; }
		public int Units { get; set; }
		public int ReservedUnits { get; set; }
        public int AvailableUnits => Units - ReservedUnits;

		public PickingSlot(Coord position, int alleyNumber, int positionNumber)
		{
			Position = position;
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

			return Address == other.Address;
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
			return (Address != null ? Address.GetHashCode() : 0);
		}

		public static bool operator ==(PickingSlot left, PickingSlot right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(PickingSlot left, PickingSlot right)
		{
			return !Equals(left, right);
		}

        #region Overrides of Object

        /// <inheritdoc />
        public override string ToString()
        {
            return Address;
        }

        #endregion
    }
}
