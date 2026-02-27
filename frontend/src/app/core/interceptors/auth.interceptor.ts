import { HttpInterceptorFn } from '@angular/common/http';

const TOKEN_KEY = 'taskmanager_token';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = typeof window !== 'undefined' ? localStorage.getItem(TOKEN_KEY) : null;

  if (token) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(authReq);
  }

  return next(req);
};
