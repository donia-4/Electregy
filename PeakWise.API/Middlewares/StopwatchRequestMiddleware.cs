using System.Diagnostics;

namespace PeakWise.API.Middlewares
{
    public class StopwatchRequestMiddleware : IMiddleware
    {
        private readonly ILogger<StopwatchRequestMiddleware> _logger;
        public StopwatchRequestMiddleware(ILogger<StopwatchRequestMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogInformation("Start Request =>\n");
            var time = new Stopwatch();
            time.Start();
            await next.Invoke(context);
            time.Stop();
            _logger.LogDebug($"Path From : {context.Request.Path}");
            _logger.LogInformation($"Time Taken For Request : {time.ElapsedMilliseconds} ms");
        }
    }
}
