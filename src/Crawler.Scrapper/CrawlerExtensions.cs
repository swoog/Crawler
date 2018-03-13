namespace Crawler.Scrapper
{
    public static class CrawlerExtensions
    {
        public static CrawlerEngine Scrap(this CrawlerEngine engine, string code, IScrapper scrapper)
        {
            engine.AddCategory(new Category
            {
                Code = code,
                IsCategory = scrapper.Is,
                CallBack = scrapper.Scrap
            });

            return engine;
        }
    }
}