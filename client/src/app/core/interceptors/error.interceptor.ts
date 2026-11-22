import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../error/notification.service';
import { SessionIdService } from '../session/session-id.service';

const MESSAGES: Record<string, string> = {
  invalid_n: 'המספר n חייב להיות בין 1 ל-20.',
  invalid_page: 'מספר הדף אינו תקין.',
  session_not_found: 'ההפעלה פגה. התחילי מחדש.',
  permutations_exhausted: 'אין עוד תמורות.',
};

/** Translates the server's ProblemDetails (field "code") into a user message. */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notify = inject(NotificationService);
  const session = inject(SessionIdService);
  const router = inject(Router);

  return next(req).pipe(catchError((err: HttpErrorResponse) => {
    const code: string | undefined = err.error?.code;
    notify.show((code && MESSAGES[code]) || (err.status === 0 ? 'אין חיבור לשרת.' : 'אירעה שגיאה בלתי צפויה.'));
    if (code === 'session_not_found') {
      session.clear();
      void router.navigate(['/']);
    }
    return throwError(() => err);
  }));
};
