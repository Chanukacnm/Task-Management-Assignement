import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Sign in - Task Management',
    loadComponent: () =>
      import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'tasks',
    title: 'My Tasks - Task Management',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/tasks/tasks-page.component').then((m) => m.TasksPageComponent)
  },
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: '**', redirectTo: 'tasks' }
];
