using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteImageCrawler.Crawlers
{
    public class CssLinkCrawler
    {
        public CssLinkCrawler()
        {
        }

        public List<Uri> GetCssLinks(HtmlDocument doc, Uri baseUri)
        {
            List<Uri> links = new List<Uri>();
            var cssLinks = doc.DocumentNode.SelectNodes("//link[@type='text/css']");

            if(cssLinks == null)
            {
                return links;
            }

            foreach (var link in cssLinks)
            {
                Uri stylesheetUri;
                bool validCSSLink = Uri.TryCreate(baseUri, link.GetAttributeValue("href", ""), out stylesheetUri);

                // Add a base URI to 

                if (validCSSLink)
                {
                    links.Add(stylesheetUri);
                }
            }

            return links;
        }

    }
}
