// An applicant's intake sheet: a widget the sandbox wrote itself, with no knowledge of Alis.Reactive.
// Its object lives on its host element as `intakeSheet`; BroughtComponents/IntakeSheet/IntakeSheet.cs
// declares it to the framework. Each fact is held the way JavaScript holds it, so a page reads every
// kind of value the framework converts:
//
//   properties  name (text), age (number), moveInDate (Date), veteran (boolean), careLevel (an
//               option's text), monthlyRate (a number with cents); roomPreference, petCount,
//               priorStayEnded, fluShot and diet, each of those or null when the sheet does not hold
//               it; contact (an object: phone and relation); allergies (a list of text);
//               waitlist (a number or null, what the waiting list says). All read and written.
//   functions   summary() (text; the sheet counts how often it was written), yearsOver(minimum)
//               (a number), waitlistPosition() (a promise of the waitlist number; the sheet counts
//               how often it was asked), daysUntilMoveIn() (a number)
//   methods     load(applicant), clear()
//   events      changed, with the field that changed, the applicant's facts and the contact; raised
//               once for each edit the coordinator commits on the sheet.
//               Listeners are added with on(name, fn) and removed with off(name, fn).

import "./intake-sheet.css";

export type CareLevel = "independent" | "assisted" | "memoryCare";

export type Diet = "regular" | "diabetic" | "pureed";

export interface Contact {
  phone: string;
  relation: string;
}

export interface Applicant {
  name: string;
  age: number;
  moveInDate: Date;
  veteran: boolean;
  careLevel: CareLevel;
  monthlyRate: number;
  roomPreference: string | null;
  petCount: number | null;
  priorStayEnded: Date | null;
  fluShot: boolean | null;
  diet: Diet | null;
  contact: Contact;
  allergies: string[];
}

/** What a changed listener receives: the field edited, the applicant's facts and the contact. */
export interface SheetChange {
  readonly field: string;
  readonly applicant: Applicant;
  readonly contact: Contact;
}

type Listener = (payload: unknown) => void;

type Control = HTMLInputElement | HTMLSelectElement;

export interface IntakeSheetHost extends HTMLElement {
  intakeSheet?: IntakeSheet;
}

export function intakeSheetHosts(root: HTMLElement): IntakeSheetHost[] {
  return Array.from(root.querySelectorAll<IntakeSheetHost>("div.intake-sheet"));
}

const CARE_LEVELS: ReadonlyArray<[CareLevel, string]> = [
  ["independent", "Independent living"],
  ["assisted", "Assisted living"],
  ["memoryCare", "Memory care"],
];

const DIETS: ReadonlyArray<[Diet | "", string]> = [
  ["", "None recorded"],
  ["regular", "Regular"],
  ["diabetic", "Diabetic"],
  ["pureed", "Pureed"],
];

const FLU_SHOTS: ReadonlyArray<[string, string]> = [
  ["", "Not asked"],
  ["yes", "Yes"],
  ["no", "No"],
];

export class IntakeSheet {
  private readonly host: IntakeSheetHost;
  private readonly listeners = new Map<string, Set<Listener>>();
  private readonly controls = new Map<string, Control>();
  private readonly summariesLine: HTMLElement;
  private readonly waitlistChecksLine: HTMLElement;
  private facts: Applicant;
  private waitlistNumber: number | null;
  private summaries = 0;
  private waitlistChecks = 0;

  constructor(host: IntakeSheetHost) {
    this.host = host;
    this.facts = parseApplicant(host.dataset.applicant ?? "{}");
    this.waitlistNumber = host.dataset.waitlist === undefined || host.dataset.waitlist === "" ? null : Number(host.dataset.waitlist);
    this.summariesLine = element("span", "is-summaries");
    this.waitlistChecksLine = element("span", "is-waitlist-checks");
    this.build();
    this.refresh();
    host.addEventListener("change", this.onEdit);
    host.intakeSheet = this;
  }

  get name(): string {
    return this.facts.name;
  }

  set name(value: string) {
    this.facts.name = value;
    this.refresh();
  }

  get age(): number {
    return this.facts.age;
  }

  set age(value: number) {
    this.facts.age = value;
    this.refresh();
  }

  get moveInDate(): Date {
    return this.facts.moveInDate;
  }

  set moveInDate(value: Date) {
    this.facts.moveInDate = value;
    this.refresh();
  }

  get veteran(): boolean {
    return this.facts.veteran;
  }

  set veteran(value: boolean) {
    this.facts.veteran = value;
    this.refresh();
  }

  get careLevel(): CareLevel {
    return this.facts.careLevel;
  }

  set careLevel(value: CareLevel) {
    this.facts.careLevel = value;
    this.refresh();
  }

  get monthlyRate(): number {
    return this.facts.monthlyRate;
  }

  set monthlyRate(value: number) {
    this.facts.monthlyRate = value;
    this.refresh();
  }

  get roomPreference(): string | null {
    return this.facts.roomPreference;
  }

  set roomPreference(value: string | null) {
    this.facts.roomPreference = value;
    this.refresh();
  }

  get petCount(): number | null {
    return this.facts.petCount;
  }

  set petCount(value: number | null) {
    this.facts.petCount = value;
    this.refresh();
  }

  get priorStayEnded(): Date | null {
    return this.facts.priorStayEnded;
  }

  set priorStayEnded(value: Date | null) {
    this.facts.priorStayEnded = value;
    this.refresh();
  }

  get fluShot(): boolean | null {
    return this.facts.fluShot;
  }

  set fluShot(value: boolean | null) {
    this.facts.fluShot = value;
    this.refresh();
  }

  get diet(): Diet | null {
    return this.facts.diet;
  }

  set diet(value: Diet | null) {
    this.facts.diet = value;
    this.refresh();
  }

  /** The family contact: an object with members of its own. */
  get contact(): Contact {
    return this.facts.contact;
  }

  get allergies(): string[] {
    return this.facts.allergies;
  }

  set allergies(value: string[]) {
    this.facts.allergies = [...value];
    this.refresh();
  }

  /** The applicant's number on the waiting list, or null while the list has not answered. */
  get waitlist(): number | null {
    return this.waitlistNumber;
  }

  set waitlist(value: number | null) {
    this.waitlistNumber = value;
    this.refresh();
  }

  /** Replaces every fact at once, as a waiting-list record arrives. */
  load(applicant: Applicant): void {
    this.facts = { ...applicant, contact: { ...applicant.contact }, allergies: [...applicant.allergies] };
    this.refresh();
  }

  clear(): void {
    this.facts = emptyApplicant();
    this.refresh();
  }

  /** The applicant in one line; the sheet counts each summary it writes. */
  summary(): string {
    this.summaries += 1;
    this.refresh();
    const { name, age, careLevel, moveInDate } = this.facts;
    return `${name}, ${age}, ${careLevel}, moving in ${dateText(moveInDate)}`;
  }

  yearsOver(minimum: number): number {
    return this.facts.age - minimum;
  }

  daysUntilMoveIn(): number {
    const today = new Date(2026, 5, 1);
    return Math.round((this.facts.moveInDate.getTime() - today.getTime()) / 86_400_000);
  }

  /** Asks the waiting list, which answers a moment later; the sheet counts each time it is asked. */
  waitlistPosition(): Promise<number | null> {
    this.waitlistChecks += 1;
    this.refresh();
    const position = this.waitlistNumber;
    return new Promise(resolve => setTimeout(() => resolve(position), 120));
  }

  on(name: string, listener: Listener): void {
    let named = this.listeners.get(name);
    if (named === undefined) {
      named = new Set();
      this.listeners.set(name, named);
    }
    named.add(listener);
  }

  off(name: string, listener: Listener): void {
    this.listeners.get(name)?.delete(listener);
  }

  destroy(): void {
    this.host.removeEventListener("change", this.onEdit);
    this.listeners.clear();
    this.controls.clear();
    this.host.replaceChildren();
    delete this.host.intakeSheet;
  }

  private readonly onEdit = (event: Event): void => {
    const control = event.target;
    if (!(control instanceof HTMLInputElement || control instanceof HTMLSelectElement)) return;
    const field = control.dataset.field;
    if (field === undefined) return;
    this.take(field, control);
    this.refresh();
    this.emit("changed", {
      field,
      applicant: { ...this.facts, contact: { ...this.facts.contact }, allergies: [...this.facts.allergies] },
      contact: { ...this.facts.contact },
    } satisfies SheetChange);
  };

  private take(field: string, control: Control): void {
    const text = control.value;
    switch (field) {
      case "name": this.facts.name = text; break;
      case "age": this.facts.age = text === "" ? 0 : Number(text); break;
      case "moveInDate": this.facts.moveInDate = localDate(text) ?? this.facts.moveInDate; break;
      case "veteran": this.facts.veteran = (control as HTMLInputElement).checked; break;
      case "careLevel": this.facts.careLevel = text as CareLevel; break;
      case "monthlyRate": this.facts.monthlyRate = text === "" ? 0 : Number(text); break;
      case "roomPreference": this.facts.roomPreference = text === "" ? null : text; break;
      case "petCount": this.facts.petCount = text === "" ? null : Number(text); break;
      case "priorStayEnded": this.facts.priorStayEnded = localDate(text); break;
      case "fluShot": this.facts.fluShot = text === "" ? null : text === "yes"; break;
      case "diet": this.facts.diet = text === "" ? null : (text as Diet); break;
      case "contactPhone": this.facts.contact.phone = text; break;
      case "contactRelation": this.facts.contact.relation = text; break;
      case "allergies": this.facts.allergies = text.split(",").map(item => item.trim()).filter(item => item !== ""); break;
      case "waitlist": this.waitlistNumber = text === "" ? null : Number(text); break;
    }
  }

  private emit(name: string, payload: unknown): void {
    for (const listener of [...(this.listeners.get(name) ?? [])]) listener(payload);
  }

  // The controls are built once; a refresh writes the facts into them, so an edit never loses focus.
  private build(): void {
    const fields = element("div", "is-fields");
    fields.append(
      this.field("Applicant", input("text", "name")),
      this.field("Age", numberInput("age", "1")),
      this.field("Move-in date", input("date", "moveInDate")),
      this.field("Veteran", input("checkbox", "veteran")),
      this.field("Care level", select("careLevel", CARE_LEVELS)),
      this.field("Monthly rate", numberInput("monthlyRate", "0.01")),
      this.field("Room preference", input("text", "roomPreference")),
      this.field("Pets", numberInput("petCount", "1")),
      this.field("Previous stay ended", input("date", "priorStayEnded")),
      this.field("Flu shot", select("fluShot", FLU_SHOTS)),
      this.field("Diet", select("diet", DIETS)),
      this.field("Contact phone", input("text", "contactPhone")),
      this.field("Contact relation", input("text", "contactRelation")),
      this.field("Allergies", input("text", "allergies")),
      this.field("Waiting list number", numberInput("waitlist", "1")),
    );

    const footer = element("div", "is-footer");
    footer.append(this.summariesLine, this.waitlistChecksLine);
    this.host.replaceChildren(fields, footer);
  }

  // Each control is named by its label alone, so a select's options never join its name.
  private field(text: string, control: Control): HTMLElement {
    this.controls.set(control.dataset.field!, control);
    control.setAttribute("aria-label", text);
    const label = document.createElement("label");
    label.className = "is-field";
    label.append(element("span", "is-label", text), control);
    return label;
  }

  private refresh(): void {
    const facts = this.facts;
    this.write("name", facts.name);
    this.write("age", String(facts.age));
    this.write("moveInDate", dateText(facts.moveInDate));
    this.tick("veteran", facts.veteran);
    this.write("careLevel", facts.careLevel);
    this.write("monthlyRate", facts.monthlyRate.toFixed(2));
    this.write("roomPreference", facts.roomPreference ?? "");
    this.write("petCount", facts.petCount === null ? "" : String(facts.petCount));
    this.write("priorStayEnded", dateText(facts.priorStayEnded));
    this.write("fluShot", facts.fluShot === null ? "" : facts.fluShot ? "yes" : "no");
    this.write("diet", facts.diet ?? "");
    this.write("contactPhone", facts.contact.phone);
    this.write("contactRelation", facts.contact.relation);
    this.write("allergies", facts.allergies.join(", "));
    this.write("waitlist", this.waitlistNumber === null ? "" : String(this.waitlistNumber));
    this.summariesLine.textContent = `Summaries written: ${this.summaries}`;
    this.waitlistChecksLine.textContent = `Waitlist checks: ${this.waitlistChecks}`;
  }

  private write(field: string, value: string): void {
    const control = this.controls.get(field);
    if (control !== undefined && control.value !== value) control.value = value;
  }

  private tick(field: string, checked: boolean): void {
    const control = this.controls.get(field);
    if (control instanceof HTMLInputElement) control.checked = checked;
  }
}

function parseApplicant(json: string): Applicant {
  const raw = JSON.parse(json) as Partial<Record<keyof Applicant, unknown>>;
  const empty = emptyApplicant();
  const contact = (raw.contact ?? {}) as Partial<Contact>;
  return {
    name: typeof raw.name === "string" ? raw.name : empty.name,
    age: typeof raw.age === "number" ? raw.age : empty.age,
    moveInDate: parseDate(raw.moveInDate) ?? empty.moveInDate,
    veteran: raw.veteran === true,
    careLevel: typeof raw.careLevel === "string" ? (raw.careLevel as CareLevel) : empty.careLevel,
    monthlyRate: typeof raw.monthlyRate === "number" ? raw.monthlyRate : empty.monthlyRate,
    roomPreference: typeof raw.roomPreference === "string" ? raw.roomPreference : null,
    petCount: typeof raw.petCount === "number" ? raw.petCount : null,
    priorStayEnded: parseDate(raw.priorStayEnded),
    fluShot: typeof raw.fluShot === "boolean" ? raw.fluShot : null,
    diet: typeof raw.diet === "string" ? (raw.diet as Diet) : null,
    contact: { phone: contact.phone ?? "", relation: contact.relation ?? "" },
    allergies: Array.isArray(raw.allergies) ? raw.allergies.map(String) : [],
  };
}

function emptyApplicant(): Applicant {
  return {
    name: "",
    age: 0,
    moveInDate: new Date(2026, 6, 1),
    veteran: false,
    careLevel: "independent",
    monthlyRate: 0,
    roomPreference: null,
    petCount: null,
    priorStayEnded: null,
    fluShot: null,
    diet: null,
    contact: { phone: "", relation: "" },
    allergies: [],
  };
}

// A date as the server writes it ("2026-07-01T00:00:00") or as a date input holds it ("2026-07-01"),
// read as that calendar day at local midnight.
function parseDate(value: unknown): Date | null {
  if (value instanceof Date) return value;
  if (typeof value !== "string" || value === "") return null;
  return localDate(value.slice(0, 10));
}

function localDate(text: string): Date | null {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(text);
  if (match === null) return null;
  return new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
}

function dateText(date: Date | null): string {
  if (date === null) return "";
  return `${date.getFullYear()}-${twoDigits(date.getMonth() + 1)}-${twoDigits(date.getDate())}`;
}

function twoDigits(value: number): string {
  return String(value).padStart(2, "0");
}

function input(type: string, field: string): HTMLInputElement {
  const box = document.createElement("input");
  box.type = type;
  box.dataset.field = field;
  return box;
}

function numberInput(field: string, step: string): HTMLInputElement {
  const box = input("number", field);
  box.step = step;
  return box;
}

function select(field: string, options: ReadonlyArray<[string, string]>): HTMLSelectElement {
  const list = document.createElement("select");
  list.dataset.field = field;
  for (const [value, text] of options) {
    const option = document.createElement("option");
    option.value = value;
    option.textContent = text;
    list.append(option);
  }
  return list;
}

function element(tag: string, className: string, text = ""): HTMLElement {
  const created = document.createElement(tag);
  created.className = className;
  created.textContent = text;
  return created;
}
