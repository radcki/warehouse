using System;
using Warehouse.Domain.Entities;

namespace Warehouse.Domain.HeapList
{
    /// <summary>
    /// Połączony stos węzłów uszeregowany wg szacunkowego kosztu przejścia do celu
    /// </summary>
    public class Heap<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Węzeł o najniższym koszcie przejścia do celu. 
        /// Kolejny w kolejności węzeł jest przechowywany jako argument węzła, czyli np. head.Next
        /// </summary>
        private HeapNode<T> HeapHead { get; set; }

        public bool HasMore() => HeapHead != null;

        /// <summary>
        /// Dodanie węzła z zapewnieniem sortowania wg kosztu
        /// </summary>
        /// <param name="node"></param>
        public void Add(HeapNode<T> node)
        {
            // nie ma czego porównywać
            if (!HasMore())
            {
                HeapHead = node;
                return;
            }

            // jeżeli dodawany węzeł ma niższy koszt niż aktualnie najlepszy lub są równe, dodawany wskakuje na szczyt
            if (HeapHead.EstimatedCost >= node.EstimatedCost)
            {
                var oldHead = HeapHead;
                node.NextNode = oldHead; // obecnie najwyższy zostaje kolejnym po nowym
                this.HeapHead = node; // nowy staje się najwyższym
                return;
            }

            // w innym przypadku poszukujemy umiejscowienia dla dodawanego węzła            
            // przeszukanie jest wykonywane aż węzeł nie ma przypisanego kolejnego lub koszt jest wyższy
            var currentNode = HeapHead;
            while (currentNode.NextNode != null && currentNode.EstimatedCost < node.EstimatedCost)
            {
                currentNode = currentNode.NextNode;
            }
            // po zatrzymaniu pętli węzeł zostaje wstawiony w kolejkę
            node.NextNode = currentNode.NextNode;
            currentNode.NextNode = node;
        }

        /// <summary>
        /// Wyciągnięcie ze stosu najwyższego elementu i wstawienie w jego miejsce kolejnego
        /// </summary>
        /// <returns></returns>
        public T TakeHeapHeadPosition()
        {
            var node = HeapHead;
            HeapHead = HeapHead.NextNode;
            return node.TravelStep;
        }

        public HeapNode<T> GetNode(T travelStep)
        {
            var currentNode = HeapHead;
            while (currentNode != null)
            {
                if (currentNode.TravelStep.Equals(travelStep))
                {
                    return currentNode;
                }
                currentNode = currentNode.NextNode;
            }
            throw new Exception("Node not found");
        }

        public void Reinsert(HeapNode<T> node)
        {
            var currentNode = HeapHead;
            while (currentNode.NextNode != null)
            {
                if (currentNode.NextNode.TravelStep.Equals(node.TravelStep))
                {
                    currentNode.NextNode = currentNode.NextNode.NextNode;
                    Add(node);
                    return;
                }
                currentNode = currentNode.NextNode;
            }
        }

    }
}
