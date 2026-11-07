export interface VitalsMonitorHost extends HTMLElement {
  monitor?: VitalsMonitor;
}

type Listener = (...args: unknown[]) => void;

class AlarmPatient {
  readonly #statusLine: HTMLParagraphElement;
  readonly #pagedLine: HTMLParagraphElement;
  #statusText = "resting";

  constructor(statusLine: HTMLParagraphElement, pagedLine: HTMLParagraphElement) {
    this.#statusLine = statusLine;
    this.#pagedLine = pagedLine;
  }

  get status(): string {
    return this.#statusText;
  }

  set status(text: string) {
    this.#statusText = text;
    this.#statusLine.textContent = text;
  }

  page(): void {
    this.#pagedLine.textContent = "paged";
  }
}

export class VitalsMonitor {
  readonly #listeners = new Map<string, Set<Listener>>();
  readonly #raised: HTMLParagraphElement;
  readonly #silencedLine: HTMLParagraphElement;
  readonly #patient: AlarmPatient;

  constructor(host: HTMLElement, older: boolean) {
    this.#raised = lineOf("monitor-raised", "quiet");
    this.#silencedLine = lineOf("monitor-silenced", "sounding");
    const statusLine = lineOf("monitor-patient", "resting");
    const pagedLine = lineOf("monitor-paged", "unpaged");
    this.#patient = new AlarmPatient(statusLine, pagedLine);
    const buttons = older
      ? [buttonOf("Raise an alarm", () => this.#raise("alarm", { bedNumber: "9", note: null, reading: { rate: 88 }, patient: this.#patient, silence: () => this.#silence() }))]
      : [
          buttonOf("Raise a noted alarm", () => this.#raise("alarm", this.#alarm("after surgery"))),
          buttonOf("Raise an unnoted alarm", () => this.#raise("alarm", this.#alarm(null))),
          buttonOf("Raise an empty alarm", () => this.#raise("alarm")),
          buttonOf("Silence the monitor", () => this.#raise("silenced")),
        ];
    host.replaceChildren(this.#raised, this.#silencedLine, statusLine, pagedLine, ...buttons);
  }

  on(event: string, listener: Listener): void {
    const listening = this.#listeners.get(event) ?? new Set<Listener>();
    listening.add(listener);
    this.#listeners.set(event, listening);
  }

  off(event: string, listener: Listener): void {
    this.#listeners.get(event)?.delete(listener);
  }

  #alarm(note: string | null): object {
    return { bed: "4", note, reading: { rate: 132 }, patient: this.#patient, silence: () => this.#silence() };
  }

  #silence(): void {
    this.#silencedLine.textContent = "silenced";
  }

  #raise(event: string, ...args: unknown[]): void {
    for (const listener of this.#listeners.get(event) ?? []) listener(...args);
    this.#raised.textContent = "raised " + event + (args.length === 0 ? " with nothing" : "");
  }
}

function buttonOf(text: string, clicked: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = text;
  button.addEventListener("click", clicked);
  return button;
}

function lineOf(className: string, text: string): HTMLParagraphElement {
  const line = document.createElement("p");
  line.className = className;
  line.textContent = text;
  return line;
}
