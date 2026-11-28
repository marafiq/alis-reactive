import { describe, expect, test } from "vitest";
import { queried, routed, Stop } from "./index";

const wardPage = { pathname: "/care/residents/wards/East", search: "?ward=West" };

describe("the page's query, read by a member's binding name (N10.7, M2.X1)", () => {
  test("a key matches the name ignoring case, as both hosts bind the query", () => {
    expect(queried({ pathname: "/", search: "?TAB=medications" }, "Tab")).toBe("medications");
  });

  test("the first value of a key the query repeats is read, as a scalar member binds it", () => {
    expect(queried({ pathname: "/", search: "?beds=7&BEDS=9" }, "Beds")).toBe("7");
  });

  test("a key the query lacks reads null", () => {
    expect(queried({ pathname: "/", search: "?ward=West" }, "Beds")).toBeNull();
  });

  test("a nested member's binding name and percent-encoded text are read as the binder reads them", () => {
    expect(queried({ pathname: "/", search: "?stay.ward=North%20Wing&note=a+b" }, "Stay.Ward")).toBe("North Wing");
    expect(queried({ pathname: "/", search: "?stay.ward=North%20Wing&note=a+b" }, "Note")).toBe("a b");
  });

  test("a name past ASCII matches only as one character to one character, as OrdinalIgnoreCase compares it", () => {
    expect(queried({ pathname: "/", search: "?stra%C3%9Fe=Hof" }, "STRASSE")).toBeNull();
    expect(queried({ pathname: "/", search: "?%C3%BCbung=1" }, "\u00DCbung")).toBe("1");
  });
});

describe("the page's path, read at a placeholder of its template (N10.7, M2.X3)", () => {
  test("the segment at the placeholder's place is read, decoded", () => {
    expect(routed(wardPage, ["care", "residents", "wards", "{Ward}"], 3)).toBe("East");
    expect(routed({ pathname: "/care/residents/wards/North%20Wing", search: "" }, ["care", "residents", "wards", "{Ward}"], 3)).toBe("North Wing");
  });

  test("a literal segment matches ignoring case, and a trailing slash is no segment", () => {
    expect(routed({ pathname: "/Care/RESIDENTS/Wards/East/", search: "" }, ["care", "residents", "wards", "{Ward}"], 3)).toBe("East");
  });

  test("a path base the C# wrote before the template is matched as its first segments", () => {
    expect(routed({ pathname: "/home/care/residents/wards/East", search: "" }, ["home", "care", "residents", "wards", "{Ward}"], 4)).toBe("East");
  });

  test("a path that does not match the template stops the pipeline, naming both", () => {
    expect(() => routed({ pathname: "/care/residents/rooms/East", search: "" }, ["care", "residents", "wards", "{Ward}"], 3))
      .toThrow(new Stop("The page's path '/care/residents/rooms/East' does not match its template '/care/residents/wards/{Ward}'."));
    expect(() => routed({ pathname: "/care/residents/wards/East/beds", search: "" }, ["care", "residents", "wards", "{Ward}"], 3))
      .toThrow(new Stop("The page's path '/care/residents/wards/East/beds' does not match its template '/care/residents/wards/{Ward}'."));
  });

  test("a segment that is no percent-encoded text stops the pipeline, naming it", () => {
    expect(() => routed({ pathname: "/care/residents/wards/%E0%A4%A", search: "" }, ["care", "residents", "wards", "{Ward}"], 3))
      .toThrow(new Stop("The page's path '/care/residents/wards/%E0%A4%A' holds '%E0%A4%A', which is no percent-encoded text."));
  });
});
