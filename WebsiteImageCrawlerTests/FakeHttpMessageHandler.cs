using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsiteImageCrawler.Utilities;

namespace WebsiteImageCrawlerTests
{
    /**
     * <summary>Outputs static responses to URI requests</summary>
     */
    class FakeHttpMessageHandler : HttpMessageHandler
    {
        private Dictionary<Uri, HttpResponseMessage> UriResponses;

        public FakeHttpMessageHandler()
        {
            initializeDictionary();
        }

        private void initializeDictionary()
        {
            string dir = Directory.GetCurrentDirectory();
            string siteMapIndex = File.ReadAllText($@"{dir}/assets/xml/SitemapIndex.xml");
            string siteMap1 = File.ReadAllText($@"{dir}/assets/xml/Sitemap.xml");
            string htmlPage1 = File.ReadAllText(@$"{dir}/assets/html/TestPageWithImagesInHtmlAndCss.html");
            string htmlPage2 = File.ReadAllText(@$"{dir}/assets/html/TestPageWithImagesInHtmlAndNotCss.html");
            string cssFile1 = File.ReadAllText(@$"{dir}/assets/css/styles.css");
            string cssFile2 = File.ReadAllText(@$"{dir}/assets/css/style-no-img.css");
            string cssFile3 = File.ReadAllText($@"{dir}/assets/css/styles-2.css");
            string cssFile4 = File.ReadAllText($@"{dir}/assets/css/styles-3.css");
            string cssFile5 = File.ReadAllText($@"{dir}/assets/css/styles-4.css");

            UriResponses = new Dictionary<Uri, HttpResponseMessage>
            {
                { 
                    new Uri("https://www.testdomain.com/test-page-1"), 
                    new HttpResponseMessage{ 
                        StatusCode = HttpStatusCode.OK, Content = new StringContent(htmlPage1),
                        RequestMessage = new HttpRequestMessage{ RequestUri = new Uri("https://www.testdomain.com/test-page-1") }
                    }
                },
                { 
                    new Uri("https://www.testdomain.com/test-page-2"),
                    new HttpResponseMessage{
                        StatusCode = HttpStatusCode.OK, Content = new StringContent(htmlPage2),
                        RequestMessage = new HttpRequestMessage{ RequestUri = new Uri("https://www.testdomain.com/test-page-2") }
                    }
                },
                { new Uri("https://www.testdomain.com/css/styles.css" ), new HttpResponseMessage{ StatusCode = HttpStatusCode.OK, Content = new StringContent(cssFile1) } },
                { new Uri("https://www.testdomain.com/css/style-no-img.css" ), new HttpResponseMessage{ StatusCode = HttpStatusCode.OK, Content = new StringContent(cssFile2) } },
                { new Uri("https://www.testdomain.com/css/styles-2.css"), new HttpResponseMessage{ StatusCode = HttpStatusCode.OK, Content = new StringContent(cssFile3) } },
                { new Uri("https://www.testdomain.com/css/styles-3.css"), new HttpResponseMessage{ StatusCode = HttpStatusCode.OK, Content = new StringContent(cssFile4) } },
                { new Uri("https://www.testdomain.com/css/styles-4.css"), new HttpResponseMessage{ StatusCode = HttpStatusCode.OK, Content = new StringContent(cssFile5) } },
                { new Uri("https://www.testdomain.com/sitemap_index.xml"), new HttpResponseMessage{ StatusCode = HttpStatusCode.OK, Content = new StringContent(siteMapIndex) } },
                { new Uri("https://www.testdomain.com/sitemap.xml"), new HttpResponseMessage{ StatusCode = HttpStatusCode.OK, Content = new StringContent(siteMap1) } },
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri == null) return null;

            return await Task.Run(() => UriResponses[request.RequestUri]);
        }
    }
}
