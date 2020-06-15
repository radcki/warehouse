using System;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Domain.Entities
{
	public interface ITravelStep: ILayoutElement
	{
		Coord Position { get; }
		int TraverseCost { get; }
		int CostFromStart { get; }
		float CostEstimation { get; }
		ITravelStep Parent { get; }
		//float EuclidianDistanceTo(TravelStep other);

	}
	public class TravelStep : ITravelStep, IEquatable<TravelStep>
	{
		public int X,
				   Y;

		public Coord Position { get; }
		public Area Area => new Area(1,1);
		public int TraverseCost { get; set; } = 1;
		public int CostFromStart { get; set; }
		public float CostEstimation { get; set; }

		public ITravelStep Parent { get; set; }

		public TravelStep(Coord position)
		{
			X = position.X;
			Y = position.Y;
			Position = position;
		}
		public TravelStep(int x, int y)
		{
			X = x;
			Y = y;
			Position = new Coord(x, y);
		}

		public float ManhattanDistanceTo(TravelStep other)
		{
			return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
		}

		public float EuclidianDistanceTo(TravelStep other)
		{
			return (float) (2 * (Math.Sqrt(Math.Pow(Math.Abs(X - other.X), 2) + Math.Pow(Math.Abs(Y - other.Y), 2))));
		}

		public bool Equals(TravelStep other)
		{
			return other != null && (X == other.X && Y == other.Y);
		}

		public bool Equals(int x, int y)
		{
			return X == x && Y == y;
		}

		public static bool operator ==(TravelStep a, TravelStep b)
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

        public static bool operator !=(TravelStep a, TravelStep b)
		{
			return !(a == b);
		}

        public override int GetHashCode()
        {
            return (X + Y).GetHashCode();
        }
	}
}