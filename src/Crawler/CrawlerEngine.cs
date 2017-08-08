namespace Crawler
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using HtmlAgilityPack;

    public class CrawlerEngine
    {
        private readonly ICrawlerRepository crawlerRepository;

        private readonly HttpClient httpClient;

        public CrawlerEngine(ICrawlerRepository crawlerRepository, HttpClient httpClient)
        {
            this.crawlerRepository = crawlerRepository;
            this.httpClient = httpClient;
        }

        public async Task Start()
        {
            var nextCrawl = this.crawlerRepository.GetNext();

            var response = await this.httpClient.GetAsync(nextCrawl.Url);

            var content = await response.Content.ReadAsStringAsync();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(content);

            foreach (var descendant in document.DocumentNode.Descendants("a"))
            {
                var url = descendant.Attributes["href"].Value;
                var newUrl = new Uri(new Uri(nextCrawl.Url), url);
                this.crawlerRepository.Insert(new CrawlItem { Url = newUrl.AbsoluteUri });
            }
        }
    }

    public class CrawlItem
    {
        public string Url { get; set; }
    }
}