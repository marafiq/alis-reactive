export interface DoseCardHost extends HTMLElement {
  card?: DoseCard;
}

interface DoseCardOptions {
  readonly step: number;
  readonly unit: string;
  readonly route: string;
  readonly due: string;
  readonly times: readonly string[];
}

interface DoseCardEvents {
  readonly change: Event;
}

export interface DoseCard {
  addEventListener<K extends keyof DoseCardEvents>(type: K, listener: (event: DoseCardEvents[K]) => void, options?: AddEventListenerOptions | boolean): void;
  addEventListener(type: string, listener: EventListenerOrEventListenerObject | null, options?: AddEventListenerOptions | boolean): void;
}

export class DoseCard extends EventTarget {
  readonly #options: DoseCardOptions;
  readonly #separator: string;
  readonly #posted: HTMLInputElement;
  readonly #shown: HTMLParagraphElement;
  #value: number | null;

  constructor(host: HTMLElement) {
    super();
    const options = host.dataset["doseCard"];
    const value = host.dataset["value"];
    const separator = host.dataset["decimalSeparator"];
    const posted = host.querySelector<HTMLInputElement>("input[type=hidden]");
    if (options === undefined || value === undefined || separator === undefined || posted === null) {
      throw new Error("A dose card reads its options, its starting value, the view's decimal separator and its posted input from the markup its render helper writes.");
    }

    this.#options = JSON.parse(options) as DoseCardOptions;
    this.#value = JSON.parse(value) as number | null;
    this.#separator = separator;
    this.#posted = posted;
    this.#shown = document.createElement("p");
    const schedule = document.createElement("p");
    schedule.textContent = this.#options.route + ", " + this.#options.times.join(" and ") + ", due " + this.#options.due.slice(0, 10);
    host.append(this.#shown, schedule, step("Step the dose up", () => this.#stepUp()));
    this.#draw();
  }

  get value(): number | null {
    return this.#value;
  }

  set value(dose: number | null) {
    this.#value = dose;
    this.#draw();
  }

  #stepUp(): void {
    this.value = (this.#value ?? 0) + this.#options.step;
    this.dispatchEvent(new Event("change"));
  }

  #draw(): void {
    this.#posted.value = this.#value === null ? "" : String(this.#value).replace(".", this.#separator);
    this.#shown.textContent = this.#value === null ? "no dose" : String(this.#value) + " " + this.#options.unit;
  }
}

function step(text: string, stepped: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = text;
  button.addEventListener("click", stepped);
  return button;
}
