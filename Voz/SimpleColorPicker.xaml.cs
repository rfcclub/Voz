using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Voz.Model;
using System.Windows.Media;
namespace Voz
{
	public partial class SimpleColorPicker : PhoneApplicationPage
	{
		public SimpleColorPicker ()
		{
			InitializeComponent ();

			this.Loaded += ColorPickerPage_Loaded;
		}

		static string[] colorNames =
        {
	        "Brown","Red","Yellow","Blue","Green","Black","White","Magenta","Dark orchid"
        };

		static uint[] uintColors =
        { 
	        0xFFA52A2A,0xFFFF0000,0xFFFFFF00,0xFF0000FF,0xFF008000,0xFF000000,0xFFFFFFFF,0xFFFF00FF,0xFF9932CC
        };

		private void ColorPickerPage_Loaded ( object sender , RoutedEventArgs e )
		{
			List<ColorItem> item = new List<ColorItem> ();
			for ( int i = 0 ; i < 9 ; i++ )
			{
				item.Add ( new ColorItem () { Text = colorNames[i] , Color = ConvertColor ( uintColors[i] ) } );
			};

			listBox.ItemsSource = item; //Fill ItemSource with all colors
		}

		private Color ConvertColor ( uint uintCol )
		{
			byte A = ( byte ) ( ( uintCol & 0xFF000000 ) >> 24 );
			byte R = ( byte ) ( ( uintCol & 0x00FF0000 ) >> 16 );
			byte G = ( byte ) ( ( uintCol & 0x0000FF00 ) >> 8 );
			byte B = ( byte ) ( ( uintCol & 0x000000FF ) >> 0 );

			return Color.FromArgb ( A , R , G , B ); ;
		}

		private void lstColor_SelectionChanged ( object sender , SelectionChangedEventArgs e )
		{
			if ( e.AddedItems.Count > 0 )
			{
				PhoneApplicationService.Current.State["EditColor"] = ( ( ColorItem ) e.AddedItems[0] ).Text;
				NavigationService.GoBack ();
			}
		}
	}
}