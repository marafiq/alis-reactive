import type { Breadcrumb } from "./types";

export class BreadcrumbBuffer {
  private readonly buffer: Breadcrumb[];
  private readonly capacity: number;
  private head = 0;
  private count = 0;

  constructor(capacity: number = 64) {
    this.capacity = capacity;
    this.buffer = new Array(capacity);
  }

  push(crumb: Breadcrumb): void {
    this.buffer[this.head] = crumb;
    this.head = (this.head + 1) % this.capacity;
    if (this.count < this.capacity) this.count++;
  }

  snapshot(): readonly Breadcrumb[] {
    if (this.count === 0) return [];
    if (this.count < this.capacity) {
      return this.buffer.slice(0, this.count);
    }
    return [...this.buffer.slice(this.head), ...this.buffer.slice(0, this.head)];
  }

  clear(): void {
    this.head = 0;
    this.count = 0;
  }
}
