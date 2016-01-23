using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Voz.Model;
using Microsoft.Phone.Tasks;

namespace Voz.AppPage
{
    public partial class Homepage : PhoneApplicationPage
    {
        public Homepage ()
        {
            UserData.settings = new AppSettings ();
            UserData.isLoggedIn = false;
            UserData.cookies = UserData.settings.GetValueOrDefault ( AppSettings.keyCookie , "" );
            UserData.settings.Add ( AppSettings.keyIsRated , false );
            UserData.settings.Add ( AppSettings.keyOpenCount , "0" );
            CheckIsRated ();
            Helper.Themes.ChangeColor ();

            LoadBookmarkData ();

            InitializeComponent ();
        }

        private void LoadBookmarkData ()
        {
            UserData.BookmarkDC = new BookmarkDataContext ( BookmarkDataContext.DBConnectionString );
            var bookmarksInDB = from Bookmark b in UserData.BookmarkDC.Bookmarks select b;
            UserData.listBookmark = new List<Bookmark> ( bookmarksInDB );
        }

        private void CheckIsRated ()
        {
            if ( !UserData.settings.GetValueOrDefault ( AppSettings.keyIsRated , false ) )
            {
                int openCount = Int32.Parse ( UserData.settings.GetValueOrDefault ( AppSettings.keyOpenCount , "0" ) );
                openCount++;
                UserData.settings.Update ( AppSettings.keyOpenCount , openCount.ToString () );
                if ( openCount == 7 )
                {
                    MessageBoxResult mbr = MessageBox.Show ( "Cảm ơn bác đã sử dụng app.\nNếu có thể thì bác rate ủng hộ mình, để mình cải thiện app tốt hơn. :)\nNếu không muốn hiện thông báo này nữa thì bác chọn Cancel." , "Voz for Windows Phone" , MessageBoxButton.OKCancel );
                    if ( mbr == MessageBoxResult.OK )
                    {
                        UserData.settings.Update ( AppSettings.keyIsRated , true );
                        MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask ();
                        marketplaceReviewTask.Show ();
                    }
                    else
                    {
                        UserData.settings.Update ( AppSettings.keyIsRated , true );
                    }
                }
            }
        }

        private void Box_Tap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            string value = ( ( TextBlock ) sender ).Tag.ToString ();
            if ( value == "33" )
            {
                if ( UserData.isLoggedIn == false )
                    MessageBox.Show ( "Bạn chưa đăng nhập" );
                else
                    NavigationService.Navigate ( new Uri ( "/AppPage/Box.xaml?parameter=" + value , UriKind.Relative ) );
            }
            else
                NavigationService.Navigate ( new Uri ( "/AppPage/Box.xaml?parameter=" + value , UriKind.Relative ) );
        }

        private void RateClick ( object sender , EventArgs e )
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask ();
            marketplaceReviewTask.Show ();
        }

        private void AboutClick ( object sender , EventArgs e )
        {
            NavigationService.Navigate ( new Uri ( "/AppPage/About.xaml" , UriKind.Relative ) );
        }

        private void SettingClick ( object sender , EventArgs e )
        {
            NavigationService.Navigate ( new Uri ( "/AppPage/Setting.xaml" , UriKind.Relative ) );
        }

        private void ThreadID ( object sender , EventArgs e )
        {
            NavigationService.Navigate ( new Uri ( "/AppPage/PageThreadID.xaml" , UriKind.Relative ) );
        }

        private void ProgressIndicatorSwitch ( bool value )
        {
            SystemTray.ProgressIndicator.IsIndeterminate = value;
            SystemTray.ProgressIndicator.IsVisible = value;
        }

        private void BookmarkClick ( object sender , EventArgs e )
        {
            NavigationService.Navigate ( new Uri ( "/AppPage/BookmarkPage.xaml" , UriKind.Relative ) );
        }

        private async void AutoLogin ()
        {
            string loginResult = "";
            try
            {
                SystemTray.ProgressIndicator.Text = "Đăng nhập...";
                ProgressIndicatorSwitch ( true );
                UserData.account = UserData.settings.GetValueOrDefault ( AppSettings.keyAccount , "" );
                UserData.password = UserData.settings.GetValueOrDefault ( AppSettings.keyPassword , "" );
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
                    if ( UserData.settings.ContainKey ( AppSettings.keyCookie ) )
                    {
                        UserData.settings.Update ( AppSettings.keyCookie , UserData.cookies );
                    }
                    else
                    {
                        UserData.settings.Add ( AppSettings.keyCookie , UserData.cookies );
                    }
                    ProgressIndicatorSwitch ( false );
                    UserData.isLoggedIn = true;
                }
            }
            catch ( Exception ex )
            {
                ProgressIndicatorSwitch ( false );
                MessageBox.Show ( ex.Message );
            }
        }

        private void PhoneApplicationPage_Loaded ( object sender , RoutedEventArgs e )
        {
            SystemTray.ProgressIndicator = new ProgressIndicator ();
            if ( UserData.isLoggedIn == false )
                if ( UserData.settings.ContainKey ( AppSettings.keyAccount ) )
                    AutoLogin ();
        }
    }
}