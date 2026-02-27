import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgIf } from '@angular/common';

const TOKEN_KEY = 'taskmanager_token';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, NgIf],
  template: `
    <nav class="navbar">
      <div class="navbar-brand">
        <a routerLink="/" routerLinkActive="active">Task Manager</a>
      </div>
      <div class="navbar-links" *ngIf="isAuthenticated(); else authLinks">
        <a routerLink="/tasks" routerLinkActive="active">Tasks</a>
        <button type="button" (click)="onLogout()">Logout</button>
      </div>
      <ng-template #authLinks>
        <div class="navbar-links">
          <a routerLink="/login" routerLinkActive="active">Login</a>
          <a routerLink="/register" routerLinkActive="active">Register</a>
        </div>
      </ng-template>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem 1rem;
      background-color: #1e293b;
      color: #f9fafb;
    }
    .navbar a {
      color: #e5e7eb;
      margin-right: 1rem;
      text-decoration: none;
      font-weight: 500;
    }
    .navbar a.active {
      text-decoration: underline;
    }
    .navbar button {
      background-color: #ef4444;
      color: #f9fafb;
      border: none;
      padding: 0.4rem 0.8rem;
      border-radius: 4px;
      cursor: pointer;
    }
    .navbar button:hover {
      background-color: #b91c1c;
    }
  `]
})
export class NavbarComponent {
  isAuthenticated(): boolean {
    if (typeof window === 'undefined') return false;
    const token = localStorage.getItem(TOKEN_KEY);
    return !!token;
  }

  onLogout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(TOKEN_KEY);
      window.location.href = '/login';
    }
  }
}
