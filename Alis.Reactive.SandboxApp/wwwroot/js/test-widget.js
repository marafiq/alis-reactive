"use strict";
(() => {
  // Alis.Reactive.SandboxApp/Scripts/components/lab/test-widget.ts
  var TestWidget = class {
    constructor(el) {
      this._el = el;
      this._listeners = {};
      this._items = [];
      this._focused = false;
      this._input = el.querySelector("input") ?? this._createInput(el);
      this._value = el.dataset.initialValue ?? "";
      this._input.value = this._value;
      this._input.addEventListener("input", () => {
        const prev = this._value;
        this._value = this._input.value;
        this._fire("change", { newValue: this._value, previousValue: prev });
      });
    }
    _createInput(el) {
      const input = document.createElement("input");
      input.type = "text";
      input.className = "test-widget-input";
      el.appendChild(input);
      return input;
    }
    // -- Properties (read/write) --
    get value() {
      return this._value;
    }
    set value(v) {
      const prev = this._value;
      this._value = v;
      this._input.value = v;
      if (prev !== v) {
        this._fire("change", { newValue: v, previousValue: prev });
      }
    }
    get items() {
      return this._items;
    }
    get focused() {
      return this._focused;
    }
    // -- Void Methods --
    focus() {
      this._focused = true;
      this._input.focus();
    }
    clear() {
      this._value = "";
      this._items = [];
      this._input.value = "";
      this._renderItems();
    }
    // -- Methods with Params (single arg) --
    setItems(items) {
      this._items = Array.isArray(items) ? items : [];
      this._el.dataset.itemsCount = String(this._items.length);
      this._renderItems();
      this._fire("items-changed", { items: this._items, count: this._items.length });
    }
    // -- Methods with Params (multi-arg) --
    addItem(item, index) {
      this._items.splice(index, 0, item);
      this._el.dataset.itemsCount = String(this._items.length);
      this._renderItems();
    }
    setProperty(name, value) {
      this["_prop_" + name] = value;
      this._el.dataset[name] = String(value);
    }
    getProperty(name) {
      return this["_prop_" + name];
    }
    // -- DOM rendering (like real SF components render their state) --
    _renderItems() {
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
    addEventListener(event, fn) {
      if (!this._listeners[event]) this._listeners[event] = [];
      this._listeners[event].push(fn);
    }
    removeEventListener(event, fn) {
      const list = this._listeners[event];
      if (!list) return;
      this._listeners[event] = list.filter((f) => f !== fn);
    }
    _fire(event, args) {
      (this._listeners[event] ?? []).forEach((fn) => fn(args ?? {}));
    }
  };
  function mount() {
    document.querySelectorAll("[data-test-widget]").forEach((el) => {
      if (el.ej2_instances) return;
      el.ej2_instances = [new TestWidget(el)];
    });
  }
  mount();
})();
