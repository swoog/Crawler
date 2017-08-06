namespace Crawler
{
    public interface ICrawlerRepository
    {
        CrawlItem GetNext();
    }
}