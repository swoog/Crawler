using System.Collections.Generic;
using HtmlAgilityPack;

namespace Crawler.Scrapper
{
    public static class ScrapperExtension
    {
        public static Scrapper<T> ScrapTo<T>(this HtmlDocument document)
            where T : new()
        {
            return new Scrapper<T>(document, new T());
        }

        public static IEnumerable<HtmlNode> Balise(this IEnumerable<HtmlNode> nodes, string balise)
        {
            foreach (var htmlNode in nodes)
            {
                foreach (var descendant in htmlNode.Descendants(balise))
                {
                    yield return descendant;
                }
            }
        }
    }
}