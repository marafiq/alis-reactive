import { describe, it, expect, beforeEach } from "vitest";
import { executeCommand } from "../../commands";

beforeEach(() => {
  document.body.innerHTML = "";
});

describe("When ValidationErrors command fires without a validation descriptor", () => {
  it("throws with a clear message", () => {
    expect(() => {
      executeCommand(
        { kind: "validation-errors", formId: "my-form" },
        { responseBody: { errors: { Name: ["required"] } } }
      );
    }).toThrow("ValidationErrors");
  });

  it("mentions the formId in the error", () => {
    expect(() => {
      executeCommand(
        { kind: "validation-errors", formId: "my-form" },
        { responseBody: { errors: {} } }
      );
    }).toThrow("my-form");
  });

  it("mentions Validate<TValidator>", () => {
    expect(() => {
      executeCommand(
        { kind: "validation-errors", formId: "test" },
        { responseBody: { errors: {} } }
      );
    }).toThrow("Validate");
  });
});
