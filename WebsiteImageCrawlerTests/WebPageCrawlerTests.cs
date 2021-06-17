using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebsiteImageCrawler.Crawlers;

namespace WebsiteImageCrawlerTests
{
    public class WebPageCrawlerTests
    {
        HttpClient Client;

        ILogger Log;

        [SetUp]
        public void Setup()
        {
            FakeHttpMessageHandler fakeHttpMessageHandler = new FakeHttpMessageHandler();
            Client = new HttpClient(fakeHttpMessageHandler);
            var logMock = new Mock<ILogger>();
            Log = logMock.Object;
        }

        [Test]
        public async Task TestWebPageCrawlerGetsImagesFromHtmlAndCSS()
        {
            WebPageCrawler webPageCrawler = new WebPageCrawler(Client, Log);
            HashSet<string> imageList = await webPageCrawler.Crawl(new Uri("https://www.testdomain.com/test-page-1"));

            HashSet<string> expected = new HashSet<string>
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

            CollectionAssert.AreEquivalent(expected, imageList);
        }

        [Test]
        public async Task TestWebPageGetsImagesFromHtmlAndNoImagesInCss()
        {
            WebPageCrawler webPageCrawler = new WebPageCrawler(Client, Log);
            HashSet<string> imageList = await webPageCrawler.Crawl(new Uri("https://www.testdomain.com/test-page-2"));

            HashSet<string> expected = new HashSet<string>
            {
                "http://www.testdomain.com/test-img1.jpg",
                "data-attr-img.jpg",
            };

            CollectionAssert.AreEquivalent(expected, imageList);
        }
    }
}