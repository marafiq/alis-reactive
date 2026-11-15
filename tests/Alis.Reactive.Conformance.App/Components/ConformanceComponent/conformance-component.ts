import { ComponentEvents } from "../component-events";
import type { Change, ComponentListener } from "../component-events";
import { ComponentView, mountComponents } from "../component-view";
import { ConformancePeer } from "../ConformancePeer/conformance-peer";
import type { BindingData, Linked, RecordData, TakenRecord } from "./data";
import { Item, Outer, itemStart } from "./live-objects";
import { Raised } from "./raised";
import type { BindingStart, ConformanceStart, ItemStart, RaiseStart, StartsStart } from "./start-options";
interface ComponentElement extends HTMLElement {
    conformanceComponent?: ConformanceComponent;
}
export class ConformanceComponent extends ComponentEvents {
    readonly element: ComponentElement;
    private readonly start: ConformanceStart;
    readonly readOnlyText: string;
    private methodCalls = 0;
    private destroyed = false;
    private pings = 0;
    private raises = 0;
    private heldText: string;
    private heldValue: string | null;
    private heldCount: number;
    private heldAmount: number;
    private heldRatio = 0;
    private heldNullableCount: number | null = null;
    private heldRecords: RecordData[];
    private readonly view: ComponentView;
    private readonly heading: HTMLHeadingElement;
    private readonly recordsList: HTMLUListElement;
    private readonly itemsList: HTMLUListElement;
    private readonly logList: HTMLUListElement;
    private readonly currentItem: Item | null;
    private peer: ConformancePeer | null = null;
    private readonly box: HTMLInputElement;
    enabled: boolean;
    day = new Date(2026, 10, 9, 9);
    nullableDay: Date | null = null;
    moment = new Date("2026-11-09T14:00:00Z");
    choice: "low" | "high" = "low";
    days = 0;
    optionalText?: string | null;
    record: RecordData;
    nullableRecords: RecordData[] | null = null;
    bindings: BindingStart[];
    binding: BindingData = {};
    starts: StartsStart;
    outer: Outer;
    nullableOuter: Outer | null = null;
    items: Item[];
    nullableItems: Item[] | null;
    answer: ((item: Item) => string) | null = null;
    pair: ((key: string, record: RecordData) => string) | null = null;
    override addEventListener(type: "change" | "commit", callback: ((value: Change) => void) | null, options?: boolean | AddEventListenerOptions): void;
    override addEventListener(type: "raised", callback: ((value: Raised) => void) | null, options?: boolean | AddEventListenerOptions): void;
    override addEventListener(type: "handed", callback: ((value: ConformancePeer) => void) | null, options?: boolean | AddEventListenerOptions): void;
    override addEventListener(type: string, callback: ComponentListener | null, options?: boolean | AddEventListenerOptions): void;
    override addEventListener(type: string, callback: ComponentListener | null, options?: boolean | AddEventListenerOptions): void {
        super.addEventListener(type, callback, options);
    }
    constructor(element: ComponentElement, start: ConformanceStart) {
        super();
        this.element = element;
        this.start = start;
        this.view = new ComponentView(element);
        this.heading = document.createElement("h2");
        element.append(this.heading);
        this.heldText = start.text ?? "";
        this.heldValue = start.value;
        this.heldCount = start.count;
        this.heldAmount = start.amount;
        this.enabled = start.enabled;
        this.readOnlyText = start.readOnlyText;
        this.record = start.record;
        this.heldRecords = start.records;
        this.bindings = start.bindings;
        this.starts = start.starts;
        this.recordsList = this.view.list("Records", []);
        this.itemsList = this.view.list("Items", []);
        this.logList = this.view.list("Log", []);
        this.items = start.items.map((item) => this.makeItem(item));
        this.nullableItems = start.nullableItems === null ? null : start.nullableItems.map((item) => this.makeItem(item));
        this.currentItem = start.currentItem === null ? null : this.makeItem(itemStart(start.currentItem));
        if (this.currentItem !== null && start.currentItemLacksAct)
            Object.defineProperty(this.currentItem, "act", { value: undefined });
        if (this.currentItem !== null && start.currentItemLacksValue)
            Object.defineProperty(this.currentItem, "value", { value: undefined });
        if (this.currentItem === null)
            Object.defineProperty(this, "current", { value: () => { this.called(); return null; } });
        if (start.text === null)
            Object.defineProperty(this, "text", { value: null, writable: true, configurable: true });
        this.outer = new Outer(start.outer, this.view, (endpoint) => this.request(endpoint), start.textEndpoint);
        this.box = this.view.input("Search", "search");
        this.draw();
        this.controls();
        this.versions();
        this.ownPeer();
        element.conformanceComponent = this;
    }
    get text(): string { return this.heldText; }
    set text(value: string) {
        if (value !== this.heldText)
            this.ask();
        this.heldText = value;
        this.heading.textContent = value;
    }
    get value(): string | null { return this.heldValue; }
    set value(value: string | null) { this.heldValue = value; this.view.status("Value", value ?? ""); }
    get count(): number { return this.heldCount; }
    set count(value: number) { this.heldCount = value; this.view.status("Count", String(value)); }
    get amount(): number { return this.heldAmount; }
    set amount(value: number) { this.heldAmount = value; this.view.status("Amount", String(value)); }
    get ratio(): number { return this.heldRatio; }
    set ratio(value: number) { this.heldRatio = value; this.view.status("Ratio", String(value)); }
    get nullableCount(): number | null { return this.heldNullableCount; }
    set nullableCount(value: number | null) { this.heldNullableCount = value; this.view.status("Nullable count", value === null ? "none" : String(value)); }
    get records(): RecordData[] { return this.heldRecords; }
    set records(value: RecordData[]) { this.heldRecords = value; this.drawRecords(); }
    get readOnlyCount(): number { return this.count; }
    get calls(): number { return this.methodCalls; }
    get isDestroyed(): boolean { return this.destroyed; }
    set writeOnlyText(value: string) { this.view.status("Write-only", "Written " + value); }
    ping(): void { this.called(); this.showPing(); }
    total(): number { this.called(); return this.count; }
    check(): boolean { this.called(); return this.enabled; }
    echo(text: string): string { this.called(); return text; }
    snapshot(): RecordData { this.called(); return this.record; }
    current(): Item {
        this.called();
        if (this.currentItem === null)
            throw new Error("The component has no current item.");
        return this.currentItem;
    }
    maybeItem(): Item | null { this.called(); return this.currentItem; }
    matching(): Item[] { this.called(); return this.items.filter((item) => item.active); }
    self(): this { this.called(); return this; }
    async settle(): Promise<void> { this.called(); await this.request(this.start.settleEndpoint); }
    async settleCount(): Promise<number> {
        this.called();
        const value = await this.request(this.start.countEndpoint);
        if (typeof value !== "number")
            throw new Error("The count endpoint did not answer a number.");
        return value;
    }
    async settleCheck(): Promise<boolean> {
        this.called();
        const value = await this.request(this.start.checkEndpoint);
        if (typeof value !== "boolean")
            throw new Error("The check endpoint did not answer a boolean.");
        return value;
    }
    async settleRecord(): Promise<RecordData> { this.called(); return this.recordAnswer(await this.request(this.start.recordEndpoint)); }
    async settleMaybe(): Promise<RecordData | null> {
        this.called();
        const value = await this.request(this.start.recordEndpoint);
        return value === null ? null : this.recordAnswer(value);
    }
    log(text: string): void { this.called(); this.appendLog("text " + text); }
    add(amount: number): void { this.called(); this.amount += amount; }
    takeNullable(count: number | null): void {
        this.called();
        this.view.status("Took", "Took " + (count === null ? "none" : String(count)));
        if (count === null)
            this.appendLog("null");
    }
    takeText(text: string | null): void {
        this.called();
        const received: unknown = text;
        this.view.status("Taken text", received === undefined ? "(undefined)" : text ?? "(null)");
        this.appendLog(text === null ? "null" : "text " + String(received));
    }
    takeDay(day: Date): void { this.called(); this.appendLog(this.localDay(day)); }
    takeMoment(moment: Date): void { this.called(); this.appendLog("Date " + moment.toISOString()); }
    take(record: TakenRecord): void {
        this.called();
        this.view.status("Records", this.inItsCode(record));
        const taken: unknown = record;
        if (taken === this.record)
            this.appendLog("same record");
    }
    takeAll(records: TakenRecord[]): void { this.called(); this.view.status("Records", records.map((record) => this.inItsCode(record)).join("; ")); }
    takeDays(days: Date[]): void { this.called(); for (const day of days)
        this.appendLog(this.localDay(day)); }
    takeLinked(linked: Linked): void {
        this.called();
        const day: unknown = linked.day;
        if (!(day instanceof Date))
            throw new Error("The linked day is not a Date.");
        this.appendLog(this.localDay(day));
    }
    sortBy(field: string): void {
        this.called();
        this.records = [...this.records].sort((left, right) => {
            const a: unknown = Reflect.get(left, field);
            const b: unknown = Reflect.get(right, field);
            if (typeof a === "number" && typeof b === "number")
                return a - b;
            if (typeof a === "string" && typeof b === "string")
                return a.localeCompare(b);
            throw new Error("The record has no comparable field " + field + ".");
        });
    }
    sortItems(key: (item: Item) => number): void { this.called(); this.items.sort((left, right) => key(left) - key(right)); this.drawItems(); }
    destroy(): void {
        if (this.destroyed)
            return;
        this.called();
        this.destroyed = true;
        this.view.lifetime.abort();
        this.dropPeer();
        delete this.element.conformanceComponent;
    }
    override removeEventListener(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | EventListenerOptions): void {
        if (this.destroyed)
            throw new Error("The component was destroyed before its listener was removed.");
        super.removeEventListener(type, callback, options);
    }
    private inItsCode(record: TakenRecord): string { return String(record.name) + " in " + record.code; }
    private called(): void { this.view.status("Calls", String(++this.methodCalls)); }
    private makeItem(start: ItemStart): Item { return new Item(start, this.view, (endpoint) => this.request(endpoint), this.start.itemEndpoint, () => { this.drawItems(); }); }
    private recordAnswer(value: unknown): RecordData {
        if (typeof value !== "object" || value === null || Array.isArray(value))
            throw new Error("The record endpoint did not answer a record.");
        return value as RecordData;
    }
    private async request(endpoint: string): Promise<unknown> {
        try {
            const response = await fetch(endpoint, { method: "POST" });
            if (!response.ok)
                throw new Error("The endpoint refused with " + String(response.status) + ": " + endpoint);
            const value: unknown = response.status === 204 ? undefined : await response.json();
            this.view.status("Settle", "Answered");
            return value;
        }
        catch (error) {
            this.view.status("Settle", "Refused");
            throw error;
        }
    }
    private appendLog(text: string): void {
        const item = document.createElement("li");
        item.textContent = text;
        this.logList.append(item);
        this.view.status("Log", text);
    }
    private localDay(day: Date): string {
        const part = (value: number): string => String(value).padStart(2, "0");
        return "Date " + String(day.getFullYear()).padStart(4, "0") + "-" + part(day.getMonth() + 1) + "-" + part(day.getDate()) + " " + part(day.getHours()) + ":" + part(day.getMinutes());
    }
    private drawRecords(): void {
        this.recordsList.replaceChildren(...this.records.map((record) => {
            const item = document.createElement("li");
            item.textContent = record.name;
            return item;
        }));
    }
    private drawItems(): void {
        this.itemsList.replaceChildren(...this.items.map((item) => {
            const line = document.createElement("li");
            line.textContent = item.name + " " + String(item.value);
            return line;
        }));
    }
    private draw(): void {
        this.heading.textContent = this.text;
        this.value = this.heldValue;
        this.count = this.heldCount;
        this.amount = this.heldAmount;
        this.ratio = this.heldRatio;
        this.nullableCount = this.heldNullableCount;
        for (const [name, text] of [["Ping", "Pinged 0 times"], ["Write-only", "Unwritten"], ["Raise", "Waiting"], ["Raises", "Raised 0"],
            ["Error", ""], ["Payload", "Payload text: none"], ["Held", "Held: none"], ["Answer", ""], ["Calls", "0"], ["Took", "Took none"], ["Records", "None"]] as const)
            this.view.status(name, text);
        this.drawItems();
        this.drawRecords();
    }
    private ask(): void {
        try {
            const pair = this.pair;
            const answer = this.answer;
            const answers = pair === null ? this.items.map((item, position) => answer === null ? String(position + 1) : answer(item)) : this.records.map((record) => pair("count", record));
            this.view.list("Answers", answers, this.view.status("Answer", ""));
            this.view.status("Error", "");
        }
        catch (error) {
            this.view.status("Error", error instanceof Error ? error.message : String(error));
            throw error;
        }
    }
    private ownPeer(): ConformancePeer {
        if (this.peer === null) {
            const element = document.createElement("section");
            this.element.after(element);
            this.peer = new ConformancePeer(element);
        }
        return this.peer;
    }
    private dropPeer(): void {
        if (this.peer === null)
            return;
        this.peer.destroy();
        this.peer.element.remove();
        this.peer = null;
    }
    private raise(option: RaiseStart): void {
        this.view.status("Raises", "Raised " + String(++this.raises));
        if (option.noArgument) {
            this.emit("raised");
            return;
        }
        const raised = new Raised(option, this.start, this.ownPeer(), this.view, (endpoint) => this.request(endpoint), this.start.textEndpoint, (item) => this.makeItem(item));
        this.emit("raised", raised);
        raised.returned();
    }
    private defaultRaise(flag: boolean): RaiseStart {
        const text = this.box.value;
        return { button: "Raise", flag, text, count: text.trim() !== "" && Number.isFinite(Number(text)) ? Number(text) : 14,
            nullableText: null, optionalText: null, noArgument: false, emptyRecord: false, record: this.start.record };
    }
    private controls(): void {
        if (this.start.raises.length === 0) {
            this.view.button("Raise", () => { this.raise(this.defaultRaise(true)); });
            this.view.button("Raise plain", () => { this.raise(this.defaultRaise(false)); });
        }
        else
            for (const option of this.start.raises)
                this.view.button(option.button, () => { this.raise(option); });
        this.view.button("Raise with a new peer", () => { this.dropPeer(); this.raise(this.defaultRaise(true)); });
        this.view.button("Hand a peer", () => { this.emit("handed", this.ownPeer()); });
        this.view.button("Drop the peer", () => { this.dropPeer(); });
        this.view.button("Ask", () => { try {
            this.ask();
        }
        catch {
            return;
        } });
        this.view.button("Free", () => { this.count = 1; });
        this.view.button("Busy", () => { this.count = 0; });
        this.view.button("Low", () => { this.choice = "low"; });
        this.view.button("High", () => { this.choice = "high"; });
        this.view.button("Hold medium", () => { Object.defineProperty(this, "choice", { value: "medium", writable: true, configurable: true }); });
        this.view.button("Hold 129", () => { this.days = 129; });
        this.view.button("Sort", () => { this.records = [...this.records].sort((a, b) => String(a.name).localeCompare(String(b.name))); });
        this.view.button("Edit first", () => { const first = this.records[0]; if (first !== undefined)
            first.count++; this.drawRecords(); });
        this.view.button("Deactivate last", () => this.items.at(-1)?.deactivate());
        this.view.button("Paged", () => { Object.defineProperty(this, "nullableRecords", { value: { result: this.records, count: this.records.length }, writable: true, configurable: true }); });
        this.view.button("Clear", () => { this.nullableRecords = null; });
        this.view.button("Remove the read-only text", () => { Reflect.deleteProperty(this, "readOnlyText"); });
        this.view.button("Destroy", () => { this.destroy(); });
        const day = this.view.input("Day", "date");
        day.addEventListener("input", () => {
            this.nullableDay = day.value === "" ? null : new Date(day.value + "T00:00:00");
            this.day = this.nullableDay ?? new Date(NaN);
        }, { signal: this.view.lifetime.signal });
        this.valueList();
    }
    private valueList(): void {
        const list = document.createElement("ul");
        list.setAttribute("role", "listbox");
        list.setAttribute("aria-label", "Value");
        list.tabIndex = 0;
        const options = this.start.options.map((text) => {
            const option = document.createElement("li");
            option.setAttribute("role", "option");
            option.textContent = text;
            list.append(option);
            return option;
        });
        let active = this.start.options.indexOf(this.value ?? "none");
        const mark = (): void => { options.forEach((option, place) => { option.setAttribute("aria-selected", String(place === active)); }); };
        mark();
        list.addEventListener("keydown", (event) => {
            if (event.key === "ArrowDown")
                active = Math.min(active + 1, options.length - 1);
            else if (event.key === "ArrowUp")
                active = Math.max(active - 1, 0);
            else if (event.key === "Enter")
                this.pick(this.start.options[active]);
            else
                return;
            event.preventDefault();
            mark();
        }, { signal: this.view.lifetime.signal });
        this.element.append(list);
    }
    private pick(text: string | undefined): void {
        if (text === undefined)
            return;
        const previousValue = this.value;
        this.value = text === "none" ? null : text;
        const change = { value: this.value, previousValue, isInteracted: true };
        this.emit("change", change);
        this.emit("commit", change);
    }
    private versions(): void {
        switch (this.start.version) {
            case 0: return;
            case 1: {
                const older = Object.create(ComponentEvents.prototype) as object;
                Object.defineProperties(older, Object.getOwnPropertyDescriptors(ConformanceComponent.prototype));
                Reflect.deleteProperty(older, "log");
                Object.setPrototypeOf(this, older);
                Reflect.deleteProperty(this, "readOnlyText");
                Object.defineProperties(this, {
                    echo: { value: "Echoed" },
                    settleRecord: { value: () => { this.called(); return this.record; } },
                });
                return;
            }
            case 2:
                Object.defineProperties(this, {
                    total: { value: () => { this.called(); return Promise.resolve(this.count); } },
                    ping: { value: () => { this.called(); return Promise.resolve().then(() => { if (!this.enabled) throw new Error("Ping refused."); this.showPing(); }); } },
                });
                return;
        }
    }
    private showPing(): void { this.view.status("Ping", "Pinged " + String(++this.pings) + (this.pings === 1 ? " time" : " times")); }
}
mountComponents("data-conformance-component", (element, value) => {
    if (typeof value !== "object" || value === null)
        throw new Error("The component start must be an object.");
    const start = value as ConformanceStart;
    let component: ConformanceComponent | null = null;
    const lifetime = new AbortController();
    if (start.builtBy === null)
        component = new ConformanceComponent(element, start);
    else {
        const button = document.getElementById(start.builtBy);
        if (button === null)
            throw new Error("No button builds the component: " + start.builtBy);
        button.addEventListener("click", () => { component ??= new ConformanceComponent(element, start); }, { signal: lifetime.signal });
    }
    return () => { lifetime.abort(); component?.destroy(); };
});
