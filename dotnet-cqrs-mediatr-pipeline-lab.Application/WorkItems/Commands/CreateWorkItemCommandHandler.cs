using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;
using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;

using DotNetCqrsMediatRPipelineLab.Domain.WorkItems;
using Microsoft.Extensions.Options;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Commands;

public  class CreateWorkItemCommandHandler(IWorkItemRepository repository, IOptions<WorkItemOptions> options, 
    TimeProvider timeProvider) : ICommandHandler<Command.CreateWorkItem, Response.WorkItemResponse>
{
    public async Task<Result<Response.WorkItemResponse>> Handle(
        Command.CreateWorkItem request,
        CancellationToken cancellationToken)
    {
        var item = WorkItem.Create(
            request.Title,
            options.Value.MaxTitleLength,
            timeProvider.GetUtcNow());

        await repository.AddAsync(item, cancellationToken);

        return Result<Response.WorkItemResponse>.Success(
            WorkItemMappings.ToResponse(item));
    }
}