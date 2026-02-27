using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;

namespace TaskManager.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class
    {
        protected readonly DbContext Context;

        public GenericRepository(DbContext context)
        {
            Context = context;
        }

        public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await Context.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
            return items;
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

        public void Update(TEntity entity)
        {
            Context.Set<TEntity>().Update(entity);
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }
    }
}
