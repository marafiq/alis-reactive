// BDD: what applyShape does for each Shape kind.
// Shape is the one type contract — the runtime must shape convert values faithfully
// for every kind and short-circuit nulls inside nullable wrappers.

import { describe, it, expect } from "vitest";
import { applyShape, toDate, toNumber, toBoolean, toString } from "../core/shape-convert";
import type { Shape } from "../types";

const S = {
  string: { kind: "string" } as Shape,
  number: { kind: "number" } as Shape,
  boolean: { kind: "boolean" } as Shape,
  date: { kind: "date" } as Shape,
  none: { kind: "none" } as Shape,
  any: { kind: "any" } as Shape,
  nullableDate: { kind: "nullable", inner: { kind: "date" } } as Shape,
  nullableString: { kind: "nullable", inner: { kind: "string" } } as Shape,
  nullableNumber: { kind: "nullable", inner: { kind: "number" } } as Shape,
  arrayOfString: { kind: "array", item: { kind: "string" } } as Shape,
  arrayOfDate: { kind: "array", item: { kind: "date" } } as Shape,
};

describe("applyShape — scalar shapes", () => {
  it("produces a string for Shape.String", () => {
    expect(applyShape("abc", S.string)).toBe("abc");
  });

  it("produces a number for Shape.Number", () => {
    expect(applyShape("42", S.number)).toBe(42);
    expect(applyShape(42, S.number)).toBe(42);
  });

  it("produces a boolean for Shape.Boolean", () => {
    expect(applyShape("true", S.boolean)).toBe(true);
    expect(applyShape("false", S.boolean)).toBe(false);
    expect(applyShape(1, S.boolean)).toBe(true);
    expect(applyShape(0, S.boolean)).toBe(false);
  });

  it("produces a Date object for Shape.Date (not epoch-ms)", () => {
    const r = applyShape("2026-09-15", S.date);
    expect(r).toBeInstanceOf(Date);
    expect((r as Date).getFullYear()).toBe(2026);
  });
});

describe("applyShape — nullable short-circuits null before applying inner shape", () => {
  it("passes null through Nullable(Date) unchanged", () => {
    expect(applyShape(null, S.nullableDate)).toBeNull();
  });

  it("passes undefined through Nullable(Date) unchanged", () => {
    expect(applyShape(undefined, S.nullableDate)).toBeNull();
  });

  it("applies inner shape to non-null value", () => {
    const r = applyShape("2026-09-15", S.nullableDate);
    expect(r).toBeInstanceOf(Date);
  });

  it("passes null through Nullable(String) as null — does NOT shape convert to empty string", () => {
    expect(applyShape(null, S.nullableString)).toBeNull();
  });
});

describe("applyShape — passthrough shapes", () => {
  it("returns value unchanged for Shape.None", () => {
    expect(applyShape("anything", S.none)).toBe("anything");
    expect(applyShape(null, S.none)).toBeNull();
  });

  it("returns value unchanged for Shape.Any", () => {
    const obj = { x: 1 };
    expect(applyShape(obj, S.any)).toBe(obj);
  });
});

describe("applyShape — array recursively shapes items", () => {
  it("shape converts each element of an array of strings", () => {
    expect(applyShape([1, 2, 3], S.arrayOfString)).toEqual(["1", "2", "3"]);
  });

  it("shape converts each element of an array of dates to Date objects", () => {
    const r = applyShape(["2026-01-01", "2026-12-31"], S.arrayOfDate) as Date[];
    expect(r).toHaveLength(2);
    expect(r[0]).toBeInstanceOf(Date);
    expect(r[1]).toBeInstanceOf(Date);
  });
});

describe("toDate — returns Date objects, not epoch-ms numbers", () => {
  it("parses YYYY-MM-DD as local midnight", () => {
    const r = toDate("2026-09-15");
    expect(r.ok).toBe(true);
    if (r.ok) {
      expect(r.value).toBeInstanceOf(Date);
      expect(r.value.getFullYear()).toBe(2026);
      expect(r.value.getMonth()).toBe(8); // September (0-indexed)
      expect(r.value.getDate()).toBe(15);
    }
  });

  it("parses an ISO timestamp string", () => {
    const r = toDate("2026-09-15T10:30:00Z");
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.value).toBeInstanceOf(Date);
  });

  it("accepts an epoch-ms number and returns a Date", () => {
    const r = toDate(1758931200000);
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.value).toBeInstanceOf(Date);
  });

  it("returns a Date instance when given an existing Date", () => {
    const input = new Date(2026, 0, 1);
    const r = toDate(input);
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.value).toBe(input);
  });

  it("errs on null — caller must short-circuit via Nullable wrapper", () => {
    const r = toDate(null);
    expect(r.ok).toBe(false);
  });

  it("errs on an unparseable string", () => {
    const r = toDate("not a date");
    expect(r.ok).toBe(false);
  });
});

describe("toString — null becomes empty string", () => {
  it("null → ok(\"\")", () => {
    const r = toString(null);
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.value).toBe("");
  });

  it("Date → ISO string", () => {
    const r = toString(new Date(Date.UTC(2026, 0, 1)));
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.value).toBe("2026-01-01T00:00:00.000Z");
  });
});

describe("toNumber — Date becomes epoch ms", () => {
  it("converts a Date to its getTime()", () => {
    const d = new Date(Date.UTC(2026, 0, 1));
    const r = toNumber(d);
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.value).toBe(d.getTime());
  });
});

describe("toBoolean — shape convert surface", () => {
  it("null → false", () => {
    const r = toBoolean(null);
    expect(r.ok && r.value).toBe(false);
  });

  it("\"true\" → true, \"false\" → false", () => {
    const t = toBoolean("true");
    const f = toBoolean("false");
    expect(t.ok && t.value).toBe(true);
    expect(f.ok && f.value).toBe(false);
  });
});
