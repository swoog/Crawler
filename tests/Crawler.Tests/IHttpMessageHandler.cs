namespace Crawler.Tests
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IHttpMessageHandler
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}