using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
//interface yêu cầu đọc dữ liệu