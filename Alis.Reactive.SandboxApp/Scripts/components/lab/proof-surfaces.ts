export interface ProofItem {
  id: string;
  label: string;
  meta: {
    enabled: boolean;
    tags: string[];
  };
}

export interface ProofState {
  status: string;
  count: number;
  selectedId: string | null;
}

export interface ProofSnapshot {
  summary: {
    label: string;
    enabledIds: string[];
    secondTag: string | null;
  };
  data: {
    value: string;
    items: ProofItem[];
    state: ProofState;
    focused: boolean;
    props: Record<string, unknown>;
    history: string[];
    payload: unknown;
  };
}

export interface NativeProofSurface extends HTMLElement {
  value: string;
  items: ProofItem[];
  state: ProofState;
  focused: boolean;
  payload: unknown;
  focusIn(): void;
  clear(): void;
  setItems(items: ProofItem[]): void;
  addItem(item: ProofItem, index: number): void;
  setState(state: ProofState): void;
  setProperty(name: string, value: unknown): void;
  getProperty(name: string): unknown;
  getItem(index: number): ProofItem | undefined;
  getSnapshot(prefix?: string): ProofSnapshot;
  canSelect(index: number, mode: string, minimumTags: number): boolean;
  emitPayload(eventName: string, payload: unknown): void;
}

function cloneItems(items: ProofItem[]): ProofItem[] {
  return items.map(item => ({
    id: item.id,
    label: item.label,
    meta: {
      enabled: item.meta.enabled,
      tags: [...item.meta.tags],
    },
  }));
}

function cloneState(state: ProofState): ProofState {
  return {
    status: state.status,
    count: state.count,
    selectedId: state.selectedId,
  };
}

class ProofModel {
  private readonly extraProps: Record<string, unknown> = {};
  private history: string[] = [];
  private _value = "";
  private _items: ProofItem[] = [];
  private _state: ProofState = { status: "idle", count: 0, selectedId: null };
  private _focused = false;
  private _payload: unknown;

  get value(): string { return this._value; }
  get items(): ProofItem[] { return cloneItems(this._items); }
  get state(): ProofState { return cloneState(this._state); }
  get focused(): boolean { return this._focused; }
  get payload(): unknown { return this._payload; }

  setValue(value: string): { changed: boolean; previousValue: string; newValue: string } {
    const previousValue = this._value;
    this._value = value;
    if (previousValue !== value) {
      this.history.push(`value:${value}`);
      return { changed: true, previousValue, newValue: value };
    }
    return { changed: false, previousValue, newValue: value };
  }

  setItems(items: ProofItem[]): { items: ProofItem[]; count: number } {
    this._items = cloneItems(items);
    this.history.push(`items:${this._items.length}`);
    return { items: cloneItems(this._items), count: this._items.length };
  }

  addItem(item: ProofItem, index: number): void {
    const clone = cloneItems([item])[0];
    this._items.splice(index, 0, clone);
    this.history.push(`add:${clone.id}@${index}`);
  }

  setState(state: ProofState): void {
    this._state = cloneState(state);
    this.history.push(`state:${state.status}:${state.count}`);
  }

  setFocused(focused: boolean): void {
    this._focused = focused;
    this.history.push(`focus:${focused ? "in" : "out"}`);
  }

  clear(): void {
    this._value = "";
    this._items = [];
    this._state = { status: "cleared", count: 0, selectedId: null };
    this.history.push("clear");
  }

  setPayload(payload: unknown): void {
    this._payload = payload;
    this.history.push("payload");
  }

  setProperty(name: string, value: unknown): void {
    this.extraProps[name] = value;
    this.history.push(`prop:${name}`);
  }

  getProperty(name: string): unknown {
    return this.extraProps[name];
  }

  getItem(index: number): ProofItem | undefined {
    const item = this._items[index];
    return item == null ? undefined : cloneItems([item])[0];
  }

  getSnapshot(prefix = "proof"): ProofSnapshot {
    const secondTag = this._items[1]?.meta.tags[0] ?? null;
    return {
      summary: {
        label: `${prefix}:${this._value}`,
        enabledIds: this._items.filter(item => item.meta.enabled).map(item => item.id),
        secondTag,
      },
      data: {
        value: this._value,
        items: cloneItems(this._items),
        state: cloneState(this._state),
        focused: this._focused,
        props: { ...this.extraProps },
        history: [...this.history],
        payload: this._payload,
      },
    };
  }

  canSelect(index: number, mode: string, minimumTags: number): boolean {
    const item = this._items[index];
    if (!item) return false;
    if (!item.meta.enabled) return false;
    if (mode === "strict" && item.meta.tags.length < minimumTags) return false;
    return true;
  }
}

export class FusionProofSurface {
  private readonly host: HTMLElement;
  private readonly input: HTMLInputElement;
  private readonly model = new ProofModel();
  private readonly listeners: Record<string, Array<(...args: unknown[]) => void>> = {};

  constructor(host: HTMLElement) {
    this.host = host;
    this.input = host.querySelector("input") ?? this.createInput(host);

    this.input.addEventListener("input", () => {
      const result = this.model.setValue(this.input.value);
      if (result.changed) {
        this.fire("change", { newValue: result.newValue, previousValue: result.previousValue });
      }
    });
  }

  private createInput(host: HTMLElement): HTMLInputElement {
    const input = document.createElement("input");
    input.type = "text";
    input.className = "proof-fusion-input";
    host.appendChild(input);
    return input;
  }

  get value(): string { return this.model.value; }
  set value(value: string) {
    const result = this.model.setValue(value);
    this.input.value = value;
    if (result.changed) {
      this.fire("change", { newValue: result.newValue, previousValue: result.previousValue });
    }
  }

  get items(): ProofItem[] { return this.model.items; }
  get state(): ProofState { return this.model.state; }
  get focused(): boolean { return this.model.focused; }
  get payload(): unknown { return this.model.payload; }

  focusIn(): void {
    this.model.setFocused(true);
    this.input.focus();
  }

  clear(): void {
    this.model.clear();
    this.input.value = "";
    this.fire("cleared", {});
  }

  setItems(items: ProofItem[]): void {
    const payload = this.model.setItems(items);
    this.fire("items-changed", payload);
  }

  addItem(item: ProofItem, index: number): void {
    this.model.addItem(item, index);
    this.fire("items-changed", { items: this.model.items, count: this.model.items.length });
  }

  setState(state: ProofState): void {
    this.model.setState(state);
    this.fire("state-changed", cloneState(state));
  }

  setProperty(name: string, value: unknown): void {
    this.model.setProperty(name, value);
    this.host.dataset[name] = typeof value === "string" ? value : JSON.stringify(value);
  }

  getProperty(name: string): unknown {
    return this.model.getProperty(name);
  }

  getItem(index: number): ProofItem | undefined {
    return this.model.getItem(index);
  }

  getSnapshot(prefix = "proof"): ProofSnapshot {
    return this.model.getSnapshot(prefix);
  }

  canSelect(index: number, mode: string, minimumTags: number): boolean {
    return this.model.canSelect(index, mode, minimumTags);
  }

  emitPayload(eventName: string, payload: unknown): void {
    this.model.setPayload(payload);
    this.fire(eventName, payload);
  }

  addEventListener(event: string, fn: (...args: unknown[]) => void): void {
    if (!this.listeners[event]) this.listeners[event] = [];
    this.listeners[event].push(fn);
  }

  removeEventListener(event: string, fn: (...args: unknown[]) => void): void {
    const list = this.listeners[event];
    if (!list) return;
    this.listeners[event] = list.filter(listener => listener !== fn);
  }

  private fire(event: string, args: unknown): void {
    for (const listener of this.listeners[event] ?? []) {
      listener(args);
    }
  }
}

export function attachNativeProofSurface(host: HTMLElement): NativeProofSurface {
  const input = host.querySelector("input") ?? createNativeInput(host);
  const model = new ProofModel();
  const surface = host as NativeProofSurface & Record<string, unknown>;

  Object.defineProperties(surface, {
    value: {
      get: () => model.value,
      set: (value: string) => {
        const result = model.setValue(value);
        input.value = value;
        if (result.changed) {
          host.dispatchEvent(new CustomEvent("change", { detail: { newValue: result.newValue, previousValue: result.previousValue } }));
        }
      },
      configurable: true,
    },
    items: {
      get: () => model.items,
      set: (items: ProofItem[]) => {
        model.setItems(items);
      },
      configurable: true,
    },
    state: {
      get: () => model.state,
      set: (state: ProofState) => {
        model.setState(state);
      },
      configurable: true,
    },
    focused: {
      get: () => model.focused,
      configurable: true,
    },
    payload: {
      get: () => model.payload,
      set: (payload: unknown) => {
        model.setPayload(payload);
      },
      configurable: true,
    },
  });

  input.addEventListener("input", () => {
    const result = model.setValue(input.value);
    if (result.changed) {
      host.dispatchEvent(new CustomEvent("change", { detail: { newValue: result.newValue, previousValue: result.previousValue } }));
    }
  });

  surface.focusIn = () => {
    model.setFocused(true);
    input.focus();
  };

  surface.clear = () => {
    model.clear();
    input.value = "";
    host.dispatchEvent(new CustomEvent("cleared"));
  };

  surface.setItems = (items: ProofItem[]) => {
    const payload = model.setItems(items);
    host.dispatchEvent(new CustomEvent("items-changed", { detail: payload }));
  };

  surface.addItem = (item: ProofItem, index: number) => {
    model.addItem(item, index);
    host.dispatchEvent(new CustomEvent("items-changed", { detail: { items: model.items, count: model.items.length } }));
  };

  surface.setState = (state: ProofState) => {
    model.setState(state);
    host.dispatchEvent(new CustomEvent("state-changed", { detail: cloneState(state) }));
  };

  surface.setProperty = (name: string, value: unknown) => {
    model.setProperty(name, value);
    host.dataset[name] = typeof value === "string" ? value : JSON.stringify(value);
  };

  surface.getProperty = (name: string) => model.getProperty(name);
  surface.getItem = (index: number) => model.getItem(index);
  surface.getSnapshot = (prefix = "proof") => model.getSnapshot(prefix);
  surface.canSelect = (index: number, mode: string, minimumTags: number) =>
    model.canSelect(index, mode, minimumTags);
  surface.emitPayload = (eventName: string, payload: unknown) => {
    model.setPayload(payload);
    host.dispatchEvent(new CustomEvent(eventName, { detail: payload }));
  };

  return surface;
}

function createNativeInput(host: HTMLElement): HTMLInputElement {
  const input = document.createElement("input");
  input.type = "text";
  input.className = "proof-native-input";
  host.appendChild(input);
  return input;
}
