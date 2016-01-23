using HtmlAgilityPack;
using System.Collections.Generic;
using Voz.Model;

namespace Voz.Helper
{
	class HAP
	{
		public static void RemoveChildKeepGrandChildren ( HtmlNode parent , HtmlNode oldChild )
		{
			if ( oldChild.ChildNodes != null )
			{
				HtmlNode previousSibling = oldChild.PreviousSibling;
				foreach ( HtmlNode newChild in oldChild.ChildNodes )
				{
					parent.InsertAfter ( newChild , previousSibling );
					previousSibling = newChild;  // Missing line in HtmlAgilityPack
				}
			}
			parent.RemoveChild ( oldChild );
		}

		public static void RemoveComment ( HtmlDocument doc )
		{
			HtmlNodeCollection cmt = doc.DocumentNode.SelectNodes ( ".//comment()" );
			if ( cmt != null )
			{
				foreach ( HtmlNode c in cmt )
				{
					c.ParentNode.RemoveChild ( c );
				}
			}
		}

		public static void ReplaceNewLine ( HtmlNode div )
		{
			HtmlNodeCollection list = div.SelectNodes ( ".//br" );
			if ( list != null )
			{
				foreach ( HtmlNode l in list )
				{
					var newNode = HtmlNode.CreateNode ( "**br**" );
					l.ParentNode.ReplaceChild ( newNode , l );
				}
			}
		}

		public static void SetEmoLink ( HtmlNode div , Post p )
		{
			HtmlNodeCollection emo = div.SelectNodes ( ".//img[@class='inlineimg' and @title]" );
			if ( emo != null )
			{
				foreach ( HtmlNode e in emo )
				{
					e.Attributes["src"].Value = "https://vozforums.com/" + e.Attributes["src"].Value;
				}
			}
		}

		public static void RemovePostColor ( HtmlNode div )
		{
			HtmlNode color = div.SelectSingleNode ( "./font[@color]" );
			if ( color != null )
			{
				Voz.Helper.HAP.RemoveChildKeepGrandChildren ( color.ParentNode , color );
			}
		}

		public static void ReplaceEmoWithText ( HtmlNode div )
		{
			HtmlNodeCollection emo = div.SelectNodes ( ".//img[@class='inlineimg' and @title]" );
			if ( emo != null )
			{
				foreach ( HtmlNode e in emo )
				{
					string newnodehtml = " :" + e.Attributes["title"].Value.ToLower () + ": ";
					HtmlNode newnode = HtmlNode.CreateNode ( newnodehtml );
					e.ParentNode.ReplaceChild ( newnode , e );
				}
			}
		}

		public static void EditLink ( HtmlNode div , Post p )
		{
			string s;
			HtmlNodeCollection links = div.SelectNodes ( ".//a" );
			if ( links != null )
			{
				p.listLink = new List<HtmlNode> ();
				foreach ( HtmlNode l in links )
				{
					s = l.Attributes["href"].Value;
					if ( s.Contains ( "/redirect/index.php?link=" ) )
					{
						l.Attributes["href"].Value = "https://vozforums.com/" + l.Attributes["href"].Value;
					}
					p.listLink.Add ( l );
					var newNode = HtmlNode.CreateNode ( "**link" + p.listLink.Count.ToString () + "**" );
					l.ParentNode.ReplaceChild ( newNode , l );
				}
			}
		}

		public static void ReplaceImage ( HtmlNode div , Post p )
		{
			HtmlNodeCollection list = div.SelectNodes ( ".//img" );
			if ( list != null )
			{
				p.listImage = new List<HtmlNode> ();
				foreach ( HtmlNode l in list )
				{
					p.listImage.Add ( l );
					string newnodehtml = "**img" + p.listImage.Count.ToString () + "**";
					HtmlNode newnode = HtmlNode.CreateNode ( newnodehtml );
					l.ParentNode.ReplaceChild ( newnode , l );
				}
			}
		}

		public static string ProcessQuote ( HtmlNode quote , HtmlDocument doc )
		{
			RemoveViewPost ( quote );
			string quoteContent = "";
			quoteContent = Model.Post.GetQuoteBorderTop ();

			HtmlNode td = quote.SelectSingleNode ( "./table" ).SelectSingleNode ( ".//td[@class='alt2']" );
			if ( td != null )
			{
				HtmlNodeCollection quotes = td.SelectNodes ( "./div[@style='margin:20px; margin-top:5px; ']" );
				if ( quotes != null )
				{
					foreach ( HtmlNode q in quotes )
					{
						string s = ProcessQuote ( q , doc );
						HtmlNode newNode = doc.CreateElement ( "title" );
						newNode.InnerHtml = HtmlDocument.HtmlEncode ( s );
						q.ParentNode.ReplaceChild ( newNode , q );
					}
				}
				//check if quote has user and link
				HtmlNode strongUserName = td.SelectSingleNode ( ".//strong" );
				if ( strongUserName != null )//user quote exist
				{
					string quoteUser = "Originally Posted by <b>" + HtmlEntity.DeEntitize ( strongUserName.InnerText.Trim () ) + "</b>";
					quoteContent += quoteUser + "<br>";
					td.RemoveChild ( td.Element ( "div" ) );
				}
				quoteContent += HtmlEntity.DeEntitize ( td.InnerText.Trim () );
				quoteContent += Model.Post.GetQuoteBorderBottom ();
			}
			else
				quoteContent += HtmlEntity.DeEntitize ( td.InnerText.Trim () ).Trim ();
			return quoteContent;
		}

		private static void RemoveViewPost ( HtmlNode node )
		{
			HtmlNodeCollection v = node.SelectNodes ( ".//a[@rel='nofollow']" );
			if ( v != null )
			{
				foreach ( HtmlNode n in v )
				{
					n.ParentNode.RemoveAllChildren ();
				}
			}
		}

		public static void GetUserAva ( HtmlNode t , Post p )
		{
			if ( UserData.settings.GetValueOrDefault ( AppSettings.keyAva , true ) == true )
			{
				HtmlNode ava = t.SelectSingleNode ( ".//td[@class='alt2']" ).SelectSingleNode ( ".//td[@class='alt2']" );
				if ( ava != null )
				{
					ava = ava.SelectSingleNode ( ".//img" );
					p.avaLink = "https://vozforums.com/";
					p.avaLink += ava.Attributes["src"].Value;
				}
				else
				{
					p.avaLink = null;
				}
			}
			else
			{
				p.avaLink = null;
			}
		}

		public static void GetUserName ( HtmlNode t , Post p )
		{
			HtmlNode user_name = t.SelectSingleNode ( ".//a[@class='bigusername']" );
			if ( user_name != null )
			{
				p.userName = HtmlEntity.DeEntitize ( user_name.InnerText.Trim () );
			}
			else
			{
				user_name = t.SelectSingleNode ( ".//td[@class='alt2']" );
				p.userName = HtmlEntity.DeEntitize ( user_name.InnerText.Trim () );
			}
		}

		public static void GetPostInfo ( HtmlNode t , Post p )
		{
			//get post info id time #
			//post info is 2 div, 1st div contain #post, 2nd div contain post time
			HtmlNodeCollection post_info = t.SelectNodes ( ".//div[@class='normal']" );
			if ( post_info != null )
			{
				p.postId = HtmlEntity.DeEntitize ( post_info[0].SelectSingleNode ( "./a" ).Attributes["id"].Value ).Trim ();
				p.postId = p.postId.Remove ( 0 , 9 );
				p.postOrder = HtmlEntity.DeEntitize ( post_info[0].InnerText.Trim () ).Trim ();
				p.postTime = post_info[1].InnerText.Trim ();
			}
			else
			{
				p.postId = "";
				p.postOrder = "";
				p.postTime = "";
			}
		}

		public static void GetUserInfo ( HtmlNode t , Post p )
		{
			//user info is 2 div class="smallfont"
			//1st div is member rank, 2nd div is member: jd, location, posts
			HtmlNodeCollection user_info;
			HtmlNode table = t.SelectSingleNode ( ".//table" );
			if ( table == null )
			{
				p.userRank = "";
				p.userJoinDate = "";
				p.userLocation = "";
				p.userPosts = "";
			}
			else
			{
				user_info = table.SelectSingleNode ( ".//tr" ).SelectNodes ( ".//div[@class='smallfont']" );
				if ( UserData.settings.GetValueOrDefault ( AppSettings.keyJoinDate , true ) == true )
					p.userJoinDate = user_info[1].SelectSingleNode ( "./div[1]" ).InnerText.Trim ();
				else
					p.userJoinDate = "";

				string s = HtmlEntity.DeEntitize ( user_info[1].SelectSingleNode ( "./div[2]" ).InnerText.Trim () );
				if ( s[0] == 'L' )
				{
					p.userLocation = s;
					p.userPosts = user_info[1].SelectSingleNode ( "./div[3]" ).InnerText.Trim ();
				}
				else
				{
					p.userPosts = user_info[1].SelectSingleNode ( "./div[2]" ).InnerText.Trim ();
					p.userLocation = "";
				}

				if ( UserData.settings.GetValueOrDefault ( AppSettings.keyLocation , true ) == false )
					p.userLocation = "";
				if ( UserData.settings.GetValueOrDefault ( AppSettings.keyPosts , true ) == false )
					p.userPosts = "";
			}
		}

		public static void GetQuoteAndReply ( HtmlNode t , Post p , HtmlDocument doc )
		{
			HtmlNode div;
			HtmlNodeCollection quotes;

			div = t.SelectSingleNode ( ".//td[@class='alt1']" ).SelectSingleNode ( "./div[@id]" );
			if ( div == null )
			{
				p.SetFullPost ( t.SelectSingleNode ( ".//td[@class='alt1']" ).InnerText.Trim () );
			}
			else
			{
				//replace <br> with new line
				HAP.ReplaceNewLine ( div );
				//if post have color, remove it
				HAP.RemovePostColor ( div );
				//set emo link
				HAP.SetEmoLink ( div , p );
				if ( UserData.settings.GetValueOrDefault ( AppSettings.keyEmo , true ) == false )
					HAP.ReplaceEmoWithText ( div );
				//edit link
				HAP.EditLink ( div , p );
				//replace img
				HAP.ReplaceImage ( div , p );

				//check quotes
				quotes = div.SelectNodes ( "./div[@style='margin:20px; margin-top:5px; ']" );
				if ( quotes != null )
				{
					foreach ( HtmlNode quote in quotes )
					{
						string s = HAP.ProcessQuote ( quote , doc );
						HtmlNode newNode = doc.CreateElement ( "title" );
						newNode.InnerHtml = HtmlDocument.HtmlEncode ( s );
						quote.ParentNode.ReplaceChild ( newNode , quote );
					}
				}
				p.content = HtmlEntity.DeEntitize ( div.InnerText.Trim () );
				p.ReplaceImage ();
				p.ReplaceNewLine ();
				p.ReplaceLink ();
				p.SetFullPost ( p.content );
			}
		}
	}
}
