using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;
//Tạo một cái khuôn Handler nhận một loại Command là TCommand
//IRequestHandler<TCommand, Result>: nới với MediatR handler này nhận TCommand và trả về Result
//where TCommand : ICommand: buộc command đó phải là một command hợp lệ



    //interface xử lý command
public interface ICommandHandler<in TCommand, TResponse>: IRequestHandler<TCommand, Result<TResponse>> 
    where TCommand : ICommand<TResponse>;
//thằng này nhận một TCommand
//Command đó phải implement ICommand<TResponse>
//và handler này trả về Result<TResponse>