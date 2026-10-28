using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DotNetCqrsMediatRPipelineLab.Application.Behaviors;

public class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation(
            "Handling application request {RequestName}",
            requestName);

        try
        {
            var response = await next();

            if (response is Result result)
            {
                logger.LogInformation(
                    "Handled application request {RequestName}. Success: {IsSuccess}",
                    requestName,
                    result.IsSuccess);
            }
            else
            {
                logger.LogInformation(
                    "Handled application request {RequestName}",
                    requestName);
            }

            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Application request {RequestName} failed",
                requestName);

            throw;
        }
    }
}