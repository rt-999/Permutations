/** Wire format: large numbers are strings. */
export interface StartResponseDto { sessionId: string; n: number; total: string; }
export interface NextResponseDto { index: string; permutation: number[]; }
export interface PageItemDto { index: string; permutation: number[]; }
export interface PageResponseDto { page: string; totalPages: string; firstIndex: string; items: PageItemDto[]; }
