using System;
using System.Collections.Generic;

namespace TaskManager.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();

        public ICollection<TaskItem> UpdatedTasks { get; set; } = new List<TaskItem>();
    }
}
