using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Interfaces;
using Warehouse.Gui.PreviewRenderer.Extensions;

namespace Warehouse.Gui.PreviewRenderer.ValueConverters
{
    public class LayoutCollectionToGeometryValueConverter : IMultiValueConverter
    {
        #region Implementation of IValueConverter

        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] is List<ILayoutElement> elements)
            {
                // Create a StreamGeometry to draw element
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    if (elements.FirstOrDefault() is Obstacle)
                    {
                        foreach (var obstacle in elements)
                        {
                            var position = obstacle.Position;
                            var pointNW = new Point(position.X, position.Y);
                            var pointNE = new Point(position.X + obstacle.Area.Width, position.Y);
                            var pointSW = new Point(position.X, position.Y + obstacle.Area.Height);
                            var pointSE = new Point(position.X + obstacle.Area.Width, position.Y + obstacle.Area.Height);
                            ctx.BeginFigure(pointNW, false, true);
                            ctx.LineTo(pointNE, true, false);
                            ctx.LineTo(pointSE, true, false);
                            ctx.LineTo(pointSW, true, false);
                            ctx.LineTo(pointNW, true, false);
                            ctx.LineTo(pointSE, true, false);
                            ctx.LineTo(pointNE, true, false);
                            ctx.LineTo(pointSW, true, false);
                        }
                        geometry.FillRule = FillRule.EvenOdd;
                    }
                    else if (elements.FirstOrDefault() is PickingSlot)
                    {
                        foreach (var pickingSlot in elements)
                        {
                            ctx.DrawGeometry(new EllipseGeometry(new Point(pickingSlot.Position.X, pickingSlot.Position.Y), 1.5, 1.5));
                        }
                    }
                    else if (elements.FirstOrDefault() is CoordLayoutElement)
                    {
                        foreach (var travelStep in elements)
                        {
                            ctx.DrawGeometry(new RectangleGeometry(new Rect(travelStep.Position.X, travelStep.Position.Y, 1, 1)));
                        }
                    }
					else if (elements.FirstOrDefault() is TravelVertex)
					{
						foreach (TravelVertex travelStep in elements)
						{
							foreach (var travelStepNeighbour in travelStep.Neighbours)
							{
								ctx.BeginFigure(new Point(travelStep.Position.X, travelStep.Position.Y), false, true);
								ctx.LineTo(new Point(travelStepNeighbour.Position.X, travelStepNeighbour.Position.Y), false, false);
                            }
						}
					}

                    geometry.Freeze();
                    return geometry;
                }
            }

            return Binding.DoNothing;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
