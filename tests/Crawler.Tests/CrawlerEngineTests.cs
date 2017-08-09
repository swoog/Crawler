namespace Crawler.Tests
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using HtmlAgilityPack;

    using NSubstitute;

    using Xunit;

    public class CrawlerEngineTests
    {
        private ICrawlerRepository crawlerRepository;

        private IHttpMessageHandler httpMessageHandler;

        private CrawlerEngine crawlerEngine;

        private Uri uri;

        private Category category;

        public CrawlerEngineTests()
        {
            this.crawlerRepository = Substitute.For<ICrawlerRepository>();
            this.httpMessageHandler = Substitute.For<IHttpMessageHandler>();

            var httpClient = new HttpClient(new FakeHttpMessageHandler(this.httpMessageHandler));
            this.crawlerEngine = new CrawlerEngine(this.crawlerRepository, httpClient);

            this.uri = new Uri("http://localhost.crawl.com");
            this.crawlerRepository.GetNext().Returns(new CrawlItem
                                                         {
                                                             Url = uri.ToString(),
                                                             Type = "localhost-none"
                                                         });
            this.httpMessageHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<a href=\"http://localhost/\"></a>")
                });

            this.category = new Category
            {
                Code = "localhost-none",
                IsCategory = u => u.AbsoluteUri.Contains("localhost")
            };

            this.crawlerEngine.AddCategory(this.category);
        }

        [Fact]
        public async void Should_get_a_first_page_When_start_crawl()
        {
            await this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).GetNext();
        }

        [Fact]
        public async void Should_call_url_When_get_first_crawl_url()
        {
            await this.crawlerEngine.Start();

            await this.httpMessageHandler.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(h => h.RequestUri == this.uri), Arg.Any<CancellationToken>());
        }


        [Fact]
        public async void Should_get_links_from_html_code_When_get_page()
        {
            await this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).Insert(Arg.Is<CrawlItem>(c => c.Url == "http://localhost/"));
        }

        [Fact]
        public async void Should_get_links_from_html_code_When_links_are_relative()
        {
            this.httpMessageHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<a href=\"/index.html\"></a>")
                });

            await this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).Insert(Arg.Is<CrawlItem>(c => c.Url == "http://localhost.crawl.com/index.html"));
        }

        [Fact]
        public async void Should_insert_crawler_item_category_type_When_get_page()
        {
            await this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).Insert(Arg.Is<CrawlItem>(c => c.Type == "localhost-none"));
        }

        [Fact]
        public async void Should_do_not_insert_crawler_item_category_type_When_link_is_not_in_category()
        {
            this.httpMessageHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                             {
                                 Content = new StringContent("<a href=\"http://neobd.fr/\"></a>")
                             });

            await this.crawlerEngine.Start();

            this.crawlerRepository.DidNotReceive().Insert(Arg.Any<CrawlItem>());
        }

        [Fact]
        public async void Should_call_category_call_back_When_crawl_page()
        {
            this.category.CallBack = Substitute.For<Action<HtmlDocument>>();

            await this.crawlerEngine.Start();

            this.category.CallBack.Received().Invoke(Arg.Any<HtmlDocument>());
        }
    }
}