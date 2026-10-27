using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;

namespace DotNetCqrsMediatRPipelineLab.Contract.WorkItems;

public class WorkItemErrors
{
    public static Error NotFound(Guid id)
        => new(
            "WorkItem.NotFound",
            $"Work item '{id}' was not found.");
    
    public static Error ConcurrencyConflict()
        => new(
            "WorkItem.ConcurrencyConflict",
            "The work item was changed by another request. " +
            "Reload the latest data and try again.");
    
    public static Error InvalidState(string message)
        => new(
            "WorkItem.InvalidState",
            message);
}