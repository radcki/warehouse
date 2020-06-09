using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Warehouse.Domain.Entities;

namespace Warehouse.App
{
	public class CalculatePickingRouteProgressEventArgs : EventArgs
	{
		public int Todo { get; set; }
		public int Done { get; set; }
		public TimeSpan Elapsed { get; set; }
		public int IterationIndex { get; set; }
		public Coord CheckeCoord { get; set; }
	}

    public class WarehouseLayout
	{
		public WarehouseLayout(int width, int height, Area minimalPassableArea)
		{
			Width = width;
			Height = height;
			_minimalPassableArea = minimalPassableArea;
		}

		public int Width { get; private set; }
		public int Height { get; private set; }
		private List<Obstacle> Obstacles { get; set; } = new List<Obstacle>();
		public List<PickingSlot> PickingSlots { get; set; } = new List<PickingSlot>();

		private readonly Area _minimalPassableArea;
		private byte[,] _pathfindingMap;
		private HashSet<RouteBetweenCoords> PickingSlotRoutes { get; set; } = new HashSet<RouteBetweenCoords>();
		public event EventHandler<CalculatePickingRouteProgressEventArgs> PickingRoutesCalculationProgress; 

		public List<Obstacle> GetObstacles()
		{
			return new List<Obstacle>(Obstacles);
		}
		public List<PickingSlot> GetPickingSlots()
		{
			return new List<PickingSlot>(PickingSlots);
		}
        public void AddObstacle(Obstacle obstacle)
		{
			if (Obstacles.Any(x => x.OverlapsWith(obstacle)))
			{
				throw new Exception("Przeszkoda nakłada się na już istniejącą przeszkodę.");
			}

			Obstacles.Add(obstacle);
			GeneratePathfindingMap();
		}

		public void AddObstacles(IEnumerable<Obstacle> obstacle)
		{
			if (Obstacles.Any(x => obstacle.Any(x.OverlapsWith)))
			{
				throw new Exception("Przeszkoda nakłada się na już istniejącą przeszkodę.");
			}

			Obstacles.AddRange(obstacle);
			GeneratePathfindingMap();
		}

        public void AddPickingSlot(PickingSlot slot)
		{
			PickingSlots.Add(slot);
		}

		public void AddPickingSlots(IEnumerable<PickingSlot> slots)
		{
			PickingSlots.AddRange(slots);
		}

        public void Clear()
		{
			Obstacles = new List<Obstacle>();
			GeneratePathfindingMap();
		}

		private void GeneratePathfindingMap()
		{
			var area = new byte[Width + 1, Height + 1];
			foreach (var obstacle in Obstacles)
			{
				foreach (var coord in obstacle.UsedCoords)
				{
					area[coord.X, coord.Y] = 1;
				}
			}

			_pathfindingMap = area;
		}

		public byte[,] GetPathfindingMap()
		{
			var area = new byte[Width + 1, Height + 1];
			foreach (var obstacle in Obstacles)
			{
				foreach (var coord in obstacle.UsedCoords)
				{
					area[coord.X, coord.Y] = 1;
				}
			}
			return area;
        }

        public bool IsAvailable(Coord coord)
		{
			return coord.X >= 0
				   && coord.Y >= 0
				   && coord.X <= Width
				   && coord.Y <= Height
				   && _pathfindingMap[coord.X, coord.Y] == 0;
		}

		public bool IsWalkable(Coord coord)
		{
			if (!IsAvailable(coord))
			{
				return false;
			}

			if (_minimalPassableArea == new Area(1, 1))
			{
				return true;
			}

			var leftAvailableCount = 0;
			var rightAvailableCount = 0;
			for (var i = 1; i <= _minimalPassableArea.Width; i++)
			{
				if (!IsAvailable(new Coord(coord.X - i, coord.Y)))
				{
					break;
				}

				leftAvailableCount++;
			}

			if (leftAvailableCount + 1 < _minimalPassableArea.Width)
			{
				for (var i = 1; i <= _minimalPassableArea.Width; i++)
				{
					if (!IsAvailable(new Coord(coord.X + i, coord.Y)))
					{
						break;
					}

					rightAvailableCount++;
				}
			}

			if (rightAvailableCount + leftAvailableCount + 1 < _minimalPassableArea.Width)
			{
				return false;
			}

			var topAvailableCount = 0;
			var bottomAvailableCount = 0;
			for (var i = 1; i <= _minimalPassableArea.Height; i++)
			{
				if (!IsAvailable(new Coord(coord.X, coord.Y + 1)))
				{
					break;
				}

				topAvailableCount++;
			}

			if (topAvailableCount + 1 < _minimalPassableArea.Height)
			{
				for (var i = 1; i <= _minimalPassableArea.Height; i++)
				{
					if (!IsAvailable(new Coord(coord.X, coord.Y - 1)))
					{
						break;
					}

					bottomAvailableCount++;
				}
			}

			if (topAvailableCount + bottomAvailableCount + 1 < _minimalPassableArea.Height)
			{
				return false;
			}

			return true;
		}


		public void CalculatePickingRoutes(PathSolver solver)
		{
			
            var coordsSet = new HashSet<Coord>(PickingSlots.Select(x => x.Position))
            {
                GetPickingStartPosition(),
                GetPickingEndPosition()
            };
            var coords = coordsSet.ToArray();

			var collection = new ConcurrentDictionary<RouteBetweenCoords, byte>();
			int done = 1;
			int todo = coords.Length;
			var sw = new Stopwatch();
			sw.Start();
			Parallel.For(0, coords.Length, startIndex =>
			{
				var coord1 = coords[startIndex];

				for (var endIndex = startIndex + 1; endIndex < PickingSlots.Count; endIndex++)
				{
					var coord2 = coords[endIndex];
					var result = solver.FindPath(new TravelStep(coord1), new TravelStep(coord2), false);
					if (result.Success)
					{
						//Debug.WriteLine($"Sprawdzono {coord1} - {coord2}");
						var route = new RouteBetweenCoords(coord1, coord2);
						route.ReadTravelsteps(result.Steps);
						collection.AddOrUpdate(route, x => 0,(x, b) => 0);
					}
				}

				done += 1;
				PickingRoutesCalculationProgress?.Invoke(null, new CalculatePickingRouteProgressEventArgs()
				{
					Done = done,
					IterationIndex = startIndex,
					Todo = todo,
					Elapsed = sw.Elapsed,
					CheckeCoord = coord1
				});
            });
			PickingSlotRoutes = new HashSet<RouteBetweenCoords>(collection.Keys);
		}

		public int TravelCostBetween(Coord coord1, Coord coord2)
		{
			if (coord1 == coord2)
			{
				return 0;
			}
			if (PickingSlotRoutes.TryGetValue(new RouteBetweenCoords(coord1, coord2), out var route))
			{
				return route.TravelCost;
			}
			else
			{
				throw new Exception("Travel cost unknown");
			}
		}

		//public RouteBetweenCoords TravelRouteBetween(Coord coord1, Coord coord2)
		//{
		//	if (coord1 == coord2)
		//	{
		//		return new RouteBetweenCoords(coord1, coord2);
		//	}
		//	if (PickingSlotRoutes.TryGetValue(new RouteBetweenCoords(coord1, coord2), out var route))
		//	{
		//		return route;
		//	}
		//	else
		//	{
		//		throw new Exception("Travel route unknown");
		//	}
		//}

        public List<PickingSlot> GetPickingSlotsWithSku(long sku)
		{
			return PickingSlots.Where(x => x.Sku == sku && x.Units-x.ReservedUnits > 0).ToList();
		}

		public Coord GetPickingStartPosition()
		{
			return new Coord(Width / 2, 1);
		}

		public Coord GetPickingEndPosition()
		{
			return new Coord(Width / 2, 1);
		}
    }
}