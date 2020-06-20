using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Warehouse.Domain.Parameters;

namespace Warehouse.Domain.Entities
{
	public class RouteBetweenCoords : IEquatable<RouteBetweenCoords>
	{
		private readonly Coord _coord1;
		private readonly Coord _coord2;

		public RouteBetweenCoords(Coord coord1, Coord coord2)
		{
			_coord1 = coord1;
			_coord2 = coord2;
		}

		public int TravelCost;
		public Coord[] Route;

        public bool IsBetween(Coord coord1, Coord coord2)
        {
            if (coord1 == _coord1 && coord2 == _coord2)
            {
                return true;
            }
			if (coord1 == _coord2 && coord2 == _coord1)
            {
                return true;
            }

            return false;
        }

		public void ReadTravelSteps(IPathFindingResult pathFindingResult)
		{
            Route = pathFindingResult.Steps.Select(x=>x.Position).ToArray();
			TravelCost = pathFindingResult.Route.CostFromStart;
		}

		public bool Equals(RouteBetweenCoords other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return (Equals(_coord1, other._coord1) && Equals(_coord2, other._coord2))
				   ||
				   (Equals(_coord1, other._coord2) && Equals(_coord2, other._coord1));
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

			return Equals((RouteBetweenCoords) obj);
		}

		public override int GetHashCode()
		{
			return (_coord1.Y+_coord2.Y)+(_coord1.X+_coord2.X);
		}

		public static bool operator ==(RouteBetweenCoords left, RouteBetweenCoords right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(RouteBetweenCoords left, RouteBetweenCoords right)
		{
			return !Equals(left, right);
		}
	}
}