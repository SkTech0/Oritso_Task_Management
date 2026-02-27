using System;

namespace TaskManager.Application.DTOs.Tasks
{
    public class TaskDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public Guid CreatedByUserId { get; set; }

        public string CreatedByName { get; set; } = string.Empty;

        public Guid UpdatedByUserId { get; set; }

        public string UpdatedByName { get; set; } = string.Empty;
    }
}
