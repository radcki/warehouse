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
    public class PickingCoordSolver
    {
        private readonly WarehouseLayout _warehouseLayout;
        private readonly PathSolver _pathSolver;


        public PickingCoordSolver(WarehouseLayout warehouseLayout)
        {
            _warehouseLayout = warehouseLayout;
            _pathSolver = new PathSolver(warehouseLayout);
        }


        public PathFindingResult<PickingTravelStep> FindPath(PickingOrder order)
        {
            var executionStopwatch = Stopwatch.StartNew();
            var result = new PathFindingResult<PickingTravelStep>
                         {
                             Success = true,
                         };

            var possiblePickingSlots = order.RequiredArticles
                                            .SelectMany(x => _warehouseLayout.GetPickingSlotsWithSku(x.Key))
                                            .ToList();
            var bestResultCost = int.MaxValue;
            var pickingTravelSteps = new List<PickingTravelStep>();
            var routesBetweenSlots = GetRoutesBetweenSlots(possiblePickingSlots);
            var requiredUnits = new Dictionary<long, int>(order.RequiredArticles);
            var startPosition = _warehouseLayout.GetPickingStartPosition();
            var endPosition = _warehouseLayout.GetPickingEndPosition();

            foreach (var originSlot in possiblePickingSlots)
            {
                var yetRequiredUnits = new Dictionary<long, int>(requiredUnits);
                var visitedSlots = new List<PickingTravelStep>();

                var origin = originSlot.Position;
                routesBetweenSlots.TryGetValue(new RouteBetweenCoords(origin, startPosition), out var fromStart);
                routesBetweenSlots.TryGetValue(new RouteBetweenCoords(origin, endPosition), out var toEnd);
                var cost = fromStart.TravelCost + toEnd.TravelCost;
                foreach (var pickingSlot in possiblePickingSlots.Where(x => x != originSlot)
                                                                .OrderBy(x =>
                                                                         {
                                                                             routesBetweenSlots.TryGetValue(new RouteBetweenCoords(origin, x.Position), out var distance);
                                                                             return distance.TravelCost;
                                                                         }))
                {
                    var unitsToTake = Math.Min(pickingSlot.Units, yetRequiredUnits[pickingSlot.Sku]);
                    if (unitsToTake == 0)
                    {
                        continue;
                    }

                    yetRequiredUnits[pickingSlot.Sku] -= unitsToTake;
                    visitedSlots.Add(new PickingTravelStep(pickingSlot, unitsToTake, yetRequiredUnits));
                    cost += pickingSlot.Position.ManhattanDistanceTo(origin);
                    if (cost > bestResultCost //fast exit - worse result
                        || yetRequiredUnits.All(x => x.Value == 0)) //lookup finished
                    {
                        break;
                    }
                }

                if (cost < bestResultCost)
                {
                    bestResultCost = cost;
                    pickingTravelSteps = visitedSlots;
                }
            }

            var pickedArticles = order.RequiredArticles.Select(x => x.Key).ToDictionary(x => x, x => 0);
            var tspSolver = new TspSolver();
            var slotVisitOrder = tspSolver.OrderSlots(pickingTravelSteps, routesBetweenSlots);

            for (var i = 0; i < slotVisitOrder.Count; i++)
            {
                var current = slotVisitOrder[i];
                var previous = slotVisitOrder.ElementAtOrDefault(i - 1);
                if (previous != null)
                {
                    current.Parent = previous;
                }

                PickingTravelStep next;
                if ((i + 2) == slotVisitOrder.Count)
                {
                    next = slotVisitOrder[i + 1];
                }
                else
                {
                    next = new PickingTravelStep(_warehouseLayout.GetPickingEndPosition(), current.PendingSkus);
                }

                var route = routesBetweenSlots.FirstOrDefault(x => x.IsBetween(current.Position, next.Position));
                current.CostFromStart = (previous?.CostFromStart ?? 0) + route.TravelCost;
                result.Paths.Add(route.Route);
                result.Route = current;
            }

            result.ExecutionTime = executionStopwatch.Elapsed;
            return result;
        }

        private HashSet<RouteBetweenCoords> GetRoutesBetweenSlots(List<PickingSlot> pickingSlots)
        {
            var coordsSet = new HashSet<Coord>(pickingSlots.Select(x => x.Position))
                            {
                                _warehouseLayout.GetPickingEndPosition(),
                                _warehouseLayout.GetPickingStartPosition(),
                            };

            var coordCombinations = coordsSet.GetKCombinations(2).Select(x => x.ToArray());
            var collection = new ConcurrentDictionary<RouteBetweenCoords, byte>();

            Parallel.ForEach(coordCombinations, combination =>
                                                {
                                                    var result = _pathSolver.FindPath(new TravelStep(combination[0]), new TravelStep(combination[1]), false);
                                                    if (result.Success)
                                                    {
                                                        var route = new RouteBetweenCoords(combination[0], combination[1]);
                                                        route.ReadTravelSteps(result);
                                                        collection.AddOrUpdate(route, x => 0, (x, b) => 0);
                                                    }
                                                });

            var pickingSlotRoutes = new HashSet<RouteBetweenCoords>(collection.Keys);
            return pickingSlotRoutes;
        }
    }
}