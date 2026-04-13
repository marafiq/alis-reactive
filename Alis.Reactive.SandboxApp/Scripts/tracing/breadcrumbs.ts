/**
 * Ring buffer for tracing breadcrumbs.
 *
 * Internal to the tracing module. Not exported from `index.ts`.
 *
 * Breadcrumbs are captured continuously and attached to error-level
 * events by the sink. The buffer drops oldest entries when capacity is
 * exceeded. `snapshot()` returns a frozen, chronologically-ordered copy.
 */

import type { Breadcrumb } from "./types";

export class BreadcrumbBuffer {
  private readonly capacity: number;
  private readonly items: Breadcrumb[];
  private nextIndex = 0;
  private filled = false;

  constructor(capacity: number) {
    if (!Number.isInteger(capacity) || capacity <= 0) {
      throw new RangeError(
        `BreadcrumbBuffer capacity must be a positive integer, got ${capacity}`,
      );
    }
    this.capacity = capacity;
    this.items = new Array<Breadcrumb>(capacity);
  }

  push(crumb: Breadcrumb): void {
    this.items[this.nextIndex] = crumb;
    this.nextIndex = (this.nextIndex + 1) % this.capacity;
    if (this.nextIndex === 0) {
      this.filled = true;
    }
  }

  /**
   * Return a chronologically-ordered snapshot of the buffer contents.
   * Oldest entry first; newest entry last. Safe to hand to sinks — the
   * returned array is a fresh copy, not a live view.
   */
  snapshot(): readonly Breadcrumb[] {
    if (!this.filled) {
      return this.items.slice(0, this.nextIndex);
    }
    return [
      ...this.items.slice(this.nextIndex),
      ...this.items.slice(0, this.nextIndex),
    ];
  }

  /** Current number of entries (0 ≤ size ≤ capacity). */
  get size(): number {
    return this.filled ? this.capacity : this.nextIndex;
  }
}
