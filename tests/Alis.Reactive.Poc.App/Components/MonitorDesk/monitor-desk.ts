export interface DeskMonitorHost extends HTMLElement {
  monitor?: DeskMonitor;
}

export interface MonitorDeskHost extends HTMLElement {
  desk?: MonitorDesk;
}

interface DeskAlarm {
  readonly resident: string;
}

type AlarmListener = (alarm: DeskAlarm) => void;

export class DeskMonitor {
  readonly element: DeskMonitorHost;
  readonly resident: string;
  readonly #listeners = new Set<AlarmListener>();
  readonly #statusLine: HTMLParagraphElement;
  readonly #pagedLine: HTMLParagraphElement;
  #status = "idle";
  #on = true;

  constructor(host: DeskMonitorHost, restarted = false) {
    const resident = host.dataset["resident"];
    if (resident === undefined) throw new Error("A desk monitor names its resident.");
    this.element = host;
    this.resident = resident;
    this.#statusLine = lineOf("desk-monitor-status", resident + ": idle" + (restarted ? " again" : ""));
    this.#pagedLine = lineOf("desk-monitor-paged", resident + " not paged");
    host.replaceChildren(
      this.#statusLine,
      this.#pagedLine,
      buttonOf("Sound " + resident + "'s monitor", () => this.#sound()),
      buttonOf("Switch off " + resident + "'s monitor", () => this.destroy()),
      buttonOf("Restart " + resident + "'s monitor", () => this.#restart()),
    );
    host.monitor = this;
  }

  get status(): string {
    return this.#status;
  }

  set status(text: string) {
    this.#status = text;
    this.#statusLine.textContent = this.resident + ": " + text;
  }

  page(): void {
    this.#pagedLine.textContent = this.resident + " paged";
  }

  check(): Promise<string> {
    return Promise.resolve(this.resident + " is steady");
  }

  on(event: "alarm", listener: AlarmListener): void {
    if (event !== "alarm") throw new Error("A desk monitor raises 'alarm' only, not '" + event + "'.");
    this.#listeners.add(listener);
  }

  off(event: "alarm", listener: AlarmListener): void {
    if (!this.#on) throw new Error(this.resident + "'s monitor is switched off, so it holds no listener to remove.");
    this.#listeners.delete(listener);
  }

  destroy(): void {
    this.#on = false;
    this.#listeners.clear();
    this.#statusLine.textContent = this.resident + ": switched off";
    if (this.element.monitor === this) delete this.element.monitor;
  }

  #restart(): void {
    this.destroy();
    new DeskMonitor(this.element, true);
  }

  #sound(): void {
    for (const listener of [...this.#listeners]) listener({ resident: this.resident });
  }
}

interface Handed {
  readonly monitor: DeskMonitor;
}

type PickedListener = (monitor: DeskMonitor) => void;
type HandedListener = (handed: Handed) => void;

export class MonitorDesk {
  readonly #picked = new Set<PickedListener>();
  readonly #handed = new Set<HandedListener>();

  constructor(host: MonitorDeskHost) {
    const ids = JSON.parse(host.dataset["monitorDesk"] ?? "[]") as string[];
    host.replaceChildren(
      ...ids.flatMap((id) => [
        buttonOf("Pick " + id, () => this.#pick(id)),
        buttonOf("Hand over " + id, () => this.#handOver(id)),
      ]),
    );
  }

  on(event: "picked", listener: PickedListener): void;
  on(event: "handed", listener: HandedListener): void;
  on(event: "picked" | "handed", listener: PickedListener | HandedListener): void {
    if (event === "picked") this.#picked.add(listener as PickedListener);
    else this.#handed.add(listener as HandedListener);
  }

  off(event: "picked", listener: PickedListener): void;
  off(event: "handed", listener: HandedListener): void;
  off(event: "picked" | "handed", listener: PickedListener | HandedListener): void {
    if (event === "picked") this.#picked.delete(listener as PickedListener);
    else this.#handed.delete(listener as HandedListener);
  }

  #pick(id: string): void {
    const monitor = monitorOn(id);
    for (const listener of [...this.#picked]) listener(monitor);
  }

  #handOver(id: string): void {
    const monitor = monitorOn(id);
    for (const listener of [...this.#handed]) listener({ monitor });
  }
}

function monitorOn(id: string): DeskMonitor {
  const host = document.getElementById(id) as DeskMonitorHost | null;
  const monitor = host?.monitor;
  if (monitor === undefined) throw new Error("No monitor is on the element '" + id + "'.");
  return monitor;
}

function lineOf(className: string, text: string): HTMLParagraphElement {
  const line = document.createElement("p");
  line.className = className;
  line.textContent = text;
  return line;
}

function buttonOf(text: string, click: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = text;
  button.addEventListener("click", click);
  return button;
}
