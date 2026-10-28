// The bell: rings and counts its rings. It knows nothing of Alis.Reactive. A listener is a callback
// property, and onRung already holds the bell's own callback, which shows how many times it rang.
// Destroyed, the bell takes its callback property away, so nothing can call into a bell that is gone.

type Callback = (ring: { readonly times: number }) => void;

interface BellHost extends HTMLElement {
  bell?: Bell;
}

export class Bell {
  onRung?: Callback | null;
  private rungTimes = 0;
  private readonly ringsLine: HTMLParagraphElement;
  private readonly button: HTMLButtonElement;

  constructor(private readonly host: BellHost) {
    this.ringsLine = document.createElement("p");
    this.ringsLine.textContent = "Not rung yet";
    this.button = document.createElement("button");
    this.button.type = "button";
    this.button.textContent = "Ring the bell";
    this.button.addEventListener("click", this.ringFromButton);
    host.replaceChildren(this.ringsLine, this.button);
    this.onRung = ring => {
      this.ringsLine.textContent = ring.times === 1 ? "Rung 1 time" : `Rung ${ring.times} times`;
    };
    host.bell = this;
  }

  get rings(): number {
    return this.rungTimes;
  }

  ring(): void {
    this.rungTimes += 1;
    this.onRung?.({ times: this.rungTimes });
  }

  destroy(): void {
    this.button.removeEventListener("click", this.ringFromButton);
    delete this.onRung;
    delete this.host.bell;
  }

  private readonly ringFromButton = (): void => this.ring();
}

interface MountDetail {
  readonly root: HTMLElement;
}

function hosts(root: HTMLElement): BellHost[] {
  const inside = Array.from(root.querySelectorAll<BellHost>("div.bell"));
  return root.matches("div.bell") ? [root as BellHost, ...inside] : inside;
}

document.addEventListener("alis:mount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) {
    if (host.bell === undefined) new Bell(host);
  }
});

document.addEventListener("alis:unmount", event => {
  for (const host of hosts((event as CustomEvent<MountDetail>).detail.root)) host.bell?.destroy();
});
