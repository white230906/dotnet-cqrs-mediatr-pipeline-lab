using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

namespace DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Commands;

public record class CreateWorkItemCommand(string Title): ICommand<Response.WorkItemResponse>;