export interface DutyRosterHost extends HTMLElement {
  instances?: (DutyRoster | DutyRosterV2)[];
}

export interface OnCallNurse {
  readonly name: string;
  readonly pager_line?: { readonly number: string };
}

export class RosterShift {
  readonly #heading: HTMLHeadingElement;
  #label: string;

  constructor(heading: HTMLHeadingElement, label: string) {
    this.#heading = heading;
    this.#label = label;
    heading.textContent = label;
  }

  get label(): string {
    return this.#label;
  }

  set label(text: string) {
    this.#label = text;
    this.#heading.textContent = text;
  }
}

export class DutyRoster {
  readonly #required: number;
  readonly #shift: RosterShift;
  readonly #onCall: OnCallNurse;
  readonly #lead: HTMLParagraphElement;
  readonly #staffing: HTMLParagraphElement;
  #leadName = "";
  #nurses = 0;

  constructor(host: HTMLElement) {
    this.#required = Number(host.dataset["nursesRequired"]);
    const heading = document.createElement("h3");
    heading.className = "roster-shift";
    this.#shift = new RosterShift(heading, host.dataset["shift"] ?? "");
    this.#onCall = onCallOf(host);
    this.#lead = document.createElement("p");
    this.#lead.className = "roster-lead";
    this.#staffing = document.createElement("p");
    this.#staffing.className = "roster-staffing";
    this.#staffing.textContent = this.staffing;
    host.replaceChildren(heading, this.#lead, this.#staffing);
  }

  get shift(): RosterShift {
    return this.#shift;
  }

  get onCall(): OnCallNurse {
    return this.#onCall;
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

export class DutyRosterV2 {
  readonly #lead: HTMLParagraphElement;
  #leadName = "";

  constructor(host: HTMLElement) {
    this.#lead = document.createElement("p");
    this.#lead.className = "roster-lead";
    host.replaceChildren(this.#lead);
  }

  get lead(): string {
    return this.#leadName;
  }

  set lead(name: string) {
    this.#leadName = name;
    this.#lead.textContent = name;
  }
}

function onCallOf(host: HTMLElement): OnCallNurse {
  const name = host.dataset["onCallName"] ?? "";
  const pager = host.dataset["onCallPager"];
  return pager === undefined ? { name } : { name, pager_line: { number: pager } };
}
