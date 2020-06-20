using System;
using System.Collections;
using System.Collections.Generic;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Domain.Parameters
{
	public interface IPathFindingResult: ILayoutElement
	{
        ITravelStep Route { get; set; }
        IEnumerable<ITravelStep> Steps { get; }
        IList<IList<Coord>> Paths { get; set; }
        bool Success { get; set; }
	}
    public class PathFindingResult<T> : IPathFindingResult
    {
        public ITravelStep Route { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        public IEnumerable<ITravelStep> Steps
        {
            get
            {
                var current = Route;
                while (current?.Parent != null)
                {
                    yield return current;
                    current = current.Parent;
                }
            }
        }

        public IList<IList<Coord>> Paths { get; set; } = new List<IList<Coord>>();

        public bool Success { get; set; }
    }
}
