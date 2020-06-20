using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Warehouse.Domain.Entities;
using Warehouse.Domain.HeapList;
using Warehouse.Domain.Parameters;

namespace Warehouse.App
{
    public class PathSolver
    {
        #region Privates

        private readonly WarehouseLayout _warehouseLayout;

        #endregion

        #region Constructors

        public PathSolver(WarehouseLayout warehouseLayout)
        {
            _warehouseLayout = warehouseLayout;
        }

        #endregion

        #region Methods

        public PathFindingResult<TravelStep> FindPath(TravelStep startTravelStep, TravelStep endTravelStep, bool traverse)
        {
            var result = new PathFindingResult<TravelStep>();

            if (!_warehouseLayout.IsWalkable(startTravelStep.Position))
            {
                return new PathFindingResult<TravelStep>()
                       {
                           Success = false
                       };
            }

            if (!_warehouseLayout.IsWalkable(endTravelStep.Position))
            {
                return new PathFindingResult<TravelStep>()
                       {
                           Success = false
                       };
            }

            var travelVertices = _warehouseLayout.GetTravelVerticesDictionary();
            var openList = new Heap<TravelStep>();

            var closedList = new byte[_warehouseLayout.Width + 1, _warehouseLayout.Height + 1];
            var addedToOpenList = new int[_warehouseLayout.Width + 1, _warehouseLayout.Height + 1];

            var currentPosition = startTravelStep;
            openList.Add(new HeapNode<TravelStep>(currentPosition, currentPosition.TraverseCost * currentPosition.EuclidianDistanceTo(endTravelStep)));

            var stepCount = 0;
            while (openList.HasMore())
            {
                stepCount++;
                currentPosition = openList.TakeHeapHeadPosition();
                if (currentPosition == endTravelStep) // success
                {
                    break;
                }

                closedList[currentPosition.X, currentPosition.Y] = 1;
                
                // get travel options from precalculated collection
                travelVertices.TryGetValue(currentPosition.Position, out var movementOptions);

                // add movement options to heap
                foreach (var position in movementOptions.Select(x => new TravelStep(x)))
                {
                    if (closedList[position.X, position.Y] == 1)
                    {
                        continue;
                    }

                    var gScore = currentPosition.CostFromStart + position.EuclidianDistanceTo(currentPosition);
                    var fScore = gScore + position.ManhattanDistanceTo(endTravelStep);

                    position.Parent = currentPosition;
                    position.CostFromStart = (int) gScore;

                    if (addedToOpenList[position.X, position.Y] == 0)
                    {
                        openList.Add(new HeapNode<TravelStep>(position, fScore));
                        addedToOpenList[position.X, position.Y] = (int) gScore;
                    }
                    else if (addedToOpenList[position.X, position.Y] > gScore)
                    {
                        openList.Reinsert(new HeapNode<TravelStep>(position, fScore));
                    }
                }
            }

            result.Success = currentPosition == endTravelStep;
            result.Route = currentPosition;

            return result;
        }
        
        #endregion
    }
}