export interface CallLogHost extends HTMLElement {
  log?: CallLog;
}

export class CallLog {
  readonly #host: HTMLElement;

  constructor(host: HTMLElement) {
    this.#host = host;
  }

  note(text: string): void {
    const line = document.createElement("li");
    line.textContent = text;
    this.#host.append(line);
  }
}
