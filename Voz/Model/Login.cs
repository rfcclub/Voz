using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Voz.Model
{
    class Login
    {
        private static string loginUrl = "https://vozforums.com/login.php?do=login";
        private static string homepageUrl = "https://vozforums.com/";
        //private static string homepageUrl = "https://vozforums.com/forumdisplay.php?f=31";
        private static string userAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:25.1) Gecko/20141110 Firefox/31.9 PaleMoon/25.1.0";

        public static async Task<string> LoginAndGetCookie ( string account , string password )
        {
            string postDataString = "vb_login_username=" + account + "&vb_login_password=" + password
                                + "&securitytoken=guest&"
                                + "cookieuser=checked&"
                                + "do=login";
            //create request
            HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create ( loginUrl );
            request.UserAgent = userAgent;
            request.Method = "POST";
            request.AllowAutoRedirect = false;
            request.ContentType = @"application/x-www-form-urlencoded";
            byte[] postData = Encoding.UTF8.GetBytes ( postDataString );
            request.ContentLength = postData.Length;
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
            //extract cookie
            string returnCookie = response.Headers["Set-Cookie"];
            string fhash = "";
            if ( returnCookie.Contains ( "vfsessionhash" ) )
            {
                int startIndex = returnCookie.IndexOf ( "vfsessionhash" ) + 14;
                int stopIndex = startIndex;
                for ( int i = startIndex ; i < returnCookie.Count () ; i++ )
                {
                    if ( returnCookie[i] == ';' )
                    {
                        fhash = returnCookie.Substring ( startIndex , i - startIndex );
                        break;
                    }
                }
            }
            //save cookie
            UserData.cookies = "vfsessionhash=" + fhash + ";";
            string r = await SendRequestToHomepage ();
            return r;
        }

        public static async Task<string> SendRequestToHomepage ()
        {
            //create request
            HttpWebRequest homepageRequest = ( HttpWebRequest ) WebRequest.Create ( homepageUrl );
            homepageRequest.UserAgent = userAgent;
            //add cookie to request
            homepageRequest.Headers["Cookie"] = UserData.cookies;
            homepageRequest.CookieContainer = new CookieContainer ();

            HttpWebResponse homePageResponse = ( HttpWebResponse ) await homepageRequest.GetResponseAsync ();

            if ( ( int ) homePageResponse.StatusCode < 200 || ( int ) homePageResponse.StatusCode > 299 )
            {
                return "Server";
            }

            Stream homePageResponseStream = homePageResponse.GetResponseStream ();
            StreamReader reader = new StreamReader ( homePageResponseStream , Encoding.UTF8 );
            string result = reader.ReadToEnd ();
            //extract token
            UserData.token = Regex.Match ( result , "SECURITYTOKEN = \".+\"" ).Value + Environment.NewLine;
            UserData.token = Regex.Match ( UserData.token , "\".+\"" ).Value;
            UserData.token = UserData.token.Remove ( 0 , 1 );
            UserData.token = UserData.token.Remove ( UserData.token.Length - 1 , 1 );

            //cannot login, token doesn't exist
            if ( string.IsNullOrEmpty ( UserData.token ) || UserData.token == "guest" )
                return "TokenError";

            string user_id = "";
            if ( result.Contains ( "Welcome, <a href=\"member.php?u=" ) )
            {
                int startIndex = result.IndexOf ( "Welcome, <a href=\"member.php?u=" ) + 31;
                for ( int i = startIndex ; i < result.Count () ; i++ )
                {
                    if ( result[i] == '"' )
                    {
                        user_id = result.Substring ( startIndex , i - startIndex );
                        UserData.id = user_id;
                        return user_id;
                    }
                }
            }
            return "";
        }

        public static async Task<string> GetResponseURL ( string url )
        {
            HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create ( url );
            request.UserAgent = userAgent;
            request.Headers["Cookie"] = UserData.cookies;
            request.CookieContainer = new CookieContainer ();

            HttpWebResponse response = ( HttpWebResponse ) await request.GetResponseAsync ();

            if ( ( int ) response.StatusCode < 200 || ( int ) response.StatusCode > 299 )
            {
                return "Error";
            }

            Stream responseStream = response.GetResponseStream ();
            StreamReader reader = new StreamReader ( responseStream , Encoding.UTF8 );
            string result = reader.ReadToEnd ();

            return result;
        }
    }
}
