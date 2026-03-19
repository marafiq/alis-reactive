/**
 * TestWidget — real component for architecture verification.
 *
 * Follows the same patterns as any third-party component library:
 * - Properties (get/set): value, items
 * - Void methods: focus(), clear()
 * - Methods with params: setItems(items)
 * - Event API: addEventListener/removeEventListener (Syncfusion-compatible)
 *
 * Mounted on elements via ej2_instances pattern (standard vendor convention).
 * This is NOT a mock — it's a real component with real JS API, compiled from TS,
 * tested in a real browser via Playwright.
 */

export class TestWidget {
  private _el: HTMLElement;
  private _input: HTMLInputElement;
  private _value: string;
  private _items: unknown[];
  private _focused: boolean;
  private _listeners: Record<string, (...args: unknown[]) => void[]>;

  constructor(el: HTMLElement) {
    this._el = el;
    this._listeners = {};
    this._items = [];
    this._focused = false;

    // Find or create inner input — this is what Playwright interacts with.
    this._input = el.querySelector("input") ?? this._createInput(el);
    this._value = el.dataset.initialValue ?? "";
    this._input.value = this._value;

    // Wire inner input events → component events (like real SF components do)
    this._input.addEventListener("input", () => {
      const prev = this._value;
      this._value = this._input.value;
      this._fire("change", { newValue: this._value, previousValue: prev });
    });
  }

  private _createInput(el: HTMLElement): HTMLInputElement {
    const input = document.createElement("input");
    input.type = "text";
    input.className = "test-widget-input";
    el.appendChild(input);
    return input;
  }

  // -- Properties (read/write) --

  get value(): string { return this._value; }
  set value(v: string) {
    const prev = this._value;
    this._value = v;
    this._input.value = v;
    if (prev !== v) {
      this._fire("change", { newValue: v, previousValue: prev });
    }
  }

  get items(): unknown[] { return this._items; }
  get focused(): boolean { return this._focused; }

  // -- Void Methods --

  focus(): void {
    this._focused = true;
    this._input.focus();
  }

  clear(): void {
    this._value = "";
    this._items = [];
    this._input.value = "";
    this._renderItems();
  }

  // -- Methods with Params (single arg) --

  setItems(items: unknown[]): void {
    this._items = Array.isArray(items) ? items : [];
    this._el.dataset.itemsCount = String(this._items.length);
    this._renderItems();
    this._fire("items-changed", { items: this._items, count: this._items.length });
  }

  // -- Methods with Params (multi-arg) --

  addItem(item: unknown, index: number): void {
    this._items.splice(index, 0, item);
    this._el.dataset.itemsCount = String(this._items.length);
    this._renderItems();
  }

  setProperty(name: string, value: unknown): void {
    (this as any)["_prop_" + name] = value;
    this._el.dataset[name] = String(value);
  }

  getProperty(name: string): unknown {
    return (this as any)["_prop_" + name];
  }

  // -- DOM rendering (like real SF components render their state) --

  private _renderItems(): void {
    let list = this._el.querySelector(".test-widget-items");
    if (!list) {
      list = document.createElement("ul");
      list.className = "test-widget-items";
      this._el.appendChild(list);
    }
    list.innerHTML = "";
    for (const item of this._items) {
      const li = document.createElement("li");
      li.className = "test-widget-item";
      li.textContent = String(item);
      list.appendChild(li);
    }
  }

  // -- Event API (Syncfusion-compatible) --

  addEventListener(event: string, fn: (...args: unknown[]) => void): void {
    if (!this._listeners[event]) this._listeners[event] = [];
    this._listeners[event].push(fn);
  }

  removeEventListener(event: string, fn: (...args: unknown[]) => void): void {
    const list = this._listeners[event];
    if (!list) return;
    this._listeners[event] = list.filter(f => f !== fn);
  }

  private _fire(event: string, args?: unknown): void {
    (this._listeners[event] ?? []).forEach(fn => fn(args ?? {}));
  }
}

// -- Auto-mount on [data-test-widget] elements --

function mount(): void {
  document.querySelectorAll<HTMLElement>("[data-test-widget]").forEach(el => {
    if ((el as any).ej2_instances) return;
    (el as any).ej2_instances = [new TestWidget(el)];
  });
}

// Mount synchronously — in Razor layout, this IIFE runs after RenderBody()
// (all [data-test-widget] elements exist) and BEFORE the alis-reactive module boots.
// In vitest, empty DOM → mount() is a no-op (tests create elements manually).
mount();
