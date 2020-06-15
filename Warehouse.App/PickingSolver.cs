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
    public class PickingSolver
    {
        #region Privates

        private readonly WarehouseLayout _warehouseLayout;
        private readonly PathSolver _pathSolver;

        #endregion

        #region Constructors

        public PickingSolver(WarehouseLayout warehouseLayout)
        {
            _warehouseLayout = warehouseLayout;
            _pathSolver = new PathSolver(warehouseLayout);
        }

        #endregion

        #region Methods

        public PathFindingResult<PickingTravelStep> FindPath(PickingOrder order)
        {
            var result = new PathFindingResult<PickingTravelStep>
                         {
                             Success = true,
                         };

            var openList = new Heap<PickingTravelStep>();

            var currentPosition = new PickingTravelStep(_warehouseLayout.GetPickingStartPosition(), -1, 0, new Dictionary<long, int>(order.RequiredArticles));

            var possiblePickingSlots = order.RequiredArticles
                                            .SelectMany(x => _warehouseLayout.GetPickingSlotsWithSku(x.Key))
                                            .ToList();

            var precalculatedRoutes = GetRoutesBetweenSlots(possiblePickingSlots);
            var endPosition = _warehouseLayout.GetPickingEndPosition();

            while (currentPosition.PendingSkus.Count > 0)
            {
                if (currentPosition.Sku > 0 && currentPosition.AvailableUnits > 0 && currentPosition.PendingSkus.ContainsKey(currentPosition.Sku))
                {
                    var requiredUnits = currentPosition.PendingSkus[currentPosition.Sku];
                    var unitsToTake = Math.Min(requiredUnits, currentPosition.AvailableUnits);
                    currentPosition.PendingSkus[currentPosition.Sku] -= unitsToTake;
                }

                var yetRequired = currentPosition.PendingSkus.Where(x => x.Value > 0).ToDictionary(x => x.Key, x => x.Value);

                if (yetRequired.Count == 0)
                {
                    result.Success = true;
                    break;
                }

                var possibleNextSlots = possiblePickingSlots.Where(x => yetRequired.ContainsKey(x.Sku) && !currentPosition.VisitedSlots.Contains(x))
                                                            .ToList();
                if (!possibleNextSlots.Any())
                {
                    result.Success = false;
                    break;
                }


                foreach (var nextSlot in possibleNextSlots)
                {
                    var remainingUnits = yetRequired[nextSlot.Sku];
                    var unitsToTake = Math.Min(remainingUnits, nextSlot.Units);
                    int tentativeCost = 0;

                    var remainingSlots = possibleNextSlots.Where(x => x != nextSlot);
                    if (unitsToTake == remainingUnits)
                    {
                        remainingSlots = remainingSlots.Where(x => x.Sku != nextSlot.Sku);
                    }

                    var remainingSlotsList = remainingSlots.ToList();

                    var next = new PickingTravelStep(nextSlot.Position, nextSlot.Sku, nextSlot.Units, yetRequired)
                               {
                                   Parent = currentPosition,
                                   UnitsToTake = unitsToTake
                               };
                    next.CostFromStart = currentPosition.CostFromStart + FindTravelCostBetween(precalculatedRoutes, next.Position, currentPosition.Position);
                    next.VisitedSlots = new List<PickingSlot>(currentPosition.VisitedSlots) {nextSlot};

                    tentativeCost = FindTravelCostBetween(precalculatedRoutes, nextSlot.Position, endPosition);
                    tentativeCost += currentPosition.CostFromStart;
                    tentativeCost += remainingSlotsList.Sum(x => FindTravelCostBetween(precalculatedRoutes, nextSlot.Position, x.Position));


                    openList.Add(new HeapNode<PickingTravelStep>(next, tentativeCost));
                }

                currentPosition = openList.TakeHeapHeadPosition();
            }

            var finalStep = new PickingTravelStep(endPosition, -1, 0, currentPosition.PendingSkus);
            finalStep.CostFromStart = currentPosition.CostFromStart + FindTravelCostBetween(precalculatedRoutes, finalStep.Position, currentPosition.Position);
            finalStep.Parent = currentPosition;
            currentPosition = finalStep;

            // powrót po śladach
            var pickedArticles = order.RequiredArticles.Select(x=>x.Key).ToDictionary(x=>x,x=>0);
            var steps = new List<ITravelStep>();
            while (currentPosition != null)
            {
                if (pickedArticles.ContainsKey(currentPosition.Sku))
                {
                    pickedArticles[currentPosition.Sku] += currentPosition.UnitsToTake;
                }

                var nextPosition = currentPosition.Parent;
                currentPosition.Parent = null;
                steps.Add(currentPosition);
                if (nextPosition != null && currentPosition != (PickingTravelStep) nextPosition)
                {
                    var route = FindTravelRouteBetween(precalculatedRoutes, currentPosition.Position, nextPosition.Position).Route;
                    foreach (var coord in route)
                    {
                        result.PathCoordinates.Add(coord);
                    }
                }

                currentPosition = nextPosition as PickingTravelStep;
            }

            result.Steps = steps.ToArray();
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

            var coordCombinations = coordsSet.GetKCombs(2).Select(x => x.ToArray());
            var collection = new ConcurrentDictionary<RouteBetweenCoords, byte>();
            //int done = 1;
            //int todo = coords.Length;
            //var sw = new Stopwatch();
            //sw.Start();

            Parallel.ForEach(coordCombinations, combination =>
                                                {
                                                    var result = _pathSolver.FindPath(new TravelStep(combination[0]), new TravelStep(combination[1]), false);
                                                    if (result.Success)
                                                    {
                                                        //Debug.WriteLine($"Sprawdzono {coord1} - {coord2}");
                                                        var route = new RouteBetweenCoords(combination[0], combination[1]);
                                                        route.ReadTravelsteps(result.Steps);
                                                        collection.AddOrUpdate(route, x => 0, (x, b) => 0);
                                                    }
                                                });

            //	done += 1;
            //	//PickingRoutesCalculationProgress?.Invoke(null, new CalculatePickingRouteProgressEventArgs()
            //	//{
            //	//	Done = done,
            //	//	IterationIndex = startIndex,
            //	//	Todo = todo,
            //	//	Elapsed = sw.Elapsed,
            //	//	CheckeCoord = coord1
            //	//});
            //});
            var pickingSlotRoutes = new HashSet<RouteBetweenCoords>(collection.Keys);
            return pickingSlotRoutes;
        }

        #endregion
    }
}