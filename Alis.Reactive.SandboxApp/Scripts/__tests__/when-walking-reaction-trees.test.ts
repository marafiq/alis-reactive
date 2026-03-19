import { describe, it, expect } from "vitest";
import { walkValidationDescriptors } from "../lifecycle/walk-reactions";
import type { Entry, ValidationDescriptor } from "../types";

function seq(...commands: any[]): any {
  return { kind: "sequential", commands };
}

function http(url: string, validation?: ValidationDescriptor, chained?: any): any {
  return {
    kind: "http",
    request: { verb: "POST", url, validation, chained },
  };
}

function parallelHttp(requests: any[], onAllSettled?: any[]): any {
  return { kind: "parallel-http", requests, onAllSettled };
}

function cond(branches: any[]): any {
  return { kind: "conditional", branches };
}

function branch(guard: any, reaction: any): any {
  return { guard, reaction };
}

function elseBranch(reaction: any): any {
  return { guard: null, reaction };
}

function valDesc(formId: string): ValidationDescriptor {
  return { formId, fields: [] };
}

function entry(trigger: any, reaction: any): Entry {
  return { trigger, reaction } as Entry;
}

const domReady = { kind: "dom-ready" as const };

describe("when walking reaction trees for validation descriptors", () => {

  it("visits validation descriptor in http reaction", () => {
    const visited: string[] = [];
    const entries = [entry(domReady, http("/api/save", valDesc("form-1")))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["form-1"]);
  });

  it("skips http reaction without validation", () => {
    const visited: string[] = [];
    const entries = [entry(domReady, http("/api/save"))]; // no validation

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual([]);
  });

  it("visits all requests in parallel-http", () => {
    const visited: string[] = [];
    const entries = [entry(domReady, parallelHttp([
      { verb: "POST", url: "/a", validation: valDesc("form-a") },
      { verb: "POST", url: "/b", validation: valDesc("form-b") },
      { verb: "POST", url: "/c" }, // no validation
    ]))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["form-a", "form-b"]);
  });

  it("skips sequential reactions (no requests)", () => {
    const visited: string[] = [];
    const entries = [entry(domReady, seq({ kind: "dispatch", event: "x" }))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual([]);
  });

  it("recurses into conditional branches", () => {
    const visited: string[] = [];
    const entries = [entry(domReady, cond([
      branch({ kind: "value" }, http("/if-true", valDesc("form-true"))),
      elseBranch(http("/else", valDesc("form-else"))),
    ]))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["form-true", "form-else"]);
  });

  it("recurses into nested conditionals", () => {
    const visited: string[] = [];
    const entries = [entry(domReady, cond([
      branch({ kind: "value" }, cond([
        branch({ kind: "value" }, http("/deep", valDesc("deep-form"))),
      ])),
    ]))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["deep-form"]);
  });

  it("follows chained requests", () => {
    const visited: string[] = [];
    const chained = { verb: "POST", url: "/step2", validation: valDesc("form-step2") };
    const entries = [entry(domReady, http("/step1", valDesc("form-step1"), chained))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["form-step1", "form-step2"]);
  });

  it("follows deeply chained requests", () => {
    const visited: string[] = [];
    const step3 = { verb: "POST", url: "/step3", validation: valDesc("f3") };
    const step2 = { verb: "POST", url: "/step2", validation: valDesc("f2"), chained: step3 };
    const entries = [entry(domReady, http("/step1", valDesc("f1"), step2))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["f1", "f2", "f3"]);
  });

  it("handles multiple entries", () => {
    const visited: string[] = [];
    const entries = [
      entry(domReady, http("/a", valDesc("form-a"))),
      entry({ kind: "custom-event", event: "click" }, http("/b", valDesc("form-b"))),
    ];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["form-a", "form-b"]);
  });

  it("handles empty entries", () => {
    const visited: string[] = [];
    walkValidationDescriptors([], desc => visited.push(desc.formId));
    expect(visited).toEqual([]);
  });

  it("handles conditional with http in one branch and sequential in another", () => {
    const visited: string[] = [];
    const entries = [entry(domReady, cond([
      branch({ kind: "value" }, http("/validated", valDesc("form-v"))),
      elseBranch(seq({ kind: "dispatch", event: "skip" })),
    ]))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["form-v"]);
  });

  it("handles parallel-http inside conditional branch", () => {
    const visited: string[] = [];
    const entries = [entry(domReady, cond([
      branch({ kind: "value" }, parallelHttp([
        { verb: "POST", url: "/a", validation: valDesc("pa") },
        { verb: "POST", url: "/b", validation: valDesc("pb") },
      ])),
    ]))];

    walkValidationDescriptors(entries, desc => visited.push(desc.formId));

    expect(visited).toEqual(["pa", "pb"]);
  });
});
