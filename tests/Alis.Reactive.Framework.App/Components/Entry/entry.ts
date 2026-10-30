// The entry: a text input that tells its listeners when the user leaves it after a change. It knows
// nothing of Alis.Reactive. The input element holds it as `entry`.

type Listener = (payload: unknown) => void;

interface EntryHost extends HTMLInputElement {
  entry?: Entry;
}

export class Entry {
  private readonly listeners = new Map<string, Set<Listener>>();

  constructor(private readonly input: EntryHost) {
    input.addEventListener("change", this.commit);
    input.entry = this;
  }

  get value(): string {
    return this.input.value;
  }

  set value(text: string) {
    this.input.value = text;
  }

  on(name: string, listener: Listener): void {
    const named = this.listeners.get(name) ?? new Set<Listener>();
    named.add(listener);
    this.listeners.set(name, named);
  }

  off(name: string, listener: Listener): void {
    this.listeners.get(name)?.delete(listener);
  }

  destroy(): void {
    this.input.removeEventListener("change", this.commit);
    this.listeners.clear();
    delete this.input.entry;
  }

  private readonly commit = (): void => {
    for (const listener of [...(this.listeners.get("committed") ?? [])]) listener({ value: this.input.value });
  };
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): EntryHost[] {
  const inside = Array.from(root.querySelectorAll<EntryHost>("input.entry"));
  return root.matches("input.entry") ? [root as EntryHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.entry === undefined) new Entry(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.entry?.destroy();
});
