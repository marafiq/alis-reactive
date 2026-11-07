export interface VitalsMonitorHost extends HTMLElement {
  monitor?: VitalsMonitor;
}

type MonitorEvent = "alarm" | "silenced";

interface Alarm {
  bed: string;
  cancel: boolean;
  readonly note: string | null;
  readonly reading: { readonly rate: number; readonly unit?: string };
  readonly patient: AlarmPatient;
  readonly silence: () => void;
}

interface OlderAlarm {
  readonly bedNumber: string;
  cancel: boolean;
  readonly note: string | null;
  readonly reading: { readonly rate: number; readonly unit?: string };
  readonly patient: AlarmPatient;
  readonly silence: () => void;
}

type AlarmListener = (alarm?: Alarm | OlderAlarm) => void;

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
  readonly #listeners = new Map<string, Set<AlarmListener>>();
  readonly #raised: HTMLParagraphElement;
  readonly #silencedLine: HTMLParagraphElement;
  readonly #alarmLine: HTMLParagraphElement;
  readonly #patient: AlarmPatient;
  #acknowledging: () => void = () => undefined;

  constructor(host: HTMLElement, older: boolean) {
    this.#raised = lineOf("monitor-raised", "quiet");
    this.#silencedLine = lineOf("monitor-silenced", "not silenced");
    this.#alarmLine = lineOf("monitor-alarm", "quiet");
    const statusLine = lineOf("monitor-patient", "resting");
    const pagedLine = lineOf("monitor-paged", "unpaged");
    this.#patient = new AlarmPatient(statusLine, pagedLine);
    const buttons = older
      ? [buttonOf("Raise an alarm", () => this.#raise("alarm", { bedNumber: "9", cancel: false, note: null, reading: { rate: 88 }, patient: this.#patient, silence: () => this.#silence() }))]
      : [
          buttonOf("Raise a noted alarm", () => this.#sound(this.#alarm("after surgery"))),
          buttonOf("Raise an unnoted alarm", () => this.#sound(this.#alarm(null))),
          buttonOf("Raise an empty alarm", () => this.#raise("alarm")),
          buttonOf("Silence the monitor", () => this.#raise("silenced")),
          buttonOf("Confirm the acknowledgement", () => this.#acknowledging()),
        ];
    host.replaceChildren(this.#raised, this.#alarmLine, this.#silencedLine, statusLine, pagedLine, ...buttons);
  }

  on(event: "alarm", listener: AlarmListener): void;
  on(event: "silenced", listener: () => void): void;
  on(event: MonitorEvent, listener: AlarmListener): void {
    const listening = this.#listeners.get(event) ?? new Set<AlarmListener>();
    listening.add(listener);
    this.#listeners.set(event, listening);
  }

  off(event: "alarm", listener: AlarmListener): void;
  off(event: "silenced", listener: () => void): void;
  off(event: MonitorEvent, listener: AlarmListener): void {
    this.#listeners.get(event)?.delete(listener);
  }

  acknowledge(): Promise<void> {
    return new Promise<void>((resolve) => {
      this.#acknowledging = resolve;
    });
  }

  #alarm(note: string | null): Alarm {
    return { bed: "4", cancel: false, note, reading: { rate: 132 }, patient: this.#patient, silence: () => this.#silence() };
  }

  #sound(alarm: Alarm): void {
    this.#raise("alarm", alarm);
    this.#alarmLine.textContent = alarm.cancel ? "cancelled" : "sounding";
    alarm.bed = "4, moved to 6";
  }

  #silence(): void {
    this.#silencedLine.textContent = "silenced";
  }

  #raise(event: MonitorEvent, ...args: [alarm?: Alarm | OlderAlarm]): void {
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
