namespace Crawler.Tests
{
    using System;
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
        }

        [Fact]
        public void Should_get_a_first_page_When_start_crawl()
        {
            this.crawlerEngine.Start();

            this.crawlerRepository.Received(1).GetNext();
        }

        [Fact]
        public void Should_call_url_When_get_first_crawl_url()
        {
            this.crawlerEngine.Start();

            this.httpMessageHandler.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(h => h.RequestUri == this.uri), Arg.Any<CancellationToken>());
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