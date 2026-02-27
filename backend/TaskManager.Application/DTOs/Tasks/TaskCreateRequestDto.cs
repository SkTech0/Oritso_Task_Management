using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Tasks
{
    public class TaskCreateRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(1000)]
        public string Remarks { get; set; } = string.Empty;
    }
}
