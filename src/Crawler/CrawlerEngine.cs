namespace Crawler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using HtmlAgilityPack;

    public class CrawlerEngine
    {
        private readonly ICrawlerRepository crawlerRepository;

        private readonly HttpClient httpClient;

        private List<Category> categories = new List<Category>();

        public CrawlerEngine(ICrawlerRepository crawlerRepository, HttpClient httpClient)
        {
            this.crawlerRepository = crawlerRepository;
            this.httpClient = httpClient;
        }

        public async Task Start()
        {
            var nextCrawl = this.crawlerRepository.GetNext(c => c.State == "Todo");

            var response = await this.httpClient.GetAsync(nextCrawl.Url);

            var content = await response.Content.ReadAsStringAsync();

            var document = new HtmlDocument();
            document.LoadHtml(content);

            nextCrawl.Type = this.GetCrawlerType(new Uri(nextCrawl.Url), document);

            this.crawlerRepository.Update(nextCrawl);

            nextCrawl.State = "Done";

            var category = this.GetCategory(nextCrawl.Type);

            category.CallBack?.Invoke(document);

            foreach (var descendant in document.DocumentNode.Descendants("a"))
            {
                var url = descendant.Attributes["href"].Value;
                var newUrl = new Uri(new Uri(nextCrawl.Url), url);
                var type = this.GetCrawlerType(newUrl, null);

                if (!string.IsNullOrEmpty(type))
                {
                    this.crawlerRepository.Insert(new CrawlItem { Url = newUrl.AbsoluteUri, Type = type });
                }
            }
        }

        private Category GetCategory(string code)
        {
            return this.categories.FirstOrDefault(c => c.Code == code);
        }

        private string GetCrawlerType(Uri newUrl, HtmlDocument document)
        {
            return this.categories.FirstOrDefault(c => c.IsCategory(newUrl, document))?.Code;
        }

        public void AddCategory(Category category)
        {
            this.categories.Add(category);
        }
    }
}