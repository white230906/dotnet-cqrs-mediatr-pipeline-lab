namespace DotNetCqrsMediatRPipelineLab.Domain.Abstractions.Repositories;

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken); 
 
    Task<int> SaveChangesAsync(CancellationToken cancellationToken); 
 
    Task CommitAsync(CancellationToken cancellationToken); 
 
    Task RollbackAsync(CancellationToken cancellationToken);
}