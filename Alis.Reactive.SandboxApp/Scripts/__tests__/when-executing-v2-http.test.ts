import { beforeEach, describe, expect, it, vi } from "vitest";
import { executeRequest } from "../execution/http";
import {
  arrayOf,
  contextObjectContract,
  createPlan,
  htmlBlockContract,
  nativeTextContract,
  property,
  scalar,
} from "./support/v2-fixtures";

function jsonResponse(body: unknown, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    headers: {
      get(name: string) {
        return name.toLowerCase() === "content-type" ? "application/json" : null;
      },
    },
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(JSON.stringify(body)),
  };
}

describe("when executing v2 http requests", () => {
  beforeEach(() => {
    document.body.innerHTML = '<div id="status"></div>';

    (globalThis as { alis?: unknown }).alis = {
      objects: {
        residentState: {
          name: "Ada",
          monthlyRate: "42",
          isActive: "true",
        },
      },
    };
  });

  it("serializes binding maps using binding shapes", async () => {
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse({ message: "saved" }));
    (globalThis as { fetch?: unknown }).fetch = fetchMock;

    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
        "app.residentState": contextObjectContract({
          name: property("name", scalar("string")),
          monthlyRate: property("monthlyRate", scalar("number")),
          isActive: property("isActive", scalar("boolean")),
        }),
      },
      objects: {
        status: { contract: "html.block", elementId: "status" },
        residentState: { contract: "app.residentState" },
      },
      bindings: {
        "Resident.Name": {
          object: "residentState",
          valueMember: "name",
          shape: scalar("string"),
        },
        "Resident.MonthlyRate": {
          object: "residentState",
          valueMember: "monthlyRate",
          shape: scalar("number"),
        },
        "Resident.IsActive": {
          object: "residentState",
          valueMember: "isActive",
          shape: scalar("boolean"),
        },
      },
    });

    await executeRequest(
      {
        method: "POST",
        url: "/api/residents",
        input: {
          transport: "json",
          value: { kind: "binding-map", include: "all" },
        },
        onSuccess: [
          {
            run: {
              kind: "set",
              target: { object: "status", member: "text" },
              value: { kind: "context", scope: "response", path: [{ prop: "message" }] },
            },
          },
        ],
      },
      { plan }
    );

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    const headers = new Headers(init.headers);

    expect(JSON.parse(init.body as string)).toEqual({
      Resident: {
        Name: "Ada",
        MonthlyRate: 42,
        IsActive: true,
      },
    });
    expect(headers.get("content-type")).toBe("application/json");
    expect(headers.get("traceparent")).toMatch(/^00-[0-9a-f]{32}-[0-9a-f]{16}-01$/);
    expect(document.getElementById("status")?.textContent).toBe("saved");
  });

  it("serializes date bindings as server-parseable JSON values", async () => {
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse({ message: "saved" }));
    (globalThis as { fetch?: unknown }).fetch = fetchMock;

    const admissionDate = new Date("2026-04-15T00:00:00.000Z");
    const stayStart = new Date("2026-04-05T04:00:00.000Z");
    const stayEnd = new Date("2026-04-20T04:00:00.000Z");

    (globalThis as { alis?: unknown }).alis = {
      objects: {
        residentState: {
          admissionDate,
          stayPeriod: [stayStart, stayEnd],
        },
      },
    };

    const plan = createPlan({
      contracts: {
        "app.residentState": contextObjectContract({
          admissionDate: property("admissionDate", scalar("date")),
          stayPeriod: property("stayPeriod", {
            kind: "array",
            item: scalar("date"),
          }),
        }),
      },
      objects: {
        residentState: { contract: "app.residentState" },
      },
      bindings: {
        AdmissionDate: {
          object: "residentState",
          valueMember: "admissionDate",
          shape: scalar("date"),
        },
        StayPeriod: {
          object: "residentState",
          valueMember: "stayPeriod",
          shape: {
            kind: "array",
            item: scalar("date"),
          },
        },
      },
    });

    await executeRequest(
      {
        method: "POST",
        url: "/api/residents",
        input: {
          transport: "json",
          value: { kind: "binding-map", include: "all" },
        },
      },
      { plan }
    );

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(JSON.parse(init.body as string)).toEqual({
      AdmissionDate: "2026-04-15T00:00:00.000Z",
      StayPeriod: [
        "2026-04-05T04:00:00.000Z",
        "2026-04-20T04:00:00.000Z",
      ],
    });
  });

  it("threads response context into the next request", async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(jsonResponse({ residentId: 9 }))
      .mockResolvedValueOnce(jsonResponse({ summary: "loaded" }));
    (globalThis as { fetch?: unknown }).fetch = fetchMock;

    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
      },
      objects: {
        status: { contract: "html.block", elementId: "status" },
      },
    });

    await executeRequest(
      {
        method: "POST",
        url: "/api/residents/save",
        input: {
          transport: "json",
          value: {
            kind: "object",
            fields: {
              name: { kind: "literal", value: "Ada" },
            },
          },
        },
        next: {
          method: "GET",
          url: "/api/residents/summary",
          input: {
            transport: "query",
            value: {
              kind: "object",
              fields: {
                id: {
                  kind: "context",
                  scope: "response",
                  path: [{ prop: "residentId" }],
                },
              },
            },
          },
          onSuccess: [
            {
              run: {
                kind: "set",
                target: { object: "status", member: "text" },
                value: {
                  kind: "context",
                  scope: "response",
                  path: [{ prop: "summary" }],
                },
              },
            },
          ],
        },
      },
      { plan }
    );

    expect(fetchMock.mock.calls[0][0]).toBe("/api/residents/save");
    expect(fetchMock.mock.calls[1][0]).toBe("/api/residents/summary?id=9");
    expect(document.getElementById("status")?.textContent).toBe("loaded");
  });

  it("builds form-data payloads without introducing a second schema path", async () => {
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse({}));
    (globalThis as { fetch?: unknown }).fetch = fetchMock;

    const plan = createPlan();

    await executeRequest(
      {
        method: "POST",
        url: "/api/uploads",
        input: {
          transport: "form-data",
          value: {
            kind: "object",
            fields: {
              name: { kind: "literal", value: "care-plan.pdf" },
              category: { kind: "literal", value: "medical" },
            },
          },
        },
      },
      { plan }
    );

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    const body = init.body as FormData;
    expect(body).toBeInstanceOf(FormData);
    expect(Array.from(body.entries())).toEqual([
      ["name", "care-plan.pdf"],
      ["category", "medical"],
    ]);
  });

  it("serializes binary carrier objects under their logical binding key", async () => {
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse({}));
    (globalThis as { fetch?: unknown }).fetch = fetchMock;

    const uploadedFile = new File(["image-bytes"], "photo.jpg", { type: "image/jpeg" });

    (globalThis as { alis?: unknown }).alis = {
      objects: {
        residentState: {
          residentName: "Margaret Thompson",
          documents: [
            {
              name: "photo.jpg",
              rawFile: uploadedFile,
            },
          ],
        },
      },
    };

    const plan = createPlan({
      contracts: {
        "app.residentState": contextObjectContract({
          residentName: property("residentName", scalar("string")),
          documents: property("documents", arrayOf({ kind: "any" })),
        }),
      },
      objects: {
        residentState: { contract: "app.residentState" },
      },
      bindings: {
        ResidentName: {
          object: "residentState",
          valueMember: "residentName",
          shape: scalar("string"),
        },
        Documents: {
          object: "residentState",
          valueMember: "documents",
          shape: arrayOf({ kind: "any" }),
        },
      },
    });

    await executeRequest(
      {
        method: "POST",
        url: "/api/uploads",
        input: {
          transport: "form-data",
          value: { kind: "binding-map", include: "all" },
        },
      },
      { plan }
    );

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    const body = init.body as FormData;
    const fileEntry = body.get("Documents");
    expect(body.get("ResidentName")).toBe("Margaret Thompson");
    expect(fileEntry).toBeInstanceOf(File);
    expect((fileEntry as File).name).toBe("photo.jpg");
  });

  it("does not run before actions when client validation blocks the request", async () => {
    const fetchMock = vi.fn();
    (globalThis as { fetch?: unknown }).fetch = fetchMock;

    document.body.innerHTML = `
      <form id="resident-form">
        <input id="resident-name" value="" />
        <span id="resident-name_error" hidden style="display:none"></span>
      </form>
      <div id="status">Ready</div>
    `;

    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
        "native.text": nativeTextContract,
      },
      objects: {
        status: { contract: "html.block", elementId: "status" },
        residentName: { contract: "native.text", elementId: "resident-name" },
      },
      bindings: {
        "Resident.Name": {
          object: "residentName",
          valueMember: "value",
          shape: scalar("string"),
        },
      },
    });

    await executeRequest(
      {
        method: "POST",
        url: "/api/residents",
        validation: {
          formId: "resident-form",
          fields: [
            {
              binding: "Resident.Name",
              rules: [{ rule: "required", message: "Resident name is required." }],
            },
          ],
        },
        before: [
          {
            kind: "set",
            target: { object: "status", member: "text" },
            value: { kind: "literal", value: "Saving..." },
          },
        ],
        onSettled: [
          {
            kind: "set",
            target: { object: "status", member: "text" },
            value: { kind: "literal", value: "Settled" },
          },
        ],
      },
      { plan }
    );

    expect(fetchMock).not.toHaveBeenCalled();
    expect(document.getElementById("status")?.textContent).toBe("Ready");
    expect(document.getElementById("resident-name_error")?.textContent).toBe("Resident name is required.");
  });
});
