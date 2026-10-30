// The banner: one per page, showing a message and how often it has flashed. It knows nothing of
// Alis.Reactive; Banner.cs declares its members by these names.

type Listener = (payload: unknown) => void;

interface BannerHost extends HTMLElement {
  banner?: Banner;
}

export class Banner {
  private text = "";
  private flashes = 0;
  private readonly listeners = new Map<string, Set<Listener>>();

  constructor(private readonly host: BannerHost) {
    host.banner = this;
    this.show();
  }

  get message(): string {
    return this.text;
  }

  set message(text: string) {
    this.text = text;
    this.show();
  }

  flash(): void {
    this.flashes += 1;
    this.show();
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
    this.listeners.clear();
    delete this.host.banner;
  }

  private show(): void {
    this.host.textContent = `Banner: ${this.text || "nothing yet"} · flashed ${this.flashes} times`;
  }
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): BannerHost[] {
  const inside = Array.from(root.querySelectorAll<BannerHost>("div.banner"));
  return root.matches("div.banner") ? [root as BannerHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.banner === undefined) new Banner(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.banner?.destroy();
});
