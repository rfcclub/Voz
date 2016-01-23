using System;
using System.Collections.Generic;

namespace Voz.Model
{
	class Post
	{
		public string postId { get; set; }
		public string postTime { get; set; }
		public string postOrder { get; set; }
		public string avaLink { get; set; }
		public string userName { get; set; }
		public string userRank { get; set; }
		public string userJoinDate { get; set; }
		public string userPosts { get; set; }
		public string userLocation { get; set; }

		public string content { get; set; }
		public string htmlContent { get; set; }
		public List<HtmlAgilityPack.HtmlNode> listImage { get; set; }
		public List<HtmlAgilityPack.HtmlNode> listLink { get; set; }

		private const string quoteBorderTop = "--------------------------------------------------------";
		private const string quoteBorderBottom = "<br>--------------------------------------------------------";

		public static string GetQuoteBorderTop ()
		{
			return quoteBorderTop;
		}

		public static string GetQuoteBorderBottom ()
		{
			return quoteBorderBottom;
		}

		public void SetFullPost ( string s )
		{
			htmlContent = @s;
		}

		public void ReplaceImage ()
		{
			int c = 0;
			if ( listImage != null )
			{
				foreach ( HtmlAgilityPack.HtmlNode n in listImage )
				{
					c++;
					content = content.Replace ( "**img" + c.ToString () + "**" , n.OuterHtml );
				}
			}
		}

		public void ReplaceLink()
		{
			int c = 0;
			if (listLink!=null)
			{
				foreach (HtmlAgilityPack.HtmlNode n in listLink)
				{
					c++;
					content = content.Replace ( "**link" + c.ToString () + "**" , n.OuterHtml );
				}
			}
		}

		public void ReplaceNewLine()
		{
			HtmlAgilityPack.HtmlNode newNode = HtmlAgilityPack.HtmlNode.CreateNode ( "<br>" );
			content = content.Replace ( "**br**" , newNode.OuterHtml );
		}
	}
}
