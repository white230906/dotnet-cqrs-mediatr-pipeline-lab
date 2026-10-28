using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;
using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using DotNetCqrsMediatRPipelineLab.Domain.Abstractions.Repositories;
using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Application.Behaviors;

public sealed class TransactionPipelineBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommandBase)
        {
            return await next();
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            if (response is not Result result)
            {
                throw new InvalidOperationException(
                    "A command must return Result or Result<T>.");
            }

            if (result.IsFailure)
            {
                await unitOfWork.RollbackAsync(
                    CancellationToken.None);

                return response;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(
                CancellationToken.None);

            throw;
        }
    }
}