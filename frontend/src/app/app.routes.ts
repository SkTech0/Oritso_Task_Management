import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { TaskListComponent } from './features/tasks/task-list/task-list.component';
import { TaskCreateComponent } from './features/tasks/task-create/task-create.component';
import { TaskEditComponent } from './features/tasks/task-edit/task-edit.component';
import { TaskDetailsComponent } from './features/tasks/task-details/task-details.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { 
    path: '', 
    redirectTo: 'login', 
    pathMatch: 'full' 
  },
  {
    path: 'tasks',
    canActivate: [authGuard],
    children: [
      { path: '', component: TaskListComponent },
      { path: 'create', component: TaskCreateComponent },
      { path: ':id', component: TaskDetailsComponent },
      { path: ':id/edit', component: TaskEditComponent }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
