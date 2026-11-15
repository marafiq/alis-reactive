import { describe, expect, test } from "vitest";
import { prepare } from "./index";

type Send = Parameters<typeof prepare>[0];
type Field = Send["query"]["fields"][number];
type Form = Field["form"];
type Zone = Parameters<typeof prepare>[1];

const clock: Zone = { kind: "clock" };
const emptyInputs: Send["query"]["inputs"] = { container: { kind: "none" }, individual: [] };
const get = (fields: readonly Field[] = []): Send => ({
  method: "GET",
  url: { parts: [{ kind: "text", text: "/Residents" }], query: "" },
  query: { fields, inputs: emptyInputs },
  body: { kind: "none" },
  headers: [],
  antiforgery: { kind: "none" },
  retry: { kind: "none" },
  busy: { kind: "none" },
  respondedJson: false,
  contracts: [],
});
const field = (name: string, value: unknown, form: Form, whenNothing: Field["whenNothing"] = "leave"): Field => ({
  name, path: [name], value: { kind: "held", value }, form, whenNothing,
});
const care: Extract<Form, { kind: "flags" }> = {
  kind: "flags",
  values: [
    { held: "weekend", name: "Weekend", number: 96 },
    { held: "sun", name: "Sun", number: 64 },
    { held: "sat", name: "Sat", number: 32 },
    { held: "mon", name: "Mon", number: 1 },
    { held: "none", name: "None", number: 0 },
  ],
};

describe("when a bodyless request reaches Guard, its wire follows D9 and schema.md", () => {
  test("PathSegmentsAndQueryEncoding: fills every segment and keeps the URL's own query before encoded fields", () => {
    const send: Send = {
      ...get([field("Term", "a b+c&d", { kind: "text" }), field("Beds", 12.5, { kind: "number" }), field("Open", false, { kind: "boolean" })]),
      url: {
        parts: [
          { kind: "text", text: "/Wards/" },
          { kind: "member", name: "Ward", value: { kind: "held", value: "Éast !'()*" }, form: { kind: "text" } },
          { kind: "text", text: "/Residents/" },
          { kind: "member", name: "Id", value: { kind: "held", value: 7 }, form: { kind: "number" } },
        ],
        query: "fixed=a%20b&keep=%2f",
      },
    };
    const prepared = prepare(send, clock);
    expect(prepared.url).toBe("/Wards/%C3%89ast%20%21%27%28%29%2A/Residents/7?fixed=a%20b&keep=%2f&Term=a+b%2Bc%26d&Beds=12.5&Open=false");
    expect(prepared.request).toStrictEqual({ Ward: "Éast !'()*", Id: 7, Term: "a b+c&d", Beds: 12.5, Open: false });
  });

  test("NothingBindsAsDeclared: leave omits a field, empty writes an empty field, and held null follows the same policy", () => {
    const missing: Field = { name: "Beds", path: ["Beds"], value: { kind: "nothing" }, form: { kind: "number" }, whenNothing: "leave" };
    const send = get([
      missing,
      { ...missing, name: "Note", path: ["Note"], whenNothing: "empty", form: { kind: "text" } },
      field("Phone", null, { kind: "text" }, "empty"),
      field("Rooms", null, { kind: "list", item: { kind: "number" }, itemWhenNothing: "stop" }),
    ]);
    const prepared = prepare(send, clock);
    expect(prepared.url).toBe("/Residents?Note=&Phone=");
    expect(prepared.request).toStrictEqual({ Note: null, Phone: null });
  });

  test("ScalarWireForms: sends finite numbers invariantly, booleans and written text or numbers as held", () => {
    const prepared = prepare(get([
      field("Ratio", 0.00001, { kind: "number" }),
      field("Open", true, { kind: "boolean" }),
      field("Code", "007", { kind: "written" }),
      field("Count", 19, { kind: "written" }),
    ]), clock);
    expect(prepared.url).toBe("/Residents?Ratio=0.00001&Open=true&Code=007&Count=19");
  });

  test("EnumWireNames: maps the held form to its C# name, including a boolean held form", () => {
    const prepared = prepare(get([
      field("Choice", "high", { kind: "enum", values: [{ held: "low", name: "Minimum" }, { held: "high", name: "Maximum" }] }),
      field("Enabled", true, { kind: "enum", values: [{ held: false, name: "No" }, { held: true, name: "Yes" }] }),
    ]), clock);
    expect(prepared.url).toBe("/Residents?Choice=Maximum&Enabled=Yes");
  });

  test("FlagsUseTheExactSplit: splits text and numeric flags largest first and sends names smallest first beyond 32 bits", () => {
    const prepared = prepare(get([
      field("Care", "weekend, mon", care),
      field("Days", 97, { ...care, values: care.values.map(row => ({ ...row, held: row.number })) }),
      field("Large", 4_294_967_297, { kind: "flags", values: [{ held: 4_294_967_296, name: "High", number: 4_294_967_296 }, { held: 1, name: "Low", number: 1 }] }),
      field("Zero", "none", care),
    ]), clock);
    expect(prepared.url).toBe("/Residents?Care=Mon%2C+Weekend&Days=Mon%2C+Weekend&Large=Low%2C+High&Zero=None");
  });

  test("ItemNothingBindsAsDeclared: repeats a list key and keeps nullable items as empty fields", () => {
    const prepared = prepare(get([
      field("Shifts", ["day", null, undefined, "night"], { kind: "list", item: { kind: "enum", values: [{ held: "day", name: "Day" }, { held: "night", name: "Night" }] }, itemWhenNothing: "empty" }),
    ]), clock);
    expect(prepared.url).toBe("/Residents?Shifts=Day&Shifts=&Shifts=&Shifts=Night");
    expect(prepared.request).toStrictEqual({ Shifts: ["day", null, null, "night"] });
  });

  test("DateWire: dates keep wall fields, midnight omits time, and instants carry the offset of their own day", () => {
    const prepared = prepare(get([
      field("Day", new Date(2026, 0, 12), { kind: "date" }),
      field("Visit", new Date(2026, 0, 12, 9, 30, 0, 125), { kind: "date" }),
      field("Winter", new Date("2026-01-12T14:30:00Z"), { kind: "instant" }),
      field("Summer", new Date("2026-07-12T13:30:00Z"), { kind: "instant" }),
    ]), clock);
    expect(prepared.url).toBe("/Residents?Day=2026-01-12&Visit=2026-01-12T09%3A30%3A00.125&Winter=2026-01-12T09%3A30%3A00-05%3A00&Summer=2026-07-12T09%3A30%3A00-04%3A00");
  });

  test("InstantUsesTheViewsZone: a named zone overrides the browser clock and UTC writes Z", () => {
    const send = get([field("At", new Date("2026-01-12T00:00:00Z"), { kind: "instant" }), field("Day", new Date(2026, 0, 12), { kind: "date" })]);
    expect(prepare(send, { kind: "zone", id: "Asia/Kolkata" }).url).toBe("/Residents?At=2026-01-12T05%3A30%3A00%2B05%3A30&Day=2026-01-12");
    expect(prepare(send, { kind: "zone", id: "UTC" }).url).toBe("/Residents?At=2026-01-12T00%3A00%3A00Z&Day=2026-01-12");
  });

  test("HistoricalInstantWaitsForAForm: an offset containing seconds has no invented wire form", () => {
    const send = get([field("At", new Date("1900-01-01T00:00:00Z"), { kind: "instant" })]);
    expect(() => prepare(send, { kind: "zone", id: "Europe/Paris" }))
      .toThrow("This page cannot run an instant with an offset containing seconds: DateWire states minute offsets yet: the framework's code for it is not built.");
  });

  test("SentMembersUseTheirPaths: the request snapshot nests fields, keeps own names and does not share a sent list", () => {
    const beds = [2, 4];
    const prepared = prepare(get([
      { ...field("Lines[0].Beds", beds, { kind: "list", item: { kind: "number" }, itemWhenNothing: "stop" }), path: ["lines", 0, "beds"] },
      { ...field("Contact.Name", "Ada", { kind: "text" }), path: ["contact", "name"] },
      { ...field("Own", "kept", { kind: "text" }), path: ["__proto__", "own"] },
    ]), clock);
    beds.push(9);
    expect(prepared.request).toStrictEqual({ lines: [{ beds: [2, 4] }], contact: { name: "Ada" }, ["__proto__"]: { own: "kept" } });
    expect(prepared.url).toBe("/Residents?Lines%5B0%5D.Beds=2&Lines%5B0%5D.Beds=4&Contact.Name=Ada&Own=kept");
  });

  test("HeadersUseTheirWire: trims field whitespace, skips nothing and adds no header of its own", () => {
    const prepared = prepare({
      ...get(),
      headers: [
        { name: "X-Name", value: { kind: "held", value: " \tAda\t " }, form: { kind: "text" } },
        { name: "X-Count", value: { kind: "held", value: 2.5 }, form: { kind: "number" } },
        { name: "X-Missing", value: { kind: "nothing" }, form: { kind: "text" } },
        { name: "X-Null", value: { kind: "held", value: null }, form: { kind: "text" } },
      ],
    }, clock);
    expect([...prepared.headers]).toStrictEqual([["x-count", "2.5"], ["x-name", "Ada"]]);
  });

  test.each([undefined, null, "", "a/b"])("MissingPathValueStops: a path with %s stops before the send", value => {
    const send: Send = { ...get(), url: { parts: [{ kind: "text", text: "/Rooms/" }, { kind: "member", name: "Code", value: value === undefined ? { kind: "nothing" } : { kind: "held", value }, form: { kind: "text" } }], query: "" } };
    expect(() => prepare(send, clock)).toThrow("The request's path member 'Code' must hold nonempty text with no '/'.");
  });

  test.each(["Zoë", "Ada\nHart", "Ada\rHart", "Ada\u007f"])("HeaderGuard: rejects %s outside the HTTP field value", value => {
    expect(() => prepare({ ...get(), headers: [{ name: "X-Applicant", value: { kind: "held", value }, form: { kind: "text" } }] }, clock))
      .toThrow("The request's header 'X-Applicant' must hold visible US-ASCII, spaces or tabs.");
  });

  test.each([
    field("Beds", "3", { kind: "number" }),
    field("Beds", Infinity, { kind: "number" }),
    field("Open", "true", { kind: "boolean" }),
    field("Name", 3, { kind: "text" }),
    field("Code", {}, { kind: "written" }),
    field("Day", new Date(NaN), { kind: "date" }),
    field("Beds", [1, null], { kind: "list", item: { kind: "number" }, itemWhenNothing: "stop" }),
    field("Beds", 1, { kind: "list", item: { kind: "number" }, itemWhenNothing: "stop" }),
  ])("WireGuard: refuses a present value outside $name's declared form", sent => {
    expect(() => prepare(get([sent]), clock)).toThrow("The request's member");
  });

  test("UnknownEnumStops: names the member and every accepted held form", () => {
    expect(() => prepare(get([field("Choice", "medium", { kind: "enum", values: [{ held: "low", name: "Minimum" }, { held: "high", name: "Maximum" }] })]), clock))
      .toThrow('The request\'s member \'Choice\' holds "medium"; its enum forms are "low", "high".');
  });

  test.each(["unknown", 97, "mon, unknown"])("FlagsGuard: refuses %s under text rows", held => {
    expect(() => prepare(get([field("Care", held, care)]), clock)).toThrow("The request's member 'Care'");
  });

  test.each([129, 0, 1.5, -1, 9_007_199_254_740_992, "1"])("FlagsGuard: refuses %s that numeric rows cannot split", held => {
    expect(() => prepare(get([field("Care", held, { kind: "flags", values: [{ held: 1, name: "Mon", number: 1 }] })]), clock))
      .toThrow("The request's member 'Care'");
  });

  test("MissingInputsStop: checks the captured absence of a container and an individual input", () => {
    expect(() => prepare({ ...get(), query: { fields: [], inputs: { container: { kind: "missing", id: "admission" }, individual: [] } } }, clock))
      .toThrow("The request's container 'admission' is missing.");
    expect(() => prepare({ ...get(), query: { fields: [], inputs: { container: { kind: "none" }, individual: [{ kind: "missing", id: "room", name: "Room" }] } } }, clock))
      .toThrow("The request's input 'room' for 'Room' is missing.");
  });
});

describe("when a request reaches a deferred U18 case, it names the piece it waits on", () => {
  test.each<Send["method"]>(["POST", "PUT", "DELETE"])("DeferredMethod: %s waits on request bodies", method => {
    expect(() => prepare({ ...get(), method }, clock)).toThrow("U18's request bodies");
  });

  test.each<Send["body"]>([
    { kind: "json", value: {}, inputs: emptyInputs },
    { kind: "urlEncoded", fields: [], inputs: emptyInputs },
    { kind: "multipart", parts: [], inputs: emptyInputs },
    { kind: "text", value: { kind: "held", value: "note" }, inputs: emptyInputs },
    { kind: "file", value: { kind: "nothing" }, inputs: emptyInputs },
  ])("DeferredBody: $kind waits on request bodies", body => {
    expect(() => prepare({ ...get(), body }, clock)).toThrow("U18's request bodies");
  });

  test("DeferredAntiforgery: a token waits on antiforgery", () => {
    expect(() => prepare({ ...get(), antiforgery: { kind: "token", value: "token", place: { kind: "header", name: "RequestVerificationToken" } } }, clock))
      .toThrow("U18's antiforgery");
  });

  test("DeferredRetry: a standard policy waits on retry", () => {
    expect(() => prepare({ ...get(), retry: { kind: "standard", attempts: 3, firstWaitMilliseconds: 2000 } }, clock)).toThrow("U18's retry");
  });

  test.each<Send["busy"]>([{ kind: "page" }, { kind: "targets", values: [] }])("DeferredBusy: $kind waits on the indicator", busy => {
    expect(() => prepare({ ...get(), busy }, clock)).toThrow("U18's busy indicator");
  });

  test.each<Send["contracts"][number]>(["busy", "notice"])("DeferredContract: %s waits on its component contract", contract => {
    expect(() => prepare({ ...get(), contracts: [contract] }, clock)).toThrow("contract: U18's");
  });
});
