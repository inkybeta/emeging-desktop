namespace emeging.Models
{
	public class Inforesp
	{
		private bool _sslenabled = false;

		public bool SSLENABLED
		{
			get { return _sslenabled; }
			set { _sslenabled = value; }
		}

		public string SERVERNAME { get; set; }

		public string SERVERVENDOR { get; set; }
	}
}
