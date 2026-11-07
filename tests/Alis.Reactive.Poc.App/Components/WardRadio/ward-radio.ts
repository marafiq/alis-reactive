interface Broadcast {
  readonly message: string;
}

type BroadcastListener = (broadcast: Broadcast) => void;

export class WardRadio {
  readonly #listeners = new Set<BroadcastListener>();

  listen(event: "broadcast", listener: BroadcastListener): void {
    if (event !== "broadcast") throw new Error("The ward radio raises 'broadcast' only, not '" + event + "'.");
    this.#listeners.add(listener);
  }

  announce(message: string): void {
    for (const listener of this.#listeners) listener({ message });
  }
}
