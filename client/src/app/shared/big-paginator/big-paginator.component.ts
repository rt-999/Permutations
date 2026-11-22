import { Component, computed, input, output, signal } from '@angular/core';
import { BigFormatPipe } from '../pipes/big-format.pipe';
import { isDigits } from '../../core/util/bigint';

/** Presentational paginator that works with bigint (MatPaginator cannot). */
@Component({
  selector: 'app-big-paginator',
  imports: [BigFormatPipe],
  template: `
    <nav class="paginator">
      <button class="btn ghost" [disabled]="disabled() || atFirst()" (click)="first()">« ראשון</button>
      <button class="btn ghost" [disabled]="disabled() || atFirst()" (click)="step(-1)">‹ קודם</button>
      <span class="page-info" dir="ltr">{{ currentPage() | bigFormat }} / {{ totalPages() | bigFormat }}</span>
      <button class="btn ghost" [disabled]="disabled() || atLast()" (click)="step(1)">הבא ›</button>
      <button class="btn ghost" [disabled]="disabled() || atLast()" (click)="last()">אחרון »</button>
    </nav>
    <div class="jump">
      <label for="jump">קפיצה לדף</label>
      <input id="jump" dir="ltr" inputmode="numeric" [value]="jumpText()" (input)="onType($event)" (keydown.enter)="jump()" />
      <button class="btn" [disabled]="disabled() || !jumpTarget()" (click)="jump()">עבור</button>
      @if (jumpText() && !jumpTarget()) { <span class="hint">הזיני מספר בין 1 ל-{{ totalPages() | bigFormat }}</span> }
    </div>`,
})
export class BigPaginatorComponent {
  currentPage = input.required<bigint>();
  totalPages = input.required<bigint>();
  disabled = input(false);
  pageChange = output<bigint>();

  protected readonly jumpText = signal('');
  protected readonly atFirst = computed(() => this.currentPage() <= 1n);
  protected readonly atLast = computed(() => this.currentPage() >= this.totalPages());

  /** The jump target as bigint, or null when the text is not a valid page. */
  protected readonly jumpTarget = computed<bigint | null>(() => {
    const t = this.jumpText().trim().replace(/,/g, '');
    if (!isDigits(t)) return null;
    const v = BigInt(t);
    return v >= 1n && v <= this.totalPages() ? v : null;
  });

  protected first(): void { this.pageChange.emit(1n); }
  protected last(): void { this.pageChange.emit(this.totalPages()); }
  protected step(delta: number): void { this.pageChange.emit(this.currentPage() + BigInt(delta)); }

  protected onType(e: Event): void { this.jumpText.set((e.target as HTMLInputElement).value); }

  protected jump(): void {
    const target = this.jumpTarget();
    if (target !== null) { this.pageChange.emit(target); this.jumpText.set(''); }
  }
}
