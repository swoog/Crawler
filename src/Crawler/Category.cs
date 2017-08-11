namespace Crawler
{
    using System;

    using HtmlAgilityPack;

    public class Category
    {
        public string Code { get; set; }

        public Func<Uri, HtmlDocument, bool> IsCategory { get; set; }

        public Action<string, HtmlDocument> CallBack { get; set; }
    }
}