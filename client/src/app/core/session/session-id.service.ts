import { Injectable } from '@angular/core';

/** sessionStorage: every browser tab is an independent user. */
@Injectable({ providedIn: 'root' })
export class SessionIdService {
  private readonly key = 'permutations.sessionId';
  get(): string | null { try { return sessionStorage.getItem(this.key); } catch { return null; } }
  set(id: string): void { try { sessionStorage.setItem(this.key, id); } catch { /* ignore */ } }
  clear(): void { try { sessionStorage.removeItem(this.key); } catch { /* ignore */ } }
}
