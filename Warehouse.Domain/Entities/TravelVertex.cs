using System;
using System.Collections.Generic;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Domain.Entities
{
	public class TravelVertex : ILayoutElement, IEquatable<TravelVertex>
	{
		public int X,
				   Y;

		public Coord Position { get; }
		public Area Area => new Area(1,1);

		public List<TravelVertex> Neighbours { get; set; }

		public TravelVertex(Coord position)
		{
			X = position.X;
			Y = position.Y;
			Position = position;
			Neighbours = new List<TravelVertex>();
		}
		public TravelVertex(int x, int y)
		{
			X = x;
			Y = y;
			Position = new Coord(x, y);
		}

		public float ManhattanDistanceTo(TravelVertex other)
		{
			return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
		}

		public float EuclidianDistanceTo(TravelVertex other)
		{
			return (float) (2 * (Math.Sqrt(Math.Pow(Math.Abs(X - other.X), 2) + Math.Pow(Math.Abs(Y - other.Y), 2))));
		}

		public bool Equals(TravelVertex other)
		{
			return other != null && (X == other.X && Y == other.Y);
		}

		public bool Equals(int x, int y)
		{
			return X == x && Y == y;
		}

		public static bool operator ==(TravelVertex a, TravelVertex b)
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

        public static bool operator !=(TravelVertex a, TravelVertex b)
		{
			return !(a == b);
		}

        public override int GetHashCode()
        {
            return (X + Y).GetHashCode();
        }
	}
}