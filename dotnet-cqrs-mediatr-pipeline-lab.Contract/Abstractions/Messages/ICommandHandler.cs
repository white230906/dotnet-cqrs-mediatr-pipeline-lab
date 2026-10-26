using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;
    
public interface ICommandHandler<in TCommand, TResponse>: IRequestHandler<TCommand, TResponse> 
    where TCommand : ICommand<TResponse>;