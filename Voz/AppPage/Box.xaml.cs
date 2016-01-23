using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Net.Http;
using HtmlAgilityPack;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Voz.AppPage
{
    public partial class Box : PhoneApplicationPage
    {
        public Box ()
        {
            Helper.Themes.ChangeColor ();
            InitializeComponent ();
            currentPage = 1;
        }

        string boxId;
        string boxTitle;
        private int currentPage;
        private int lastPage;
        private string url;
        private HttpClient client;
        private HtmlDocument doc;
        private List<Model.Thread> listThreads;

        private async void LoadBox ()
        {
            listBoxTopics.IsEnabled = false;
            ProgressIndicatorSwitch ( true );
            url = "";
            if ( currentPage == 1 )
                url = "https://vozforums.com/forumdisplay.php?f=" + boxId;
            else
                url = "https://vozforums.com/forumdisplay.php?f=" + boxId + "&order=desc&page=" + currentPage;
            client = new HttpClient ();
            doc = new HtmlDocument ();

            //neu chua dang nhap
            if ( Model.UserData.isLoggedIn == false )
            {
                try
                {
                    doc.LoadHtml ( await client.GetStringAsync ( url ) );
                }
                catch ( Exception ex )
                {
                    MessageBox.Show ( "Lỗi server Voz: " + ex.Message );
                    Application.Current.Terminate ();
                }
            }
            //neu da dang nhap
            else
            {
                string responseresult = await Model.Login.GetResponseURL ( url );
                if ( responseresult == "Error" )
                {
                    MessageBox.Show ( "Server Voz đang bị lỗi, thử lại sau" );
                }
                else
                {
                    doc.LoadHtml ( responseresult );
                }
            }
            
            Helper.HAP.RemoveComment ( doc );

            GetBoxTitle ();
            GetMaxPage ();
            GetListThreads ();

            listBoxTopics.ItemsSource = listThreads;
            listBoxTopics.IsEnabled = true;
            ProgressIndicatorSwitch ( false );
        }

        private void GetListThreads ()
        {
            listThreads = new List<Model.Thread> ();

            HtmlNodeCollection listThreadNode = doc.DocumentNode
                .SelectSingleNode ( "//tbody[@id='threadbits_forum_" + boxId + "']" ).SelectNodes ( "./tr" );
            HtmlNodeCollection nodesTd, nodeDiv;
            HtmlNode node_second_td, node_id, node_replies, node_view, node_lastpost;

            foreach ( HtmlNode nodeSingleThread in listThreadNode )
            {
                //get all <td> (total 5)
                nodesTd = nodeSingleThread.SelectNodes ( "./td" );
                if ( nodesTd[1].Attributes["class"].Value == "alt2" )
                    nodesTd.RemoveAt ( 1 );
                if ( nodesTd.Count == 5 )
                {
                    Model.Thread thread = new Model.Thread ();

                    //1st node contain id
                    node_id = nodesTd[0];
                    thread.id = node_id.Attributes["id"].Value.Remove ( 0 , 20 );

                    //2nd node contain title and creating user and first unread
                    node_second_td = nodesTd[1];
                    nodeDiv = node_second_td.SelectNodes ( "./div" );
                    HtmlNodeCollection nodesInFirstDiv = nodeDiv[0].SelectNodes ( "./a" );
                    foreach ( HtmlNode a in nodesInFirstDiv )
                    {
                        thread.title += a.InnerText + " ";
                    }
                    thread.title = HtmlEntity.DeEntitize ( thread.title );
                    thread.creatingUser = HtmlEntity.DeEntitize ( nodeDiv[1].InnerText.Trim () );

                    //3rd node contain last post info
                    node_lastpost = nodesTd[2];
                    thread.lastPost = HtmlEntity.DeEntitize ( node_lastpost.InnerText.Trim () );
                    string[] s = thread.lastPost.Split ();
                    thread.lastPost = string.Join ( " " , s );

                    //4th contain replies
                    node_replies = nodesTd[3];
                    thread.replies = node_replies.InnerText;

                    //5th contain view
                    node_view = nodesTd[4];
                    thread.views = node_view.InnerText;

                    //get new post link
                    HtmlNode newPostNode = node_second_td.SelectSingleNode ( ".//a[@id='thread_gotonew_" + thread.id + "']" );
                    if ( newPostNode != null )
                    {
                        thread.newPost = HtmlEntity.DeEntitize ( newPostNode.Attributes["href"].Value );
                        thread.title = "[NEW] " + thread.title;
                    }
                    else
                    {
                        thread.newPost = "";
                    }

                    //last page
                    HtmlNode span = node_second_td.SelectSingleNode ( ".//span[@class='smallfont']" );
                    if ( span != null )
                    {
                        HtmlNodeCollection pages = span.SelectNodes ( ".//a" );
                        thread.lastPage = HtmlEntity.DeEntitize ( ( ( HtmlNode ) pages.Last () ).Attributes["href"].Value );
                        thread.lastPage = thread.lastPage.Split ( new char[] { '&' } )[1];
                    }
                    else
                    {
                        thread.lastPage = "";
                    }
                    listThreads.Add ( thread );
                }
            }
        }

        private void ScrollToTop ()
        {
            scrollViewer_Main.ScrollToVerticalOffset ( 0 );
        }

        private void ProgressIndicatorSwitch ( bool value )
        {
            SystemTray.ProgressIndicator.IsIndeterminate = value;
            SystemTray.ProgressIndicator.IsVisible = value;
        }

        protected override void OnNavigatedTo ( NavigationEventArgs e )
        {
            //if user back from thread page, then do not reload, else load the box
            SystemTray.ProgressIndicator = new ProgressIndicator ();
            SystemTray.ProgressIndicator.Text = "Đang tải...";
            if ( e.NavigationMode != NavigationMode.Back )
            {
                string parameter = string.Empty;
                if ( NavigationContext.QueryString.TryGetValue ( "parameter" , out parameter ) )
                {
                    boxId = parameter;
                }
                LoadBox ();
            }
            else if ( State.ContainsKey ( "BoxId" ) )
            {
                boxId = ( string ) State["BoxId"];
                State.Remove ( "BoxId" );
            }
        }

        protected override void OnNavigatedFrom ( System.Windows.Navigation.NavigationEventArgs e )
        {
            //save box id when navigate away
            if ( e.NavigationMode != NavigationMode.Back )
                State["BoxId"] = boxId;
        }

        private void GetBoxTitle ()
        {
            boxTitle = HtmlEntity.DeEntitize ( doc.DocumentNode.SelectSingleNode ( "//td[@class='navbar']" ).InnerText.Trim () );
            textBlockBoxTitle.Text = boxTitle;
        }

        private void GetMaxPage ()
        {
            string mp;
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

        private void FirstPageTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            if ( currentPage != 1 )
            {
                currentPage = 1;
                LoadBox ();
                ScrollToTop ();
            }
        }

        private void PreviousPageTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            if ( currentPage > 1 )
            {
                currentPage--;
                LoadBox ();
                ScrollToTop ();
            }
        }

        private void NextPageTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            if ( currentPage < lastPage )
            {
                currentPage++;
                LoadBox ();
                ScrollToTop ();
            }
        }

        private void LastPageTap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            if ( currentPage != lastPage )
            {
                currentPage = lastPage;
                LoadBox ();
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
                    LoadBox ();
                    ScrollToTop ();
                    this.Focus ();
                }
                else
                {
                    ( ( TextBox ) sender ).Text = "";
                }
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

        private void textBlockBoxTitle_Tap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            Refresh ();
        }

        private void Refresh ()
        {
            LoadBox ();
        }

        private void textBlockTitle_Tap ( object sender , System.Windows.Input.GestureEventArgs e )
        {
            string value = ( ( TextBlock ) sender ).Tag.ToString ();
            NavigationService.Navigate ( new Uri ( "/AppPage/Thread.xaml?parameter1=" + value , UriKind.Relative ) );
        }

        private void ThreadLastPageClick ( object sender , RoutedEventArgs e )
        {
            string lastpage = ( ( MenuItem ) sender ).Tag.ToString ();
            string id = ( ( ( sender as MenuItem ).Parent as ContextMenu ).Owner as TextBlock ).Tag.ToString ();
            NavigationService.Navigate ( new Uri ( "/AppPage/Thread.xaml?parameter1=" + id + "&parameter2=" + lastpage , UriKind.Relative ) );
        }

        private void ThreadNewPostTap ( object sender , RoutedEventArgs e )
        {
            string newPost = ( ( MenuItem ) sender ).Tag.ToString ();
            if ( newPost != "" )
            {
                string id = ( ( ( sender as MenuItem ).Parent as ContextMenu ).Owner as TextBlock ).Tag.ToString ();
                PhoneApplicationService.Current.State["ToNewPost"] = newPost;
                NavigationService.Navigate ( new Uri ( "/AppPage/Thread.xaml?parameter1=" + id , UriKind.Relative ) );
            }
        }

        private void RefreshClick ( object sender , EventArgs e )
        {
            Refresh ();
        }

        private void BookmarkClick ( object sender , EventArgs e )
        {
            NavigationService.Navigate ( new Uri ( "/AppPage/BookmarkPage.xaml" , UriKind.Relative ) );
        }
    }
}