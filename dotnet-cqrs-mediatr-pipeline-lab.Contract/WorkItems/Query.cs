using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

namespace DotNetCqrsMediatRPipelineLab.Contract.WorkItems;

public static class Query
{
    public sealed record GetWorkItems : IQuery<IReadOnlyCollection<Response.WorkItemResponse>>;

    public sealed record GetWorkItemById(Guid Id) : IQuery<Response.WorkItemResponse>;
    
    public sealed record GetWorkItemHistory(Guid WorkItemId) : IQuery<IReadOnlyCollection<Response.WorkItemStatusHistoryResponse>>;
}
