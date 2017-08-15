namespace Crawler
{
    using System;
    using System.Threading.Tasks;

    using HtmlAgilityPack;

    public class Category
    {
        public string Code { get; set; }

        public Func<Uri, HtmlDocument, bool> IsCategory { get; set; }

        public Func<string, HtmlDocument, Task> CallBack { get; set; }

        public bool IgnoreCrawl { get; set; }
    }
}