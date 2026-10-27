using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand, IRequest<Result>;

    //interface xử lý command
public interface ICommandHandler<in TCommand, TResponse>: IRequestHandler<TCommand, Result<TResponse>> 
    where TCommand : ICommand<TResponse>;