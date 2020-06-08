using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Windows;
using Warehouse.App;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Parameters;
using Point = System.Drawing.Point;
using System.Drawing.Imaging;

namespace Wahehouse.Gui.PreviewRenderer
{
	public class PreviewRenderer
	{
		private readonly WarehouseLayout _layout;
		private readonly Image _layoutBitmap;

		public PreviewRenderer(WarehouseLayout layout)
		{
			_layout = layout;
			_layoutBitmap = DrawLayout();
		}
        private int _renderScale { get; set; } = 8;



		private Image DrawLayout()
		{
			Image bitmap = new Bitmap(_layout.Width * _renderScale, _layout.Height * _renderScale);
			Graphics graphics = Graphics.FromImage(bitmap);
			var obstacles = _layout.GetObstacles();
			for (var i = 0; i < obstacles.Count; i++)
			{
				var obstacle = obstacles[i];
				var sprite = DrawObstacle(obstacle);
				graphics.DrawImage(sprite, new Point(obstacle.Position.X * _renderScale, (obstacle.Position.Y * _renderScale)));
			}

			var pickingSlots = _layout.GetPickingSlots();
			foreach (var pickingSlot in pickingSlots)
			{
				var sprite = DrawPickingPoint(pickingSlot.Coords);
				graphics.DrawImage(sprite, new Point(pickingSlot.Coords.X * _renderScale-_renderScale/2, (pickingSlot.Coords.Y * _renderScale - _renderScale / 2)));
            }

            return bitmap;
		}

		public Image GetLayoutPreview()
		{
			return _layoutBitmap;
		}

		public Image GetPickingPathPreview(IPathFindingResult result)
		{
			Image layout = new Bitmap(_layoutBitmap);
			var path = DrawPath(result.PathCoordinates);
			Graphics graphics = Graphics.FromImage(layout);
			graphics.DrawImage(path, new Point(0,0));
			return layout;
		}

		public Image GetPickingPathsPreview(IEnumerable<IPathFindingResult> results)
		{
			Image layout = new Bitmap(_layoutBitmap);
            Graphics graphics = Graphics.FromImage(layout);
			foreach (var pathFindingResreult in results)
			{
				var path = DrawPath(pathFindingResreult.PathCoordinates);
				graphics.DrawImage(path, new Point(0, 0));
			}
            return layout;
		}

        private Image DrawPath(IEnumerable<Coord> pathCoords)
		{
			Image bitmap = new Bitmap(_layout.Width* _renderScale, _layout.Height*_renderScale);
			Graphics graphics = Graphics.FromImage(bitmap);
			var color = Color.FromArgb(50, Color.Red);
			var brush = new SolidBrush(color);

            foreach (var pathCoord in pathCoords)
			{
				var point = new Rectangle(pathCoord.X * _renderScale, pathCoord.Y * _renderScale, _renderScale*2, _renderScale*2);
				
				graphics.FillRectangle(brush, point);
				//graphics.DrawRectangle(blackPen,point);
			}
			return bitmap;
		}


		private Image DrawObstacle(Obstacle obstacle)
		{
			int borderWidth = _renderScale;
			Image bitmap = new Bitmap((obstacle.Area.Width+1)*_renderScale, (obstacle.Area.Height+1)*_renderScale);
			Graphics graphics = Graphics.FromImage(bitmap);
			var blackPen = new Pen(Color.Black, borderWidth);

			graphics.DrawRectangle(blackPen, borderWidth/2, borderWidth/2, bitmap.Width-borderWidth, bitmap.Height- borderWidth);
			graphics.DrawLine(blackPen, 0, 0, bitmap.Width, bitmap.Height);
			graphics.DrawLine(blackPen, bitmap.Width, 0, 0, bitmap.Height);
			return bitmap;
		}

		private Image DrawPickingPoint(Coord coord)
		{
			int borderWidth = _renderScale;
			Image bitmap = new Bitmap(_renderScale*2, _renderScale*2);
			Graphics graphics = Graphics.FromImage(bitmap);
			var pen = new Pen(Color.Transparent, 0);

			var point = new Rectangle(1,1, bitmap.Width, bitmap.Height);
			graphics.FillRectangle(Brushes.DarkViolet, point);
            //graphics.DrawRectangle(pen, borderWidth / 2, borderWidth / 2, bitmap.Width - borderWidth, bitmap.Height - borderWidth);
			return bitmap;
		}
    }
}