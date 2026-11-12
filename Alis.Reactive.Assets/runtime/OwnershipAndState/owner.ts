export type Removal = () => void;

export class Owner {
  readonly #removals: Removal[] = [];

  holds(removal: Removal): void {
    this.#removals.push(removal);
  }
}

export const page = new Owner();
