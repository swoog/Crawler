using System;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Crawler.Scrapper
{
    public interface IScrapper
    {
        bool Is(Uri u, HtmlDocument c);

        Task Scrap(string uri, HtmlDocument obj);
    }
}