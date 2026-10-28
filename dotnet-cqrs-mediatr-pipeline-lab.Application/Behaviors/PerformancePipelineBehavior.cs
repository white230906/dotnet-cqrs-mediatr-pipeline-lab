using MediatR;
using Microsoft.Extensions.Logging;

namespace DotNetCqrsMediatRPipelineLab.Application.Behaviors;

public class PerformancePipelineBehavior<TRequest, TResponse>(
    TimeProvider timeProvider,
    ILogger<PerformancePipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly TimeSpan WarningThreshold = TimeSpan.FromMilliseconds(500);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetTimestamp();

        try
        {
            return await next();
        }
        finally
        {
            var elapsed = timeProvider.GetElapsedTime(startedAt);

            if (elapsed >= WarningThreshold)
            {
                logger.LogWarning(
                    "Slow application request {RequestName} took {ElapsedMilliseconds} ms",
                    typeof(TRequest).Name,
                    elapsed.TotalMilliseconds);
            }
        }
    }
}