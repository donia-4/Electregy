using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PeakWise.API.ExceptionHandling
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            var problemDeatils = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error has occurred, check Logger files ",
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            _logger.LogError(exception.Message);
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDeatils, cancellationToken: default);
            //await _problemDetailsService.WriteAsync(new ProblemDetailsContext
            //{
            //    ProblemDetails = problemDeatils,
            //    HttpContext = httpContext,
            //});
            return true;
        }
    }
}
