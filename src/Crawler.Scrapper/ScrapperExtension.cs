using System.Collections.Generic;
using System.Linq;
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
        public static IEnumerable<HtmlNode> Att(this IEnumerable<HtmlNode> nodes, string name, string value)
        {
            return nodes.Where(n => n.Attributes.Contains(name) && n.Attributes[name].Value == value);
        }

        public static string InnerText(this IEnumerable<HtmlNode> nodes)
        {
            foreach (var htmlNode in nodes)
            {
                return htmlNode.InnerText;
            }

            throw new KeyNotFoundException();
        }
    }
}