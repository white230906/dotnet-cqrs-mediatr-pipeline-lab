using DotNetCqrsMediatRPipelineLab.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCqrsMediatRPipelineLab.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(AssemblyReference.Assembly);
        //thằng Validator nó sẽ đi quét hết tất cả những thằng nào mà đăng kí
        //Fluent Validator và nó tự đăng kí vào trong DI
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(AssemblyReference.Assembly);
            //IRequestHandler
            configuration.AddOpenBehavior(
                typeof(ValidationPipelineBehavior<,>));

            configuration.AddOpenBehavior(
                typeof(RequestLoggingPipelineBehavior<,>));

            configuration.AddOpenBehavior(
                typeof(PerformancePipelineBehavior<,>));

            configuration.AddOpenBehavior(
                typeof(TransactionPipelineBehavior<,>));
        });

        services.AddSingleton(TimeProvider.System);
        
        return services;
    }
    
}