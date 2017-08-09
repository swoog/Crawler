namespace Crawler.Tests
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private IHttpMessageHandler subtitute;

        public FakeHttpMessageHandler(IHttpMessageHandler subtitute)
        {
            this.subtitute = subtitute;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.subtitute.SendAsync(request, cancellationToken);
        }
    }
}