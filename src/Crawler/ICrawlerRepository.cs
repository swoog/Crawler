namespace Crawler
{
    using System;

    public interface ICrawlerRepository
    {
        CrawlItem GetNext(Func<CrawlItem, bool> filter);

        void Insert(CrawlItem crawlItem);

        void Update(CrawlItem crawlItem);

        bool Exist(string uri);
    }
}