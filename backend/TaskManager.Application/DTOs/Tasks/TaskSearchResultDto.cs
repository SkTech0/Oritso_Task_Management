using System;
using System.Collections.Generic;

namespace TaskManager.Application.DTOs.Tasks
{
    public class TaskSearchResultDto
    {
        public IReadOnlyCollection<TaskDto> Items { get; set; } = Array.Empty<TaskDto>();

        public int TotalCount { get; set; }
    }
}
