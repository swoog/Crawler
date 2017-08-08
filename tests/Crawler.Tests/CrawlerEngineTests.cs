namespace Crawler.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using NSubstitute;

    using Xunit;

    public class CrawlerEngineTests
    {
        private ICrawlerRepository crawlerRepository;

        private IHttpMessageHandler httpMessageHandler;

        private CrawlerEngine crawlerEngine;

        private Uri uri;

        public CrawlerEngineTests()
        {
            this.crawlerRepository = Substitute.For<ICrawlerRepository>();
            this.httpMessageHandler = Substitute.For<IHttpMessageHandler>();

            var httpClient = new HttpClient(new FakeHttpMessageHandler(this.httpMessageHandler));
            this.crawlerEngine = new CrawlerEngine(this.crawlerRepository, httpClient);

            this.uri = new Uri("http://localhost.crawl.com");
            this.crawlerRepository.GetNext().Returns(new CrawlItem() { Url = uri.ToString() });
            this.httpMessageHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                             {
                                 Content = new StringContent("<a href=\"http://localhost/\"></a>")
                             });
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
    }

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private IHttpMessageHandler subtitute;

        public FakeHttpMessageHandler(IHttpMessageHandler subtitute)
        {
            this.subtitute = subtitute;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.subtitute.SendAsync(request, cancellationToken);
        }
    }

    public interface IHttpMessageHandler
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}