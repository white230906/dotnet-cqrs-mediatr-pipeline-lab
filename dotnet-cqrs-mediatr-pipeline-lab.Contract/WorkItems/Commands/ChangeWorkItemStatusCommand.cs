using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

namespace DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Commands;

public  record ChangeWorkItemStatusCommand(
    Guid Id,
    Request.WorkItemStatusRequest Status,
    uint ExpectedVersion)
    : ICommand<Response.WorkItemResponse>;