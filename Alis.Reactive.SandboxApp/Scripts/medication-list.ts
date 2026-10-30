// A resident's medication list: a widget the sandbox wrote itself, with no knowledge of Alis.Reactive.
// Its object lives on its host element as `medicationList`; BroughtComponents/MedicationList/
// MedicationList.cs declares it to the framework. It holds a list of objects, each with members of
// its own, the way a chart holds its series:
//
//   properties  lines (the list: each line has name, units, dailyCost, hoursApart, route, scheduledAt
//               (a Date), controlled and reconciled, and its own reconcile() and isDue()), count,
//               deliveries (a list the page records: pharmacy, deliveredAt, lines)
//   methods     add(name, units, route), remove(name), clear(), reconcileAll()
//   events      changed, with the line that changed and the list as it now stands; raised once for
//               each edit made on the list.
//               Listeners are added with on(name, fn) and removed with off(name, fn).

import "./medication-list.css";

export type Route = "oral" | "injection" | "topical";

export interface LineFacts {
  name: string;
  units: number;
  dailyCost: number;
  hoursApart: number;
  route: Route;
  scheduledAt: Date;
  controlled: boolean;
  reconciled: boolean;
}

/** One line of the list: its facts and its own members. */
export interface Line extends LineFacts {
  reconcile(): void;
  isDue(): boolean;
}

export interface Delivery {
  pharmacy: string;
  deliveredAt: Date;
  lines: string[];
}

/** What a changed listener receives. */
export interface ListChange {
  readonly changed: string;
  readonly items: LineFacts[];
  readonly count: number;
}

type Listener = (payload: unknown) => void;

export interface MedicationListHost extends HTMLElement {
  medicationList?: MedicationList;
}

export function medicationListHosts(root: HTMLElement): MedicationListHost[] {
  return Array.from(root.querySelectorAll<MedicationListHost>("div.medication-list"));
}

const ROUTES: ReadonlyArray<[Route, string]> = [
  ["oral", "By mouth"],
  ["injection", "Injection"],
  ["topical", "Topical"],
];

// A dose is due at 08:00 on the list's day; the list reads its lines against that hour.
const DUE_HOUR = 8;

export class MedicationList {
  private readonly host: MedicationListHost;
  private readonly listeners = new Map<string, Set<Listener>>();
  private items: Line[];
  private recorded: Delivery[] = [];

  constructor(host: MedicationListHost) {
    this.host = host;
    this.items = parseLines(host.dataset.lines ?? "[]").map(facts => this.line(facts));
    host.addEventListener("click", this.onClick);
    host.addEventListener("change", this.onChange);
    host.medicationList = this;
    this.render();
  }

  /** The list: each line an object with members of its own. */
  get lines(): Line[] {
    return this.items;
  }

  get count(): number {
    return this.items.length;
  }

  /** The deliveries the page recorded against the list. */
  get deliveries(): Delivery[] {
    return this.recorded;
  }

  set deliveries(value: Delivery[]) {
    this.recorded = value.map(delivery => ({
      pharmacy: delivery.pharmacy,
      deliveredAt: delivery.deliveredAt instanceof Date ? delivery.deliveredAt : new Date(String(delivery.deliveredAt)),
      lines: [...delivery.lines],
    }));
    this.render();
  }

  add(name: string, units: number, route: Route): void {
    const facts: LineFacts = {
      name,
      units,
      dailyCost: 0,
      hoursApart: 24,
      route,
      scheduledAt: new Date(2026, 5, 2, DUE_HOUR, 0),
      controlled: false,
      reconciled: false,
    };
    this.items = [...this.items, this.line(facts)];
    this.render();
    this.emit("changed", this.change(name));
  }

  remove(name: string): void {
    this.items = this.items.filter(line => line.name !== name);
    this.render();
    this.emit("changed", this.change(name));
  }

  clear(): void {
    this.items = [];
    this.render();
    this.emit("changed", this.change(""));
  }

  reconcileAll(): void {
    for (const line of this.items) line.reconciled = true;
    this.render();
    this.emit("changed", this.change(""));
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
    this.host.removeEventListener("click", this.onClick);
    this.host.removeEventListener("change", this.onChange);
    this.listeners.clear();
    this.host.replaceChildren();
    delete this.host.medicationList;
  }

  private line(facts: LineFacts): Line {
    const list = this;
    return {
      ...facts,
      reconcile(): void {
        this.reconciled = true;
        list.render();
      },
      isDue(): boolean {
        return this.scheduledAt.getHours() <= DUE_HOUR;
      },
    };
  }

  private change(changed: string): ListChange {
    return {
      changed,
      items: this.items.map(line => ({
        name: line.name,
        units: line.units,
        dailyCost: line.dailyCost,
        hoursApart: line.hoursApart,
        route: line.route,
        scheduledAt: line.scheduledAt,
        controlled: line.controlled,
        reconciled: line.reconciled,
      })),
      count: this.items.length,
    };
  }

  private emit(name: string, payload: unknown): void {
    for (const listener of [...(this.listeners.get(name) ?? [])]) listener(payload);
  }

  private readonly onClick = (event: Event): void => {
    const button = (event.target as Element).closest<HTMLButtonElement>("button[data-act]");
    if (button === null) return;
    const act = button.dataset.act;
    if (act === "remove") this.remove(button.dataset.line ?? "");
    if (act === "clear") this.clear();
    if (act === "add") this.addFromRow();
  };

  private readonly onChange = (event: Event): void => {
    const box = event.target;
    if (!(box instanceof HTMLInputElement) || box.dataset.reconcile === undefined) return;
    const line = this.items.find(candidate => candidate.name === box.dataset.reconcile);
    if (line === undefined) return;
    line.reconciled = box.checked;
    this.render();
    this.emit("changed", this.change(line.name));
  };

  private addFromRow(): void {
    const name = this.host.querySelector<HTMLInputElement>("input[data-add='name']")?.value.trim() ?? "";
    const units = Number(this.host.querySelector<HTMLInputElement>("input[data-add='units']")?.value ?? "0");
    const route = (this.host.querySelector<HTMLSelectElement>("select[data-add='route']")?.value ?? "oral") as Route;
    if (name === "") return;
    this.add(name, Number.isFinite(units) ? units : 0, route);
  }

  private render(): void {
    const table = document.createElement("table");
    table.className = "ml-table";
    const head = document.createElement("thead");
    head.innerHTML = "<tr><th>Medication</th><th>Units</th><th>Route</th><th>Scheduled</th><th>Controlled</th><th>Reconciled</th><th></th></tr>";
    const body = document.createElement("tbody");
    for (const line of this.items) {
      const row = document.createElement("tr");
      row.className = "ml-line";
      row.dataset.line = line.name;
      const reconciled = document.createElement("input");
      reconciled.type = "checkbox";
      reconciled.checked = line.reconciled;
      reconciled.dataset.reconcile = line.name;
      reconciled.setAttribute("aria-label", `${line.name} reconciled`);
      const remove = document.createElement("button");
      remove.type = "button";
      remove.className = "ml-remove";
      remove.dataset.act = "remove";
      remove.dataset.line = line.name;
      remove.textContent = "Remove";
      remove.setAttribute("aria-label", `Remove ${line.name}`);
      row.append(
        cell("ml-name", line.name),
        cell("ml-units", String(line.units)),
        cell("ml-route", line.route),
        cell("ml-scheduled", timeText(line.scheduledAt)),
        cell("ml-controlled", line.controlled ? "Controlled" : ""),
        wrap("td", reconciled),
        wrap("td", remove),
      );
      body.append(row);
    }
    table.append(head, body);

    const addRow = element("div", "ml-add");
    const name = document.createElement("input");
    name.type = "text";
    name.dataset.add = "name";
    name.placeholder = "Medication";
    name.setAttribute("aria-label", "New medication");
    const units = document.createElement("input");
    units.type = "number";
    units.dataset.add = "units";
    units.value = "1";
    units.setAttribute("aria-label", "New medication units");
    const route = document.createElement("select");
    route.dataset.add = "route";
    route.setAttribute("aria-label", "New medication route");
    for (const [value, text] of ROUTES) {
      const option = document.createElement("option");
      option.value = value;
      option.textContent = text;
      route.append(option);
    }
    const add = document.createElement("button");
    add.type = "button";
    add.dataset.act = "add";
    add.textContent = "Add to the list";
    const clear = document.createElement("button");
    clear.type = "button";
    clear.dataset.act = "clear";
    clear.textContent = "Clear the list";
    addRow.append(name, units, route, add, clear);

    const footer = element("div", "ml-footer");
    footer.append(
      element("span", "ml-count", this.items.length === 1 ? "1 medication" : `${this.items.length} medications`),
      element("span", "ml-deliveries", this.recorded.length === 0 ? "No delivery recorded" : `${this.recorded.length} delivery recorded`),
    );

    this.host.replaceChildren(table, addRow, footer);
  }
}

function parseLines(json: string): LineFacts[] {
  const raw = JSON.parse(json) as Array<Partial<Record<keyof LineFacts, unknown>>>;
  return raw.map(line => ({
    name: String(line.name ?? ""),
    units: Number(line.units ?? 0),
    dailyCost: Number(line.dailyCost ?? 0),
    hoursApart: Number(line.hoursApart ?? 0),
    route: (typeof line.route === "string" ? line.route : "oral") as Route,
    scheduledAt: new Date(String(line.scheduledAt ?? "2026-06-02T08:00:00")),
    controlled: line.controlled === true,
    reconciled: line.reconciled === true,
  }));
}

function timeText(date: Date): string {
  return `${String(date.getHours()).padStart(2, "0")}:${String(date.getMinutes()).padStart(2, "0")}`;
}

function cell(className: string, text: string): HTMLElement {
  return element("td", className, text);
}

function wrap(tag: string, child: HTMLElement): HTMLElement {
  const created = document.createElement(tag);
  created.append(child);
  return created;
}

function element(tag: string, className: string, text = ""): HTMLElement {
  const created = document.createElement(tag);
  created.className = className;
  created.textContent = text;
  return created;
}
