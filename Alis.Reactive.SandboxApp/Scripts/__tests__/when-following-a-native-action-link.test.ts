import { afterEach, describe, expect, it, vi } from "vitest";
import { boot } from "../boot";
import { initNativeActionLinks } from "../native-action-link";

describe("when following a native action link", () => {
  afterEach(() => {
    document.body.innerHTML = "";
    vi.restoreAllMocks();
  });

  it("delegates a click into the existing http pipeline", async () => {
    document.body.innerHTML = `
      <a id="delete-link" href="/orders/delete/42"></a>
      <div id="result"></div>
    `;

    const fetchSpy = vi.fn(async () =>
      new Response(JSON.stringify({ ok: true }), { status: 200, headers: { "Content-Type": "application/json" } })
    );
    globalThis.fetch = fetchSpy as typeof fetch;

    const anchor = document.getElementById("delete-link") as HTMLAnchorElement;
    anchor.setAttribute("data-reactive-link", JSON.stringify({
      planId: "Test.Model",
      reaction: {
        kind: "http",
        request: {
          verb: "POST",
          url: "/orders/delete/42",
          onSuccess: [{
            commands: [{
              kind: "mutate-element",
              target: "result",
              mutation: { kind: "set-prop", prop: "textContent" },
              value: "Deleted row 42",
            }],
          }],
        },
      },
    }));

    initNativeActionLinks();
    anchor.click();
    await new Promise(resolve => setTimeout(resolve, 25));

    expect(fetchSpy).toHaveBeenCalledOnce();
    expect(document.getElementById("result")!.textContent).toBe("Deleted row 42");
  });

  it("reuses the booted component registry for IncludeAll gather", async () => {
    document.body.innerHTML = `
      <input id="filter-name" value="Adnan" />
      <a id="search-link" href="/orders/search"></a>
    `;

    boot({
      planId: "Search.Model",
      components: {
        Name: { id: "filter-name", vendor: "native", readExpr: "value" },
      },
      entries: [],
    });

    const fetchSpy = vi.fn(async (url: string | URL | Request) =>
      new Response(JSON.stringify({ ok: true }), { status: 200, headers: { "Content-Type": "application/json" } })
    );
    globalThis.fetch = fetchSpy as typeof fetch;

    const anchor = document.getElementById("search-link") as HTMLAnchorElement;
    anchor.setAttribute("data-reactive-link", JSON.stringify({
      planId: "Search.Model",
      reaction: {
        kind: "http",
        request: {
          verb: "GET",
          url: "/orders/search",
          gather: [{ kind: "all" }],
        },
      },
    }));

    initNativeActionLinks();
    anchor.click();
    await new Promise(resolve => setTimeout(resolve, 25));

    expect(fetchSpy).toHaveBeenCalledOnce();
    expect(String(fetchSpy.mock.calls[0][0])).toContain("Name=Adnan");
  });

  it("enriches validation fields from the booted plan before validating", async () => {
    document.body.innerHTML = `
      <form id="search-form">
        <input id="filter-name" value="" />
        <span data-valmsg-for="Name"></span>
      </form>
      <a id="validate-link" href="/orders/search"></a>
    `;

    boot({
      planId: "Validation.Model",
      components: {
        Name: { id: "filter-name", vendor: "native", readExpr: "value" },
      },
      entries: [],
    });

    const fetchSpy = vi.fn(async () =>
      new Response(JSON.stringify({ ok: true }), { status: 200, headers: { "Content-Type": "application/json" } })
    );
    globalThis.fetch = fetchSpy as typeof fetch;

    const anchor = document.getElementById("validate-link") as HTMLAnchorElement;
    anchor.setAttribute("data-reactive-link", JSON.stringify({
      planId: "Validation.Model",
      reaction: {
        kind: "http",
        request: {
          verb: "POST",
          url: "/orders/search",
          validation: {
            formId: "search-form",
            fields: [{
              fieldName: "Name",
              rules: [{ rule: "required", message: "Name is required" }],
            }],
          },
        },
      },
    }));

    initNativeActionLinks();
    anchor.click();
    await new Promise(resolve => setTimeout(resolve, 25));

    expect(fetchSpy).not.toHaveBeenCalled();
    expect(document.querySelector("[data-valmsg-for='Name']")!.textContent).toBe("Name is required");
  });
});
