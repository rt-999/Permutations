/** App format: large numbers are bigint. */
export interface StartResult { sessionId: string; n: number; total: bigint; }
export interface PermutationItem { index: bigint; permutation: number[]; }
export interface PageResult { page: bigint; totalPages: bigint; firstIndex: bigint; items: PermutationItem[]; }
