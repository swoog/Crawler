namespace Crawler
{
    using System;

    using HtmlAgilityPack;

    public class Category
    {
        public string Code { get; set; }

        public Func<Uri, bool> IsCategory { get; set; }

        public Action<HtmlDocument> CallBack { get; set; }
    }
}