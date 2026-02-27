using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        ITaskRepository Tasks { get; }

        IUserRepository Users { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        void Update(TEntity entity);

        void Remove(TEntity entity);
    }

    public interface ITaskRepository : IGenericRepository<TaskItem>
    {
        Task<(IReadOnlyCollection<TaskItem> Items, int TotalCount)> SearchAsync(
            string query,
            CancellationToken cancellationToken = default);
    }

    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
