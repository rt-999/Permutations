import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { PermutationStore } from '../../state/permutation.store';
import { BigFormatPipe } from '../../shared/pipes/big-format.pipe';
import { ControlPanelComponent } from './control-panel.component';
import { SinglePermutationComponent } from './single-permutation.component';

@Component({
  selector: 'app-explorer',
  imports: [ControlPanelComponent, SinglePermutationComponent, BigFormatPipe],
  template: `
    <section class="card">
     <header class="head">
  <h1 class="total">סה"כ האפשרויות לסדר <span dir="ltr">{{ store.n() }}</span> מספרים בשורה הוא: <span dir="ltr">{{ store.total() | bigFormat }}</span>.</h1>
</header>
      <app-single-permutation [item]="store.lastShown()" [total]="store.total()" />
      <app-control-panel [busy]="store.loading()" (next)="store.next()" (openAll)="openAll()" (reset)="reset()" />
    </section>`,
})
export class ExplorerComponent {
  protected readonly store = inject(PermutationStore);
  private readonly router = inject(Router);

  protected async openAll(): Promise<void> {
    if (await this.store.openBrowse()) void this.router.navigate(['/explore/all']);
  }

  protected reset(): void {
    this.store.reset();
    void this.router.navigate(['/']);
  }
}
