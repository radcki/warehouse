using System;
using System.Collections.Generic;
using System.Linq;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Domain.Entities
{
    public class Obstacle: ILayoutElement 
	{
		public Coord Position { get; private set; }
		public Area Area { get; private set; }


		public Obstacle(Coord positon, Area area)
		{
			if (area.Height < 1 || area.Width < 1)
			{
				throw new Exception("Przeszkoda musi mieć rozmiar co najmniej 1 w obu płaszczyznach");
			}

			Position = positon;
			Area = area;
		}

        public Coord[] Corners =>
            new[]
            {
                Position,
                new Coord(Position.X + Area.Width, Position.Y),
                new Coord(Position.X + Area.Width, Position.Y + Area.Height),
                new Coord(Position.X, Position.Y + Area.Height),
            };

        public Coord[] UsedCoords
		{
			get
			{
				var usedCoords = new HashSet<Coord>();
				for (var x = 0; x <= Area.Width; x++)
				{
					for (var y = 0; y <= Area.Height; y++)
					{
						usedCoords.Add(new Coord(Position.X+x, Position.Y+y));
					}
                }
				return usedCoords.ToArray();
			}
		}

		public bool OverlapsWith(Obstacle other)
		{
			var usedCoords = UsedCoords;
			return other.UsedCoords.Any(otherUsedCoord => usedCoords.Any(x => x == otherUsedCoord));
		}
	}
}
