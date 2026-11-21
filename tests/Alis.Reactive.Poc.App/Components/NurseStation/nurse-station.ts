export interface NurseStationHost extends HTMLElement {
  station?: NurseStation;
}

export interface StationCensusHost extends HTMLElement {
  census?: StationCensus;
}

interface BedSeed {
  readonly resident: string;
  readonly alerts: readonly string[];
}

interface PagedRounds {
  readonly result: readonly unknown[];
  readonly count: number;
}

export class BedAlert {
  readonly reason: string;

  constructor(reason: string) {
    this.reason = reason;
  }
}

export class OlderStationBed {
  readonly #resident: string;
  readonly #alerts: readonly BedAlert[];
  readonly #statusCell: HTMLTableCellElement;
  #status = "Ready";

  constructor(row: HTMLTableRowElement, seed: BedSeed) {
    this.#resident = seed.resident;
    this.#alerts = seed.alerts.map((reason) => new BedAlert(reason));
    const residentCell = row.insertCell();
    residentCell.className = "bed-resident";
    residentCell.textContent = seed.resident;
    this.#statusCell = row.insertCell();
    this.#statusCell.className = "bed-status";
    this.#statusCell.textContent = this.#status;
  }

  get resident(): string {
    return this.#resident;
  }

  get alerts(): readonly BedAlert[] {
    return this.#alerts;
  }

  get status(): string {
    return this.#status;
  }

  set status(text: string) {
    this.#status = text;
    this.#statusCell.textContent = text;
  }
}

export class StationBed extends OlderStationBed {
  get occupied(): boolean {
    return this.resident !== "";
  }
}

export class NurseStation {
  readonly beds: readonly OlderStationBed[];
  readonly schedule: { readonly rounds?: readonly unknown[] | PagedRounds };

  constructor(host: HTMLElement) {
    const older = host.dataset["nurseStation"] === "older";
    const table = document.createElement("table");
    table.className = "station-beds";
    const seeds = JSON.parse(host.dataset["beds"] ?? "[]") as readonly BedSeed[];
    this.beds = seeds.map((seed) => (older ? new OlderStationBed(table.insertRow(), seed) : new StationBed(table.insertRow(), seed)));
    this.schedule = scheduleOf(host.dataset["schedule"], older);
    host.replaceChildren(table);
  }
}

interface RoundSeed {
  readonly time: string;
  readonly [member: string]: unknown;
}

export class StationCensus {
  readonly #countLine: HTMLParagraphElement;
  readonly #chargesLine: HTMLParagraphElement;
  readonly #volumeLine: HTMLParagraphElement;
  readonly #nameList: HTMLUListElement;
  readonly #countList: HTMLUListElement;
  readonly #answerLine: HTMLParagraphElement;
  readonly #leadLine: HTMLParagraphElement;
  readonly #roundLine: HTMLParagraphElement;
  #answer: boolean | null = null;
  #lead: string | null = null;
  #nextRound: RoundSeed | null = null;
  #count: number | null = null;
  #charges: number | null = null;
  #volume: number | null = null;
  #names: readonly (string | null)[] = [];
  #counts: readonly (number | null)[] = [];

  constructor(host: HTMLElement) {
    this.#countLine = document.createElement("p");
    this.#countLine.className = "census-count";
    this.#countLine.textContent = "unknown";
    this.#nameList = document.createElement("ul");
    this.#nameList.className = "census-names";
    this.#countList = document.createElement("ul");
    this.#countList.className = "census-counts";
    this.#answerLine = lineOf("census-answer");
    this.#leadLine = lineOf("census-lead");
    this.#roundLine = lineOf("census-next-round");
    this.#chargesLine = lineOf("census-charges");
    this.#volumeLine = lineOf("census-volume");
    host.replaceChildren(this.#countLine, this.#chargesLine, this.#volumeLine, this.#nameList, this.#countList, this.#answerLine, this.#leadLine, this.#roundLine);
  }

  get answer(): boolean | null {
    return this.#answer;
  }

  set answer(answer: boolean | null) {
    this.#answer = answer;
    this.#answerLine.textContent = answer === null ? "" : answer ? "yes" : "no";
  }

  get lead(): string | null {
    return this.#lead;
  }

  set lead(name: string | null) {
    this.#lead = name;
    this.#leadLine.textContent = name ?? "";
  }

  get nextRound(): RoundSeed | null {
    return this.#nextRound;
  }

  set nextRound(round: RoundSeed | null) {
    this.#nextRound = round;
    this.#roundLine.textContent = round === null ? "" : round.time;
  }

  get count(): number | null {
    return this.#count;
  }

  set count(count: number | null) {
    this.#count = count;
    this.#countLine.textContent = count === null ? "" : String(count);
  }

  get charges(): number | null {
    return this.#charges;
  }

  set charges(charges: number | null) {
    this.#charges = charges;
    this.#chargesLine.textContent = charges === null ? "" : String(charges);
  }

  get volume(): number | null {
    return this.#volume;
  }

  set volume(volume: number | null) {
    this.#volume = volume;
    this.#volumeLine.textContent = volume === null ? "" : String(volume);
  }

  get names(): readonly (string | null)[] {
    return this.#names;
  }

  set names(names: readonly (string | null)[] | null) {
    this.#names = names ?? [];
    listInto(this.#nameList, this.#names);
  }

  get counts(): readonly (number | null)[] {
    return this.#counts;
  }

  set counts(counts: readonly (number | null)[] | null) {
    this.#counts = counts ?? [];
    listInto(this.#countList, this.#counts);
  }
}

function scheduleOf(json: string | undefined, older: boolean): { readonly rounds?: readonly unknown[] | PagedRounds } {
  if (json === undefined) return {};
  const schedule = JSON.parse(json) as { readonly rounds: readonly unknown[] };
  return older ? { rounds: { result: schedule.rounds, count: schedule.rounds.length } } : schedule;
}

function lineOf(className: string): HTMLParagraphElement {
  const line = document.createElement("p");
  line.className = className;
  line.textContent = "unknown";
  return line;
}

function listInto(list: HTMLUListElement, items: readonly (string | number | null)[]): void {
  list.replaceChildren(
    ...items.map((item) => {
      const entry = document.createElement("li");
      entry.textContent = item === null ? "" : String(item);
      return entry;
    }),
  );
}
