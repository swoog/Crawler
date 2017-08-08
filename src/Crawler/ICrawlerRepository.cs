namespace Crawler
{
    public interface ICrawlerRepository
    {
        CrawlItem GetNext();

        void Insert(CrawlItem crawlItem);
    }
}