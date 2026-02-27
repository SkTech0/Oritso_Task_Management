import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskFormComponent } from '../task-form/task-form.component';
import { TaskService } from '../../../core/services/task.service';
import { Router } from '@angular/router';
import { TaskCreateRequest } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-create',
  standalone: true,
  imports: [CommonModule, TaskFormComponent],
  template: `
    <section>
      <h1>Create Task</h1>
      <app-task-form
        [initialValue]="null"
        [loading]="loading"
        (submitted)="onSubmit($event)">
      </app-task-form>
      <div class="error" *ngIf="error">{{ error }}</div>
    </section>
  `,
  styles: [`
    h1 {
      margin-bottom: 1rem;
    }
    .error {
      color: #b91c1c;
      margin-top: 0.5rem;
    }
  `]
})
export class TaskCreateComponent {
  loading = false;
  error: string | null = null;

  constructor(
    private taskService: TaskService,
    private router: Router
  ) { }

  onSubmit(value: TaskCreateRequest): void {
    this.loading = true;
    this.error = null;

    this.taskService.create(value).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/tasks']);
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.error || 'Failed to create task.';
      }
    });
  }
}
