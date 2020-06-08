using System.Collections;
using System.Collections.Generic;
using Warehouse.Domain.Entities;

namespace Warehouse.Domain.Parameters
{
	public interface IPathFindingResult
	{
		ITravelStep[] Steps { get; }
		IList<Coord> PathCoordinates { get; set; }
		bool Success { get; set; }
	}
    public class PathFindingResult<T> : IPathFindingResult
    {
        public ITravelStep[] Steps { get; set; }
        public IList<Coord> PathCoordinates { get; set; } = new List<Coord>();
        public List<Coord> CheckedCoordinates { get; set; } = new List<Coord>();
        public bool Success { get; set; }
    }
}
