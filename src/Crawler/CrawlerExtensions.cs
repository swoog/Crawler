using System;
using HtmlAgilityPack;

namespace Crawler
{
    public static class CrawlerExtensions
    {
        public static CrawlerEngine Crawl(this CrawlerEngine engine, string code, Func<Uri, HtmlDocument, bool> isCrawled)
        {
            engine.AddCategory(new Category()
            {
                Code = code,
                IsCategory = isCrawled,
                IgnoreCrawl = false,
                CallBack = null

            });

            return engine;
        }

        public static CrawlerEngine Ignore(this CrawlerEngine engine, string code, Func<Uri, HtmlDocument, bool> isIgnored)
        {
            engine.AddCategory(new Category()
            {
                Code = code,
                IsCategory = isIgnored,
                IgnoreCrawl = true,
                CallBack = null

            });

            return engine;
        }
    }
}