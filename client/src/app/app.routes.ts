import { Routes } from '@angular/router';
import { sessionGuard } from './core/session/session.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/start/start-screen.component').then(m => m.StartScreenComponent) },
  { path: 'explore', canActivate: [sessionGuard],
    loadComponent: () => import('./features/explorer/explorer.component').then(m => m.ExplorerComponent) },
  { path: 'explore/all', canActivate: [sessionGuard],
    loadComponent: () => import('./features/browse/browse.component').then(m => m.BrowseComponent) },
  { path: '**', redirectTo: '' },
];
