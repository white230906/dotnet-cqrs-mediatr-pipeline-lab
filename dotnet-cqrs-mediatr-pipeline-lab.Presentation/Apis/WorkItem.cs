using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;
using DotNetCqrsMediatRPipelineLab.Presentation.Abstractions;
using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Presentation.Apis;

public class WorkItem: ApiEndpoint, ICarterModule
{
    private const string Route = "/api/v{version:apiVersion}/work-items";
    //thằng swagger explore sẽ lấy dữ liệu đẹp hơn từ chỗ này, explore của thằng Carter
    //cộng thêm nữa là DTO khá rõ ràng nên được như vậy
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var versionedApi = app.NewVersionedApi("WorkItems");
        var group = versionedApi
            .MapGroup(Route)
            .HasApiVersion(1);

        group.MapGet("", GetAll)
            .WithName("GetWorkItems")
            .Produces<IReadOnlyCollection<Response.WorkItemResponse>>();

        group.MapGet("{id:guid}", GetById)
            .WithName("GetWorkItemById")
            .Produces<Response.WorkItemResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("", Create)
            .WithName("CreateWorkItem")
            .Produces<Response.WorkItemResponse>(
                StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("{id:guid}/status", UpdateStatus)
            .WithName("UpdateWorkItemStatus")
            .Produces<Response.WorkItemResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
        
        group.MapGet("{id:guid}/history", GetHistory) 
            .WithName("GetWorkItemStatusHistory") 
            .Produces<IReadOnlyCollection< 
                    Response.WorkItemStatusHistoryResponse>>() 
            .ProducesProblem(StatusCodes.Status404NotFound);

    }
    
    private static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken) 
    { 
        var result = await sender.Send(new Query.GetWorkItems(), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result);
    }

    private static async Task<IResult> GetById(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new Query.GetWorkItemById(id), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result);
    }

    private static async Task<IResult> Create(Request.CreateWorkItemRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new Command.CreateWorkItem(request.Title), cancellationToken);

        return result.IsSuccess ? Results.Created($"/api/v1/work-items/{result.Value.Id}", result.Value) : Failure(result);
    }

    private static async Task<IResult> UpdateStatus(Guid id, Request.UpdateWorkItemStatusRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new Command.ChangeWorkItemStatus(id, request.Status, request.Version);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result);
    }
    
    private static async Task<IResult> GetHistory(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new Query.GetWorkItemHistory(id), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result);
    }
}