using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fmfm.Models
{
	public class ArtistViewModel
	{
		public string Name { get; set; }
		public string Url { get; set; }

		[AllowHtml]
		public string Summary { get; set; }
		public int Listeners { get; set; }
		public int PlayCount { get; set; }
		public List<string> Tags { get; set; }
	}

	public class ArtistsViewModel
	{
		public string ArtistInput { get; set; }
		public List<ArtistViewModel> Artists { get; set; }
	}
}