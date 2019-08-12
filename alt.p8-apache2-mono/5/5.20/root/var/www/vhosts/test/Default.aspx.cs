using System;
using System.Web;
using System.Web.UI;

namespace test
{
	
	public partial class Default : System.Web.UI.Page
	{
		public void button1Clicked (object sender, EventArgs args)
		{
			buttonОдин.Text = "You clicked me";
		}
	}
}

