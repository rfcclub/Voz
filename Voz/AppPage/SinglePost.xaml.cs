using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HtmlAgilityPack;
using System.Net.Http;
using Voz.Model;
using Voz.Helper;

namespace Voz.AppPage
{
	public partial class SinglePost : PhoneApplicationPage
	{
		public SinglePost ()
		{
			InitializeComponent ();
		}

		private HttpClient client;
		private HtmlDocument doc;
		private string postNumber;
		private string postCount;

		protected override void OnNavigatedTo ( NavigationEventArgs e )
		{
			SystemTray.ProgressIndicator = new ProgressIndicator ();
			SystemTray.ProgressIndicator.Text = "Đang tải...";

			postNumber = NavigationContext.QueryString["parameter1"];
			postCount = NavigationContext.QueryString["parameter2"];
		}

		private void PhoneApplicationPage_Loaded ( object sender , RoutedEventArgs e )
		{
			LoadPost ();
		}

		private async void LoadPost ()
		{
			ProgressIndicatorSwitch ( true );
			client = new HttpClient ();
			doc = new HtmlDocument ();
			string url = "https://vozforums.com/showpost.php?p=" + postNumber + "&postcount=" + postCount;
			//neu chua dang nhap
			if ( UserData.isLoggedIn == false )
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
				string result = await Login.GetResponseURL ( url );
				if ( result == "Error" )
				{
					MessageBox.Show ( "Server Voz đang bị lỗi, thử lại sau" );
				}
				else
				{
					doc.LoadHtml ( result );
				}
			}
			Helper.HAP.RemoveComment ( doc );
			GetPostContent ();
			ProgressIndicatorSwitch ( false );
		}

		private void GetPostContent ()
		{
			string postId = "post" + postNumber;
			HtmlNode t = doc.DocumentNode.SelectSingleNode ( "//table[@class='tborder voz-postbit' and @id='" + postId + "']" );

			Post p = new Post ();

			HAP.GetUserInfo ( t , p );
			HAP.GetPostInfo ( t , p );
			HAP.GetUserName ( t , p );
			HAP.GetUserAva ( t , p );
			HAP.GetQuoteAndReply ( t , p , doc );

			Display ( p );
		}

		private void Display ( Post p )
		{
			textBlock_PostTime.Text = p.postTime;
			textBlock_PostCount.Text = p.postOrder;
			textBlock_User.Text = p.userName;
			textBlock_JD.Text = p.userJoinDate;
			textBlock_Posts.Text = p.userPosts;
			textBlock_Location.Text = p.userLocation;
			htmlViewer.Html = @p.htmlContent;
			htmlViewer.Html = "";
			htmlViewer.Html = @p.htmlContent;
		}

		private void ProgressIndicatorSwitch ( bool value )
		{
			SystemTray.ProgressIndicator.IsIndeterminate = value;
			SystemTray.ProgressIndicator.IsVisible = value;
		}

		private void RefreshClick ( object sender , EventArgs e )
		{
			htmlViewer.Html = "";
			LoadPost ();
		}
	}
}