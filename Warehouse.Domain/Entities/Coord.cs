using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Domain.Entities
{
    public struct Coord : IEquatable<Coord>
	{
		public readonly int X;
		public readonly int Y; 

		public Coord(int x, int y)
		{
            X = x;
            Y = y;
		}

		public override string ToString()
		{
			return $"({X},{Y})";
		}


		public bool Equals(Coord other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Coord other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (X*Y).GetHashCode();
		}

		public static bool operator == (Coord a, Coord b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Coord a, Coord b)
		{
			return !a.Equals(b);
		}

		public static Coord operator + (Coord a, Coord b)
		{
			return new Coord(a.X+b.X,a.Y+b.Y);
		}
	}
}
