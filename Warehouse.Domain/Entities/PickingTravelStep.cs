using System;
using System.Collections.Generic;

namespace Warehouse.Domain.Entities
{
	public class PickingTravelStep : ITravelStep, IEquatable<PickingTravelStep>
	{
		public int X,
				   Y;

		public Coord Position { get; }
		public Area Area { get; }
		public int TraverseCost { get; }
        public int UnitsToTake { get; set; }
        public long Sku { get; private set; }
		public int CostFromStart { get; set; }
		public float CostEstimation { get; set; }
		public Dictionary<long, int> PendingSkus { get; }

		public ITravelStep Parent { get; set; }

		public PickingSlot PickingSlot { get; }

		public List<PickingSlot> VisitedSlots { get; set; } = new List<PickingSlot>();

        public PickingTravelStep(Coord position, Dictionary<long, int> pendingSkus)
		{
			X = position.X;
            Y = position.Y;
            Position = position;
			PendingSkus = pendingSkus;
		}
		public PickingTravelStep(PickingSlot pickingSlot, int unitsToTake, Dictionary<long, int> pendingSkus)
        {
			X = pickingSlot.Position.X;
			Y = pickingSlot.Position.Y;
			Position = pickingSlot.Position;
			Sku = pickingSlot.Sku;
            UnitsToTake = unitsToTake;
			PendingSkus = pendingSkus;
            PickingSlot = pickingSlot;
        }

	
		public bool Equals(PickingTravelStep other)
		{
			return other != null && (X == other.X && Y == other.Y);
		}

		public bool Equals(int x, int y)
		{
			return X == x && Y == y;
		}

		public static bool operator ==(PickingTravelStep a, PickingTravelStep b)
		{
			if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
			{
				return true;
			}

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
			{
				return false;
			}

			return a.Equals(b);
		}

		public static bool operator !=(PickingTravelStep a, PickingTravelStep b)
		{
			return !(a == b);
		}
	}
}