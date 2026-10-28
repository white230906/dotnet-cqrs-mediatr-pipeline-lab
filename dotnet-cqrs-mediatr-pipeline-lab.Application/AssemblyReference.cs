using System.Reflection;

namespace DotNetCqrsMediatRPipelineLab.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
//AssemblyReference đóng vai trò như là một class đánh dấu vị trí của một project vậy
//Nó cung cấp assembly cho MediatR hoặc FluentValidateion quét, tìm các
//Handler/Validator phù hợp và tự động đăng ký chúng vào DI

