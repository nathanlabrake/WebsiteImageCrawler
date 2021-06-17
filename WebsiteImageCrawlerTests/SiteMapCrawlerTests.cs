using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebsiteImageCrawler.Crawlers;
using WebsiteImageCrawler.Store;

namespace WebsiteImageCrawlerTests
{
    public class SiteMapCrawlerTests
    {
        HttpClient Client;

        ILogger Log;

        Mock<IListStore> ListStoreMock;

        [SetUp]
        public void Setup()
        {
            FakeHttpMessageHandler fakeHttpMessageHandler = new FakeHttpMessageHandler();
            Client = new HttpClient(fakeHttpMessageHandler);
            var logMock = new Mock<ILogger>();
            Log = logMock.Object;
            ListStoreMock = new Mock<IListStore>();
        }

        [Test]
        public async Task TestSiteMapCrawlerSavesAllImagesFromSiteMapIndex()
        {
            WebPageCrawler webPageCrawler = new WebPageCrawler(Client, Log);
            IListStore listStore = ListStoreMock.Object;
            SitemapCrawler sitemapCrawler = new SitemapCrawler(Client, Log, webPageCrawler, listStore);
            await sitemapCrawler.Crawl(new Uri("https://www.testdomain.com/sitemap_index.xml"));

            HashSet<string> images = new HashSet<string>
            {
                "http://www.testdomain.com/test-img1.jpg",
                "data-attr-img.jpg",
                "test-bg-img.jpg",
                "test-bg-img-2.jpg",
                "inline-test-bg-img.jpg",
                "http://www.testdomain.com/test-style-bg-img.jpg",
                "/img/test-bg-img-2.jpg",
                "/img/test-bg-img-3.jpg",
                "/img/test-bg-img-4.jpg",
            };

            ListStoreMock.Verify(listStore => listStore.Store(It.Is<HashSet<string>>(set => set.SetEquals(images)), new Uri("https://www.testdomain.com/test-page-1")), Times.Once());
        }

    }
}
