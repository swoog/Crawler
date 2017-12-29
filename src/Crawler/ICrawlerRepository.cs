using System.Linq.Expressions;

namespace Crawler
{
    using System;

    public interface ICrawlerRepository
    {
        CrawlItem GetNext(Expression<Func<CrawlItem, bool>> filter);

        void Insert(CrawlItem crawlItem);

        void Update(CrawlItem crawlItem);

        bool Exist(string uri);
    }
}