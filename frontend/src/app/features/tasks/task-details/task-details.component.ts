import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';
import { Task } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-details',
  standalone: true,
  imports: [CommonModule, DatePipe, RouterLink],
  template: `
    <section>
      <div *ngIf="loading" class="loading">Loading task...</div>
      <div *ngIf="error" class="error">{{ error }}</div>
      
      <div *ngIf="task" class="task-details">
        <div class="header">
          <h1>{{ task.title }}</h1>
          <div class="actions">
            <a [routerLink]="['/tasks', task.id, 'edit']" class="btn-primary">Edit</a>
            <button (click)="deleteTask()" class="btn-danger">Delete</button>
          </div>
        </div>

        <div class="status-badge" [class]="'status-' + task.status.toLowerCase()">
          {{ task.status }}
        </div>

        <div class="content">
          <div class="section">
            <h3>Description</h3>
            <p>{{ task.description || 'No description provided.' }}</p>
          </div>

          <div class="section">
            <h3>Due Date</h3>
            <p *ngIf="task.dueDate">{{ task.dueDate | date:'full' }}</p>
            <p *ngIf="!task.dueDate" class="muted">No due date set</p>
          </div>

          <div class="section" *ngIf="task.remarks">
            <h3>Remarks</h3>
            <p>{{ task.remarks }}</p>
          </div>

          <div class="meta">
            <p><strong>Created On:</strong> {{ task.createdOn | date:'short' }}</p>
            <p><strong>Last Updated On:</strong> {{ task.updatedOn | date:'short' }}</p>
            <p><strong>Created By:</strong> {{ task.createdByName }} (ID: {{ task.createdByUserId }})</p>
            <p><strong>Last Updated By:</strong> {{ task.updatedByName }} (ID: {{ task.updatedByUserId }})</p>
          </div>
        </div>

        <div class="footer">
          <a routerLink="/tasks" class="btn-link">← Back to Tasks</a>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: start;
      margin-bottom: 1rem;
    }
    h1 {
      margin: 0;
      flex: 1;
    }
    .actions {
      display: flex;
      gap: 0.5rem;
    }
    .btn-primary {
      padding: 0.5rem 1rem;
      background-color: #2563eb;
      color: #f9fafb;
      text-decoration: none;
      border-radius: 4px;
      font-weight: 500;
    }
    .btn-danger {
      padding: 0.5rem 1rem;
      background-color: #ef4444;
      color: #f9fafb;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 500;
    }
    .status-badge {
      display: inline-block;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      font-weight: 600;
      margin-bottom: 1.5rem;
    }
    .status-pending {
      background-color: #fef3c7;
      color: #92400e;
    }
    .status-inprogress {
      background-color: #dbeafe;
      color: #1e40af;
    }
    .status-completed {
      background-color: #d1fae5;
      color: #065f46;
    }
    .content {
      background-color: #f9fafb;
      padding: 1.5rem;
      border-radius: 8px;
      margin-bottom: 1.5rem;
    }
    .section {
      margin-bottom: 1.5rem;
    }
    .section:last-child {
      margin-bottom: 0;
    }
    .section h3 {
      margin: 0 0 0.5rem 0;
      color: #1e293b;
    }
    .section p {
      margin: 0;
      color: #64748b;
    }
    .muted {
      color: #94a3b8;
      font-style: italic;
    }
    .meta {
      margin-top: 1.5rem;
      padding-top: 1.5rem;
      border-top: 1px solid #e5e7eb;
      font-size: 0.9rem;
      color: #64748b;
    }
    .footer {
      margin-top: 1.5rem;
    }
    .btn-link {
      color: #2563eb;
      text-decoration: none;
    }
    .loading, .error {
      padding: 2rem;
      text-align: center;
    }
    .error {
      color: #b91c1c;
    }
  `]
})
export class TaskDetailsComponent implements OnInit {
  task: Task | null = null;
  loading = false;
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

  deleteTask(): void {
    if (!this.task) return;

    if (confirm('Are you sure you want to delete this task?')) {
      this.taskService.delete(this.task.id).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (err) => {
          this.error = err?.error?.error || 'Failed to delete task.';
        }
      });
    }
  }
}
