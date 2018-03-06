using System;
using Pattern.Logging;

namespace Crawler.Pattern.Logging
{
    public class PatternLogger : ICrawlerLogger
    {
        private ILogger logger;

        public PatternLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void Error(Exception any)
        {
            this.logger.Error(any, "Error on crawler");
        }
    }
}