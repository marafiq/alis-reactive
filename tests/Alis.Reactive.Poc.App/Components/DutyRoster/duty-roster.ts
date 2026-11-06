export interface DutyRosterHost extends HTMLElement {
  instances?: DutyRoster[];
}

export class DutyRoster {
  readonly #required: number;
  readonly #lead: HTMLParagraphElement;
  readonly #staffing: HTMLParagraphElement;
  #leadName = "";
  #nurses = 0;

  constructor(host: HTMLElement) {
    this.#required = Number(host.dataset["nursesRequired"]);
    this.#lead = document.createElement("p");
    this.#lead.className = "roster-lead";
    this.#staffing = document.createElement("p");
    this.#staffing.className = "roster-staffing";
    this.#staffing.textContent = this.staffing;
    host.replaceChildren(this.#lead, this.#staffing);
  }

  get lead(): string {
    return this.#leadName;
  }

  set lead(name: string) {
    this.#leadName = name;
    this.#lead.textContent = name;
  }

  get nurses(): number {
    return this.#nurses;
  }

  set nurses(count: number) {
    this.#nurses = count;
    this.#staffing.textContent = this.staffing;
  }

  get staffing(): string {
    return this.#nurses === this.#required ? "Fully staffed" : "Short staffed";
  }
}
