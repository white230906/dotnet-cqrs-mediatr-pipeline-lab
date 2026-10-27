using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

namespace DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Queries;

public  record GetWorkItemsQuery : IQuery<IReadOnlyCollection<Response.WorkItemResponse>>;