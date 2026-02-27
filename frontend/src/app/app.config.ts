import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { FormBuilder } from '@angular/forms';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    // Provide FormBuilder directly using factory (works in both dev and production)
    { provide: FormBuilder, useFactory: () => new FormBuilder() },
    // Provide HttpClient with auth interceptor
    provideHttpClient(withInterceptors([authInterceptor])),
    // Provide router with component input binding
    provideRouter(routes, withComponentInputBinding())
  ]
};
