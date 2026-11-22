import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-control-panel',
  template: `
    <div class="row">
      <button class="btn primary" [disabled]="busy()" (click)="next.emit()">הבא</button>
      <button class="btn" [disabled]="busy()" (click)="openAll.emit()">כל התמורות</button>
      <button class="btn ghost" [disabled]="busy()" (click)="reset.emit()">איפוס</button>
    </div>`,
})
export class ControlPanelComponent {
  busy = input(false);
  next = output<void>();
  openAll = output<void>();
  reset = output<void>();
}
