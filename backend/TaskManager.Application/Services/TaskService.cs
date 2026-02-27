using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<TaskService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<TaskDto> CreateTaskAsync(TaskCreateRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var now = DateTime.UtcNow;
            var entity = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                DueDate = request.DueDate.HasValue ? DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc) : null,
                Status = request.Status.Trim(),
                Remarks = request.Remarks?.Trim() ?? string.Empty,
                CreatedOn = now,
                UpdatedOn = now,
                CreatedByUserId = userId,
                UpdatedByUserId = userId
            };

            await _unitOfWork.Tasks.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} created by {UserId}", entity.Id, userId);

            var createdBy = await _unitOfWork.Users.GetByIdAsync(entity.CreatedByUserId, cancellationToken)
                           ?? throw new InvalidOperationException("CreatedBy user not found.");
            var updatedBy = await _unitOfWork.Users.GetByIdAsync(entity.UpdatedByUserId, cancellationToken)
                           ?? throw new InvalidOperationException("UpdatedBy user not found.");

            return MapToDto(entity, createdBy, updatedBy);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
            if (task == null)
            {
                return null;
            }

            var createdBy = await _unitOfWork.Users.GetByIdAsync(task.CreatedByUserId, cancellationToken)
                           ?? throw new InvalidOperationException("CreatedBy user not found.");
            var updatedBy = await _unitOfWork.Users.GetByIdAsync(task.UpdatedByUserId, cancellationToken)
                           ?? throw new InvalidOperationException("UpdatedBy user not found.");

            return MapToDto(task, createdBy, updatedBy);
        }

        public async Task<IReadOnlyCollection<TaskDto>> GetAllTasksAsync(CancellationToken cancellationToken = default)
        {
            var tasks = await _unitOfWork.Tasks.GetAllAsync(cancellationToken);

            var userIds = tasks.SelectMany(t => new[] { t.CreatedByUserId, t.UpdatedByUserId }).Distinct().ToList();
            var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);

            var userLookup = users.Where(u => userIds.Contains(u.Id)).ToDictionary(u => u.Id);

            var result = new List<TaskDto>();

            foreach (var task in tasks)
            {
                if (!userLookup.TryGetValue(task.CreatedByUserId, out var createdBy))
                {
                    throw new InvalidOperationException("CreatedBy user not found.");
                }
                if (!userLookup.TryGetValue(task.UpdatedByUserId, out var updatedBy))
                {
                    throw new InvalidOperationException("UpdatedBy user not found.");
                }

                result.Add(MapToDto(task, createdBy, updatedBy));
            }

            return result;
        }

        public async Task<TaskDto?> UpdateTaskAsync(Guid id, TaskUpdateRequestDto request, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
            if (task == null)
            {
                return null;
            }

            var userId = _currentUserService.GetCurrentUserId();
            var now = DateTime.UtcNow;

            task.Title = request.Title.Trim();
            task.Description = request.Description?.Trim() ?? string.Empty;
            task.DueDate = request.DueDate.HasValue ? DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc) : null;
            task.Status = request.Status.Trim();
            task.Remarks = request.Remarks?.Trim() ?? string.Empty;
            task.UpdatedOn = now;
            task.UpdatedByUserId = userId;

            _unitOfWork.Tasks.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} updated by {UserId}", task.Id, userId);

            var createdBy = await _unitOfWork.Users.GetByIdAsync(task.CreatedByUserId, cancellationToken)
                           ?? throw new InvalidOperationException("CreatedBy user not found.");
            var updatedBy = await _unitOfWork.Users.GetByIdAsync(task.UpdatedByUserId, cancellationToken)
                           ?? throw new InvalidOperationException("UpdatedBy user not found.");

            return MapToDto(task, createdBy, updatedBy);
        }

        public async Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
            if (task == null)
            {
                return false;
            }

            _unitOfWork.Tasks.Remove(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} deleted", task.Id);
            return true;
        }

        public async Task<TaskSearchResultDto> SearchTasksAsync(string query, CancellationToken cancellationToken = default)
        {
            var trimmedQuery = query?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedQuery))
            {
                var all = await GetAllTasksAsync(cancellationToken);
                return new TaskSearchResultDto
                {
                    Items = all,
                    TotalCount = all.Count
                };
            }

            var (items, totalCount) = await _unitOfWork.Tasks.SearchAsync(trimmedQuery, cancellationToken);

            var userIds = items.SelectMany(t => new[] { t.CreatedByUserId, t.UpdatedByUserId }).Distinct().ToList();
            var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
            var userLookup = users.Where(u => userIds.Contains(u.Id)).ToDictionary(u => u.Id);

            var result = new List<TaskDto>();

            foreach (var task in items)
            {
                if (!userLookup.TryGetValue(task.CreatedByUserId, out var createdBy))
                {
                    throw new InvalidOperationException("CreatedBy user not found.");
                }
                if (!userLookup.TryGetValue(task.UpdatedByUserId, out var updatedBy))
                {
                    throw new InvalidOperationException("UpdatedBy user not found.");
                }

                result.Add(MapToDto(task, createdBy, updatedBy));
            }

            return new TaskSearchResultDto
            {
                Items = result,
                TotalCount = totalCount
            };
        }

        private TaskDto MapToDto(TaskItem task, User createdBy, User updatedBy)
        {
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                Remarks = task.Remarks,
                CreatedOn = task.CreatedOn,
                UpdatedOn = task.UpdatedOn,
                CreatedByUserId = task.CreatedByUserId,
                UpdatedByUserId = task.UpdatedByUserId,
                CreatedByName = createdBy.Name,
                UpdatedByName = updatedBy.Name
            };
        }
    }
}
