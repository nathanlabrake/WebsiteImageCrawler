using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebsiteImageCrawler.Utilities;

namespace WebsiteImageCrawler.Crawlers
{
    /**
     * <summary>Crawls a web page for images, checking CSS stylesheets and inline styles as well.</summary>
     */
    public class WebPageCrawler
    {
        HttpClient Client;

        ILogger Log;

        public WebPageCrawler(HttpClient client, ILogger log)
        {
            Client = client;
            Log = log;
        }

        public async Task<HashSet<string>> Crawl(Uri uri)
        {
            HashSet<string> imageList = new HashSet<string>();
            string pageContent = await getContent(uri);

            if (string.IsNullOrEmpty(pageContent))
            {
                return null;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            var htmlElement = doc.DocumentNode.SelectSingleNode("//html");

            // If this is a sitemap, don't crawl it for images.
            string xmlnsSitemap = htmlElement.GetAttributeValue("xmlns:sitemap", "");
            if(!string.IsNullOrEmpty(xmlnsSitemap))
            {
                return null;
            }

            var bodyElement = doc.DocumentNode.SelectSingleNode("//body");

            ImageParser imageParser = new ImageParser();
            CssCrawler cssCrawler = new CssCrawler();

            foreach (var node in bodyElement.Descendants())
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if(node.Name != "style")
                    {
                        // For all tags, check in the list of attributes to see whether an image is present
                        foreach (var attr in node.Attributes)
                        {
                            imageList.UnionWith(imageParser.GetImages(attr.Value));
                        }
                    }
                }
            }

            CssLinkCrawler cssLinkCrawler = new CssLinkCrawler();
            List<Uri> cssLinks = cssLinkCrawler.GetCssLinks(doc, uri);

            foreach(Uri cssLink in cssLinks)
            {
                string cssContent = await getContent(cssLink, false).ConfigureAwait(false);
                try
                {
                    HashSet<string> cssImgs = cssCrawler.Crawl(cssContent);
                    imageList.UnionWith(cssImgs);
                }
                catch (OverflowException e)
                {
                    Log.LogError("Overflow exception: " + e.Message + "; " + e.StackTrace.ToString());
                }
            }

            // Loop through <style> tags
            var styleTags = doc.DocumentNode.SelectNodes("//style");

            if(styleTags != null)
            {
                foreach (var node in styleTags)
                {
                    if (node.NodeType == HtmlNodeType.Element)
                    {
                        // !!!!!!!!!!!!!!!!!!!!!!!!!! NOTE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Find out why it's throwing exceptions like OverflowException.
                        try
                        {
                            imageList.UnionWith(cssCrawler.Crawl(node.InnerText));
                        }
                        catch (OverflowException e)
                        {
                            Log.LogError("Overflow exception: " + e.Message + "; " + e.StackTrace.ToString());
                        }
                    }
                }
            }

            // Loop through JS files?
            // JS is unlikely to contain that many images.
            // However, we could just check the files while ignoring comments.
            // Or add in support for JS files later.

            return imageList;
        }

        /**
         * Makes a request to the URL and retrieves the content.
         */
        private async Task<string> getContent(Uri url, bool checkDomain = true)
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                if (checkDomain)
                {
                    string requestUriStr = response.RequestMessage.RequestUri.ToString();
                    bool validUrl = Uri.TryCreate(requestUriStr, UriKind.Absolute, out Uri requestUri);

                    if (validUrl && !requestUri.Host.Equals(url.Host))
                    {
                        return "";
                    }
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                Log.LogError("HttpRequestException on Page URL {0}: {1}; got status code {2}", url.AbsoluteUri, e.ToString(), e.StatusCode);
                return "";
            }
        }
    }
}
