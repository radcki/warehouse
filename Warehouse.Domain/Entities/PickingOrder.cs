using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Domain.Entities
{
    public class PickingOrder
    {
        public Dictionary<long, int> RequiredArticles { get; set; } = new Dictionary<long, int>();
    }
}
