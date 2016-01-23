using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Net.Http;
using HtmlAgilityPack;
using System.Windows.Input;
using System.Windows.Media;
using Voz.Helper;
using Voz.Model;
using Microsoft.Phone.Shell;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Voz.AppPage
{
    public partial class Thread : PhoneApplicationPage
    {
        public Thread ()
        {
            InitializeComponent ();
            timer = new DispatcherTimer ();
            timer.Interval = TimeSpan.FromSeconds ( 1 );
            timer.Tick += new EventHandler ( TimerTick );
        }
        private void TimerTick ( object sender , EventArgs e )
        {
            timer.Stop ();
            scrollViewerMain.IsEnabled = true;
            ProgressIndicatorSwitch ( false );
        }

        private string threadId;
        private string threadTitle;
        private int currentPage;
        private int lastPage;
        private HttpClient client;
        private HtmlDocument doc;
        private List<Post> listPosts;
        DispatcherTimer timer;

        protected override void OnNavigatedTo ( NavigationEventArgs e )
        {
            SystemTray.ProgressIndicator = new ProgressIndicator ();
            SystemTray.ProgressIndicator.Text = "Đang tải...";
            if ( e.NavigationMode != NavigationMode.Back )
            {
                threadId = NavigationContext.QueryString["parameter1"];
                if ( NavigationContext.QueryString.ContainsKey ( "parameter2" ) )
                {
                    currentPage = Int32.Parse ( NavigationContext.QueryString["parameter2"].Remove ( 0 , 5 ) );
                }
                else
                {
                    currentPage = 1;
                }
                LoadThread ();
            }
            else if ( State.ContainsKey ( "LastPageView" ) )
            {
                currentPage = Int32.Parse ( ( string ) State["LastPageView"] );
                State.Remove ( "LastPageView" );
                LoadThread ();
            }
            else if ( State.ContainsKey ( "ThreadId" ) )
            {
                threadId = ( string ) State["ThreadId"];
                State.Remove ( "ThreadId" );
                currentPage = Int32.Parse ( ( string ) State["ThreadPage"] );
                State.Remove ( "ThreadPage" );
            }
        }

        protected override void OnNavigatedFrom ( System.Windows.Navigation.NavigationEventArgs e )
        {
            //save thread id when navigate away
            if ( e.NavigationMode != NavigationMode.Back )
            {
                State["ThreadId"] = threadId;
                State["ThreadPage"] = currentPage.ToString ();
            }
        }

        private async void LoadThread ()
        {
            scrollViewerMain.IsEnabled = false;
            ProgressIndicatorSwitch ( true );

            string url = "";
            if ( currentPage == 1 )
                url = "https://vozforums.com/showthread.php?t=" + threadId;
            else
                url = "https://vozforums.com/showthread.php?t=" + threadId + "&page=" + currentPage.ToString ();

            if ( PhoneApplicationService.Current.State.ContainsKey ( "ToNewPost" ) )
            {
                url = "https://vozforums.com/" + PhoneApplicationService.Current.State["ToNewPost"];
            }

            client = new HttpClient ();
            doc = new HtmlDocument ();
            listPosts = new List<Post> ();

            //neu chua dang nhap
            if ( UserData.isLoggedIn == false )
            {
                try
                {
                    doc.LoadHtml ( await client.GetStringAsync ( url ) );
                }
                catch ( Exception ex )
                {
                    ProgressIndicatorSwitch ( false );
                    MessageBox.Show ( "Lỗi server Voz: " + ex.Message );
                    Application.Current.Terminate ();
                }
            }
            //neu da dang nhap
            else
            {
                string result = "";
                try
                {
                    result = await Login.GetResponseURL ( url );
                }
                catch ( Exception ex )
                {
                    ProgressIndicatorSwitch ( false );
                    MessageBox.Show ( "Lỗi server Voz: " + ex.Message );
                    Application.Current.Terminate ();
                }
                if ( result == "Error" )
                {
                    ProgressIndicatorSwitch ( false );
                    MessageBox.Show ( "Server Voz đang bị lỗi, thử lại sau" );
                }
                else
                {
                    doc.LoadHtml ( result );
                }
            }
            ProgressIndicatorSwitch ( false );
            Helper.HAP.RemoveComment ( doc );
            if ( PhoneApplicationService.Current.State.ContainsKey ( "ToNewPost" ) )
            {
                PhoneApplicationService.Current.State.Remove ( "ToNewPost" );
                GetCurrentPage ();
            }
            GetTitle ();
            GetMaxPage ();
            GetAllPost ();

            timer.Start ();
        }

        private void GetCurrentPage ()
        {
            HtmlNode t = doc.DocumentNode.SelectSingleNode ( "//td[@class='vbmenu_control' and @style='font-weight:normal']" );
            if ( t != null )
            {
                currentPage = Int32.Parse ( t.InnerText.Split ( ' ' )[1] );
            }
            else
            {
                currentPage = 1;
            }
        }

        private void GetAllPost ()
        {
            listPosts.Clear ();

            //get all post, each post is a table
            HtmlNodeCollection table =
                doc.DocumentNode.SelectSingleNode ( "//div[@id='posts']" ).SelectNodes ( ".//table[@class='tborder voz-postbit']" );
            foreach ( HtmlNode t in table )
            {
                Post p = new Post ();

                HAP.GetUserInfo ( t , p );
                HAP.GetPostInfo ( t , p );
                HAP.GetUserName ( t , p );
                HAP.GetUserAva ( t , p );
                HAP.GetQuoteAndReply ( t , p , doc );

                listPosts.Add ( p );
            }
            //set binding
            listBoxPosts.ItemsSource = null;
            listBoxPosts.ItemsSource = listPosts;
        }

        private void GetMaxPage ()
        {
            string mp = "";
            HtmlNode t = doc.DocumentNode.SelectSingleNode ( "//td[@class='vbmenu_control' and @style='font-weight:normal']" );
            if ( t != null )
            {
                mp = t.InnerText;
                lastPage = Int32.Parse ( mp.Split ( ' ' ).Last () );
                textBoxCurrentPage.Text = currentPage.ToString () + "/" + lastPage.ToString ();
                textBoxCurrentPageBottom.Text = currentPage.ToString () + "/" + lastPage.ToString ();
            }
            else
            {
                lastPage = 1;
                textBoxCurrentPage.Text = currentPage.ToString () + "/" + lastPage.ToString ();
                textBoxCurrentPageBottom.Text = currentPage.ToString () + "/" + lastPage.ToString ();
            }
        }

        private void GetTitle ()
        {
            threadTitle = HtmlEntity.DeEntitize ( doc.DocumentNode.SelectSingleNode ( "//td[@class='navbar']" ).InnerText.Trim () );
            textBlockTitle.Text = threadTitle;
        }

        private void ScrollToTop ()
        {
            scrollViewerMain.ScrollToVerticalOffset ( 0 );
        }

        private void ReplyClick ( object sender , EventArgs e )
        {
            if ( UserData.isLoggedIn == false )
            {
                MessageBoxResult result =
                    MessageBox.Show
                    ( "Bác chưa đăng nhập, bác có muốn đăng nhập ko?" , "Lỗi chưa đăng nhập" , MessageBoxButton.OKCancel );
                if ( result == MessageBoxResult.OK )
                {
                    NavigationService.Navigate ( new Uri ( "/AppPage/Setting.xaml" , UriKind.Relative ) );
                }
            }
            else
            {
                if ( currentPage != 1 )
                {
                    State["LastPageView"] = currentPage.ToString ();
                }
                NavigationService.Navigate ( new Uri ( "/AppPage/Reply.xaml?parameter=" + threadId , UriKind.Relative ) );
            }
        }

        private void ProgressIndicatorSwitch ( bool value )
        {
            SystemTray.ProgressIndicator.IsIndeterminate = value;
            SystemTray.ProgressIndicator.IsVisible = value;
        }

        private void QuoteTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            string quotePostid = ( ( Button ) sender ).Tag.ToString ();
            PhoneApplicationService.Current.State["QuotePostId"] = quotePostid;
            if ( currentPage != 1 )
            {
                State["LastPageView"] = currentPage.ToString ();
            }

            if ( UserData.isLoggedIn == false )
            {
                MessageBoxResult result =
                    MessageBox.Show
                    ( "Bác chưa đăng nhập, bác có muốn đăng nhập ko?" , "Lỗi chưa đăng nhập" , MessageBoxButton.OKCancel );
                if ( result == MessageBoxResult.OK )
                {
                    NavigationService.Navigate ( new Uri ( "/AppPage/Settings.xaml" , UriKind.Relative ) );
                }
            }
            else
            {
                NavigationService.Navigate ( new Uri ( "/AppPage/Reply.xaml?parameter=" + threadId , UriKind.Relative ) );
            }
        }

        private void FirstPageTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            if ( currentPage != 1 )
            {
                currentPage = 1;
                LoadThread ();
                ScrollToTop ();
            }
        }

        private void PreviousPageTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            if ( currentPage > 1 )
            {
                currentPage--;
                LoadThread ();
                ScrollToTop ();
            }
        }

        private void textBoxPageGotFocus ( object sender , RoutedEventArgs e )
        {
            ( ( TextBox ) sender ).Text = "";
            ( ( TextBox ) sender ).Foreground = new SolidColorBrush ( Colors.Black );
        }

        private void textBoxPageLostFocus ( object sender , RoutedEventArgs e )
        {
            ( ( TextBox ) sender ).Text = currentPage.ToString () + "/" + lastPage.ToString ();
            ( ( TextBox ) sender ).Foreground = App.Current.Resources["TextColor"] as SolidColorBrush;
        }

        private void NextPageTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            if ( currentPage < lastPage )
            {
                currentPage++;
                LoadThread ();
                ScrollToTop ();
            }
        }

        private void LastPageTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            if ( currentPage != lastPage )
            {
                currentPage = lastPage;
                LoadThread ();
                ScrollToTop ();
            }
        }

        private void MovePage ( object sender , KeyEventArgs e )
        {
            if ( e.Key == Key.Enter )
            {
                int n;
                bool result = Int32.TryParse ( ( ( TextBox ) sender ).Text , out n );
                if ( result && n <= lastPage && n > 0 && n != currentPage )
                {
                    currentPage = n;
                    ( ( TextBox ) sender ).Foreground = new SolidColorBrush ( Colors.White );
                    LoadThread ();
                    ScrollToTop ();
                    this.Focus ();
                }
                else
                {
                    ( ( TextBox ) sender ).Text = "";
                }
            }
        }

        private void RefreshClick ( object sender , EventArgs e )
        {
            LoadThread ();
        }

        private void AddBookmarkClick ( object sender , EventArgs e )
        {
            SystemTray.ProgressIndicator.Text = "Bookmarked";
            ProgressIndicatorSwitch ( true );
            if ( UserData.listBookmark.Exists ( b => b.threadBmId == threadId ) )
            {
                Bookmark existBookmark = ( from b in UserData.BookmarkDC.Bookmarks
                                           where b.threadBmId == threadId
                                           select b ).First ();
                existBookmark.threadBmPage = currentPage;
            }
            else
            {
                Bookmark newBookmark = new Bookmark
                {
                    threadBmId = threadId ,
                    threadBmTitle = threadTitle ,
                    threadBmPage = currentPage
                };
                UserData.listBookmark.Add ( newBookmark );
                UserData.BookmarkDC.Bookmarks.InsertOnSubmit ( newBookmark );
            }
            UserData.BookmarkDC.SubmitChanges ();
            timer.Start ();
        }

        private void BookmarkClick ( object sender , EventArgs e )
        {
            NavigationService.Navigate ( new Uri ( "/AppPage/BookmarkPage.xaml" , UriKind.Relative ) );
        }

        private void HyperLinkClicked ( object sender , MSPToolkit.Controls.HyperlinkClickEventArgs e )
        {
            string url = e.NavigationUri.AbsoluteUri;
            //https://vozforums.com/showpost.php?p=70734414&postcount=25
            //http://vozforums.com/showthread.php?t=1703338
            if ( ( url.StartsWith ( "http://vozforums.com/showpost.php?p=" ) ||
                url.StartsWith ( "https://vozforums.com/showpost.php?p=" ) )
                && url.Contains ( '&' ) )
            {
                int equalPos = url.IndexOf ( '=' );
                int andPos = url.IndexOf ( '&' );
                string postId = url.Substring ( equalPos + 1 , andPos - equalPos - 1 );
                string postCount = url.Substring ( andPos + 10 + 1 );
                NavigationService.Navigate
                    (
                    new Uri ( "/AppPage/SinglePost.xaml?parameter1=" + postId + "&parameter2=" + postCount , UriKind.Relative )
                    );
            }
            else if ( url.StartsWith ( "http://vozforums.com/showthread.php?t=" ) ||
                url.StartsWith ( "https://vozforums.com/showthread.php?t=" ) )
            {
                int equalPos = url.IndexOf ( '=' );
                if ( url.Contains ( '&' ) )
                {
                    int andPos = url.IndexOf ( '&' );
                    string threadId = url.Substring ( equalPos + 1 , andPos - equalPos - 1 );
                    string page = url.Substring ( andPos + 1 );
                    NavigationService.Navigate
                    (
                        new Uri ( "/AppPage/Thread.xaml?parameter1=" + threadId + "&parameter2=" + page , UriKind.Relative )
                    );
                }
                else
                {
                    string threadId = url.Substring ( equalPos + 1 );
                    NavigationService.Navigate
                    (
                    new Uri ( "/AppPage/Thread.xaml?parameter1=" + threadId , UriKind.Relative )
                    );
                }
            }
            else
            {
                Microsoft.Phone.Tasks.WebBrowserTask task = new Microsoft.Phone.Tasks.WebBrowserTask ();
                task.Uri = e.NavigationUri;
                task.Show ();
            }
        }
    }
}