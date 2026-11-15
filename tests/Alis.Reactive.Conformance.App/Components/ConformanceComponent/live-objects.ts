import { ComponentView } from "../component-view";
import type { ItemStart, OuterStart } from "./start-options";
export type RequestValue = (endpoint: string) => Promise<unknown>;
export async function textAnswer(request: RequestValue, endpoint: string): Promise<string> {
    const value = await request(endpoint);
    if (typeof value !== "string")
        throw new Error("The endpoint did not answer text: " + endpoint);
    return value;
}
export class Item {
    private readonly request: RequestValue;
    private readonly endpoint: string;
    private readonly changed: () => void;
    private heldValue: number;
    private heldActive: boolean;
    private checks = 0;
    private readonly line: HTMLElement;
    private readonly checked: HTMLElement;
    optionalCount?: number;
    optionalScore?: number;
    readonly name: string;
    constructor(start: ItemStart, view: ComponentView, request: RequestValue, endpoint: string, changed: () => void) {
        this.request = request;
        this.endpoint = endpoint;
        this.changed = changed;
        this.name = start.name;
        this.heldValue = start.value;
        this.heldActive = start.active;
        const own = new ComponentView(document.createElement("div"));
        view.element.append(own.element);
        this.line = own.status("Item " + start.name, start.name + ", not acted");
        this.checked = own.status("Checks", "0");
        if (start.optionalCount !== null)
            this.optionalCount = start.optionalCount;
        if (start.optionalScore !== null)
            this.optionalScore = start.optionalScore;
        if (start.lacksName)
            Reflect.deleteProperty(this, "name");
        if (start.lacksActive)
            Object.defineProperty(this, "active", { value: undefined, configurable: true });
    }
    get value(): number { return this.heldValue; }
    set value(value: number) { this.heldValue = value; this.changed(); }
    get active(): boolean { return this.heldActive; }
    deactivate(): void { this.heldActive = false; }
    act(): void { this.line.textContent = this.name + ", acted"; }
    check(): boolean { this.checked.textContent = String(++this.checks); return this.active; }
    settle(): Promise<string> { return textAnswer(this.request, this.endpoint); }
}
export function itemStart(name: string, active = true, value = 0): ItemStart {
    return { name, active, value, optionalCount: null, optionalScore: null, lacksName: false, lacksActive: false };
}
export class Inner {
    private readonly view: ComponentView;
    private readonly request: RequestValue;
    private readonly endpoint: string;
    readonly name = "Inner";
    text = "";
    constructor(view: ComponentView, request: RequestValue, endpoint: string) { this.view = view; this.request = request; this.endpoint = endpoint; }
    act(): void { this.view.status("Inner", "Inner acted"); }
    settle(): Promise<string> { return textAnswer(this.request, this.endpoint); }
}
export class Middle {
    readonly name: string;
    text = "";
    readonly inner: Inner;
    constructor(name: string, view: ComponentView, request: RequestValue, endpoint: string) {
        this.name = name;
        this.inner = new Inner(view, request, endpoint);
    }
}
export class Outer {
    private readonly view: ComponentView;
    readonly name = "Outer";
    readonly middle: Middle;
    private heldText: string;
    private heldCount: number;
    constructor(start: OuterStart, view: ComponentView, request: RequestValue, endpoint: string, middle = "M-14") {
        this.view = view;
        this.heldText = start.text;
        this.heldCount = start.count;
        this.middle = new Middle(middle, view, request, endpoint);
        this.text = start.text;
        this.count = start.count;
    }
    get text(): string { return this.heldText; }
    set text(value: string) { this.heldText = value; this.view.status("Outer", "Outer: " + value); }
    get count(): number { return this.heldCount; }
    set count(value: number) { this.heldCount = value; this.view.status("Outer count", "Outer count: " + String(value)); }
    act(): void { this.view.status("Outer", "Outer acted"); }
}
