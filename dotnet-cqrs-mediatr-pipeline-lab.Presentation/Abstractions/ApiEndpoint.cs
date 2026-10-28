using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using Microsoft.AspNetCore.Http;

namespace DotNetCqrsMediatRPipelineLab.Presentation.Abstractions;

public abstract class ApiEndpoint
{
    protected static IResult NotFound(
        string errorCode,
        string detail)
    {
        return CreateProblem(
            StatusCodes.Status404NotFound,
            "Resource not found",
            errorCode,
            detail);
    }
 
    protected static IResult ValidationFailure(
        string errorCode,
        string detail)
    {
        return CreateProblem(
            StatusCodes.Status422UnprocessableEntity,
            "Validation error",
            errorCode,
            detail);
    }
    
    protected static IResult Failure(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                "A successful result cannot be mapped as a failure.");
        }

        var error = result.Errors.First();

        return error.Code switch
        {
            "WorkItem.NotFound" =>
                NotFound(error.Code, error.Message),

            "WorkItem.InvalidState" or
                "WorkItem.ConcurrencyConflict" =>
                Conflict(error.Code, error.Message),

            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal server error",
                detail: "An unexpected error occurred.",
                type: "Server.UnexpectedError")
        };
    }
 
    protected static IResult Conflict(
        string errorCode,
        string detail)
    {
        return CreateProblem(
            StatusCodes.Status409Conflict,
            "Conflict",
            errorCode,
            detail);
    }
    
    private static IResult CreateProblem(
        int statusCode,
        string title,
        string errorCode,
        string detail)
    {
        return Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            type: errorCode,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = errorCode
            });
    }
}