import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermutationStore } from '../../state/permutation.store';

export const sessionGuard: CanActivateFn = () =>
  inject(PermutationStore).hasSession() || inject(Router).createUrlTree(['/']);
