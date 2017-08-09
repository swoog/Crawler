﻿namespace Crawler
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
            var nextCrawl = this.crawlerRepository.GetNext();

            var response = await this.httpClient.GetAsync(nextCrawl.Url);

            var content = await response.Content.ReadAsStringAsync();

            var document = new HtmlDocument();
            document.LoadHtml(content);

            var category = GetCategory(nextCrawl.Type);

            category.CallBack?.Invoke(document);

            foreach (var descendant in document.DocumentNode.Descendants("a"))
            {
                var url = descendant.Attributes["href"].Value;
                var newUrl = new Uri(new Uri(nextCrawl.Url), url);
                var type = this.GetCrawlerType(newUrl);

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

        private string GetCrawlerType(Uri newUrl)
        {
            return this.categories.FirstOrDefault(c => c.IsCategory(newUrl))?.Code;
        }

        public void AddCategory(Category category)
        {
            this.categories.Add(category);
        }
    }
}