using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Domain.Entities;

namespace Warehouse.Gui.PreviewRenderer.StateModels
{
    public class FilledPickingSlot : PickingSlot
    {
        public FilledPickingSlot(PickingSlot pickingSlot) : base(pickingSlot.Position, pickingSlot.AlleyNumber, pickingSlot.PositionNumber)
        {
            Sku = pickingSlot.Sku;
            Units = pickingSlot.Units;
            ReservedUnits = pickingSlot.ReservedUnits;
        }
        /// <inheritdoc />
        public FilledPickingSlot(Coord position, int alleyNumber, int positionNumber) : base(position, alleyNumber, positionNumber)
        {
        }
    }
}