using Microsoft.Phone.Shell;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Voz.Helper
{
	class Utilities
	{
		public static string GetVersion ()
		{
			string s = XDocument.Load ( "WMAppManifest.xml" ).Root.Element ( "App" ).Attribute ( "Version" ).Value;
			return s;
		}

		public static async Task<bool> CheckServer ()
		{
			HttpClient client = new HttpClient ();
			string testUrl = "https://vozforums.com/forumdisplay.php?f=31";
			var response = await client.GetAsync ( testUrl );
			if ( response.IsSuccessStatusCode )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
