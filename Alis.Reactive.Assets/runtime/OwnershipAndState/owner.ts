export type Removal = () => void;

const releaseOrder = ["runs", "unmount", "listeners", "holds", "registrations"] as const;

export type ReleaseStep = (typeof releaseOrder)[number];

export interface Owner {
  runs(end: Removal): void;
  mounts(unmount: Removal): void;
  listens(remove: Removal): void;
  holds(release: Removal): void;
  registers(drop: Removal): void;
}

export interface ThrownAtAStep {
  readonly step: ReleaseStep;
  readonly error: unknown;
}

export class Holdings implements Owner {
  readonly #held: Record<ReleaseStep, Removal[]> = { runs: [], unmount: [], listeners: [], holds: [], registrations: [] };

  runs(end: Removal): void {
    this.#held.runs.push(end);
  }

  mounts(unmount: Removal): void {
    this.#held.unmount.push(unmount);
  }

  listens(remove: Removal): void {
    this.#held.listeners.push(remove);
  }

  holds(release: Removal): void {
    this.#held.holds.push(release);
  }

  registers(drop: Removal): void {
    this.#held.registrations.push(drop);
  }

  release(): ThrownAtAStep[] {
    const thrown: ThrownAtAStep[] = [];
    for (const step of releaseOrder) {
      for (const removal of this.#held[step].splice(0)) {
        try {
          removal();
        } catch (error) {
          thrown.push({ step, error });
        }
      }
    }
    return thrown;
  }
}
