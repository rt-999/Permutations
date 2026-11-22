export const parseBigInt = (s: string): bigint => BigInt(s);
export const formatBigInt = (v: bigint): string => v.toLocaleString('en-US');
export const ceilDiv = (a: bigint, b: bigint): bigint => (a + b - 1n) / b;
export const isDigits = (s: string): boolean => /^\d+$/.test(s);
