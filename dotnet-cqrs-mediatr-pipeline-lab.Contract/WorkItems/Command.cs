using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

namespace DotNetCqrsMediatRPipelineLab.Contract.WorkItems;

public static class Command
{
    public sealed record CreateWorkItem(string Title) : ICommand<Response.WorkItemResponse>;

    public sealed record ChangeWorkItemStatus(Guid Id, Request.WorkItemStatusRequest Status, 
        uint ExpectedVersion) : ICommand<Response.WorkItemResponse>;
}
