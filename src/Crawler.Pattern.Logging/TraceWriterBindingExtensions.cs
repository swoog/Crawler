using Pattern.Core.Interfaces;
using Pattern.Core.Interfaces.Factories;
using Pattern.Logging;

namespace Crawler.Pattern.Logging
{
    public static class TraceWriterBindingExtensions
    {
        public static IKernel BindCrawlerLogger(this IKernel kernel)
        {
            kernel.Bind(
                typeof(ICrawlerLogger),
                new TypeFactory(typeof(PatternLogger), kernel));

            return kernel;
        }
    }
}