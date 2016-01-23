using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Voz.Converters
{
	public class ColorToBrushConverter : IValueConverter
	{
		public object Convert ( object value , Type targetType , object parameter , System.Globalization.CultureInfo culture )
		{
			if ( value != null )
			{
				return new SolidColorBrush ( ( Color ) ( value ) );
			}

			return null;
		}

		public object ConvertBack ( object value , Type targetType , object parameter , System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}
