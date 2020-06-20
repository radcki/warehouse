using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GalaSoft.MvvmLight.Ioc;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Interfaces;
using Warehouse.Domain.Parameters;
using Warehouse.Gui.ViewModel;

namespace Warehouse.Gui.PreviewRenderer.ValueConverters
{
    public class LayoutCollectionToStrokeValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] is List<ILayoutElement> elements)
            {
                if (elements.FirstOrDefault() is Obstacle)
                {
                    var stroke = new SolidColorBrush { Color = Color.FromRgb(0, 0, 0) };

                    if (parameter is string opacityMultiplier)
                    {
                        var multiplier = double.Parse(opacityMultiplier, CultureInfo.InvariantCulture);
                        stroke.Opacity = Math.Max(0, Math.Min(1, stroke.Opacity * multiplier));
                    }
                    return stroke;
                }

                if (elements.FirstOrDefault() is PickingSlot)
                {
                    return new SolidColorBrush { Color = Color.FromArgb(0, 0, 0, 0) };
                }

                if (elements.FirstOrDefault() is CoordLayoutElement)
                {
                    return new SolidColorBrush { Color = Color.FromArgb(255, 230, 0, 0) };
                }

                if (elements.FirstOrDefault() is IPathFindingResult)
                {
                    return new SolidColorBrush { Color = Color.FromArgb(255, 230, 0, 0) };
                }

                if (elements.FirstOrDefault() is TravelVertex)
				{
					return new SolidColorBrush { Color = Color.FromArgb(255, 150, 150, 150) };
				}
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
