using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Voz.Model;

namespace Voz.Helper
{
	class Themes
	{
		public static void ChangeColor()
		{
			Color accentColor = UserData.settings.GetValueOrDefault ( AppSettings.keyAccentColor , Colors.Green );
			App.Current.Resources.Remove ( "AccentColor" );
			App.Current.Resources.Add ( "AccentColor" , new SolidColorBrush ( accentColor ) );

			if ( UserData.settings.GetValueOrDefault ( AppSettings.keyDarkTheme , true ) )
			{
				//dark
				App.Current.Resources.Remove ( "TextColor" );
				App.Current.Resources.Add ( "TextColor" , new SolidColorBrush ( Colors.White ) );
				App.Current.Resources.Remove ( "BackgroundColor" );
				App.Current.Resources.Add ( "BackgroundColor" , new SolidColorBrush ( Colors.Black ) );
			}
			else
			{
				//light
				App.Current.Resources.Remove ( "TextColor" );
				App.Current.Resources.Add ( "TextColor" , new SolidColorBrush ( Colors.Black ) );
				App.Current.Resources.Remove ( "BackgroundColor" );
				App.Current.Resources.Add ( "BackgroundColor" , new SolidColorBrush ( Colors.White ) );
			}
		}
	}
}
