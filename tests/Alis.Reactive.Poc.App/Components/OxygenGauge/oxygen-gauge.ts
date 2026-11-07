interface Alarm {
  readonly message: string;
}

type AlarmListener = (alarm: Alarm) => void;

export class OxygenGauge {
  alarmBelow = 90;
  #saturation: number;
  readonly #face: HTMLParagraphElement;
  readonly #listeners = new Set<AlarmListener>();

  constructor(host: HTMLElement) {
    this.#saturation = Number(host.dataset["oxygenGauge"]);
    this.#face = document.createElement("p");
    this.#face.className = "gauge-face";
    host.append(this.#face);
    this.#draw();
  }

  get reading(): string {
    return String(this.#saturation) + "%";
  }

  lower(points: number): void {
    this.#saturation -= points;
    this.#draw();
    if (this.#saturation < this.alarmBelow) for (const listener of this.#listeners) listener({ message: "Saturation " + this.reading });
  }

  on(event: "alarm", listener: AlarmListener): void {
    if (event !== "alarm") throw new Error("The oxygen gauge raises 'alarm' only, not '" + event + "'.");
    this.#listeners.add(listener);
  }

  off(event: "alarm", listener: AlarmListener): void {
    if (event === "alarm") this.#listeners.delete(listener);
  }

  #draw(): void {
    this.#face.textContent = this.reading;
  }
}

const gauges = new WeakMap<HTMLElement, OxygenGauge>();

export const OxygenGauges = {
  mount(host: HTMLElement): void {
    gauges.set(host, new OxygenGauge(host));
  },
  of(host: HTMLElement): OxygenGauge | undefined {
    return gauges.get(host);
  },
};
