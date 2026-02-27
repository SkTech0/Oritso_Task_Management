using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(TaskCreateRequestDto request, CancellationToken cancellationToken = default);

        Task<TaskDto?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<TaskDto>> GetAllTasksAsync(CancellationToken cancellationToken = default);

        Task<TaskDto?> UpdateTaskAsync(Guid id, TaskUpdateRequestDto request, CancellationToken cancellationToken = default);

        Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);

        Task<TaskSearchResultDto> SearchTasksAsync(string query, CancellationToken cancellationToken = default);
    }
}
