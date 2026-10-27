using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;
using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;

using DotNetCqrsMediatRPipelineLab.Domain.WorkItems;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Queries;

public class GetWorkItemHistoryQueryHandler(IWorkItemRepository repository) : IQueryHandler<
        Query.GetWorkItemHistory, IReadOnlyCollection<Response.WorkItemStatusHistoryResponse>>
{
    public async Task<Result<IReadOnlyCollection<Response.WorkItemStatusHistoryResponse>>> Handle(
        Query.GetWorkItemHistory request,
        CancellationToken cancellationToken)
    {
        var workItem = await repository.GetByIdAsync(request.WorkItemId, trackChanges: false, cancellationToken);

        if (workItem is null)
        {
            return Result<IReadOnlyCollection<Response.WorkItemStatusHistoryResponse>>
                .Failure(WorkItemErrors.NotFound(request.WorkItemId));
        }

        var histories = await repository.GetHistoryAsync(
            request.WorkItemId,
            cancellationToken);

        IReadOnlyCollection<Response.WorkItemStatusHistoryResponse> response = histories
            .Select(WorkItemMappings.ToResponse)
            .ToArray();

        return Result<IReadOnlyCollection<Response.WorkItemStatusHistoryResponse>>
            .Success(response);
    }
}