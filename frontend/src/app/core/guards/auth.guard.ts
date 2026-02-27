import { CanActivateFn } from '@angular/router';

const TOKEN_KEY = 'taskmanager_token';

export const authGuard: CanActivateFn = (route, state) => {
  if (typeof window === 'undefined') {
    return false;
  }

  const token = localStorage.getItem(TOKEN_KEY);
  const isAuthenticated = !!token;

  if (isAuthenticated) {
    return true;
  }

  if (window.location.pathname !== '/login') {
    window.location.href = '/login';
  }

  return false;
};
