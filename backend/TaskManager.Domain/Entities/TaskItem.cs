using System;

namespace TaskManager.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public string Status { get; set; } = "Pending";

        public string Remarks { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public Guid CreatedByUserId { get; set; }

        public Guid UpdatedByUserId { get; set; }

        public User CreatedByUser { get; set; } = null!;

        public User UpdatedByUser { get; set; } = null!;
    }
}
