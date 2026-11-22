import { Component, input } from '@angular/core';

/** Presentational: one permutation as colored digit chips (hue follows the value). */
@Component({
  selector: 'app-permutation-chips',
  template: `
    <span class="chips" dir="ltr">
      @for (v of values(); track $index) {
        <span class="chip" [style.--h]="v * 17">{{ v }}</span>
      }
    </span>`,
})
export class PermutationChipsComponent {
  values = input.required<number[]>();
}
