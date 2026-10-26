using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
