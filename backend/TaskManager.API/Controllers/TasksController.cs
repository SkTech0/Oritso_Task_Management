using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> Create([FromBody] TaskCreateRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var created = await _taskService.CreateTaskAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<TaskDto>>> GetAll(CancellationToken cancellationToken)
        {
            var tasks = await _taskService.GetAllTasksAsync(cancellationToken);
            return Ok(tasks);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] TaskUpdateRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var updated = await _taskService.UpdateTaskAsync(id, request, cancellationToken);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _taskService.DeleteTaskAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<TaskSearchResultDto>> Search([FromQuery(Name = "q")] string q, CancellationToken cancellationToken)
        {
            var result = await _taskService.SearchTasksAsync(q, cancellationToken);
            _logger.LogInformation("Search performed for query {Query} with {Count} results.", q, result.TotalCount);
            return Ok(result);
        }
    }
}

