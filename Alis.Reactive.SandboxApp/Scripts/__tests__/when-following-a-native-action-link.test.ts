import { afterEach, describe, expect, it, vi } from "vitest";
import { initNativeActionLinks } from "../components/native/native-action-link";

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
          url: "",
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
    expect(String(fetchSpy.mock.calls[0][0])).toContain("/orders/delete/42");
    expect(document.getElementById("result")!.textContent).toBe("Deleted row 42");
  });

  it("delegates a click into a confirm wrapped http pipeline", async () => {
    document.body.innerHTML = `
      <a id="delete-link" href="/orders/delete/42"></a>
      <div id="result"></div>
    `;

    (window as unknown as { alis: { confirm: (message: string) => Promise<boolean> } }).alis = {
      confirm: vi.fn().mockResolvedValue(true),
    };

    const fetchSpy = vi.fn(async () =>
      new Response(JSON.stringify({ ok: true }), { status: 200, headers: { "Content-Type": "application/json" } })
    );
    globalThis.fetch = fetchSpy as typeof fetch;

    const anchor = document.getElementById("delete-link") as HTMLAnchorElement;
    anchor.setAttribute("data-reactive-link", JSON.stringify({
      reaction: {
        kind: "conditional",
        branches: [{
          guard: { kind: "confirm", message: "Delete row?" },
          reaction: {
            kind: "http",
            request: {
              verb: "POST",
              url: "",
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
        }],
      },
    }));

    initNativeActionLinks();
    anchor.click();
    await new Promise(resolve => setTimeout(resolve, 25));

    expect(fetchSpy).toHaveBeenCalledOnce();
    expect(document.getElementById("result")!.textContent).toBe("Deleted row 42");
  });

  it("does not execute the request when confirm is cancelled", async () => {
    document.body.innerHTML = `
      <a id="delete-link" href="/orders/delete/42"></a>
    `;

    (window as unknown as { alis: { confirm: (message: string) => Promise<boolean> } }).alis = {
      confirm: vi.fn().mockResolvedValue(false),
    };

    const fetchSpy = vi.fn(async () =>
      new Response(JSON.stringify({ ok: true }), { status: 200, headers: { "Content-Type": "application/json" } })
    );
    globalThis.fetch = fetchSpy as typeof fetch;

    const anchor = document.getElementById("delete-link") as HTMLAnchorElement;
    anchor.setAttribute("data-reactive-link", JSON.stringify({
      reaction: {
        kind: "conditional",
        branches: [{
          guard: { kind: "confirm", message: "Delete row?" },
          reaction: {
            kind: "http",
            request: {
              verb: "POST",
              url: "",
            },
          },
        }],
      },
    }));

    initNativeActionLinks();
    anchor.click();
    await new Promise(resolve => setTimeout(resolve, 25));

    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("throws when native action link uses include all gather", () => {
    document.body.innerHTML = `<a id="search-link" href="/orders/search"></a>`;
    const errors: string[] = [];
    const onError = (event: ErrorEvent) => {
      errors.push(String(event.error ?? event.message));
      event.preventDefault();
    };
    window.addEventListener("error", onError);

    const anchor = document.getElementById("search-link") as HTMLAnchorElement;
    anchor.setAttribute("data-reactive-link", JSON.stringify({
      reaction: {
        kind: "http",
        request: {
          verb: "GET",
          url: "",
          gather: [{ kind: "all" }],
        },
      },
    }));

    initNativeActionLinks();
    anchor.click();
    window.removeEventListener("error", onError);

    expect(errors[0]).toMatch(/IncludeAll|include all|all gather/i);
  });

  it("throws when native action link uses validation", () => {
    document.body.innerHTML = `<a id="save-link" href="/orders/save"></a>`;
    const errors: string[] = [];
    const onError = (event: ErrorEvent) => {
      errors.push(String(event.error ?? event.message));
      event.preventDefault();
    };
    window.addEventListener("error", onError);

    const anchor = document.getElementById("save-link") as HTMLAnchorElement;
    anchor.setAttribute("data-reactive-link", JSON.stringify({
      reaction: {
        kind: "http",
        request: {
          verb: "POST",
          url: "",
          validation: {
            formId: "orders-form",
            fields: [],
          },
        },
      },
    }));

    initNativeActionLinks();
    anchor.click();
    window.removeEventListener("error", onError);

    expect(errors[0]).toMatch(/validation/i);
  });

  it("throws when a native action link reaction tree contains more than one request", () => {
    document.body.innerHTML = `<a id="delete-link" href="/orders/delete/42"></a>`;
    const errors: string[] = [];
    const onError = (event: ErrorEvent) => {
      errors.push(String(event.error ?? event.message));
      event.preventDefault();
    };
    window.addEventListener("error", onError);

    const anchor = document.getElementById("delete-link") as HTMLAnchorElement;
    anchor.setAttribute("data-reactive-link", JSON.stringify({
      reaction: {
        kind: "conditional",
        branches: [{
          guard: { kind: "confirm", message: "Delete row?" },
          reaction: {
            kind: "http",
            request: {
              verb: "POST",
              url: "",
              onSuccess: [{
                reaction: {
                  kind: "http",
                  request: {
                    verb: "POST",
                    url: "",
                  },
                },
              }],
            },
          },
        }],
      },
    }));

    initNativeActionLinks();
    anchor.click();
    window.removeEventListener("error", onError);

    expect(errors[0]).toMatch(/exactly one request|nested http|second http/i);
  });
});
