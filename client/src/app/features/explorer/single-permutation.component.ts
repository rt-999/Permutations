import { Component, input } from '@angular/core';
import { PermutationItem } from '../../core/api/domain.models';
import { BigFormatPipe } from '../../shared/pipes/big-format.pipe';
import { PermutationChipsComponent } from '../../shared/permutation-chips.component';

@Component({
  selector: 'app-single-permutation',
  imports: [BigFormatPipe, PermutationChipsComponent],
  template: `
    @if (item(); as it) {
      <p class="muted">תמורה מספר <strong dir="ltr">{{ it.index | bigFormat }}</strong> מתוך <span dir="ltr">{{ total() | bigFormat }}</span></p>
      <app-permutation-chips class="big" [values]="it.permutation" />
    } @else {
      <p class="muted">עוד לא הוצגה תמורה. לחצי על "הבא".</p>
    }`,
})
export class SinglePermutationComponent {
  item = input<PermutationItem | null>(null);
  total = input(0n);
}
