export type Removal = () => void;

export class Owner {
  readonly #removals: Removal[] = [];

  holds(removal: Removal): void {
    this.#removals.push(removal);
  }

  release(): void {
    for (const removal of this.#removals.splice(0)) removal();
  }
}

export const page = new Owner();
