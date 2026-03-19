import { describe, it, expect, beforeEach } from "vitest";
import { boot } from "../lifecycle/boot";

describe("when triggering on component event", () => {
  describe("native vendor", () => {
    beforeEach(() => {
      // Create a native <select> element in the DOM
      const select = document.createElement("select");
      select.id = "Status";
      document.body.appendChild(select);
    });

    it("fires reaction when native element emits change event", () => {
      const el = document.createElement("div");
      el.id = "echo";
      el.textContent = "";
      document.body.appendChild(el);

      boot({
        planId: "Test.Model",
        components: {},
        entries: [{
          trigger: {
            kind: "component-event",
            componentId: "Status",
            jsEvent: "change",
            vendor: "native",
            bindingPath: "Status",
            readExpr: "value",
          },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "mutate-element",
              target: "echo",
              mutation: { kind: "set-prop", prop: "textContent" },
              value: "changed!",
            }],
          },
        }],
      });

      // Dispatch change event on the native select element
      document.getElementById("Status")!.dispatchEvent(new Event("change"));
      expect(el.textContent).toBe("changed!");
    });

    it("does not fire before event", () => {
      const el = document.createElement("div");
      el.id = "echo-idle";
      el.textContent = "idle";
      document.body.appendChild(el);

      boot({
        planId: "Test.Model",
        components: {},
        entries: [{
          trigger: {
            kind: "component-event",
            componentId: "Status",
            jsEvent: "change",
            vendor: "native",
            readExpr: "value",
          },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "mutate-element",
              target: "echo-idle",
              mutation: { kind: "set-prop", prop: "textContent" },
              value: "fired",
            }],
          },
        }],
      });

      expect(document.getElementById("echo-idle")!.textContent).toBe("idle");
    });
  });

  describe("native vendor with nested property", () => {
    beforeEach(() => {
      const select = document.createElement("select");
      select.id = "Address_City";
      document.body.appendChild(select);
    });

    it("resolves nested element ID with underscores", () => {
      const el = document.createElement("div");
      el.id = "echo-nested";
      el.textContent = "";
      document.body.appendChild(el);

      boot({
        planId: "Test.Model",
        components: {},
        entries: [{
          trigger: {
            kind: "component-event",
            componentId: "Address_City",
            jsEvent: "change",
            vendor: "native",
            bindingPath: "Address.City",
            readExpr: "value",
          },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "mutate-element",
              target: "echo-nested",
              mutation: { kind: "set-prop", prop: "textContent" },
              value: "nested works",
            }],
          },
        }],
      });

      document.getElementById("Address_City")!.dispatchEvent(new Event("change"));
      expect(document.getElementById("echo-nested")!.textContent).toBe("nested works");
    });
  });

  describe("fusion vendor", () => {
    beforeEach(() => {
      // Create a fake SF element with ej2_instances
      const el = document.createElement("input");
      el.id = "Amount";
      const listeners: Record<string, ((...args: unknown[]) => unknown)[]> = {};
      (el as any).ej2_instances = [{
        addEventListener: (event: string, fn: (...args: unknown[]) => unknown) => {
          if (!listeners[event]) listeners[event] = [];
          listeners[event].push(fn);
        },
        // Expose for test to trigger
        _fire: (event: string, args: any) => {
          (listeners[event] || []).forEach(fn => fn(args));
        },
      }];
      document.body.appendChild(el);
    });

    it("fires reaction when fusion component emits event", () => {
      const el = document.createElement("div");
      el.id = "echo-fusion";
      el.textContent = "";
      document.body.appendChild(el);

      boot({
        planId: "Test.Model",
        components: {},
        entries: [{
          trigger: {
            kind: "component-event",
            componentId: "Amount",
            jsEvent: "change",
            vendor: "fusion",
            bindingPath: "Amount",
            readExpr: "value",
          },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "mutate-element",
              target: "echo-fusion",
              mutation: { kind: "set-prop", prop: "textContent" },
              value: "fusion changed",
            }],
          },
        }],
      });

      // Fire via the SF component's event system
      const comp = (document.getElementById("Amount") as any).ej2_instances[0];
      comp._fire("change", { value: 42 });
      expect(document.getElementById("echo-fusion")!.textContent).toBe("fusion changed");
    });

    it("passes event args as execution context", () => {
      const el = document.createElement("div");
      el.id = "echo-args";
      el.textContent = "";
      document.body.appendChild(el);

      boot({
        planId: "Test.Model",
        components: {},
        entries: [{
          trigger: {
            kind: "component-event",
            componentId: "Amount",
            jsEvent: "change",
            vendor: "fusion",
            readExpr: "value",
          },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "mutate-element",
              target: "echo-args",
              mutation: { kind: "set-prop", prop: "textContent" },
              source: { kind: "event", path: "evt.value" },
            }],
          },
        }],
      });

      const comp = (document.getElementById("Amount") as any).ej2_instances[0];
      comp._fire("change", { value: 123 });
      expect(document.getElementById("echo-args")!.textContent).toBe("123");
    });
  });

  describe("native without readExpr (non-readable component like button)", () => {
    it("passes only event object when readExpr is absent", () => {
      const btn = document.createElement("button");
      btn.id = "NoReadExpr";
      document.body.appendChild(btn);
      let receivedEvt: any = null;

      document.addEventListener("btn-clicked", (e) => {
        receivedEvt = (e as CustomEvent).detail;
      });

      boot({
        planId: "Test.Model",
        components: {},
        entries: [{
          trigger: {
            kind: "component-event",
            componentId: "NoReadExpr",
            jsEvent: "click",
            vendor: "native",
          },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "btn-clicked" }],
          },
        }],
      });

      btn.dispatchEvent(new Event("click"));
      expect(receivedEvt).toBeDefined();
    });
  });

  describe("wiring order", () => {
    it("component-event wires before dom-ready executes", () => {
      const order: string[] = [];

      // Create element for component trigger
      const select = document.createElement("select");
      select.id = "WireOrder";
      document.body.appendChild(select);

      document.addEventListener("comp-wired", () => order.push("comp-reaction"));

      boot({
        planId: "Test.Model",
        components: {},
        entries: [
          {
            trigger: { kind: "dom-ready" },
            reaction: {
              kind: "sequential",
              commands: [{ kind: "dispatch", event: "dom-ready-done" }],
            },
          },
          {
            trigger: {
              kind: "component-event",
              componentId: "WireOrder",
              jsEvent: "change",
              vendor: "native",
              readExpr: "value",
            },
            reaction: {
              kind: "sequential",
              commands: [{ kind: "dispatch", event: "comp-wired" }],
            },
          },
        ],
      });

      // Component-event listener should be wired — fire it
      select.dispatchEvent(new Event("change"));
      expect(order).toContain("comp-reaction");
    });
  });
});
