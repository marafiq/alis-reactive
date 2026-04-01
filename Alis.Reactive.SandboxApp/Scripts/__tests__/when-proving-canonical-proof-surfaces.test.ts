import { afterEach, describe, expect, it } from "vitest";
import { boot } from "../lifecycle/boot";
import { evalRead } from "../resolution/component";
import {
  attachNativeProofSurface,
  FusionProofSurface,
  type NativeProofSurface,
  type ProofItem,
  type ProofState,
} from "../components/lab/proof-surfaces";

function flushMicrotasks() {
  return new Promise(resolve => setTimeout(resolve, 0));
}

function proofItems(): ProofItem[] {
  return [
    { id: "a", label: "Alpha", meta: { enabled: false, tags: ["cold"] } },
    { id: "b", label: "Bravo", meta: { enabled: true, tags: ["warm", "urgent"] } },
  ];
}

function proofState(): ProofState {
  return { status: "loaded", count: 2, selectedId: "b" };
}

function mountNative(id: string): NativeProofSurface {
  const el = document.createElement("div");
  el.id = id;
  document.body.appendChild(el);
  return attachNativeProofSurface(el);
}

function mountFusion(id: string): FusionProofSurface {
  const el = document.createElement("div");
  el.id = id;
  document.body.appendChild(el);
  const widget = new FusionProofSurface(el);
  (el as any).ej2_instances = [widget];
  return widget;
}

afterEach(() => {
  document.body.innerHTML = "";
});

describe("when proving canonical proof surfaces", () => {
  it("native and fusion expose the same readable nested surface", () => {
    const native = mountNative("native-proof");
    const fusion = mountFusion("fusion-proof");

    native.value = "Amina";
    native.setItems(proofItems());
    native.setState(proofState());
    native.setProperty("mode", "live");

    fusion.value = "Amina";
    fusion.setItems(proofItems());
    fusion.setState(proofState());
    fusion.setProperty("mode", "live");

    expect(evalRead("native-proof", "native", "value")).toBe("Amina");
    expect(evalRead("fusion-proof", "fusion", "value")).toBe("Amina");

    expect(evalRead("native-proof", "native", "items.1.meta.tags.1")).toBe("urgent");
    expect(evalRead("fusion-proof", "fusion", "items.1.meta.tags.1")).toBe("urgent");

    expect(evalRead("native-proof", "native", "state.selectedId")).toBe("b");
    expect(evalRead("fusion-proof", "fusion", "state.selectedId")).toBe("b");

    expect(native.getProperty("mode")).toBe("live");
    expect(fusion.getProperty("mode")).toBe("live");
    expect(native.getItem(1)?.meta.tags[1]).toBe("urgent");
    expect(fusion.getItem(1)?.meta.tags[1]).toBe("urgent");
    expect(native.getSnapshot("resident").summary.enabledIds[0]).toBe("b");
    expect(fusion.getSnapshot("resident").summary.enabledIds[0]).toBe("b");

    expect(native.canSelect(1, "strict", 2)).toBe(true);
    expect(fusion.canSelect(1, "strict", 2)).toBe(true);
  });

  it("shared runtime mutations and conditions work across both proof surfaces", async () => {
    const native = mountNative("native-proof");
    const fusion = mountFusion("fusion-proof");

    const result = document.createElement("div");
    result.id = "result";
    document.body.appendChild(result);

    boot({
      planId: "Proof.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "hydrate" },
          reaction: {
            kind: "sequential",
            commands: [
              {
                kind: "mutate-element",
                target: "native-proof",
                mutation: {
                  kind: "set-prop",
                  prop: "value",
                  value: { kind: "source", source: { kind: "event", path: "evt.payload.person.name" } },
                },
              },
              {
                kind: "mutate-element",
                target: "native-proof",
                mutation: {
                  kind: "call",
                  method: "setItems",
                  args: [{ kind: "source", source: { kind: "event", path: "evt.payload.items" } }],
                },
              },
              {
                kind: "mutate-element",
                target: "fusion-proof",
                vendor: "fusion",
                mutation: {
                  kind: "set-prop",
                  prop: "value",
                  value: { kind: "source", source: { kind: "event", path: "evt.payload.person.name" } },
                },
              },
              {
                kind: "mutate-element",
                target: "fusion-proof",
                vendor: "fusion",
                mutation: {
                  kind: "call",
                  method: "setItems",
                  args: [{ kind: "source", source: { kind: "event", path: "evt.payload.items" } }],
                },
              },
            ],
          },
        },
        {
          trigger: { kind: "custom-event", event: "hydrate" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "all",
                  guards: [
                    {
                      kind: "value",
                      source: { kind: "component", componentId: "native-proof", vendor: "native", readExpr: "value" },
                      coerceAs: "string",
                      op: "eq",
                      operand: "Amina",
                    },
                    {
                      kind: "value",
                      source: { kind: "component", componentId: "fusion-proof", vendor: "fusion", readExpr: "items.1.meta.enabled" },
                      coerceAs: "boolean",
                      op: "truthy",
                    },
                  ],
                },
                reaction: {
                  kind: "sequential",
                  commands: [
                    {
                      kind: "mutate-element",
                      target: "result",
                      mutation: {
                        kind: "set-prop",
                        prop: "textContent",
                        value: { kind: "literal", value: "matched" },
                      },
                    },
                  ],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [
                    {
                      kind: "mutate-element",
                      target: "result",
                      mutation: {
                        kind: "set-prop",
                        prop: "textContent",
                        value: { kind: "literal", value: "not-matched" },
                      },
                    },
                  ],
                },
              },
            ],
          },
        },
      ],
    });

    document.dispatchEvent(new CustomEvent("hydrate", {
      detail: {
        payload: {
          person: { name: "Amina" },
          items: proofItems(),
        },
      },
    }));

    await new Promise(resolve => setTimeout(resolve, 0));

    expect(native.value).toBe("Amina");
    expect(fusion.value).toBe("Amina");
    expect(native.items[1].meta.enabled).toBe(true);
    expect(fusion.items[1].meta.enabled).toBe(true);
    expect(result.textContent).toBe("matched");
  });

  it("shared runtime call mutations with and without args work across both proof surfaces", async () => {
    const native = mountNative("native-methods");
    const fusion = mountFusion("fusion-methods");
    const result = document.createElement("div");
    result.id = "method-result";
    document.body.appendChild(result);

    native.setItems(proofItems());
    fusion.setItems(proofItems());

    boot({
      planId: "Proof.Methods",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "drive-proof-methods" },
          reaction: {
            kind: "sequential",
            commands: [
              {
                kind: "mutate-element",
                target: "native-methods",
                vendor: "native",
                mutation: { kind: "call", method: "focusIn" },
              },
              {
                kind: "mutate-element",
                target: "fusion-methods",
                vendor: "fusion",
                mutation: { kind: "call", method: "focusIn" },
              },
              {
                kind: "mutate-element",
                target: "native-methods",
                vendor: "native",
                mutation: {
                  kind: "call",
                  method: "setProperty",
                  args: [
                    { kind: "literal", value: "mode" },
                    { kind: "source", source: { kind: "event", path: "evt.meta.mode" } },
                  ],
                },
              },
              {
                kind: "mutate-element",
                target: "fusion-methods",
                vendor: "fusion",
                mutation: {
                  kind: "call",
                  method: "setProperty",
                  args: [
                    { kind: "literal", value: "mode" },
                    { kind: "source", source: { kind: "event", path: "evt.meta.mode" } },
                  ],
                },
              },
              {
                kind: "mutate-element",
                target: "native-methods",
                vendor: "native",
                mutation: {
                  kind: "call",
                  method: "addItem",
                  args: [
                    { kind: "source", source: { kind: "event", path: "evt.item" } },
                    { kind: "source", source: { kind: "event", path: "evt.index" }, coerce: "number" },
                  ],
                },
              },
              {
                kind: "mutate-element",
                target: "fusion-methods",
                vendor: "fusion",
                mutation: {
                  kind: "call",
                  method: "addItem",
                  args: [
                    { kind: "source", source: { kind: "event", path: "evt.item" } },
                    { kind: "source", source: { kind: "event", path: "evt.index" }, coerce: "number" },
                  ],
                },
              },
            ],
          },
        },
        {
          trigger: { kind: "custom-event", event: "drive-proof-methods" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "all",
                  guards: [
                    {
                      kind: "value",
                      source: { kind: "component", componentId: "native-methods", vendor: "native", readExpr: "focused" },
                      coerceAs: "boolean",
                      op: "truthy",
                    },
                    {
                      kind: "value",
                      source: { kind: "component", componentId: "fusion-methods", vendor: "fusion", readExpr: "items.1.meta.enabled" },
                      coerceAs: "boolean",
                      op: "truthy",
                    },
                  ],
                },
                reaction: {
                  kind: "sequential",
                  commands: [{
                    kind: "mutate-element",
                    target: "method-result",
                    mutation: {
                      kind: "set-prop",
                      prop: "textContent",
                      value: { kind: "literal", value: "matched" },
                    },
                  }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{
                    kind: "mutate-element",
                    target: "method-result",
                    mutation: {
                      kind: "set-prop",
                      prop: "textContent",
                      value: { kind: "literal", value: "not-matched" },
                    },
                  }],
                },
              },
            ],
          },
        },
      ],
    });

    document.dispatchEvent(new CustomEvent("drive-proof-methods", {
      detail: {
        meta: { mode: "strict" },
        item: { id: "z", label: "Zulu", meta: { enabled: true, tags: ["extra"] } },
        index: "1",
      },
    }));

    await flushMicrotasks();

    expect(native.focused).toBe(true);
    expect(fusion.focused).toBe(true);
    expect(native.getProperty("mode")).toBe("strict");
    expect(fusion.getProperty("mode")).toBe("strict");
    expect(native.items[1].id).toBe("z");
    expect(fusion.items[1].id).toBe("z");
    expect(result.textContent).toBe("matched");
  });

  it("component events from both proof surfaces drive downstream reads and conditions", async () => {
    const native = mountNative("native-events");
    const fusion = mountFusion("fusion-events");
    const nativeEcho = document.createElement("div");
    nativeEcho.id = "native-echo";
    document.body.appendChild(nativeEcho);
    const nativeResult = document.createElement("div");
    nativeResult.id = "native-result";
    document.body.appendChild(nativeResult);
    const fusionEcho = document.createElement("div");
    fusionEcho.id = "fusion-echo";
    document.body.appendChild(fusionEcho);
    const fusionResult = document.createElement("div");
    fusionResult.id = "fusion-result";
    document.body.appendChild(fusionResult);

    fusion.value = "Amina";

    boot({
      planId: "Proof.Events",
      components: {},
      entries: [
        {
          trigger: {
            kind: "component-event",
            componentId: "native-events",
            jsEvent: "change",
            vendor: "native",
            readExpr: "value",
          },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "mutate-element",
              target: "native-echo",
              mutation: {
                kind: "set-prop",
                prop: "textContent",
                value: { kind: "source", source: { kind: "event", path: "evt.value" } },
              },
            }],
          },
        },
        {
          trigger: {
            kind: "component-event",
            componentId: "native-events",
            jsEvent: "change",
            vendor: "native",
            readExpr: "value",
          },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "value",
                  source: { kind: "event", path: "evt.value" },
                  coerceAs: "string",
                  op: "eq",
                  rightSource: { kind: "component", componentId: "fusion-events", vendor: "fusion", readExpr: "value" },
                },
                reaction: {
                  kind: "sequential",
                  commands: [{
                    kind: "mutate-element",
                    target: "native-result",
                    mutation: {
                      kind: "set-prop",
                      prop: "textContent",
                      value: { kind: "literal", value: "matched" },
                    },
                  }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{
                    kind: "mutate-element",
                    target: "native-result",
                    mutation: {
                      kind: "set-prop",
                      prop: "textContent",
                      value: { kind: "literal", value: "not-matched" },
                    },
                  }],
                },
              },
            ],
          },
        },
        {
          trigger: {
            kind: "component-event",
            componentId: "fusion-events",
            jsEvent: "change",
            vendor: "fusion",
            readExpr: "value",
          },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "mutate-element",
              target: "fusion-echo",
              mutation: {
                kind: "set-prop",
                prop: "textContent",
                value: { kind: "source", source: { kind: "event", path: "evt.newValue" } },
              },
            }],
          },
        },
        {
          trigger: {
            kind: "component-event",
            componentId: "fusion-events",
            jsEvent: "change",
            vendor: "fusion",
            readExpr: "value",
          },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "value",
                  source: { kind: "event", path: "evt.newValue" },
                  coerceAs: "string",
                  op: "eq",
                  rightSource: { kind: "component", componentId: "native-events", vendor: "native", readExpr: "value" },
                },
                reaction: {
                  kind: "sequential",
                  commands: [{
                    kind: "mutate-element",
                    target: "fusion-result",
                    mutation: {
                      kind: "set-prop",
                      prop: "textContent",
                      value: { kind: "literal", value: "matched" },
                    },
                  }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{
                    kind: "mutate-element",
                    target: "fusion-result",
                    mutation: {
                      kind: "set-prop",
                      prop: "textContent",
                      value: { kind: "literal", value: "not-matched" },
                    },
                  }],
                },
              },
            ],
          },
        },
      ],
    });

    native.value = "Amina";
    await flushMicrotasks();

    expect(nativeEcho.textContent).toBe("Amina");
    expect(nativeResult.textContent).toBe("matched");

    native.value = "Basil";
    fusion.value = "Basil";
    await flushMicrotasks();

    expect(fusionEcho.textContent).toBe("Basil");
    expect(fusionResult.textContent).toBe("matched");
  });
});
