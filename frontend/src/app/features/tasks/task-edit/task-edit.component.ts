import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TaskFormComponent } from '../task-form/task-form.component';
import { TaskService } from '../../../core/services/task.service';
import { Task, TaskUpdateRequest } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-edit',
  standalone: true,
  imports: [CommonModule, TaskFormComponent],
  template: `
    <section>
      <h1>Edit Task</h1>
      <div *ngIf="loading && !task" class="loading">Loading task...</div>
      <div *ngIf="error" class="error">{{ error }}</div>
      <app-task-form
        *ngIf="task"
        [initialValue]="task"
        [loading]="saving"
        (submitted)="onSubmit($event)">
      </app-task-form>
    </section>
  `,
  styles: [`
    h1 {
      margin-bottom: 1rem;
    }
    .loading, .error {
      padding: 1rem;
    }
    .error {
      color: #b91c1c;
    }
  `]
})
export class TaskEditComponent implements OnInit {
  task: Task | null = null;
  loading = false;
  saving = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private taskService: TaskService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadTask(id);
    }
  }

  loadTask(id: string): void {
    this.loading = true;
    this.error = null;

    this.taskService.getById(id).subscribe({
      next: (task) => {
        this.task = task;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error?.error || 'Failed to load task.';
        this.loading = false;
      }
    });
  }

  onSubmit(value: TaskUpdateRequest): void {
    if (!this.task) return;

    this.saving = true;
    this.error = null;

    this.taskService.update(this.task.id, value).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/tasks', this.task!.id]);
      },
      error: (err) => {
        this.saving = false;
        this.error = err?.error?.error || 'Failed to update task.';
      }
    });
  }
}
