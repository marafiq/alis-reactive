import { ComponentView } from "../component-view";
import type { ConformancePeer } from "../ConformancePeer/conformance-peer";
import type { EmptyRecord, RecordData } from "./data";
import { Item, Outer, itemStart, textAnswer } from "./live-objects";
import type { RequestValue } from "./live-objects";
import type { ConformanceStart, RaiseStart } from "./start-options";
export class Held {
    private readonly start: ConformanceStart;
    private readonly view: ComponentView;
    private readonly request: RequestValue;
    readonly items: Item[];
    readonly item: Item;
    answer: ((item: Item) => string) | null = null;
    constructor(start: ConformanceStart, view: ComponentView, request: RequestValue) {
        this.start = start;
        this.view = view;
        this.request = request;
        this.items = start.held.items.map((item) => new Item(item, view, request, start.itemEndpoint, () => undefined));
        this.item = new Item(itemStart("One"), view, request, start.itemEndpoint, () => undefined);
    }
    set writeOnlyText(value: string) { this.view.status("Held", "Held: " + value); }
    total(): number { return this.start.held.total; }
    async settleCount(): Promise<number> {
        const value = await this.request(this.start.heldCountEndpoint);
        if (typeof value !== "number")
            throw new Error("The held count endpoint did not answer a number.");
        return value;
    }
    itemAt(position: number): Item {
        const item = this.items[position - 1];
        if (item === undefined)
            throw new Error("There is no held item at " + String(position) + ".");
        return item;
    }
    matching(): Item[] { return this.items.slice(0, this.start.held.matchingItems); }
    act(): void { this.view.status("Held", "Held: acted for " + (this.answer === null ? this.item.name : this.answer(this.item))); }
}
export class Raised {
    readonly peer: ConformancePeer;
    private readonly view: ComponentView;
    private readonly request: RequestValue;
    private readonly endpoint: string;
    private readonly makeItem: (start: ReturnType<typeof itemStart>) => Item;
    readonly flag: boolean;
    readonly count: number;
    readonly text: string;
    readonly nullableText: string | null;
    readonly optionalText?: string;
    readonly day = "2026-11-12T10:00:00";
    readonly time: string;
    readonly record: RecordData | EmptyRecord | null;
    readonly outer: Outer;
    readonly items: Item[];
    readonly held: Held;
    answer: ((item: Item) => string) | null = null;
    private quantity: number;
    private currentItem: Item;
    private readonly matches: Item[];
    constructor(option: RaiseStart, start: ConformanceStart, peer: ConformancePeer, view: ComponentView, request: RequestValue, endpoint: string, makeItem: (start: ReturnType<typeof itemStart>) => Item) {
        this.peer = peer;
        this.view = view;
        this.request = request;
        this.endpoint = endpoint;
        this.makeItem = makeItem;
        this.flag = option.flag;
        this.count = option.count;
        this.text = option.text ?? "";
        this.nullableText = option.nullableText;
        this.time = option.text ?? "00:00:00";
        if (option.optionalText !== null)
            this.optionalText = option.optionalText;
        if (option.text === null)
            Reflect.deleteProperty(this, "text");
        this.record = option.emptyRecord ? {} : option.record;
        this.outer = new Outer(start.outer, view, request, start.textEndpoint, option.flag ? "M-14" : "M-12");
        this.items = [makeItem(itemStart(option.flag ? "Missed" : "Done")), makeItem(itemStart(option.flag ? "Missed" : "Done")), makeItem(itemStart("Done"))];
        this.quantity = option.flag ? 3 : 1;
        this.currentItem = makeItem(itemStart(option.flag ? "One" : "Two"));
        this.matches = option.flag ? this.items.slice(0, 2) : [];
        this.held = new Held(start, view, request);
        if (start.version === 2)
            Object.defineProperty(this, "count", { value: Promise.resolve(option.count) });
    }
    set writeOnlyText(value: string) { this.view.status("Payload", "Payload text: " + value); this.view.status("Note", value); }
    act(): void { this.view.status("Raise", "Acted"); this.view.status("Payload action", "Payload acted"); }
    total(): number { return this.quantity; }
    settle(): Promise<string> { return textAnswer(this.request, this.endpoint); }
    current(): Item { return this.currentItem; }
    matching(): Item[] { return this.matches; }
    fill(names: string[]): void { this.view.list("Names", names, this.view.status("Names", "")); }
    returned(): void {
        this.view.status("Answer", this.answer === null ? this.currentItem.name + " unanswered" : this.answer(this.currentItem));
        this.quantity = 2;
        this.currentItem = this.makeItem(itemStart("Two"));
        const first = this.items[0];
        if (first !== undefined)
            Object.defineProperty(first, "name", { value: "Done" });
    }
}
