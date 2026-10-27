using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;
using DotNetCqrsMediatRPipelineLab.Domain.WorkItems;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems;

public static class WorkItemMappings
{
    public static Response.WorkItemResponse ToResponse(WorkItem item)
    {
        return new Response.WorkItemResponse(
            item.Id,
            item.Title,
            item.Status.ToString(),
            item.CreatedAt.UtcDateTime,
            item.Version);
    }
    
    public static Response.WorkItemStatusHistoryResponse ToResponse(WorkItemStatusHistory history)
    {
        return new Response.WorkItemStatusHistoryResponse(
            history.Id,
            history.WorkItemId,
            history.PreviousStatus.ToString(),
            history.NewStatus.ToString(),
            history.ChangedAt.UtcDateTime);
    }
    
    public static WorkItemStatus ToDomainStatus(Request.WorkItemStatusRequest status)
    {
        return status switch
        {
            Request.WorkItemStatusRequest.Pending =>
                WorkItemStatus.Pending,

            Request.WorkItemStatusRequest.InProgress =>
                WorkItemStatus.InProgress,

            Request.WorkItemStatusRequest.Completed =>
                WorkItemStatus.Completed,

            _ => throw new ArgumentOutOfRangeException(
                nameof(status), status, "Unsupported work item status.")
        };
    }
}