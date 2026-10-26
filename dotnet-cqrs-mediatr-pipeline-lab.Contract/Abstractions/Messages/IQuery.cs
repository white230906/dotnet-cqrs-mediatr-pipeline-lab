using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

public interface IQuery<out TResponse> : IRequest<TResponse>;