using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Domain.Entities
{
    public struct Area: IEquatable<Area>
    {
		public Area(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public int Width { get; }
        public int Height { get; }

		public bool Equals(Area other)
		{
			return Width == other.Width && Height == other.Height;
		}

		public static bool operator ==(Area a, Area b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Area a, Area b)
		{
			return !a.Equals(b);
		}

        public override bool Equals(object obj)
		{
			return obj is Area other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Width*Height;
		}
	}
}
