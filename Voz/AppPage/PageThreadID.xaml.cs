using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Input;
using System.Net.Http;
using System.Windows.Threading;
using System.Windows;

namespace Voz.AppPage
{
	public partial class PageThreadID : PhoneApplicationPage
	{
		public PageThreadID ()
		{
			InitializeComponent ();
			client = new HttpClient ();
			timer = new DispatcherTimer ();
		}

		private HttpClient client;
		DispatcherTimer timer;

		private async void textBox_IdInput_KeyDown ( object sender , System.Windows.Input.KeyEventArgs e )
		{
			if ( e.Key == Key.Enter )
			{
				int n = 0;
				bool result = Int32.TryParse ( textBox_IdInput.Text , out n );
				if ( result )
				{
					string url = "https://vozforums.com/showthread.php?t=" + textBox_IdInput.Text;
					try
					{
						ProgressIndicatorSwitch ( true );
						string responseResult = await client.GetStringAsync ( url );
						if ( responseResult.Contains ( "No Thread specified." ) )
						{
							SystemTray.ProgressIndicator.Text = "ID ko tồn tại, thử ID khác.";
							timer.Interval = TimeSpan.FromSeconds ( 2 );
							timer.Tick += new EventHandler ( TimerTick );
							timer.Start ();
						}
						else
						{
							ProgressIndicatorSwitch ( false );
							NavigationService.Navigate ( new Uri ( "/AppPage/Thread.xaml?parameter1=" + textBox_IdInput.Text , UriKind.Relative ) );
						}
					}
					catch ( Exception ex )
					{
						MessageBox.Show ( "Lỗi server Voz: " + ex.Message );
						Application.Current.Terminate ();
					}
				}
			}
		}

		private void ProgressIndicatorSwitch ( bool value )
		{
			SystemTray.ProgressIndicator.IsIndeterminate = value;
			SystemTray.ProgressIndicator.IsVisible = value;
		}

		private void TimerTick ( object sender , EventArgs e )
		{
			ProgressIndicatorSwitch ( false );
			timer.Stop ();
		}

		protected override void OnNavigatedTo ( NavigationEventArgs e )
		{
			base.OnNavigatedTo ( e );
			SystemTray.ProgressIndicator = new ProgressIndicator ();
		}
	}
}