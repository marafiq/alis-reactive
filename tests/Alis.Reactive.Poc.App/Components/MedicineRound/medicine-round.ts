export interface MedicineRoundHost extends HTMLElement {
  round?: MedicineRound;
}

type Signature = (dose: RoundDose) => string;

export class RoundDose {
  readonly drug: string;
  readonly due: boolean;
  given: boolean;
  readonly #line: HTMLParagraphElement;

  constructor(drug: string, given: boolean, line: HTMLParagraphElement) {
    this.drug = drug;
    this.due = true;
    this.given = given;
    this.#line = line;
    line.textContent = drug + (given ? " given" : " due");
  }

  give(): void {
    this.given = true;
    this.#line.textContent = this.drug + " given";
  }
}

export class RoundTrolley {
  signature: Signature | null = null;
  readonly #labelLine: HTMLParagraphElement;
  readonly #signedLine: HTMLParagraphElement;
  readonly #first: RoundDose;

  constructor(labelLine: HTMLParagraphElement, signedLine: HTMLParagraphElement, first: RoundDose) {
    this.#labelLine = labelLine;
    this.#signedLine = signedLine;
    this.#first = first;
  }

  set label(text: string) {
    this.#labelLine.textContent = "Trolley: " + text;
  }

  sign(): void {
    this.#signedLine.textContent = this.signature === null ? "Trolley unsigned" : "Trolley signed for " + this.signature(this.#first);
  }
}

interface RoundChart {
  readonly resident: string;
}

export class RoundStart {
  signature: Signature | null = null;
  readonly doses: readonly RoundDose[];
  readonly trolley: RoundTrolley;
  #note = "";

  constructor(doses: readonly RoundDose[], trolley: RoundTrolley) {
    this.doses = doses;
    this.trolley = trolley;
  }

  set note(text: string) {
    this.#note = text;
  }

  writtenNote(): string {
    return this.#note;
  }

  dueCount(): number {
    return this.doses.filter((dose) => dose.due).length;
  }

  chart(): Promise<RoundChart> {
    return Promise.resolve({ resident: "Ada Byron" });
  }

  dose(position: number): RoundDose {
    const found = this.doses[position];
    if (found === undefined) throw new Error("The round holds no dose at position " + String(position) + ".");
    return found;
  }

  missed(): readonly RoundDose[] {
    return this.doses.filter((dose) => dose.due && !dose.given);
  }
}

type StartListener = (start: RoundStart) => void;

export class MedicineRound {
  readonly #listeners = new Set<StartListener>();
  readonly #doses: readonly RoundDose[];
  readonly #trolley: RoundTrolley;
  readonly #noteLine = lineOf("round-note", "No note");
  readonly #signedLine = lineOf("round-signed", "Unsigned");
  readonly #dueLine = lineOf("round-due", "Due: unknown");
  readonly #missedLine = lineOf("round-missed", "Missed: unknown");
  readonly #listedLine = lineOf("round-listed", "Listed: nothing");
  readonly #residentLine = lineOf("round-resident", "Chart: unread");
  #due: number | null = null;
  #missed: number | null = null;
  #listed: readonly string[] = [];
  #resident: string | null = null;

  constructor(host: HTMLElement) {
    const doseLines = [lineOf("round-dose", ""), lineOf("round-dose", ""), lineOf("round-dose", "")];
    const [insulin, heparin, vitamin] = doseLines;
    if (insulin === undefined || heparin === undefined || vitamin === undefined) throw new Error("The round needs three dose lines.");
    const first = new RoundDose("Insulin", false, insulin);
    this.#doses = [first, new RoundDose("Heparin", false, heparin), new RoundDose("Vitamin D", true, vitamin)];
    const trolleyLine = lineOf("round-trolley", "Trolley: unlabelled");
    const trolleySignedLine = lineOf("round-trolley-signed", "Trolley unsigned");
    this.#trolley = new RoundTrolley(trolleyLine, trolleySignedLine, first);
    const start = buttonOf("Start the round", () => this.#start());
    const sign = buttonOf("Sign the trolley", () => this.#trolley.sign());
    host.replaceChildren(...doseLines, this.#noteLine, this.#signedLine, this.#dueLine, this.#missedLine, this.#listedLine, this.#residentLine, trolleyLine, trolleySignedLine, start, sign);
  }

  on(event: "started", listener: StartListener): void {
    if (event !== "started") throw new Error("A medicine round raises 'started' only, not '" + event + "'.");
    this.#listeners.add(listener);
  }

  off(event: "started", listener: StartListener): void {
    if (event !== "started") throw new Error("A medicine round raises 'started' only, not '" + event + "'.");
    this.#listeners.delete(listener);
  }

  get due(): number | null {
    return this.#due;
  }

  set due(count: number | null) {
    this.#due = count;
    this.#dueLine.textContent = "Due: " + String(count);
  }

  get missed(): number | null {
    return this.#missed;
  }

  set missed(count: number | null) {
    this.#missed = count;
    this.#missedLine.textContent = "Missed: " + String(count);
  }

  get listed(): readonly string[] {
    return this.#listed;
  }

  set listed(drugs: readonly string[]) {
    this.#listed = drugs;
    this.#listedLine.textContent = "Listed: " + drugs.join(", ");
  }

  get resident(): string | null {
    return this.#resident;
  }

  set resident(name: string | null) {
    this.#resident = name;
    this.#residentLine.textContent = "Chart: " + String(name);
  }

  #start(): void {
    const start = new RoundStart(this.#doses, this.#trolley);
    for (const listener of [...this.#listeners]) listener(start);
    this.#noteLine.textContent = "Note: " + start.writtenNote();
    const first = this.#doses[0];
    if (first === undefined) throw new Error("The round holds no dose.");
    this.#signedLine.textContent = start.signature === null ? "Unsigned" : "Signed for " + start.signature(first);
  }
}

function lineOf(className: string, text: string): HTMLParagraphElement {
  const line = document.createElement("p");
  line.className = className;
  line.textContent = text;
  return line;
}

function buttonOf(text: string, click: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = text;
  button.addEventListener("click", click);
  return button;
}
