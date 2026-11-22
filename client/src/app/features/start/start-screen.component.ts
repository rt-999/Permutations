import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PermutationStore } from '../../state/permutation.store';
import { MAX_N, MIN_N } from '../../core/constants';

@Component({
  selector: 'app-start-screen',
  template: `
    <section class="card hero">
      <h1>תמורות</h1>
      <p class="lead">כל הסידורים האפשריים של המספרים 1 עד n. עד 20! ‏= 2,432,902,008,176,640,000.</p>
      <label for="n">בחרי n (בין {{ min }} ל-{{ max }})</label>
      <div class="row">
        <input id="n" type="number" dir="ltr" [min]="min" [max]="max" [value]="text()"
               (input)="text.set($any($event.target).value)" (keydown.enter)="start()" />
        <button class="btn primary" [disabled]="!valid() || store.loading()" (click)="start()">התחל</button>
      </div>
      @if (text() && !valid()) { <p class="hint">הזיני מספר שלם בין {{ min }} ל-{{ max }}.</p> }
    </section>`,
})
export class StartScreenComponent {
  protected readonly store = inject(PermutationStore);
  private readonly router = inject(Router);
  protected readonly min = MIN_N;
  protected readonly max = MAX_N;

  protected readonly text = signal('5');
  protected readonly valid = computed(() => {
    const n = Number(this.text());
    return this.text().trim() !== '' && Number.isInteger(n) && n >= MIN_N && n <= MAX_N;
  });

  protected async start(): Promise<void> {
    if (!this.valid()) return;
    if (await this.store.start(Number(this.text()))) void this.router.navigate(['/explore']);
  }
}
