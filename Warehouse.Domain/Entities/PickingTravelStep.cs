using System;
using System.Collections.Generic;

namespace Warehouse.Domain.Entities
{
	public class PickingTravelStep : ITravelStep, IEquatable<PickingTravelStep>
	{
		public int X,
				   Y;

		public Coord Coords { get; }
		public int TraverseCost { get; }
		public long Sku { get; private set; }
		public int AvailableUnits { get; private set; }
		public int CostFromStart { get; set; }
		public float CostEstimation { get; set; }
		public Dictionary<long, int> PendingSkus { get; private set; }

		public ITravelStep Parent { get; set; }

		public List<PickingSlot> VisitedSlots { get; set; } = new List<PickingSlot>();

		public PickingTravelStep(Coord coords, long sku, int availableUnits, Dictionary<long, int> pendingSkus)
		{
			X = coords.X;
			Y = coords.Y;
			Coords = coords;
			Sku = sku;
			AvailableUnits = availableUnits;
			PendingSkus = pendingSkus;
		}

		public PickingTravelStep(int x, int y, int availableUnits, Dictionary<long, int> pendingSkus)
		{
			X = x;
			Y = y;
			AvailableUnits = availableUnits;
			PendingSkus = pendingSkus;
			Coords = new Coord(x, y);
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