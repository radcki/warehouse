using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Warehouse.Domain.Entities;
using Warehouse.Domain.HeapList;
using Warehouse.Domain.Parameters;

namespace Warehouse.App
{
	public class PickingScanSolver
	{
		#region Privates

		private readonly WarehouseLayout _warehouseLayout;
		private readonly PathSolver _pathSolver;

		#endregion

		#region Constructors

		public PickingScanSolver(WarehouseLayout warehouseLayout)
		{
			_warehouseLayout = warehouseLayout;
			_pathSolver = new PathSolver(warehouseLayout);
		}

		#endregion

		#region Methods

		public PathFindingResult<PickingTravelStep> FindPath(PickingOrder order)
		{
            var executionStopwatch = Stopwatch.StartNew();
			var result = new PathFindingResult<PickingTravelStep>
                         {
				Success = true,
			};
			var currentPosition = new PickingTravelStep(_warehouseLayout.GetPickingStartPosition(), new Dictionary<long, int>(order.RequiredArticles));

			var possiblePickingSlots = order.RequiredArticles
											.SelectMany(x => _warehouseLayout.GetPickingSlotsWithSku(x.Key))
											.OrderBy(x=>x.AlleyNumber).ThenBy(x=>x.PositionNumber)
											.ToList();

			var precalculatedRoutes = GetRoutesBetweenSlots(possiblePickingSlots);
			var remainingInOrder = new Dictionary<long, int>(order.RequiredArticles);
			foreach (var possiblePickingSlot in possiblePickingSlots)
			{
				var previousPosition = currentPosition;
				if (!remainingInOrder.TryGetValue(possiblePickingSlot.Sku, out int remainingUnits) || remainingInOrder[possiblePickingSlot.Sku] < 1)
				{
					continue;
				}

				if (remainingUnits < 1)
				{
					continue;
				}

				var unitsToTake = Math.Min(remainingUnits, possiblePickingSlot.Units);

				remainingInOrder[possiblePickingSlot.Sku] -= unitsToTake;
				currentPosition = new PickingTravelStep(possiblePickingSlot, unitsToTake, new Dictionary<long, int>(remainingInOrder));
				currentPosition.Parent = previousPosition;
                currentPosition.UnitsToTake = unitsToTake;
				currentPosition.CostFromStart = previousPosition.CostFromStart + FindTravelCostBetween(precalculatedRoutes, previousPosition.Position, currentPosition.Position);

				if (!remainingInOrder.Any(x => x.Value > 0))
				{
					break;
				}
			}

			var endPosition = _warehouseLayout.GetPickingEndPosition();
			var finalStep = new PickingTravelStep(endPosition, currentPosition.PendingSkus);
			finalStep.CostFromStart = currentPosition.CostFromStart + FindTravelCostBetween(precalculatedRoutes,finalStep.Position, currentPosition.Position);
			finalStep.Parent = currentPosition;
			currentPosition = finalStep;
            var pickedArticles = order.RequiredArticles.Select(x => x.Key).ToDictionary(x => x, x => 0);

			var steps = new List<ITravelStep>();
            result.Route = finalStep;
			while (currentPosition != null)
			{
                if (pickedArticles.ContainsKey(currentPosition.Sku))
                {
                    pickedArticles[currentPosition.Sku] += currentPosition.UnitsToTake;
                }

                if (currentPosition.PickingSlot != null)
                {
                    _warehouseLayout.ReserveArticles(currentPosition.PickingSlot.Address, currentPosition.Sku, currentPosition.UnitsToTake);
                }

				var nextPosition = currentPosition.Parent;
				currentPosition.Parent = null;
				steps.Add(currentPosition);
				if (nextPosition != null && currentPosition!=(PickingTravelStep)nextPosition)
				{
					var route = FindTravelRouteBetween(precalculatedRoutes, currentPosition.Position, nextPosition.Position).Route;
					result.Paths.Add(route);
				}
				currentPosition = nextPosition as PickingTravelStep;
			}

            result.ExecutionTime = executionStopwatch.Elapsed;
			return result;
		}

		public int FindTravelCostBetween(HashSet<RouteBetweenCoords> pickingSlotRoutes, Coord coord1, Coord coord2)
		{
			if (coord1 == coord2)
			{
				return 0;
			}
			if (pickingSlotRoutes.TryGetValue(new RouteBetweenCoords(coord1, coord2), out var route))
			{
				return route.TravelCost;
			}
			else
			{
				throw new Exception("Travel cost unknown");
			}
		}

		private RouteBetweenCoords FindTravelRouteBetween(HashSet<RouteBetweenCoords> pickingSlotRoutes, Coord coord1, Coord coord2)
		{
			if (coord1 == coord2)
			{
				return new RouteBetweenCoords(coord1, coord2);
			}
			if (pickingSlotRoutes.TryGetValue(new RouteBetweenCoords(coord1, coord2), out var route))
			{
				return route;
			}
			else
			{
				throw new Exception("Travel route unknown");
			}
		}

        private HashSet<RouteBetweenCoords> GetRoutesBetweenSlots(List<PickingSlot> pickingSlots)
		{
			var coordsSet = new HashSet<Coord>(pickingSlots.Select(x => x.Position))
			{
				_warehouseLayout.GetPickingEndPosition(),
				_warehouseLayout.GetPickingStartPosition(),
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

				for (var endIndex = 0; endIndex < pickingSlots.Count; endIndex++)
				{
					var coord2 = coords[endIndex];
					var result = _pathSolver.FindPath(new TravelStep(coord1), new TravelStep(coord2), false);
					if (result.Success)
					{
						//Debug.WriteLine($"Sprawdzono {coord1} - {coord2}");
						var route = new RouteBetweenCoords(coord1, coord2);
						route.ReadTravelSteps(result);
						collection.AddOrUpdate(route, x => 0, (x, b) => 0);
					}
				}

				done += 1;
				//PickingRoutesCalculationProgress?.Invoke(null, new CalculatePickingRouteProgressEventArgs()
				//{
				//	Done = done,
				//	IterationIndex = startIndex,
				//	Todo = todo,
				//	Elapsed = sw.Elapsed,
				//	CheckeCoord = coord1
				//});
			});
			var pickingSlotRoutes = new HashSet<RouteBetweenCoords>(collection.Keys);
			return pickingSlotRoutes;
		}

		#endregion
	}
}