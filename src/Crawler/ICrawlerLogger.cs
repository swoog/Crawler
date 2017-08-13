namespace Crawler
{
    using System;

    public interface ICrawlerLogger
    {
        void Error(Exception any);
    }
}