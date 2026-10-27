using DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Results;
using MediatR;

namespace DotNetCqrsMediatRPipelineLab.Contract.Abstractions.Messages;

public interface ICommand : IRequest<Result>;
//các interface yêu cầu thay đổi dữ liệu  | có trả payload và không
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

//Abstraction: là nhuwxg thằng quy ước được dùng chung

//Message: hiểu đơn giản là đối tượng được gửi qua MediatR
//ISender -> Message -> MediatR -> Tìm handler

//TResponse: là kiểu dữ liệu mà Command mốn nhận lại sau khi Handler xử lý xong