import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationService } from './core/error/notification.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
    <main class="shell">
      @if (notify.error(); as msg) {
        <div class="alert" role="alert">{{ msg }} <button class="link" (click)="notify.clear()">סגור</button></div>
      }
      <router-outlet />
    </main>`,
})
export class AppComponent {
  protected readonly notify = inject(NotificationService);
}
