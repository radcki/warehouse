using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Warehouse.Gui.ValueConverters
{
    public class ImageSourceValueConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Image image)
			{
				BitmapImage source = new BitmapImage();
				try
				{
					using (MemoryStream ms = new MemoryStream())
					{
						image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

						source.BeginInit();
						source.CacheOption = BitmapCacheOption.OnLoad;
						source.UriSource = null;
						source.DecodePixelHeight = image.Height / 2;
						source.DecodePixelWidth = image.Width / 2;
						source.StreamSource = ms;
						source.EndInit();

						return source;
					}
				}
				catch
				{
					//ignore
				}
                
            }

			return new BitmapImage();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
