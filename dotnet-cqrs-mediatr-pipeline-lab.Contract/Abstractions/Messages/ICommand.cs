using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

public interface ICommand : IRequest;

public interface ICommand<out TResponse> : IRequest<TResponse>;