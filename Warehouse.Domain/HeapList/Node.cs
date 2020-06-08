using System;

namespace Warehouse.Domain.HeapList
{
    public class HeapNode<T> where T:IEquatable<T>
    {
        /// <summary>
        /// Następny na stosie węzeł
        /// </summary>
        public HeapNode<T> NextNode { get; set; }


        public float EstimatedCost { get; }
        public T TravelStep { get; }

        public HeapNode(T travelStep, float estimatedCost)
        {
            EstimatedCost = estimatedCost;
            TravelStep = travelStep;
        }
    }
}
