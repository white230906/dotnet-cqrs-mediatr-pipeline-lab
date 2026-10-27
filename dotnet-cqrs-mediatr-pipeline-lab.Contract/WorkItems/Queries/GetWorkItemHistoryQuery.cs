using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

namespace DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Queries;

public sealed record GetWorkItemHistoryQuery(Guid WorkItemId)
    : IQuery<IReadOnlyCollection<Response.WorkItemStatusHistoryResponse>>;