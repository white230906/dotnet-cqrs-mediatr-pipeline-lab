using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

namespace DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Queries;

public sealed record GetWorkItemByIdQuery(Guid Id) : IQuery<Response.WorkItemResponse>;