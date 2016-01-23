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

namespace Voz.AppPage
{
	public partial class BookmarkPage : PhoneApplicationPage
	{
		public BookmarkPage ()
		{
			InitializeComponent ();
		}

		protected override void OnNavigatedTo ( NavigationEventArgs e )
		{
			listBoxBookmarks.ItemsSource = UserData.listBookmark;
			base.OnNavigatedTo ( e );
		}

		protected override void OnNavigatedFrom ( NavigationEventArgs e )
		{
			//UserData.BookmarkDC.SubmitChanges ();
			base.OnNavigatedFrom ( e );
		}

		private void BookmarkFirstPage ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			string threadId = ( ( TextBlock ) sender ).Tag.ToString ();
			NavigationService.Navigate ( new Uri ( "/AppPage/Thread.xaml?parameter1=" + threadId , UriKind.Relative ) );
		}

		private void BookmarkCustomPage ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			string threadId = ( ( TextBlock ) sender ).Tag.ToString ();
			string page = "page=" + UserData.listBookmark.First ( o => o.threadBmId == threadId ).threadBmPage.ToString ();
			NavigationService.Navigate ( new Uri ( "/AppPage/Thread.xaml?parameter1=" + threadId + "&parameter2=" + page , UriKind.Relative ) );
		}

		private void DeleteBookmark ( object sender , RoutedEventArgs e )
		{
			string threadId = ( ( MenuItem ) sender ).Tag.ToString ();
			Bookmark bm = ( from b in UserData.BookmarkDC.Bookmarks
							where b.threadBmId == threadId
							select b ).First ();
			UserData.listBookmark.Remove ( bm );
			UserData.BookmarkDC.Bookmarks.DeleteOnSubmit ( bm );
			UserData.BookmarkDC.SubmitChanges ();
			listBoxBookmarks.ItemsSource = null;
			listBoxBookmarks.ItemsSource = UserData.listBookmark;
		}
	}
}