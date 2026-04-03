import { beforeEach, describe, expect, it } from "vitest";
import { boot, getBootedPlan, mergePlan } from "../lifecycle/boot";
import { injectHtml } from "../execution/inject";
import type { Plan } from "../types";
import { composeInitialPlans } from "../lifecycle/merge-plan";
import { createPlan, htmlBlockContract, method, nativeTextContract, property, scalar } from "./support/v2-fixtures";

function rootPlan(): Plan {
  return createPlan({
    planId: "Resident.Editor",
    contracts: {
      "html.block": htmlBlockContract,
      "native.text": nativeTextContract,
    },
    objects: {
      slot: { contract: "html.block", elementId: "slot" },
      status: { contract: "html.block", elementId: "status" },
    },
    workflows: [
      {
        when: { kind: "document-event", name: "root-live" },
        run: {
          kind: "set",
          target: { object: "status", member: "text" },
          value: { kind: "literal", value: "root" },
        },
      },
    ],
  });
}

function partialPlan(binding: string, objectName: string, eventName: string, label: string): Plan {
  return createPlan({
    planId: "Resident.Editor",
    contracts: {
      "native.text": nativeTextContract,
    },
    objects: {
      [objectName]: { contract: "native.text", elementId: objectName },
    },
    bindings: {
      [binding]: {
        object: objectName,
        valueMember: "value",
        shape: { kind: "scalar", type: "string" },
      },
    },
    workflows: [
      {
        when: { kind: "document-event", name: eventName },
        run: {
          kind: "set",
          target: { object: "status", member: "text" },
          value: { kind: "literal", value: label },
        },
      },
    ],
  });
}

function partialMarkup(plan: Plan, html: string): string {
  return `${html}<script type="application/json" data-reactive-plan>${JSON.stringify(plan)}</script>`;
}

const fusionDropDownWithChange = {
  kind: "component" as const,
  resolver: "fusion-instance" as const,
  members: {
    value: property("value", scalar("string")),
  },
  events: {
    change: { channel: "change" },
  },
};

const fusionDropDownWithPopupOnly = {
  kind: "component" as const,
  resolver: "fusion-instance" as const,
  members: {
    value: property("value", scalar("string")),
    showPopup: method("showPopup"),
  },
};

const hiddenFieldReadOnly = {
  kind: "component" as const,
  resolver: "native-element" as const,
  members: {
    value: property("value", scalar("string"), "read"),
  },
};

const genericHiddenFieldWriteOnly = {
  kind: "component" as const,
  resolver: "native-element" as const,
  members: {
    value: property("value", scalar("string"), "write"),
  },
};

describe("when managing v2 plan lifecycle", () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <div id="slot"></div>
      <div id="status"></div>
    `;
  });

  it("replaces source-owned objects, bindings, and workflows on lazy partial reinjection", () => {
    boot(rootPlan());

    const slot = document.getElementById("slot") as HTMLElement;
    injectHtml(
      slot,
      partialMarkup(
        partialPlan("Resident.Name", "partialName", "partial-live", "partial-one"),
        '<input id="partialName" value="Ada" />'
      )
    );

    let merged = getBootedPlan("Resident.Editor");
    expect(merged?.objects.partialName).toBeDefined();
    expect(merged?.bindings["Resident.Name"]).toBeDefined();

    document.dispatchEvent(new CustomEvent("partial-live"));
    expect(document.getElementById("status")?.textContent).toBe("partial-one");

    document.getElementById("status")!.textContent = "";

    injectHtml(
      slot,
      partialMarkup(
        partialPlan("Resident.Email", "partialEmail", "partial-live-2", "partial-two"),
        '<input id="partialEmail" value="ada@example.com" />'
      )
    );

    merged = getBootedPlan("Resident.Editor");
    expect(merged?.objects.partialName).toBeUndefined();
    expect(merged?.bindings["Resident.Name"]).toBeUndefined();
    expect(merged?.objects.partialEmail).toBeDefined();
    expect(merged?.bindings["Resident.Email"]).toBeDefined();

    document.dispatchEvent(new CustomEvent("partial-live"));
    expect(document.getElementById("status")?.textContent).toBe("");

    document.dispatchEvent(new CustomEvent("partial-live-2"));
    expect(document.getElementById("status")?.textContent).toBe("partial-two");

    document.dispatchEvent(new CustomEvent("root-live"));
    expect(document.getElementById("status")?.textContent).toBe("root");
  });

  it("moves a shared source id to a different plan without leaving stale listeners behind", () => {
    document.body.innerHTML = `
      <div id="status-a"></div>
      <div id="status-b"></div>
    `;

    mergePlan(
      createPlan({
        planId: "Plan.A",
        sourceId: "shared-slot",
        contracts: { "html.block": htmlBlockContract },
        objects: {
          statusA: { contract: "html.block", elementId: "status-a" },
        },
        workflows: [
          {
            when: { kind: "document-event", name: "fire-a" },
            run: {
              kind: "set",
              target: { object: "statusA", member: "text" },
              value: { kind: "literal", value: "A" },
            },
          },
        ],
      })
    );

    document.dispatchEvent(new CustomEvent("fire-a"));
    expect(document.getElementById("status-a")?.textContent).toBe("A");

    document.getElementById("status-a")!.textContent = "";

    mergePlan(
      createPlan({
        planId: "Plan.B",
        sourceId: "shared-slot",
        contracts: { "html.block": htmlBlockContract },
        objects: {
          statusB: { contract: "html.block", elementId: "status-b" },
        },
        workflows: [
          {
            when: { kind: "document-event", name: "fire-b" },
            run: {
              kind: "set",
              target: { object: "statusB", member: "text" },
              value: { kind: "literal", value: "B" },
            },
          },
        ],
      })
    );

    document.dispatchEvent(new CustomEvent("fire-a"));
    document.dispatchEvent(new CustomEvent("fire-b"));

    expect(document.getElementById("status-a")?.textContent).toBe("");
    expect(document.getElementById("status-b")?.textContent).toBe("B");
    expect(getBootedPlan("Plan.A")).toBeUndefined();
    expect(getBootedPlan("Plan.B")?.objects.statusB).toBeDefined();
  });

  it("keeps shared contract events when a later fragment adds members to the same contract", () => {
    const merged = createPlan({
      planId: "Resident.Editor",
      contracts: {
        "fusion.dropdownlist": fusionDropDownWithChange,
      },
      objects: {
        statusField: { contract: "fusion.dropdownlist", elementId: "statusField" },
      },
    });

    boot(merged);

    mergePlan(
      createPlan({
        planId: "Resident.Editor",
        sourceId: "popup-fragment",
        contracts: {
          "fusion.dropdownlist": fusionDropDownWithPopupOnly,
        },
        objects: {
          statusField: { contract: "fusion.dropdownlist", elementId: "statusField" },
        },
      })
    );

    const contract = getBootedPlan("Resident.Editor")?.contracts["fusion.dropdownlist"];
    expect(contract?.events?.change?.channel).toBe("change");
    expect(contract?.members.showPopup).toBeDefined();
  });

  it("preserves shared contract events during initial composition of server-rendered fragments", () => {
    const [merged] = composeInitialPlans([
      createPlan({
        planId: "Resident.Editor",
        contracts: {
          "fusion.dropdownlist": fusionDropDownWithChange,
        },
        objects: {
          statusField: { contract: "fusion.dropdownlist", elementId: "statusField" },
        },
      }),
      createPlan({
        planId: "Resident.Editor",
        contracts: {
          "fusion.dropdownlist": fusionDropDownWithPopupOnly,
        },
        objects: {
          statusField: { contract: "fusion.dropdownlist", elementId: "statusField" },
        },
      }),
    ]);

    expect(merged.contracts["fusion.dropdownlist"]?.events?.change?.channel).toBe("change");
    expect(merged.contracts["fusion.dropdownlist"]?.members.showPopup).toBeDefined();
  });

  it("promotes a shared object to the registered contract during initial composition", () => {
    const [merged] = composeInitialPlans([
      createPlan({
        planId: "Resident.Editor",
        contracts: {
          "native.component.careUnit": genericHiddenFieldWriteOnly,
        },
        objects: {
          careUnit: { contract: "native.component.careUnit", elementId: "careUnit" },
        },
        workflows: [
          {
            when: { kind: "document-event", name: "write-care-unit" },
            run: {
              kind: "set",
              target: { object: "careUnit", member: "value" },
              value: { kind: "literal", value: "Memory Care" },
            },
          },
        ],
      }),
      createPlan({
        planId: "Resident.Editor",
        contracts: {
          "native.hiddenfield": hiddenFieldReadOnly,
        },
        objects: {
          careUnit: { contract: "native.hiddenfield", elementId: "careUnit" },
        },
        bindings: {
          CareUnit: {
            object: "careUnit",
            valueMember: "value",
            shape: scalar("string"),
          },
        },
      }),
    ]);

    expect(merged.objects.careUnit?.contract).toBe("native.hiddenfield");
    expect(merged.contracts["native.hiddenfield"]?.members.value).toMatchObject({ access: "readwrite" });
    expect(merged.contracts["native.component.careUnit"]).toBeUndefined();
  });

  it("removes source-owned contract events when a partial is replaced by a fragment that no longer contributes them", () => {
    boot(rootPlan());

    mergePlan(
      createPlan({
        planId: "Resident.Editor",
        sourceId: "step-2-partial",
        contracts: {
          "fusion.dropdownlist": fusionDropDownWithChange,
        },
        objects: {
          statusField: { contract: "fusion.dropdownlist", elementId: "statusField" },
        },
      })
    );

    expect(getBootedPlan("Resident.Editor")?.contracts["fusion.dropdownlist"]?.events?.change?.channel).toBe("change");

    mergePlan(
      createPlan({
        planId: "Resident.Editor",
        sourceId: "step-2-partial",
        contracts: {
          "fusion.dropdownlist": fusionDropDownWithPopupOnly,
        },
        objects: {
          statusField: { contract: "fusion.dropdownlist", elementId: "statusField" },
        },
      })
    );

    const contract = getBootedPlan("Resident.Editor")?.contracts["fusion.dropdownlist"];
    expect(contract?.events?.change).toBeUndefined();
    expect(contract?.members.showPopup).toBeDefined();
  });

  it("restores the baseline shared object contract when a source-owned capability is replaced", () => {
    boot(
      createPlan({
        planId: "Resident.Editor",
        contracts: {
          "native.hiddenfield": hiddenFieldReadOnly,
        },
        objects: {
          careUnit: { contract: "native.hiddenfield", elementId: "careUnit" },
        },
        bindings: {
          CareUnit: {
            object: "careUnit",
            valueMember: "value",
            shape: scalar("string"),
          },
        },
      })
    );

    mergePlan(
      createPlan({
        planId: "Resident.Editor",
        sourceId: "care-unit-fragment",
        contracts: {
          "native.component.careUnit": genericHiddenFieldWriteOnly,
        },
        objects: {
          careUnit: { contract: "native.component.careUnit", elementId: "careUnit" },
        },
        workflows: [
          {
            when: { kind: "document-event", name: "write-care-unit" },
            run: {
              kind: "set",
              target: { object: "careUnit", member: "value" },
              value: { kind: "literal", value: "Enhanced" },
            },
          },
        ],
      })
    );

    let merged = getBootedPlan("Resident.Editor");
    expect(merged?.objects.careUnit?.contract).toBe("native.hiddenfield");
    expect(merged?.contracts["native.hiddenfield"]?.members.value).toMatchObject({ access: "readwrite" });

    mergePlan(
      createPlan({
        planId: "Resident.Editor",
        sourceId: "care-unit-fragment",
        contracts: {},
        objects: {},
        bindings: {},
        workflows: [],
      })
    );

    merged = getBootedPlan("Resident.Editor");
    expect(merged?.objects.careUnit?.contract).toBe("native.hiddenfield");
    expect(merged?.contracts["native.hiddenfield"]?.members.value).toMatchObject({ access: "read" });
    expect(merged?.contracts["native.component.careUnit"]).toBeUndefined();
    expect(merged?.bindings.CareUnit).toBeDefined();
  });
});
