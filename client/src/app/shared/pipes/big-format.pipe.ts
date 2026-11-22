import { Pipe, PipeTransform } from '@angular/core';
import { formatBigInt } from '../../core/util/bigint';

@Pipe({ name: 'bigFormat' })
export class BigFormatPipe implements PipeTransform {
  transform(value: bigint | null | undefined): string {
    return value == null ? '' : formatBigInt(value);
  }
}
