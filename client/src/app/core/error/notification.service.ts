import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly _error = signal<string | null>(null);
  readonly error = this._error.asReadonly();
  show(message: string): void { this._error.set(message); }
  clear(): void { this._error.set(null); }
}
