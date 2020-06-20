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
using Warehouse.Domain.Parameters;
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
                        foreach (Obstacle obstacle in elements)
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
                        foreach (PickingSlot pickingSlot in elements)
                        {
                            ctx.DrawGeometry(new EllipseGeometry(new Point(pickingSlot.Position.X, pickingSlot.Position.Y), 1.5, 1.5));
                        }
                    }
                    else if (elements.FirstOrDefault() is CoordLayoutElement)
                    {
                        foreach (CoordLayoutElement travelStep in elements)
                        {
                            ctx.DrawGeometry(new RectangleGeometry(new Rect(travelStep.Position.X, travelStep.Position.Y, 1, 1)));
                        }
                    }
                    else if (elements.FirstOrDefault() is IPathFindingResult)
                    {
                        foreach (IPathFindingResult pathFindingResult in elements)
                        {
                            foreach (var path in pathFindingResult.Paths)
                            {
                                var current = path[0];
                                ctx.BeginFigure(new Point(current.X, current.Y), false, false);
                                for (var i = 1; i < path.Count; i++)
                                {
                                    current = path[i];
                                    ctx.LineTo(new Point(current.X, current.Y), true, false);
                                }
                            }
                            
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