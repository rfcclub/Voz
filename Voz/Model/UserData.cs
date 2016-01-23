using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voz.Model
{
	public class UserData
	{
		public static string token { get; set; }
		public static string cookies { get; set; }
		public static string id { get; set; }
		public static string account { get; set; }
		public static string password { get; set; }
		public static string defaultSignature = "[I]Sent from <devicename> using [URL=\"http://www.windowsphone.com/vi-vn/store/app/voz-for-windows-phone/04e525ea-6190-456d-a5bd-cd91ea1a3473\"]Voz for Windows Phone[/URL][/I]";
		public static bool isLoggedIn { get; set; }
		public static AppSettings settings;
		public static BookmarkDataContext BookmarkDC;
		public static List<Bookmark> listBookmark;
	}
}
