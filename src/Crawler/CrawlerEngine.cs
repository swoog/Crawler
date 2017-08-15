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

        private readonly ICrawlerLogger crawlerLogger;

        private List<Category> categories = new List<Category>();

        public CrawlerEngine(
            ICrawlerRepository crawlerRepository,
            HttpClient httpClient,
            ICrawlerLogger crawlerLogger)
        {
            this.crawlerRepository = crawlerRepository;
            this.httpClient = httpClient;
            this.crawlerLogger = crawlerLogger;
        }

        public async Task Start(int callNumber = 1)
        {
            for (int i = 0; i < callNumber; i++)
            {
                var nextCrawl = this.crawlerRepository.GetNext(c => c.State == "Todo");

                try
                {
                    nextCrawl.Type = this.GetCrawlerType(new Uri(nextCrawl.Url), null);
                    var category = this.GetCategory(nextCrawl.Type);

                    if (category.IgnoreCrawl)
                    {
                        nextCrawl.State = "Ignore";
                        this.crawlerRepository.Update(nextCrawl);
                        continue;
                    }

                    var response = await this.httpClient.GetAsync(nextCrawl.Url);

                    var content = await response.Content.ReadAsStringAsync();

                    var document = new HtmlDocument();
                    document.LoadHtml(content);

                    nextCrawl.Type = this.GetCrawlerType(new Uri(nextCrawl.Url), document);
                    nextCrawl.State = "Done";

                    this.crawlerRepository.Update(nextCrawl);

                    category = this.GetCategory(nextCrawl.Type);

                    var categoryCallBack = category.CallBack;
                    if (categoryCallBack != null)
                    {
                        await categoryCallBack(nextCrawl.Url, document);
                    }

                    foreach (var descendant in document.DocumentNode.Descendants("a"))
                    {
                        var url = descendant.Attributes["href"]?.Value;

                        if (string.IsNullOrEmpty(url))
                        {
                            continue;
                        }

                        if (url.Contains("#"))
                        {
                            url = url.Split('#')[0];
                        }

                        var newUrl = new Uri(new Uri(nextCrawl.Url), url);
                        var type = this.GetCrawlerType(newUrl, null);

                        if (!string.IsNullOrEmpty(type))
                        {
                            if (!this.crawlerRepository.Exist(newUrl.AbsoluteUri))
                            {
                                this.crawlerRepository.Insert(new CrawlItem
                                {
                                    Url = newUrl.AbsoluteUri,
                                    Type = type,
                                    State = "Todo"
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.crawlerLogger.Error(ex);
                    nextCrawl.State = "Error";
                    this.crawlerRepository.Update(nextCrawl);
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