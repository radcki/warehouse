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
            var result = new PathFindingResult<PickingTravelStep>
                         {
                             Success = true,
                         };

            var possiblePickingSlots = order.RequiredArticles
                                            .SelectMany(x => _warehouseLayout.GetPickingSlotsWithSku(x.Key))
                                            .ToList();
            var bestResultCost = int.MaxValue;
            var pickingTravelSteps = new List<PickingTravelStep>();

            foreach (var originSlot in possiblePickingSlots)
            {
                var requiredUnits = new Dictionary<long, int>(order.RequiredArticles);
                var visitedSlots = new List<PickingTravelStep>();

                var origin = originSlot.Position;
                var cost = _warehouseLayout.GetPickingStartPosition().ManhattanDistanceTo(origin) + _warehouseLayout.GetPickingEndPosition().ManhattanDistanceTo(origin);
                foreach (var pickingSlot in possiblePickingSlots.OrderBy(x => x.Position.ManhattanDistanceTo(origin)))
                {
                    var unitsToTake = Math.Min(pickingSlot.Units, requiredUnits[pickingSlot.Sku]);
                    if (unitsToTake == 0)
                    {
                        continue;
                    }

                    requiredUnits[pickingSlot.Sku] -= unitsToTake;
                    visitedSlots.Add(new PickingTravelStep(pickingSlot, unitsToTake, requiredUnits));
                    cost += pickingSlot.Position.ManhattanDistanceTo(origin);
                    if (cost > bestResultCost //fast exit - worse result
                        || requiredUnits.All(x => x.Value == 0)) //lookup finished
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

            var positions = pickingTravelSteps.Select(x => x.PickingSlot).ToList();
            var routesBetweenSlots = GetRoutesBetweenSlots(positions);
            var tspSolver = new TspSolver();
            var slotVisitOrder = tspSolver.OrderSlots(pickingTravelSteps, routesBetweenSlots);
            result.Steps = slotVisitOrder.ToArray();
            result.PathCoordinates = new List<Coord>();
            for (var i = 0; i < pickingTravelSteps.Count; i++)
            {
                var current = pickingTravelSteps[i];
                PickingTravelStep next;
                if ((i + 2) == pickingTravelSteps.Count)
                {
                    next = pickingTravelSteps[i + 1];
                }
                else
                {
                    next = new PickingTravelStep(_warehouseLayout.GetPickingEndPosition(), current.PendingSkus);
                }

                var route = routesBetweenSlots.FirstOrDefault(x => x.IsBetween(current.Position, next.Position));

                foreach (var coord in route.Route)
                {
                    result.PathCoordinates.Add(coord);
                }
            }

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
                                                        route.ReadTravelsteps(result.Steps);
                                                        collection.AddOrUpdate(route, x => 0, (x, b) => 0);
                                                    }
                                                });

            var pickingSlotRoutes = new HashSet<RouteBetweenCoords>(collection.Keys);
            return pickingSlotRoutes;
        }
    }
}