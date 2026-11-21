export interface BedsideMonitorHost extends HTMLElement {
  monitor?: BedsideMonitor;
}

interface MonitorAlarm {
  readonly resident: string;
}

type AlarmListener = (alarm: MonitorAlarm) => void;

export class BedsideMonitor {
  readonly #host: BedsideMonitorHost;
  readonly #badge: HTMLParagraphElement;
  readonly #listeners = new Set<AlarmListener>();
  #switchedOff = false;

  constructor(host: BedsideMonitorHost, restarted = false) {
    this.#host = host;
    const resident = host.dataset["resident"] ?? "";
    this.#badge = document.createElement("p");
    this.#badge.className = "monitor-badge";
    this.#badge.textContent = resident + "'s monitor is on" + (restarted ? " again" : "");
    document.body.append(this.#badge);
    host.replaceChildren(
      buttonOf("Raise " + resident + "'s alarm", () => this.#raise(resident)),
      buttonOf("Switch off " + resident + "'s monitor", () => this.destroy()),
      buttonOf("Restart " + resident + "'s monitor", () => this.#restart()),
    );
  }

  on(event: "alarm", listener: AlarmListener): void {
    if (event !== "alarm") throw new Error("The bedside monitor raises 'alarm' only, not '" + event + "'.");
    this.#listeners.add(listener);
  }

  off(event: "alarm", listener: AlarmListener): void {
    if (this.#switchedOff) throw new Error("The bedside monitor is switched off, so it holds no listener of '" + event + "' to remove.");
    this.#listeners.delete(listener);
  }

  destroy(): void {
    if (this.#switchedOff) return;
    this.#switchedOff = true;
    this.#listeners.clear();
    this.#badge.remove();
    this.#host.replaceChildren();
    delete this.#host.monitor;
  }

  #restart(): void {
    const host = this.#host;
    this.destroy();
    host.monitor = new BedsideMonitor(host, true);
  }

  #raise(resident: string): void {
    for (const listener of this.#listeners) listener({ resident });
  }
}

function buttonOf(text: string, clicked: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = text;
  button.addEventListener("click", clicked);
  return button;
}
