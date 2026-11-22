import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { SessionIdService } from '../session/session-id.service';

export const sessionIdInterceptor: HttpInterceptorFn = (req, next) => {
  const id = inject(SessionIdService).get();
  return next(id ? req.clone({ setHeaders: { 'X-Session-Id': id } }) : req);
};
