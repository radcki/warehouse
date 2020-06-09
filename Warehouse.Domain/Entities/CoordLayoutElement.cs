using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Domain.Entities
{
    public class CoordLayoutElement : ILayoutElement
    {
        public CoordLayoutElement(Coord position)
        {
            Position = position;
        }
        public Coord Position { get; }

        public Area Area => new Area(1,1);

    }
}
