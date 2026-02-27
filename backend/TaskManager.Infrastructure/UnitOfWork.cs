using System.Threading;
using System.Threading.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        public UnitOfWork(
            ApplicationDbContext dbContext,
            ITaskRepository taskRepository,
            IUserRepository userRepository)
        {
            _dbContext = dbContext;
            Tasks = taskRepository;
            Users = userRepository;
        }

        public ITaskRepository Tasks { get; }

        public IUserRepository Users { get; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _dbContext.DisposeAsync();
        }
    }
}
