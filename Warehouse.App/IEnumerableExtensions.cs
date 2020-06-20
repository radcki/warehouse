using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.App
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> GetKCombinations<T>(this IEnumerable<T> list, int length) where T : IComparable<T>
        {
            if (length == 1) return list.Select(t => new[] {t});
            var comparables = list as T[] ?? list.ToArray();
            return GetKCombinations(comparables, length - 1)
               .SelectMany(t => comparables.Where(o => o.CompareTo(t.Last()) > 0),
                           (t1, t2) => t1.Concat(new [] {t2}));
        }
    }
}