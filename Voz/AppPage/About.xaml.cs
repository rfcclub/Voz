using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;

namespace Voz.AppPage
{
	public partial class About : PhoneApplicationPage
	{
		public About ()
		{
			InitializeComponent ();
			textBlockVersion.Text = "Version: " + Voz.Helper.Utilities.GetVersion ();
		}
	}
}