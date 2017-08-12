namespace Crawler.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

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
            this.crawlerRepository.GetNext(Arg.Any<Func<CrawlItem, bool>>()).Returns(new CrawlItem
            {
                Url = uri.ToString(),
                Type = "localhost-none"
            });
            this.httpMessageHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<a href=\"http://localhost/\"></a>")
                });

            var categoryProduct = new Category
            {
                Code = "localhost-product",
                IsCategory = (u, c) => c?.DocumentNode.Descendants("h1").FirstOrDefault()?.Attributes["class"].Value == "product"
            };

            this.crawlerEngine.AddCategory(categoryProduct);

            this.category = new Category
            {
                Code = "localhost-none",
                IsCategory = (u, c) => u.AbsoluteUri.Contains("localhost")
            };

            this.crawlerEngine.AddCategory(this.category);
        }

        [Fact]
        public async void Should_get_a_first_page_When_start_crawl()
        {
            await this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).GetNext(Arg.Any<Func<CrawlItem, bool>>());
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
        public async void Should_insert_crawler_item_state_When_get_page()
        {
            await this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).Insert(Arg.Is<CrawlItem>(c => c.State == "Todo"));
        }

        [Fact]
        public async void Should_change_type_When_page_is_crawled()
        {
            this.httpMessageHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<h1 class=\"product\"></h1>")
                });

            await this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).Update(Arg.Is<CrawlItem>(c => c.Type == "localhost-product"));
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
            this.category.CallBack = Substitute.For<Func<string, HtmlDocument, Task>>();

            await this.crawlerEngine.Start();

            await this.category.CallBack.Received().Invoke(Arg.Is<string>(u => u == "http://localhost.crawl.com/"), Arg.Any<HtmlDocument>());
        }

        [Fact]
        public async void Should_get_next_todo_When_get_next_item()
        {
            await this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).GetNext(Arg.Is<Func<CrawlItem, bool>>(c => c(new CrawlItem { State = "Todo" })));
        }

        [Fact]
        public async void Should_did_not_get_next_When_state_is_not_todo()
        {
            await this.crawlerEngine.Start();

            this.crawlerRepository.DidNotReceive().GetNext(Arg.Is<Func<CrawlItem, bool>>(c => c(new CrawlItem { State = "Done" })));
        }

        [Fact]
        public async void Should_update_status_to_done_When_page_is_crawl()
        {
            await this.crawlerEngine.Start();

            this.crawlerRepository.Received().Update(Arg.Is<CrawlItem>(c => c.State == "Done"));
        }

        [Fact]
        public async void Should_do_not_insert_link_When_link_is_in_repository()
        {
            this.crawlerRepository.Exist("http://localhost/").Returns(true);

            await this.crawlerEngine.Start();

            this.crawlerRepository.DidNotReceive().Insert(Arg.Any<CrawlItem>());
        }

        [Fact]
        public async void Should_do_not_insert_link_When_balise_a_has_no_href()
        {
            this.httpMessageHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<a>Toto</a>")
                });

            await this.crawlerEngine.Start();

            this.crawlerRepository.DidNotReceive().Insert(Arg.Any<CrawlItem>());
        }

        [Fact]
        public async void Should_do_not_insert_link_When_link_is_in_repository_andlink_contains_sharp()
        {
            this.httpMessageHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<a href=\"http://localhost/#toto\"></a>")
                });

            this.crawlerRepository.Exist("http://localhost/").Returns(true);

            await this.crawlerEngine.Start();

            this.crawlerRepository.DidNotReceive().Insert(Arg.Any<CrawlItem>());
        }

        [Fact]
        public async void Should_call_getpage_twice_When_specify_start_twice_call()
        {
            await this.crawlerEngine.Start(2);

            this.crawlerRepository.Received(2).GetNext(Arg.Any<Func<CrawlItem, bool>>());
        }
    }
}