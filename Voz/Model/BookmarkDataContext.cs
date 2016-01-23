using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voz.Model
{
	public class BookmarkDataContext : DataContext
	{
		public static string DBConnectionString = "Data Source=isostore:/BM.sdf";
		public BookmarkDataContext ( string connectionString )
			: base ( connectionString ) { }
		public Table<Bookmark> Bookmarks;
	}
}
