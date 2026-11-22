import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PermutationApiService } from '../core/api/permutation-api.service';
import { PermutationItem } from '../core/api/domain.models';
import { NotificationService } from '../core/error/notification.service';
import { SessionIdService } from '../core/session/session-id.service';
import { DEFAULT_PAGE_SIZE } from '../core/constants';

export type ViewMode = 'single' | 'browse';

@Injectable({ providedIn: 'root' })
export class PermutationStore {
  private readonly api = inject(PermutationApiService);
  private readonly session = inject(SessionIdService);
  private readonly notify = inject(NotificationService);

  private readonly _n = signal<number | null>(null);
  private readonly _total = signal(0n);
  private readonly _lastShown = signal<PermutationItem | null>(null);
  private readonly _view = signal<ViewMode>('single');
  private readonly _page = signal(1n);
  private readonly _totalPages = signal(0n);
  private readonly _pageItems = signal<PermutationItem[]>([]);
  private readonly _pageSize = signal(DEFAULT_PAGE_SIZE);
  private readonly _loading = signal(false);

  readonly n = this._n.asReadonly();
  readonly total = this._total.asReadonly();
  readonly lastShown = this._lastShown.asReadonly();
  readonly view = this._view.asReadonly();
  readonly page = this._page.asReadonly();
  readonly totalPages = this._totalPages.asReadonly();
  readonly pageItems = this._pageItems.asReadonly();
  readonly pageSize = this._pageSize.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly hasSession = computed(() => this._n() !== null);

  /** Each action resolves to true on success, false on failure (the error is already shown). */
  async start(n: number): Promise<boolean> {
    return this.run(async () => {
      const r = await firstValueFrom(this.api.start(n));
      this.session.set(r.sessionId);
      this._n.set(r.n);
      this._total.set(r.total);
      this._lastShown.set(null);
      this._view.set('single');
      this._pageItems.set([]);
    });
  }

  async next(): Promise<boolean> {
    return this.run(async () => this._lastShown.set(await firstValueFrom(this.api.next())));
  }

  openBrowse(): Promise<boolean> { return this.goToPage(1n); }

  async goToPage(page: bigint): Promise<boolean> {
    return this.run(async () => {
      const r = await firstValueFrom(this.api.getPage(page, this._pageSize()));
      this._page.set(r.page);
      this._totalPages.set(r.totalPages);
      this._pageItems.set(r.items);
      this._lastShown.set(r.items[r.items.length - 1] ?? null);  // "last shown" = last item of the current page
      this._view.set('browse');
    });
  }

  setPageSize(size: number): Promise<boolean> {
    this._pageSize.set(size);
    return this.goToPage(1n);
  }

  async exitBrowse(): Promise<boolean> {
    return this.run(async () => {
      await firstValueFrom(this.api.exitBrowse());
      this._view.set('single');
    });
  }

  reset(): void {
    this.session.clear();
    this.notify.clear();
    this._n.set(null);
    this._total.set(0n);
    this._lastShown.set(null);
    this._pageItems.set([]);
    this._view.set('single');
  }

  private async run(action: () => Promise<void>): Promise<boolean> {
    this.notify.clear();
    this._loading.set(true);
    try { await action(); return true; }
    catch { return false; }
    finally { this._loading.set(false); }
  }
}
