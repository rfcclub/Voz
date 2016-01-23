using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using Voz.Model;
using System.Text;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;
using Voz.Helper;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Voz.AppPage
{
	public partial class Reply : PhoneApplicationPage
	{
		public Reply ()
		{
			InitializeComponent ();
			textBox_Reply.Text = "";
		}

		public static string threadId;
		private static string posturl = "https://vozforums.com/newreply.php?do=postreply&t=";
		private static string userAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:25.1) Gecko/20141110 Firefox/31.9 PaleMoon/25.1.0";
		private static string sign;
		DispatcherTimer timer;

		private async void SendClick ( object sender , EventArgs e )
		{
			if ( textBox_Reply.Text.Length <= 10 )
			{
				MessageBox.Show ( "Reply phải dài hơn 10 kí tự" );
			}
			else
			{
				string result = "";
				bool hasSign = checkBox_Sign.IsChecked.HasValue ? checkBox_Sign.IsChecked.Value : false;
				SystemTray.ProgressIndicator.Text = "Đang gửi";
				ProgressIndicatorSwitch ( true );

				//check server
				bool canConnect = await Helper.Utilities.CheckServer ();
				if ( !canConnect )
				{
					ProgressIndicatorSwitch ( false );
					MessageBox.Show ( "Server Voz bị lỗi, thử lại sau" );
				}

				result = await PostReply ( textBox_Reply.Text , hasSign );
				ProgressIndicatorSwitch ( false );
				if ( result == "Server" )
				{
					MessageBox.Show ( "Server Voz bị lỗi, thử lại sau" );
				}
				else if ( result == "Deleted" )
				{
					MessageBox.Show ( "Thread đã bị xóa" );
				}
				else if ( result == "Closed" )
				{
					MessageBox.Show ( "Thread đã bị khóa" );
				}
				else if ( result == "20" )
				{
					MessageBox.Show ( "Ko đủ 20 post" );
				}
				else if ( result == "Yes" )
				{
					SystemTray.ProgressIndicator.Text = "Gửi thành công";
					ProgressIndicatorSwitch ( true );
					timer.Start ();
				}
				else
				{
					MessageBox.Show ( "Gửi thất bại" );
				}
			}
		}

		private void TimerTick ( object sender , EventArgs e )
		{
			ProgressIndicatorSwitch ( false );
			timer.Stop ();
			NavigationService.GoBack ();
		}

		public async Task<string> PostReply ( string message , bool hasSign )
		{
			string sourceMes = message;
			if ( hasSign )
			{
				message = message + Environment.NewLine + Environment.NewLine + sign;
			}
			message = HttpUtility.UrlEncode ( message );
			string data = "message=" + message
							+ "&wysiwyg=0&styleid=0&fromquickreply=1&s="
							+ "&securitytoken=" + UserData.token
							+ "&do=postreply&t=" + threadId
							+ "&p=who cares&specifiedpost=0&parseurl=1&loggedinuser=" + UserData.id
							+ "&sbutton=Post Quick Reply";

			HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create ( posturl + threadId );
			request.UserAgent = userAgent;
			request.Method = "POST";
			request.ContentType = @"application/x-www-form-urlencoded";
			byte[] postData = Encoding.UTF8.GetBytes ( data );
			request.ContentLength = data.Length;
			request.Headers["Cookie"] = UserData.cookies;
			request.CookieContainer = new CookieContainer ();

			using ( Stream writer = await Task.Factory.FromAsync<Stream> ( request.BeginGetRequestStream , request.EndGetRequestStream , request ) )
			{
				writer.Write ( postData , 0 , postData.Length );
				writer.Close ();
			}

			HttpWebResponse response = ( HttpWebResponse ) await request.GetResponseAsync ();

			if ( ( int ) response.StatusCode < 200 || ( int ) response.StatusCode > 299 )
			{
				return "Server";
			}

			Stream responseStream = response.GetResponseStream ();
			StreamReader reader = new StreamReader ( responseStream , Encoding.UTF8 );
			string result = reader.ReadToEnd ();

			if ( result.Contains ( "To be able to post links or images" ) )
				return "20";
			if ( result.Contains ( "Invalid Thread specified" ) )
				return "Deleted";
			if ( result.Contains ( "This thread is closed" ) )
				return "Closed";
			if ( response.StatusCode == HttpStatusCode.OK )
				return "Yes";
			return "No";
		}

		private void PhoneApplicationPage_Loaded ( object sender , RoutedEventArgs e )
		{
			string parameter = string.Empty;
			if ( NavigationContext.QueryString.TryGetValue ( "parameter" , out parameter ) )
			{
				threadId = parameter;
			}
			else
			{
				threadId = "";
			}
			string device = UserData.settings.GetValueOrDefault ( AppSettings.keyDevice , "my Windows Phone" );
			if ( device == "" )
				device = "my Windows Phone";
			sign = UserData.defaultSignature.Replace ( "<devicename>" , device );
			timer = new DispatcherTimer ();
			timer.Interval = TimeSpan.FromSeconds ( 1 );
			timer.Tick += new EventHandler ( TimerTick );
		}

		protected async override void OnNavigatedTo ( System.Windows.Navigation.NavigationEventArgs e )
		{
			base.OnNavigatedTo ( e );
			SystemTray.ProgressIndicator = new ProgressIndicator ();

			if ( PhoneApplicationService.Current.State.ContainsKey ( "Emo" ) )
			{
				textBox_Reply.SelectedText = ( string ) PhoneApplicationService.Current.State["Emo"];
				PhoneApplicationService.Current.State.Remove ( "Emo" );
			}
			if ( PhoneApplicationService.Current.State.ContainsKey ( "EditColor" ) )
			{
				textBox_Reply.SelectedText =
					"[COLOR=\"" + ( string ) PhoneApplicationService.Current.State["EditColor"] + "\"]" + textBox_Reply.SelectedText + "[/COLOR]";
				PhoneApplicationService.Current.State.Remove ( "EditColor" );
			}
			if ( PhoneApplicationService.Current.State.ContainsKey ( "QuotePostId" ) )
			{
				string postId = ( string ) PhoneApplicationService.Current.State["QuotePostId"];
				PhoneApplicationService.Current.State.Remove ( "QuotePostId" );
				SystemTray.ProgressIndicator.Text = "Load quote...";
				ProgressIndicatorSwitch ( true );
				textBox_Reply.SelectedText = await LoadQuote ( postId );
				ProgressIndicatorSwitch ( false );
			}
		}

		private void ProgressIndicatorSwitch ( bool value )
		{
			SystemTray.ProgressIndicator.IsIndeterminate = value;
			SystemTray.ProgressIndicator.IsVisible = value;
		}

		private async Task<string> LoadQuote ( string postId )
		{
			string quoteResult = "";
			HttpClient client = new HttpClient ();
			HtmlDocument doc = new HtmlDocument ();
			string url = "https://vozforums.com/newreply.php?do=newreply&p=" + postId;
			string result = await Login.GetResponseURL ( url );
			if ( result == "Error" )
			{
				MessageBox.Show ( "Server Voz đang bị lỗi, thử lại sau" );
			}
			else
			{
				doc.LoadHtml ( result );
			}
			HtmlNode textareaNode = doc.DocumentNode.SelectSingleNode ( "//textarea" );
			if ( textareaNode != null )
			{
				quoteResult = textareaNode.InnerText.Trim ();
				quoteResult = HtmlAgilityPack.HtmlEntity.DeEntitize ( quoteResult );
				return quoteResult + Environment.NewLine;
			}
			else return "";
		}

		private void EmoClick ( object sender , EventArgs e )
		{
			NavigationService.Navigate ( new Uri ( "/AppPage/Emo.xaml" , UriKind.Relative ) );
		}

		private void BoldTap ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			textBox_Reply.SelectedText = "[B]" + textBox_Reply.SelectedText + "[/B]";
		}

		private void ItalicTap ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			textBox_Reply.SelectedText = "[I]" + textBox_Reply.SelectedText + "[/I]";
		}

		private void UnderlineTap ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			textBox_Reply.SelectedText = "[U]" + textBox_Reply.SelectedText + "[/U]";
		}

		private void ColorTap ( object sender , System.Windows.Input.GestureEventArgs e )
		{
			NavigationService.Navigate ( new Uri ( "/SimpleColorPicker.xaml" , UriKind.Relative ) );
		}
	}
}