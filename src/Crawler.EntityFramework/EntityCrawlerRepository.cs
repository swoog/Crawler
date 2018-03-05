using System;
using System.Linq;
using System.Linq.Expressions;

namespace Crawler.EntityFramework
{
    public class EntityCrawlerRepository<T> : ICrawlerRepository
        where T : ImportDbContext
    {
        private readonly T db;

        public EntityCrawlerRepository(T db)
        {
            this.db = db;
        }

        public CrawlItem GetNext(Expression<Func<CrawlItem, bool>> filter)
        {
            return this.db.CrawlUrls.FirstOrDefault(filter);
        }

        public void Insert(CrawlItem crawlItem)
        {
            this.db.CrawlUrls.Add(new CrawlUrl()
            {
                State = crawlItem.State,
                Url = crawlItem.Url,
                Type = crawlItem.Type,
            });

            this.db.SaveChanges();
        }

        public void Update(CrawlItem crawlItem)
        {
            var cItem = this.db.CrawlUrls.First(c => c.Url == crawlItem.Url);
            cItem.State = crawlItem.State;
            cItem.Type = crawlItem.Type;

            this.db.SaveChanges();
        }

        public bool Exist(string uri)
        {
            return this.db.CrawlUrls.Any(c => c.Url == uri);
        }
    }
}