import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TaskService } from '../../../core/services/task.service';
import { Task } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, FormsModule],
  template: `
    <section>
      <div class="header">
        <h1>Tasks</h1>
        <a routerLink="/tasks/create" class="btn-primary">Create Task</a>
      </div>

      <div class="filters">
        <input 
          type="text" 
          placeholder="Search tasks..." 
          [(ngModel)]="searchQuery"
          (input)="onSearch()"
          class="search-input" />
        <select [(ngModel)]="statusFilter" (change)="onSearch()" class="status-filter">
          <option value="">All Status</option>
          <option value="Pending">Pending</option>
          <option value="InProgress">In Progress</option>
          <option value="Completed">Completed</option>
        </select>
      </div>

      <div *ngIf="loading" class="loading">Loading tasks...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <div *ngIf="!loading && !error && tasks.length === 0" class="empty">
        No tasks found. <a routerLink="/tasks/create">Create one</a>?
      </div>

      <div class="task-grid" *ngIf="!loading && tasks.length > 0">
        <div *ngFor="let task of tasks" class="task-card" [class.completed]="task.status === 'Completed'">
          <div class="task-header">
            <h3>
              <a [routerLink]="['/tasks', task.id]">{{ task.title }}</a>
            </h3>
            <span class="status-badge" [class]="'status-' + task.status.toLowerCase()">
              {{ task.status }}
            </span>
          </div>
          <p class="description">{{ task.description || 'No description' }}</p>
          <div class="task-meta">
            <span *ngIf="task.dueDate">
              Due: {{ task.dueDate | date:'short' }}
            </span>
            <span *ngIf="!task.dueDate" class="no-due-date">No due date</span>
            <span class="created-by">Created by: {{ task.createdByName }}</span>
          </div>
          <div class="task-actions">
            <a [routerLink]="['/tasks', task.id]" class="btn-link">View</a>
            <a [routerLink]="['/tasks', task.id, 'edit']" class="btn-link">Edit</a>
            <button (click)="deleteTask(task.id)" class="btn-danger">Delete</button>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }
    h1 {
      margin: 0;
    }
    .btn-primary {
      padding: 0.5rem 1rem;
      background-color: #2563eb;
      color: #f9fafb;
      text-decoration: none;
      border-radius: 4px;
      font-weight: 500;
    }
    .filters {
      display: flex;
      gap: 1rem;
      margin-bottom: 1.5rem;
    }
    .search-input, .status-filter {
      padding: 0.5rem;
      border-radius: 4px;
      border: 1px solid #cbd5f5;
    }
    .search-input {
      flex: 1;
    }
    .task-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1rem;
    }
    .task-card {
      padding: 1rem;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      background-color: #ffffff;
    }
    .task-card.completed {
      opacity: 0.7;
    }
    .task-header {
      display: flex;
      justify-content: space-between;
      align-items: start;
      margin-bottom: 0.5rem;
    }
    .task-header h3 {
      margin: 0;
      flex: 1;
    }
    .task-header h3 a {
      color: #1e293b;
      text-decoration: none;
    }
    .status-badge {
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-size: 0.75rem;
      font-weight: 600;
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
    .description {
      color: #64748b;
      font-size: 0.9rem;
      margin: 0.5rem 0;
    }
    .task-meta {
      font-size: 0.85rem;
      color: #64748b;
      margin: 0.5rem 0;
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }
    .created-by {
      font-style: italic;
    }
    .task-actions {
      display: flex;
      gap: 0.5rem;
      margin-top: 1rem;
    }
    .btn-link {
      padding: 0.25rem 0.5rem;
      color: #2563eb;
      text-decoration: none;
      font-size: 0.85rem;
    }
    .btn-danger {
      padding: 0.25rem 0.5rem;
      background-color: #ef4444;
      color: #f9fafb;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 0.85rem;
    }
    .loading, .error, .empty {
      text-align: center;
      padding: 2rem;
    }
    .error {
      color: #b91c1c;
    }
  `]
})
export class TaskListComponent implements OnInit {
  tasks: Task[] = [];
  loading = false;
  error: string | null = null;
  searchQuery = '';
  statusFilter = '';

  constructor(private taskService: TaskService) { }

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading = true;
    this.error = null;

    this.taskService.getAll().subscribe({
      next: (tasks) => {
        this.tasks = tasks;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error?.error || 'Failed to load tasks.';
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.loading = true;
    this.error = null;

    if (this.searchQuery.trim()) {
      // If there's a search query, use the search endpoint
      this.taskService.search(this.searchQuery).subscribe({
        next: (result) => {
          // Filter by status on client side if status filter is set
          let filteredTasks = result.tasks;
          if (this.statusFilter) {
            filteredTasks = filteredTasks.filter(task => task.status === this.statusFilter);
          }
          this.tasks = filteredTasks;
          this.loading = false;
        },
        error: (err) => {
          this.error = err?.error?.error || 'Search failed.';
          this.loading = false;
        }
      });
    } else {
      // If no search query, get all tasks and filter by status if needed
      this.taskService.getAll().subscribe({
        next: (tasks) => {
          // Filter by status on client side if status filter is set
          if (this.statusFilter) {
            this.tasks = tasks.filter(task => task.status === this.statusFilter);
          } else {
            this.tasks = tasks;
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err?.error?.error || 'Failed to load tasks.';
          this.loading = false;
        }
      });
    }
  }

  deleteTask(id: string): void {
    if (confirm('Are you sure you want to delete this task?')) {
      this.taskService.delete(id).subscribe({
        next: () => {
          this.loadTasks();
        },
        error: (err) => {
          this.error = err?.error?.error || 'Failed to delete task.';
        }
      });
    }
  }
}
