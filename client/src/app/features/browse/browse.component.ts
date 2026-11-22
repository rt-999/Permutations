import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { PermutationStore } from '../../state/permutation.store';
import { PAGE_SIZE_OPTIONS } from '../../core/constants';
import { BigPaginatorComponent } from '../../shared/big-paginator/big-paginator.component';
import { PermutationsListComponent } from './permutations-list.component';

@Component({
  selector: 'app-browse',
  imports: [BigPaginatorComponent, PermutationsListComponent],
  template: `
    <section class="card wide">
      <header class="head">
        <h1>כל התמורות</h1>
        <div class="row">
          <label for="size">בדף</label>
          <select id="size" [disabled]="store.loading()" (change)="onSize($event)">
            @for (s of sizes; track s) { <option [value]="s" [selected]="s === store.pageSize()">{{ s }}</option> }
          </select>
          <button class="btn" [disabled]="store.loading()" (click)="back()">חזור</button>
        </div>
      </header>
      <app-permutations-list [items]="store.pageItems()" />
      <app-big-paginator [currentPage]="store.page()" [totalPages]="store.totalPages()"
                         [disabled]="store.loading()" (pageChange)="store.goToPage($event)" />
    </section>`,
})
export class BrowseComponent {
  protected readonly store = inject(PermutationStore);
  private readonly router = inject(Router);
  protected readonly sizes = PAGE_SIZE_OPTIONS;

  protected onSize(e: Event): void { void this.store.setPageSize(Number((e.target as HTMLSelectElement).value)); }

  protected async back(): Promise<void> {
    if (await this.store.exitBrowse()) void this.router.navigate(['/explore']);
  }
}
