namespace Crawler
{
    using System.Net.Http;

    public class CrawlerEngine
    {
        private readonly ICrawlerRepository crawlerRepository;

        private readonly HttpClient httpClient;

        public CrawlerEngine(ICrawlerRepository crawlerRepository, HttpClient httpClient)
        {
            this.crawlerRepository = crawlerRepository;
            this.httpClient = httpClient;
        }

        public void Start()
        {
           var nextCrawl = this.crawlerRepository.GetNext();

            this.httpClient.GetAsync(nextCrawl.Url);
        }
    }

    public class CrawlItem
    {
        public string Url { get; set; }
    }
}