using fmfm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace fmfm.Controllers
{
	public class ArtistsController : Controller
	{
		// GET: Artists
		public ActionResult Index()
		{
			return View(new ArtistsViewModel());
		}

		[HttpPost]
		public ActionResult Index(ArtistsViewModel viewModel)
		{
			viewModel.Artists = new List<ArtistViewModel>();

			if (string.IsNullOrEmpty(viewModel.ArtistInput))
				return View(viewModel);

			var list = viewModel.ArtistInput.Split(new string[] { "\r\n" } , StringSplitOptions.RemoveEmptyEntries);
			foreach (var artist in list)
			{
				var artistViewModel = new ArtistViewModel()
				{
					Name = artist
				};

				try
				{
					var request = WebRequest.Create(string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getInfo&api_key=57ee3318536b23ee81d6b27e36997cde&artist={0}", HttpUtility.UrlEncode(artist)));

					var stream = request.GetResponse().GetResponseStream();
					var response = new StreamReader(stream).ReadToEnd();

					var xmlResponse = new XmlDocument();
					xmlResponse.LoadXml(response);

					artistViewModel.Summary = xmlResponse.SelectSingleNode("/lfm/artist/bio/summary").InnerText;
					artistViewModel.Url = xmlResponse.SelectSingleNode("/lfm/artist/url").InnerText;

					artistViewModel.Listeners = Convert.ToInt32(xmlResponse.SelectSingleNode("/lfm/artist/stats/listeners").InnerText);
					artistViewModel.PlayCount = Convert.ToInt32(xmlResponse.SelectSingleNode("/lfm/artist/stats/playcount").InnerText);

					artistViewModel.Tags = new List<string>();
					var tags = xmlResponse.SelectNodes("/lfm/artist/tags/tag/name");

					for (var i = 0; i < tags.Count; ++i)
					{
						var tag = tags[i];
						artistViewModel.Tags.Add(tag.InnerText);
					}
				}
				catch (Exception ex)
				{
					artistViewModel.Summary = ex.ToString();
				}

				viewModel.Artists.Add(artistViewModel);
			}

			Session.Add(typeof(ArtistsViewModel).FullName, viewModel);

			return View(viewModel);
		}


		public ActionResult Order(string by)
		{
			var viewModel = Session[typeof(ArtistsViewModel).FullName] as ArtistsViewModel;
			if (viewModel == null)
				return View("Index", new ArtistsViewModel());

			if (by == "listeners")
				viewModel.Artists = viewModel.Artists.OrderByDescending(a => a.Listeners).ToList();

			else if (by == "playCount")
				viewModel.Artists = viewModel.Artists.OrderByDescending(a => a.PlayCount).ToList();

			return View("Index", viewModel);
		}
	}
}