import { Component, input } from '@angular/core';
import { PermutationItem } from '../../core/api/domain.models';
import { BigFormatPipe } from '../../shared/pipes/big-format.pipe';
import { PermutationChipsComponent } from '../../shared/permutation-chips.component';

@Component({
  selector: 'app-permutations-list',
  imports: [BigFormatPipe, PermutationChipsComponent],
  template: `
    <ol class="list">
      @for (it of items(); track it.index) {
        <li><span class="idx" dir="ltr">{{ it.index | bigFormat }}</span><app-permutation-chips [values]="it.permutation" /></li>
      }
    </ol>`,
})
export class PermutationsListComponent {
  items = input.required<PermutationItem[]>();
}
