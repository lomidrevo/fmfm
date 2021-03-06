﻿using fmfm.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace fmfm.Controllers
{
	public class PlaylistController : Controller
	{
		// GET: Playlist
		public ActionResult Index(int p = 1)
		{
			var actualTable = string.Empty;
			try
			{
				var rnd = new Random();

				if (p < 1 || p > 100)
					throw new ArgumentOutOfRangeException("p");

				var contentCsv = string.Empty;

				var currentPage = 1;

				var songs = new Dictionary<DateTime, string>();
				do
				{
					var playlistUrl = string.Format("https://fm.rtvs.sk/playlist?page={0}#playlist", currentPage);
					var request = WebRequest.Create(playlistUrl);

					var stream = request.GetResponse().GetResponseStream();
					var response = new StreamReader(stream).ReadToEnd();

					var tableStart = response.IndexOf("<table class=\"playlist\">");
					var tableLength = response.IndexOf("</table>", tableStart) - tableStart + "</table>".Length;

					actualTable = response.Substring(tableStart, tableLength);
					actualTable = actualTable.Replace("&", "--AND--"); ;

					try
					{
						var xmlResponse = new XmlDocument();
						xmlResponse.LoadXml(actualTable);

						var nodes = xmlResponse.SelectNodes("/table/tbody/tr");
						foreach (XmlNode node in nodes)
						{
							try
							{
								var dateTime = DateTime.Parse(string.Format("{0} {1}", node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText));
								var artist = node.ChildNodes[2].InnerText;
								var song = node.ChildNodes[3].InnerText;

								songs.Add(dateTime, string.Format("{0} - {1}", ToTitleCase(artist), ToTitleCase(song)));
							}
							catch (Exception)
							{

							}
						}
					}
					catch (Exception)
					{
						currentPage = p;
					}

					currentPage++;

					System.Threading.Thread.Sleep(rnd.Next(100));
				}
				while (currentPage <= p);

				if (songs.Count > 0)
				{
					contentCsv = "<html><body>" + string.Join("<br/>", songs.OrderByDescending(s => s.Key).Select(s => s.Value)) + "</body></html>";
					contentCsv = contentCsv.Replace("--And--", "&");
				}
				else
					contentCsv = "nothing here :(";
				
				return Content(contentCsv, "text/html");
			}
			catch (Exception ex)
			{
				return Content(ex.ToString() + "<br/><br/>" + HttpUtility.HtmlEncode(actualTable), "text/html");
			}
		}

		public static string ToTitleCase(string str)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
		}
	}
}