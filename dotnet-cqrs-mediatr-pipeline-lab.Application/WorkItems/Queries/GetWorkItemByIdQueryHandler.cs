using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;
using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Queries;
using DotNetCqrsMediatRPipelineLab.Domain.WorkItems;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Queries;

public  class GetWorkItemByIdQueryHandler(IWorkItemRepository repository) : IQueryHandler<
        GetWorkItemByIdQuery,
        Response.WorkItemResponse>
{
    public async Task<Result<Response.WorkItemResponse>> Handle(
        GetWorkItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(
            request.Id,
            trackChanges: false,
            cancellationToken);

        return item is null
            ? Result<Response.WorkItemResponse>.Failure(
                WorkItemErrors.NotFound(request.Id))
            : Result<Response.WorkItemResponse>.Success(
                WorkItemMappings.ToResponse(item));
    }
}