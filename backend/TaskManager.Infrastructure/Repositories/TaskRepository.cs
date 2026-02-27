using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories
{
    public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TaskRepository(ApplicationDbContext context)
            : base(context)
        {
            _dbContext = context;
        }

        public async Task<(IReadOnlyCollection<TaskItem> Items, int TotalCount)> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            query = query.Trim();

            var baseQuery = _dbContext.Tasks
                .AsNoTracking()
                .Where(t =>
                    EF.Functions.ILike(t.Title, $"%{query}%") ||
                    EF.Functions.ILike(t.Description, $"%{query}%") ||
                    EF.Functions.ILike(t.Remarks, $"%{query}%") ||
                    EF.Functions.ILike(t.Status, $"%{query}%"));

            var totalCount = await baseQuery.CountAsync(cancellationToken);
            var items = await baseQuery
                .OrderByDescending(t => t.CreatedOn)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
