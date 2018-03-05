using Microsoft.EntityFrameworkCore;

namespace Crawler.EntityFramework
{
    public class ImportDbContext : DbContext
    {
        public DbSet<CrawlUrl> CrawlUrls { get; set; }
    }
}