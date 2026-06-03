import { describe, expect, it } from "vitest";
import { BrowserPluginCatalog } from "../plugins/catalog";

describe("BrowserPluginCatalog", () => {
  it("rejects plugin names containing whitespace", () => {
    const catalog = new BrowserPluginCatalog();

    expect(() => catalog.register("array manager", {})).toThrow("must not contain whitespace");
  });

  it("registers root function plugins", () => {
    const catalog = new BrowserPluginCatalog();
    const slugify = (value: string): string => value.toLowerCase();

    catalog.register("slugify", slugify);

    expect(catalog.resolve("slugify")).toBe(slugify);
  });

  it("clears registered plugin instances for runtime lifecycle reset", () => {
    const catalog = new BrowserPluginCatalog();
    catalog.register("slugify", (value: string): string => value.toLowerCase());

    catalog.clear();

    expect(() => catalog.resolve("slugify")).toThrow("plugin not found");
  });
});
