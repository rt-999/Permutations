import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { NextResponseDto, PageItemDto, PageResponseDto, StartResponseDto } from './api.models';
import { PageResult, PermutationItem, StartResult } from './domain.models';
import { parseBigInt } from '../util/bigint';

/** The only place that converts string <-> bigint. */
@Injectable({ providedIn: 'root' })
export class PermutationApiService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/permutations';

  start(n: number): Observable<StartResult> {
    return this.http.post<StartResponseDto>(`${this.base}/start`, { n }).pipe(
      map(r => ({ sessionId: r.sessionId, n: r.n, total: parseBigInt(r.total) })));
  }

  next(): Observable<PermutationItem> {
    return this.http.post<NextResponseDto>(`${this.base}/next`, null).pipe(map(r => this.toItem(r)));
  }

  getPage(page: bigint, pageSize: number): Observable<PageResult> {
    const params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize);
    return this.http.get<PageResponseDto>(`${this.base}/all`, { params }).pipe(
      map(r => ({
        page: parseBigInt(r.page),
        totalPages: parseBigInt(r.totalPages),
        firstIndex: parseBigInt(r.firstIndex),
        items: r.items.map(i => this.toItem(i)),
      })));
  }

  exitBrowse(): Observable<void> {
    return this.http.post<void>(`${this.base}/all/exit`, null);
  }

  private toItem(d: NextResponseDto | PageItemDto): PermutationItem {
    return { index: parseBigInt(d.index), permutation: d.permutation };
  }
}
