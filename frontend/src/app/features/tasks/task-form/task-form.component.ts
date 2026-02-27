import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Task, TaskCreateRequest } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>
        Title
        <input type="text" formControlName="title" />
      </label>
      <div class="error" *ngIf="form.controls['title'].invalid && form.controls['title'].touched">
        Title is required.
      </div>

      <label>
        Description
        <textarea formControlName="description"></textarea>
      </label>

      <label>
        Due Date
        <input type="date" formControlName="dueDate" />
      </label>

      <label>
        Status
        <select formControlName="status">
          <option value="Pending">Pending</option>
          <option value="InProgress">In Progress</option>
          <option value="Completed">Completed</option>
        </select>
      </label>

      <label>
        Remarks
        <textarea formControlName="remarks"></textarea>
      </label>

      <button type="submit" [disabled]="form.invalid || loading">
        {{ loading ? 'Saving...' : 'Save' }}
      </button>
    </form>
  `,
  styles: [`
    form {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-width: 600px;
    }
    label {
      display: flex;
      flex-direction: column;
      font-size: 0.9rem;
    }
    input, select, textarea {
      padding: 0.4rem;
      border-radius: 4px;
      border: 1px solid #cbd5f5;
    }
    textarea {
      min-height: 80px;
    }
    button {
      margin-top: 0.5rem;
      align-self: flex-start;
      padding: 0.5rem 0.9rem;
      border-radius: 4px;
      border: none;
      background-color: #2563eb;
      color: #f9fafb;
      font-weight: 500;
      cursor: pointer;
    }
    button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .error {
      color: #b91c1c;
      font-size: 0.8rem;
    }
  `]
})
export class TaskFormComponent implements OnChanges {
  @Input() initialValue: Task | null = null;
  @Input() loading = false;
  @Output() submitted = new EventEmitter<TaskCreateRequest>();

  form!: ReturnType<FormBuilder['group']>;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      title: ['', [Validators.required]],
      description: [''],
      dueDate: [''],
      status: ['Pending', [Validators.required]],
      remarks: ['']
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && this.initialValue) {
      this.form.patchValue({
        title: this.initialValue.title,
        description: this.initialValue.description,
        dueDate: this.initialValue.dueDate ? this.initialValue.dueDate.substring(0, 10) : '',
        status: this.initialValue.status,
        remarks: this.initialValue.remarks || ''
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value;
    const payload: TaskCreateRequest = {
      title: value.title || '',
      description: value.description || '',
      dueDate: value.dueDate ? value.dueDate : null,
      status: value.status || 'Pending',
      remarks: value.remarks || ''
    };

    this.submitted.emit(payload);
  }
}
