using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voz.Model
{
	[Table]
	public class Bookmark
	{
		[Column ( IsPrimaryKey = true , IsDbGenerated = true ,
			DbType = "INT NOT NULL Identity" , CanBeNull = false , AutoSync = AutoSync.OnInsert )]
		public string id { get; set; }

		[Column]
		public string threadBmId { get; set; }

		[Column]
		public string threadBmTitle { get; set; }

		[Column]
		public int threadBmPage { get; set; }
	}
}
