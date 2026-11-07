export interface InfusionPumpHost extends HTMLElement {
  pump?: InfusionPump;
}

interface ChannelSeed {
  readonly label: string;
  readonly running: boolean;
}

const channelSeeds: readonly ChannelSeed[] = [
  { label: "A", running: true },
  { label: "B", running: true },
  { label: "C", running: false },
];

const ratesByMedicine: ReadonlyMap<string, number> = new Map([
  ["Heparin", 18],
  ["Insulin", 4],
]);

export class OlderPumpChannel {
  readonly label: string;
  protected running: boolean;
  readonly #line: HTMLLIElement;
  #partner: OlderPumpChannel = this;

  constructor(seed: ChannelSeed, list: HTMLUListElement) {
    this.label = seed.label;
    this.running = seed.running;
    this.#line = document.createElement("li");
    list.append(this.#line);
    this.show();
  }

  isRunning(): boolean {
    return this.running;
  }

  partner(): OlderPumpChannel {
    return this.#partner;
  }

  pairWith(partner: OlderPumpChannel): void {
    this.#partner = partner;
  }

  protected show(): void {
    this.#line.textContent = this.label + ": " + (this.running ? "running" : "paused");
  }
}

export class PumpChannel extends OlderPumpChannel {
  pause(): void {
    this.running = false;
    this.show();
  }
}

export class InfusionPump {
  readonly alarm = "occlusion";
  readonly #state: HTMLParagraphElement;
  readonly #rate: HTMLParagraphElement;
  readonly #summaryCalls: HTMLParagraphElement;
  readonly #caption: HTMLParagraphElement;
  readonly #labels: HTMLParagraphElement;
  readonly #channels: readonly OlderPumpChannel[];
  #rateText = "none";
  #summaries = 0;

  constructor(host: HTMLElement, channelOf: (seed: ChannelSeed, list: HTMLUListElement) => OlderPumpChannel) {
    this.#state = lineOf("pump-state", "idle");
    this.#rate = lineOf("pump-rate", this.#rateText);
    this.#summaryCalls = lineOf("pump-summary-calls", "0");
    this.#caption = lineOf("pump-caption", host.dataset["caption"] ?? "");
    this.#labels = lineOf("pump-labels", "unshown");
    const list = document.createElement("ul");
    list.className = "pump-channels";
    this.#channels = channelSeeds.map((seed) => channelOf(seed, list));
    this.#channels.forEach((channel, position, channels) => {
      const partner = channels[(position + 2) % channels.length];
      if (partner !== undefined) channel.pairWith(partner);
    });
    host.replaceChildren(this.#caption, this.#state, this.#rate, this.#summaryCalls, this.#labels, list);
  }

  start(): void {
    this.showState("running");
  }

  setRate(volume: number, unit: string): void {
    this.#rateText = String(volume) + " " + unit;
    this.#rate.textContent = this.#rateText;
  }

  caption(text: string | null): void {
    this.#caption.textContent = text === null ? "(no caption)" : String(text);
  }

  showLabels(labels: readonly string[]): void {
    this.#labels.textContent = labels.join(", ");
  }

  summary(): string {
    this.#summaries++;
    this.#summaryCalls.textContent = String(this.#summaries);
    return "rate " + this.#rateText;
  }

  rateFor(medicine: string): number {
    return ratesByMedicine.get(medicine) ?? 0;
  }

  lastAlarm(): string | undefined {
    return undefined;
  }

  settings(): { readonly limit: { readonly volume: number }; readonly note: string | null } {
    return { limit: { volume: 500 }, note: null };
  }

  calibrate(): Promise<void> {
    return Promise.resolve();
  }

  channel(position: number): OlderPumpChannel | undefined {
    return this.#channels[position];
  }

  channels(): readonly OlderPumpChannel[] {
    return this.#channels;
  }

  protected showState(state: string): void {
    this.#state.textContent = state;
  }
}

export class CurrentInfusionPump extends InfusionPump {
  constructor(host: HTMLElement) {
    super(host, (seed, list) => new PumpChannel(seed, list));
  }

  prime(): void {
    this.showState("primed");
  }
}

export class OlderInfusionPump extends InfusionPump {
  constructor(host: HTMLElement) {
    super(host, (seed, list) => new OlderPumpChannel(seed, list));
  }
}

function lineOf(className: string, text: string): HTMLParagraphElement {
  const line = document.createElement("p");
  line.className = className;
  line.textContent = text;
  return line;
}
