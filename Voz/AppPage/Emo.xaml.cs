using System.Windows;
using Microsoft.Phone.Controls;
using Voz.Model;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Phone.Shell;

namespace Voz.AppPage
{
	public partial class Emo : PhoneApplicationPage
	{
		public Emo ()
		{
			InitializeComponent ();
			emo = "";
		}

		private string emo { get; set; }

		private void Image_Tap ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			emo = ( ( Image ) sender ).Tag.ToString ();
			NavigationService.GoBack ();
		}

		protected override void OnNavigatedFrom ( System.Windows.Navigation.NavigationEventArgs e )
		{
			base.OnNavigatedFrom ( e );
			PhoneApplicationService.Current.State["Emo"] = emo;
		}
	}
}