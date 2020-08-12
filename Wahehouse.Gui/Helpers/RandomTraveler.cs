using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Warehouse.App;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Parameters;

namespace Warehouse.Gui.Helpers
{
    public class RandomTraveler
    {
        private readonly WarehouseLayout _warehouseLayout;
        private readonly PathSolver _pathSolver;
        private CancellationTokenSource _canceler;
        private Coord CurrentCoord { get; set; }
        private ConcurrentBag<IPathFindingResult> Results { get; set; }
        public event EventHandler<RouteFoundEventArgs> RouteFound;
        public event EventHandler<TravelStoppedEventArgs> TravelStopped;
        public bool IsInProgress { get; private set; }

        public RandomTraveler(WarehouseLayout layout)
        {
            _warehouseLayout = layout;
            _pathSolver = new PathSolver(layout);
        }

        private void PrepareToStart()
        {
            _canceler?.Cancel();
            _canceler = new CancellationTokenSource();
            Results = new ConcurrentBag<IPathFindingResult>();
            IsInProgress = true;
        }

        public async void StartParallel()
        {
            PrepareToStart();
            ThreadPool.GetMaxThreads(out var threadCount, out _);

            var tasks = Enumerable.Range(0, threadCount)
                                  .Select(async x => await Task.Run(() =>
                                                                    {
                                                                        do
                                                                        {
                                                                            if (_canceler.IsCancellationRequested)
                                                                            {
                                                                                break;
                                                                            }

                                                                            TravelToRandomDestination();
                                                                        }
                                                                        while (true);
                                                                    }))
                                  .ToArray();

            await Task.WhenAll(tasks);
        }

        public async void Start()
        {
            PrepareToStart();
            await Task.Run(() =>
                           {
                               do
                               {
                                   if (_canceler.IsCancellationRequested)
                                   {
                                       break;
                                   }

                                   ContinueTravelToRandomDestination();
                               }
                               while (true);
                           });
        }

        private void TravelToRandomDestination()
        {
            var random = new Random();
            var source = _warehouseLayout.PickingSlots.ElementAt(random.Next(0, _warehouseLayout.PickingSlots.Count - 1)).Value.Position;
            var destination = _warehouseLayout.PickingSlots.ElementAt(random.Next(0, _warehouseLayout.PickingSlots.Count - 1)).Value.Position;

            var path = _pathSolver.FindPath(new TravelStep(source), new TravelStep(destination), true);
            RegisterFoundPath(path);
        }

        private void ContinueTravelToRandomDestination()
        {
            var random = new Random();
            var source = CurrentCoord != default
                             ? CurrentCoord
                             : _warehouseLayout.PickingSlots.ElementAt(random.Next(0, _warehouseLayout.PickingSlots.Count - 1)).Value.Position;

            var destination = _warehouseLayout.PickingSlots.ElementAt(random.Next(0, _warehouseLayout.PickingSlots.Count - 1)).Value.Position;

            var path = _pathSolver.FindPath(new TravelStep(source), new TravelStep(destination), true);
            RegisterFoundPath(path);
            CurrentCoord = destination;
        }

        private void RegisterFoundPath(IPathFindingResult pathFindingResult)
        {
            Results.Add(pathFindingResult);
            /*
            RouteFound?.Invoke(this, new RouteFoundEventArgs()
                                     {
                                         FoundSoFar = Results.Count,
                                         AverageExecutionTime = TimeSpan.FromMilliseconds(Results.Average(x => x.ExecutionTime.Milliseconds)),
                                         PathFindingResult = pathFindingResult
                                     });*/
        }


        public void Stop()
        {
            _canceler.Cancel();
            IsInProgress = false;
            TravelStopped?.Invoke(this, new TravelStoppedEventArgs()
                                        {
                                            FoundSoFar = Results.Count,
                                            AverageExecutionTime = TimeSpan.FromTicks((long)Results.Average(x => x.ExecutionTime.Ticks)),
                                            PathFindingResults = Results.ToList()
                                        });
        }
    }

    public class RouteFoundEventArgs : EventArgs
    {
        public int FoundSoFar { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public IPathFindingResult PathFindingResult { get; set; }
    }

    public class TravelStoppedEventArgs : EventArgs
    {
        public int FoundSoFar { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public List<IPathFindingResult> PathFindingResults { get; set; }
    }
}