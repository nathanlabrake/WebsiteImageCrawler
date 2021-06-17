using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WebsiteImageCrawler.Store;
using WebsiteImageCrawler.Utilities;

namespace WebsiteImageCrawler.Crawlers
{
    public class SitemapCrawler
    {
        private HttpClient Client;

        private ILogger Log;

        private WebPageCrawler WebPageCrawler;

        private IListStore ListStore;

        public SitemapCrawler(HttpClient client, ILogger log, WebPageCrawler webPageCrawler, IListStore listStore)
        {
            Client = client;
            Log = log;
            WebPageCrawler = webPageCrawler;
            ListStore = listStore;
        }

        /**
         * Crawls the specified sitemap URI.
         */
        public async Task Crawl(Uri sitemapUrl)
        {
            XmlDocument doc = await makeXMLDoc(sitemapUrl);
            await crawlSiteMapIndex(doc);
        }

        /**
         * Crawls a sitemap index
         */
        private async Task crawlSiteMapIndex(XmlDocument doc)
        {
            XmlNodeList elemList = doc.GetElementsByTagName("sitemap");

            if (elemList.Count == 0)
            {
                await crawlSiteMap(doc);
                return;
            }

            foreach (XmlNode node in elemList)
            {
                if (node.HasChildNodes)
                {
                    XmlNode sitemapUrlNode = node.ChildNodes[0];
                    if (sitemapUrlNode != null)
                    {
                        string sitemapURL = node.ChildNodes[0].InnerText;
                        Uri sitemapUri;
                        bool validUri = Uri.TryCreate(sitemapURL, UriKind.Absolute, out sitemapUri);

                        if (validUri)
                        {
                            XmlDocument sitemapDoc = await makeXMLDoc(sitemapUri);
                            if(sitemapDoc == null)
                            {
                                continue;
                            }

                            await crawlSiteMap(sitemapDoc);

                            Log.LogInformation($"Finished crawling all pages on {sitemapURL}");
                        }
                    }
                }
            }
        }

        /**
         * Generates a XmlDocument from the contents of the sitemap with the specified URL.
         */
        private async Task<XmlDocument> makeXMLDoc(Uri sitemapUri)
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync(sitemapUri);
                response.EnsureSuccessStatusCode();

                if (response.StatusCode.ToString() != "OK")
                {
                    return null;
                }

                string sitemapContent = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(sitemapContent))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(sitemapContent);
                    return doc;
                }
            }
            catch (HttpRequestException e)
            {
                Log.LogError("HttpRequestException on Sitemap URL {0}: {1} ", sitemapUri.AbsoluteUri, e.ToString());
            }

            return null;
        }

        /**
         * Loops through all links on the sitemap in parallel.
         */
        private async Task crawlSiteMap(XmlDocument doc)
        {
            XmlNodeList urlNodes = doc.GetElementsByTagName("url");

            if (urlNodes.Count == 0)
            {
                return;
            }

            var parallelGroups = Enumerable.Range(0, urlNodes.Count).GroupBy(r => r % 10);
            var parallelTasks = parallelGroups.Select(groups =>
            {
                return GetWebPageImages(urlNodes, groups);
            });

            await Task.WhenAll(parallelTasks);
        }

        private async Task GetWebPageImages(XmlNodeList elemList, IGrouping<int, int> groups)
        {
            foreach(int i in groups)
            {
                if (elemList == null || elemList[i] == null || !elemList[i].HasChildNodes || string.IsNullOrEmpty(elemList[i].FirstChild.InnerText))
                {
                    return;
                }

                string url = elemList[i].FirstChild.InnerText;

                // If the link contains a site map, skip it.
                if (url.Contains("/sitemap.html"))
                {
                    return;
                }

                Uri pageUri;
                bool validUri = Uri.TryCreate(url, UriKind.Absolute, out pageUri);

                if (validUri)
                {
                    // Create a WebPageCrawler instance and pass the page Uri.
                    HashSet<string> imageList = await WebPageCrawler.Crawl(pageUri);
                    await ListStore.Store(imageList, pageUri);
                }
            }
        }

    }
}
