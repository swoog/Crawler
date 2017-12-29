using System;
using System.Collections.Generic;
using Crawler.Scrapper;
using HtmlAgilityPack;

namespace Crawler.Scrapper
{
    public class Scrapper<TScrapper>
    {
        private readonly HtmlDocument document;

        private readonly TScrapper instance;

        public Scrapper(HtmlDocument document, TScrapper instance)
        {
            this.document = document;
            this.instance = instance;
        }

        public Scrapper<TScrapper> Property<T>(Action<TScrapper, T> setter, Func<Scrapper<TScrapper>, T> scrap)
        {
            T value = scrap(this);
            setter(this.instance, value);
            return this;
        }

        public TScrapper Get()
        {
            return this.instance;
        }

        public IEnumerable<HtmlNode> Balise(string balise)
        {
            return this.document.DocumentNode.Descendants(balise);
        }
    }
}