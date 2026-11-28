export interface DoseChartHost extends HTMLElement {
  chart?: DoseChart;
}

interface Tick {
  readonly label?: string;
  readonly doses: number;
}

type TickLabel = (tick: Tick, position: number) => unknown;

type ValueAccessor = (field: "doses", tick: Tick) => unknown;

export class DoseChart {
  readonly #count: HTMLParagraphElement;
  readonly #labels: HTMLParagraphElement;
  readonly #values: HTMLParagraphElement;
  #ticks: readonly Tick[] = [
    { label: "Mon", doses: 8 },
    { label: "Tue", doses: 5 },
    { label: "Wed", doses: 6 },
  ];
  #tickLabel: TickLabel = (_tick, position) => position + 1;
  #valueAccessor: ValueAccessor = (field, tick) => tick[field];
  #unit = "mg";

  constructor(host: HTMLElement) {
    this.#count = document.createElement("p");
    this.#count.className = "chart-count";
    this.#labels = document.createElement("p");
    this.#labels.className = "chart-labels";
    this.#values = document.createElement("p");
    this.#values.className = "chart-values";
    host.replaceChildren(
      this.#count,
      this.#labels,
      this.#values,
      button("Redraw the chart", () => this.#draw()),
      button("Use millilitres", () => {
        this.#unit = "ml";
        this.#draw();
      }),
      button("Add a tick with no label", () => {
        this.#ticks = [...this.#ticks, { doses: 3 }];
        this.#draw();
      }),
    );
    this.#draw();
  }

  get tickLabel(): TickLabel {
    return this.#tickLabel;
  }

  set tickLabel(label: TickLabel) {
    this.#tickLabel = label;
    this.#draw();
  }

  get valueAccessor(): ValueAccessor {
    return this.#valueAccessor;
  }

  set valueAccessor(accessor: ValueAccessor) {
    this.#valueAccessor = accessor;
    this.#draw();
  }

  sortTicks(key: (tick: Tick) => number): void {
    this.#ticks = [...this.#ticks].sort((first, second) => key(first) - key(second));
    this.#draw();
  }

  unit(): string {
    return this.#unit;
  }

  legend(): Promise<string> {
    return Promise.resolve("doses a day");
  }

  #draw(): void {
    this.#count.textContent = String(this.#ticks.length) + " ticks";
    this.#labels.textContent = this.#ticks.map((tick, position) => String(this.#tickLabel(tick, position))).join(", ");
    this.#values.textContent = this.#ticks.map((tick) => String(this.#valueAccessor("doses", tick))).join(", ");
  }
}

function button(text: string, press: () => void): HTMLButtonElement {
  const pressed = document.createElement("button");
  pressed.type = "button";
  pressed.textContent = text;
  pressed.addEventListener("click", press);
  return pressed;
}
