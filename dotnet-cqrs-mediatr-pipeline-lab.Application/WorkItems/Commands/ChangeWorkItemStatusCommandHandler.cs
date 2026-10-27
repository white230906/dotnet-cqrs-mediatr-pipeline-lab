using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;
using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Commands;
using DotNetCqrsMediatRPipelineLab.Domain.WorkItems;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Commands;

public  class ChangeWorkItemStatusCommandHandler(IWorkItemRepository repository, TimeProvider timeProvider)
    : ICommandHandler<
        ChangeWorkItemStatusCommand,
        Response.WorkItemResponse>
{
    public async Task<Result<Response.WorkItemResponse>> Handle(
        ChangeWorkItemStatusCommand request,
        CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(
            request.Id,
            trackChanges: true,
            cancellationToken);

        if (item is null)
        {
            return Result<Response.WorkItemResponse>.Failure(
                WorkItemErrors.NotFound(request.Id));
        }

        if (item.Version != request.ExpectedVersion)
        {
            return Result<Response.WorkItemResponse>.Failure(
                WorkItemErrors.ConcurrencyConflict());
        }

        var previousStatus = item.Status;
        var newStatus = WorkItemMappings.ToDomainStatus(request.Status);

        try
        {
            item.ChangeStatus(newStatus);
        }
        catch (InvalidOperationException exception)
        {
            return Result<Response.WorkItemResponse>.Failure(
                WorkItemErrors.InvalidState(exception.Message));
        }

        if (previousStatus == item.Status)
        {
            return Result<Response.WorkItemResponse>.Success(
                WorkItemMappings.ToResponse(item));
        }

        var history = WorkItemStatusHistory.Create(
            item.Id,
            previousStatus,
            item.Status,
            timeProvider.GetUtcNow());

        await repository.AddHistoryAsync(
            history,
            cancellationToken);

        return Result<Response.WorkItemResponse>.Success(
            WorkItemMappings.ToResponse(item));
    }
}