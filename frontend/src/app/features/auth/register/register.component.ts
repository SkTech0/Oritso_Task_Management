import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="auth-container">
      <h1>Register</h1>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <label>
          Name
          <input type="text" formControlName="name" />
        </label>
        <div class="error" *ngIf="form.controls['name'].invalid && form.controls['name'].touched">
          Name is required.
        </div>

        <label>
          Email
          <input type="email" formControlName="email" />
        </label>
        <div class="error" *ngIf="form.controls['email'].invalid && form.controls['email'].touched">
          Valid email is required.
        </div>

        <label>
          Password
          <input type="password" formControlName="password" />
        </label>
        <div class="error" *ngIf="form.controls['password'].invalid && form.controls['password'].touched">
          Password is required (min 6 characters).
        </div>

        <button type="submit" [disabled]="form.invalid || loading">
          {{ loading ? 'Registering...' : 'Register' }}
        </button>

        <div class="error" *ngIf="error">{{ error }}</div>

        <p>
          Already have an account?
          <a routerLink="/login">Login</a>
        </p>
      </form>
    </section>
  `,
  styles: [`
    .auth-container {
      max-width: 400px;
      margin: 2rem auto;
      padding: 2rem;
      border-radius: 8px;
      background-color: #f9fafb;
      box-shadow: 0 2px 4px rgba(15, 23, 42, 0.1);
    }
    h1 {
      margin-bottom: 1rem;
      text-align: center;
    }
    form {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }
    label {
      display: flex;
      flex-direction: column;
      font-size: 0.9rem;
    }
    input {
      padding: 0.4rem;
      border-radius: 4px;
      border: 1px solid #cbd5f5;
    }
    button {
      margin-top: 0.5rem;
      padding: 0.6rem;
      border: none;
      border-radius: 4px;
      background-color: #16a34a;
      color: #f9fafb;
      font-weight: 600;
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
    p {
      margin-top: 0.5rem;
      font-size: 0.85rem;
      text-align: center;
    }
  `]
})
export class RegisterComponent {
  loading = false;
  error: string | null = null;
  form!: ReturnType<FormBuilder['group']>;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = null;

    this.authService.register(this.form.value as any).subscribe({
      next: () => {
        this.loading = false;
        window.location.href = '/tasks';
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.error || 'Registration failed.';
      }
    });
  }
}
