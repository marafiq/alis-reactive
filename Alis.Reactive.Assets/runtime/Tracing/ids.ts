import type { Ids } from "./spans";

const traceIdBytes = 16;
const spanIdBytes = 8;

function hexOf(bytes: number): string {
  const minted = new Uint8Array(bytes);
  do crypto.getRandomValues(minted);
  while (minted.every((held) => held === 0));
  return Array.from(minted, (held) => held.toString(16).padStart(2, "0")).join("");
}

export const randomIds: Ids = {
  traceId: () => hexOf(traceIdBytes),
  spanId: () => hexOf(spanIdBytes),
};

export function wallClock(): number {
  return performance.timeOrigin + performance.now();
}
