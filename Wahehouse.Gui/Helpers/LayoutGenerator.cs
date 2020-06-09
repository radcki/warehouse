using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.App;
using Warehouse.Domain.Entities;

namespace Warehouse.Gui.Helpers
{
    public class LayoutGenerator
    {
		private int _width;
		private int _height;
		private WarehouseLayout _warehouseLayout;
		private readonly Area _palletArea = new Area(12, 8);
		private readonly Area _corridorArea = new Area(15, 15);

		public LayoutGenerator()
		{
			
		}

		private void GenerateAlleys(Coord startingPosition, int corridorCount, int corridorPallets, int[] gaps)
		{
			var obstacles = new List<Obstacle>();
			for (var k = 0; k < corridorCount; k++)
			{
				var alleyStartCoord = new Coord(startingPosition.X + k * (_corridorArea.Width + _palletArea.Width*2 + 2), 5);
				for (var i = 0; i < corridorPallets; i++)
				{
					if (gaps.Contains(i))
					{
						continue;
					}

					var coord = new Coord(alleyStartCoord.X, alleyStartCoord.Y + (i * (_palletArea.Height + 1)));
					var obstacleLeft = new Obstacle(coord, _palletArea);
					var obstacleRight = new Obstacle(new Coord(coord.X+obstacleLeft.Area.Width+1, coord.Y), _palletArea);
					obstacles.Add(obstacleLeft);
					obstacles.Add(obstacleRight);
					var pickingCoordsRight = GetPickingCoords(obstacleRight, eSide.Right, 2);
					var pickingCoordsLeft = GetPickingCoords(obstacleLeft, eSide.Left, 2);
					_warehouseLayout.AddPickingSlots(pickingCoordsLeft.Select(x=>new PickingSlot(x, k,i)).ToList());
					_warehouseLayout.AddPickingSlots(pickingCoordsRight.Select(x=>new PickingSlot(x, k+1, i)).ToList());
                }
			}
			_warehouseLayout.AddObstacles(obstacles);
		}

		private enum eSide
		{
			Left,
			Right
		}

		private List<Coord> GetPickingCoords(Obstacle obstacle, eSide side, int count)
		{
			var coords = new List<Coord>();
			int x = side == eSide.Left ? obstacle.Position.X - 1 : obstacle.Position.X + obstacle.Area.Width + 1;
			for (int i = 0; i < count; i++)
			{
				var y = obstacle.Position.Y + obstacle.Area.Height/count*i+((obstacle.Area.Height/count)/2);
				coords.Add(new Coord(x, y));
			}
			return coords;
		}

		public LayoutGenerator GenerateLayout(int corridorCount, int corridorPallets, int[] gaps)
		{
			_width = (corridorCount * (_corridorArea.Width+_palletArea.Width*2)) + _corridorArea.Width*5;
			_height = corridorPallets*_palletArea.Height + _corridorArea.Height*10;
			_warehouseLayout = new WarehouseLayout(_width, _height, new Area(_corridorArea.Width / 2, _corridorArea.Height / 2));
            GenerateAlleys(new Coord(_corridorArea.Width*2, _corridorArea.Height*2), corridorCount, corridorPallets, gaps);
			return this;
		}

		private class SkuSummary
		{
			public long Sku { get; set; }
			public int Units { get; set; }
		}

		public List<PickingOrder> GetPickingOrders(int orderCount, int maxUnits, int maxSku)
		{
			var rnd = new Random();
			var skuInfo = _warehouseLayout.PickingSlots.GroupBy(x => x.Sku).Select(x => new SkuSummary { Sku = x.Key, Units = x.Sum(s=>s.Units) }).ToList();
			var orders = new List<PickingOrder>();
			for (var i = 0; i < orderCount; i++)
			{
				var skuCount = rnd.Next(1, maxSku);
				var unitsCount = rnd.Next(1, maxUnits);
				var skus = skuInfo.Where(x=>x.Units > 0).OrderBy(x=>rnd.Next()).Take(skuCount);
				var articles = new Dictionary<long, int>();
				foreach (var sku in skus)
				{
					var units = Math.Min(sku.Units, unitsCount);
					articles.Add(sku.Sku, units);
					sku.Units -= units;
					if (articles.Sum(x => x.Value) >= maxUnits)
					{
						break;
					}
				}
				orders.Add(new PickingOrder()
				{
					RequiredArticles = articles
				});
            }

			return orders;
		}

        public LayoutGenerator FillWithArticles(int targetFillPercent, int skuCount, int maxLocationUnits)
		{
			var rnd = new Random();
			var skus = Enumerable.Range(10000000, skuCount*10)
                                 .OrderBy(x => rnd.Next())
                                 .Take(skuCount)
                                 //.Select(x=> (long)x)
                                 .ToArray();


			int filledPercent = 0;
			int filledLocations = 0;
            var available = _warehouseLayout.PickingSlots.Where(x => x.Units == 0).ToArray();
            var fillOrder = Enumerable.Range(0, available.Length)
                                      .OrderBy(i => rnd.Next())
                                      .Take(available.Length)
                                      .ToArray();

            for (var i = 0; i < available.Length; i++)
            {
                var location = available[fillOrder[i]];
                location.Sku = skus[rnd.Next(0, skus.Length)];
                location.Units = rnd.Next(1, maxLocationUnits);
                filledLocations += 1;
                filledPercent = 100 * filledLocations / _warehouseLayout.PickingSlots.Count;
                if (filledPercent >= targetFillPercent)
                {
					break;
                }
			}

			return this;
		}

		public WarehouseLayout GetLayout()
		{
			return _warehouseLayout;
		}

    }
}
