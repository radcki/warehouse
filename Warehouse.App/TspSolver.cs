using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Parameters;

namespace Warehouse.App
{
    public class TspSolver
    {
        private HashSet<RouteBetweenCoords> _distances;

        public List<PickingTravelStep> OrderSlots(List<PickingTravelStep> pickingSteps, HashSet<RouteBetweenCoords> distances)
        {
            _distances = distances;
            //create an initial tour out of nearest neighbors
            var stops = pickingSteps
                       .Select(i => new Stop(i, distances))
                       .NearestNeighbors()
                       .ToList();

            //create next pointers between them
            stops.Connect(true);

            //wrap in a tour object
            Tour startingTour = new Tour(stops);

            //the actual algorithm
            while (true)
            {
                var newTour = startingTour.GenerateMutations()
                                          .MinBy(tour => tour.Cost());
                if (newTour != null && newTour.Cost() < startingTour.Cost())
                {
                    startingTour = newTour;
                }
                else break;
            }

            var current = startingTour.Anchor.CanGetTo();
 

            return current.Select(x=>x.Step).ToList();
        }

        public class Stop
        {
            private HashSet<RouteBetweenCoords> _distances;

            public Stop(PickingTravelStep step, HashSet<RouteBetweenCoords> distances)
            {
                Step = step;
                _distances = distances;
            }


            public Stop Next { get; set; }

            public PickingTravelStep Step { get; set; }


            public Stop Clone()
            {
                return new Stop(Step, _distances);
            }


            public double Distance(Stop other)
            {
                _distances.TryGetValue(new RouteBetweenCoords(Step.Position, other.Step.Position), out var route);
                return route.TravelCost;
            }


            //list of nodes, including this one, that we can get to
            public IEnumerable<Stop> CanGetTo()
            {
                var current = this;
                while (true)
                {
                    yield return current;
                    current = current.Next;
                    if (current == this) break;
                }
            }


            public override bool Equals(object obj)
            {
                return Step == ((Stop) obj).Step;
            }


            public override int GetHashCode()
            {
                return Step.GetHashCode();
            }
        }


        private class Tour
        {
            public Tour(IEnumerable<Stop> stops)
            {
                Anchor = stops.First();
            }


            //the set of tours we can make with 2-opt out of this one
            public IEnumerable<Tour> GenerateMutations()
            {
                for (Stop stop = Anchor; stop.Next != Anchor; stop = stop.Next)
                {
                    //skip the next one, since you can't swap with that
                    Stop current = stop.Next.Next;
                    while (current != Anchor)
                    {
                        yield return CloneWithSwap(stop.Step, current.Step);
                        current = current.Next;
                    }
                }
            }


            public Stop Anchor { get; set; }


            public Tour CloneWithSwap(PickingTravelStep firstCity, PickingTravelStep secondCity)
            {
                Stop firstFrom = null, secondFrom = null;
                var stops = UnconnectedClones();
                stops.Connect(true);

                foreach (Stop stop in stops)
                {
                    if (stop.Step == firstCity) firstFrom = stop;

                    if (stop.Step == secondCity) secondFrom = stop;
                }

                //the swap part
                var firstTo = firstFrom.Next;
                var secondTo = secondFrom.Next;

                //reverse all of the links between the swaps
                firstTo.CanGetTo()
                       .TakeWhile(stop => stop != secondTo)
                       .Reverse()
                       .Connect(false);

                firstTo.Next = secondTo;
                firstFrom.Next = secondFrom;

                var tour = new Tour(stops);
                return tour;
            }


            public IList<Stop> UnconnectedClones()
            {
                return Cycle().Select(stop => stop.Clone()).ToList();
            }


            public double Cost()
            {
                return Cycle()
                   .Aggregate(0.0,
                              (sum, stop) => sum + stop.Distance(stop.Next));
            }

            private IEnumerable<Stop> Cycle()
            {
                return Anchor.CanGetTo();
            }

            public override string ToString()
            {
                string path = string.Join(
                                          "->",
                                          Cycle().Select(stop => stop.ToString()).ToArray());
                return String.Format("Cost: {0}, Path:{1}", Cost(), path);
            }
        }
    }
}