using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;
using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Queries;
using DotNetCqrsMediatRPipelineLab.Domain.WorkItems;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Queries;

public class GetWorkItemsQueryHandler(IWorkItemRepository repository) : 
    IQueryHandler<GetWorkItemsQuery, IReadOnlyCollection<Response.WorkItemResponse>>
{
    public async Task<Result<IReadOnlyCollection<Response.WorkItemResponse>>> Handle(GetWorkItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await repository.GetAllAsync(cancellationToken);

        IReadOnlyCollection<Response.WorkItemResponse> response = items
            .Select(WorkItemMappings.ToResponse)
            .ToArray();

        return Result<IReadOnlyCollection<Response.WorkItemResponse>>
            .Success(response);
    }
}