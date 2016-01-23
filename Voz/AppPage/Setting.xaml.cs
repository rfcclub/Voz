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
using System.Windows.Threading;

namespace Voz.AppPage
{
	public partial class Setting : PhoneApplicationPage
	{
		public Setting ()
		{
			InitializeComponent ();
			timer = new DispatcherTimer ();
			timer.Tick += new EventHandler ( TimerTick );
			SystemTray.ProgressIndicator = new ProgressIndicator ();
			UserData.account = "";
			UserData.password = "";
			AddSettings ();
		}

		DispatcherTimer timer;

		private void AddSettings ()
		{
			//add join date setting
			UserData.settings.Add ( AppSettings.keyJoinDate , true );
			if ( UserData.settings.GetValueOrDefault ( AppSettings.keyJoinDate , true ) == true )
			{
				toggleSwitchJoinDate.IsChecked = true;
			}
			else
			{
				toggleSwitchJoinDate.IsChecked = false;
			}

			//add location setting
			UserData.settings.Add ( AppSettings.keyLocation , true );
			if ( UserData.settings.GetValueOrDefault ( AppSettings.keyLocation , true ) == true )
			{
				toggleSwitchLocation.IsChecked = true;
			}
			else
			{
				toggleSwitchLocation.IsChecked = false;
			}

			//add post setting
			UserData.settings.Add ( AppSettings.keyPosts , true );
			if ( UserData.settings.GetValueOrDefault ( AppSettings.keyPosts , true ) == true )
			{
				toggleSwitchPosts.IsChecked = true;
			}
			else
			{
				toggleSwitchPosts.IsChecked = false;
			}

			//add emo setting
			UserData.settings.Add ( AppSettings.keyEmo , false );
			if ( UserData.settings.GetValueOrDefault ( AppSettings.keyEmo , true ) == true )
			{
				toggleSwitchEmo.IsChecked = true;
			}
			else
			{
				toggleSwitchEmo.IsChecked = false;
			}

			//add ava setting
			UserData.settings.Add ( AppSettings.keyAva , true );
			if ( UserData.settings.GetValueOrDefault ( AppSettings.keyAva , true ) == true )
			{
				toggleSwitchAvatar.IsChecked = true;
			}
			else
			{
				toggleSwitchAvatar.IsChecked = false;
			}

			//add dark theme
			UserData.settings.Add ( AppSettings.keyDarkTheme , true );
			if ( UserData.settings.GetValueOrDefault ( AppSettings.keyDarkTheme , true ) == true )
			{
				toggleSwitchDarkTheme.IsChecked = true;
			}
			else
			{
				toggleSwitchDarkTheme.IsChecked = false;
			}
		}

		private async void buttonLogin_Tap ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			if ( textBoxAccount.Text != string.Empty && passwordBox.Password != string.Empty )
			{
				UserData.account = textBoxAccount.Text;
				UserData.password = passwordBox.Password;
				string loginResult = "";
				try
				{
					//send login request
					SystemTray.ProgressIndicator.Text = "Đăng nhập...";
					ProgressIndicatorSwitch ( true );
					loginResult = await Login.LoginAndGetCookie ( UserData.account , UserData.password );
					if ( loginResult == "Server" )
					{
						throw new Exception ( "Server Voz đang gặp lỗi" );
					}
					else if ( loginResult == "TokenError" || loginResult == "" )
					{
						throw new Exception ( "Đăng nhập thất bại, kiểm tra tài khoản hoặc thử lại sau" );
					}
					else if ( loginResult != "" && UserData.token != "" )
					{
						SystemTray.ProgressIndicator.Text = "Đăng nhập thành công";
						timer.Interval = TimeSpan.FromSeconds ( 1.5 );
						timer.Start ();
						//save user and password
						if ( checkBoxRememberLogin.IsChecked == true )
						{
							UserData.settings.Add ( AppSettings.keyAccount , UserData.account );
							UserData.settings.Add ( AppSettings.keyPassword , UserData.password );
							if ( UserData.settings.ContainKey ( AppSettings.keyCookie ) )
							{
								UserData.settings.Update ( AppSettings.keyCookie , UserData.cookies );
							}
							else
							{
								UserData.settings.Add ( AppSettings.keyCookie , UserData.cookies );
							}
						}
						UserData.isLoggedIn = true;
					}
				}
				catch ( Exception ex )
				{
					ProgressIndicatorSwitch ( false );
					MessageBox.Show ( ex.Message );
				}
			}
			else
			{
				MessageBox.Show ( "Nhập tài khoản và mật khẩu" );
			}
		}

		private void TimerTick ( object sender , EventArgs e )
		{
			ProgressIndicatorSwitch ( false );
			timer.Stop ();
		}

		private void ProgressIndicatorSwitch ( bool value )
		{
			SystemTray.ProgressIndicator.IsIndeterminate = value;
			SystemTray.ProgressIndicator.IsVisible = value;
		}

		private void SaveClick ( object sender , EventArgs e )
		{
			SystemTray.ProgressIndicator.Text = "Đã lưu";
			ProgressIndicatorSwitch ( true );

			timer.Interval = TimeSpan.FromSeconds ( 1 );
			timer.Start ();

			if ( UserData.settings.ContainKey ( AppSettings.keyDevice ) )
			{
				UserData.settings.Update ( AppSettings.keyDevice , textBoxDevice.Text.Trim () );
			}
			else
			{
				UserData.settings.Add ( AppSettings.keyDevice , textBoxDevice.Text.Trim () );
			}
		}

		private void toggleSwitchJoinDate_Checked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyJoinDate , true );
		}

		private void toggleSwitchJoinDate_Unchecked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyJoinDate , false );
		}

		private void toggleSwitchLocation_Checked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyLocation , true );
		}

		private void toggleSwitchLocation_Unchecked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyLocation , false );
		}

		private void toggleSwitchPosts_Checked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyPosts , true );
		}

		private void toggleSwitchPosts_Unchecked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyPosts , false );
		}

		private void toggleSwitchEmo_Checked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyEmo , true );
		}

		private void toggleSwitchEmo_Unchecked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyEmo , false );
		}

		private void toggleSwitchAvatar_Checked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyAva , true );
		}

		private void toggleSwitchAvatar_Unchecked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyAva , false );
		}

		private void toggleSwitchDarkTheme_Checked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyDarkTheme , true );
		}

		private void toggleSwitchDarkTheme_Unchecked ( object sender , RoutedEventArgs e )
		{
			UserData.settings.Update ( AppSettings.keyDarkTheme , false );
		}

		private void PhoneApplicationPage_Loaded ( object sender , RoutedEventArgs e )
		{
			SystemTray.ProgressIndicator = new ProgressIndicator ();
			if ( UserData.settings.ContainKey ( AppSettings.keyAccount ) )
			{
				UserData.account = UserData.settings.GetValueOrDefault ( AppSettings.keyAccount , "" );
				UserData.password = UserData.settings.GetValueOrDefault ( AppSettings.keyPassword , "" );
				textBoxAccount.Text = UserData.account;
				passwordBox.Password = UserData.password;
				textBoxDevice.Text = UserData.settings.GetValueOrDefault ( AppSettings.keyDevice , "" );
			}
		}

		private void buttonLogout_Tap ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			SystemTray.ProgressIndicator.Text = "Đã xóa";
			ProgressIndicatorSwitch ( true );

			timer.Interval = TimeSpan.FromSeconds ( 2 );
			timer.Tick += new EventHandler ( TimerTick );
			timer.Start ();

			UserData.settings.RemoveKey ( AppSettings.keyAccount );
			UserData.settings.RemoveKey ( AppSettings.keyPassword );
			UserData.settings.RemoveKey ( AppSettings.keyCookie );
			UserData.isLoggedIn = false;
		}

		private void buttonChangeColor_Tap ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			NavigationService.Navigate ( new Uri ( "/ColorPickerPage.xaml" , UriKind.Relative ) );
		}

		private void textBox_GotFocus ( object sender , RoutedEventArgs e )
		{
			( sender as TextBox ).Background = new SolidColorBrush ( Colors.LightGray );
			( sender as TextBox ).Foreground = new SolidColorBrush ( Colors.Black );
		}

		private void passwordBox_GotFocus ( object sender , RoutedEventArgs e )
		{
			passwordBox.Background = new SolidColorBrush ( Colors.LightGray );
			passwordBox.Foreground = new SolidColorBrush ( Colors.Black );
		}

		protected override void OnNavigatedFrom ( NavigationEventArgs e )
		{
			if ( UserData.settings.ContainKey ( AppSettings.keyDevice ) )
			{
				UserData.settings.Update ( AppSettings.keyDevice , textBoxDevice.Text.Trim () );
			}
			else
			{
				UserData.settings.Add ( AppSettings.keyDevice , textBoxDevice.Text.Trim () );
			}
		}
	}
}