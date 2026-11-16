import { describe, expect, test } from "vitest";
import { prepare, Submission } from "./index";

type Send = Parameters<typeof prepare>[0];
type Field = Send["query"]["fields"][number];
type Form = Field["form"];
type Zone = Parameters<typeof prepare>[1];
type Culture = Parameters<typeof prepare>[2];
type Body = Send["body"];

const clock: Zone = { kind: "clock" };
const enUs: Culture = { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } };
const deDe: Culture = { ...enUs, name: "de-DE", collation: "de-DE", decimal: ",", group: "." };
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
    const prepared = prepare(send, clock, enUs);
    expect(prepared.url).toBe("/Wards/%C3%89ast%20%21%27%28%29%2A/Residents/7?fixed=a%20b&keep=%2f&Term=a+b%2Bc%26d&Beds=12.5&Open=false");
    expect(prepared.request).toStrictEqual({ Ward: "Éast !'()*", Id: 7, Term: "a b+c&d", Beds: 12.5, Open: false });
  });

  test("RouteTemplate: an attempt is named by its path's template, each placeholder by its name, with neither its origin nor its query (D20.4, D20.5)", () => {
    const ward: Send["url"]["parts"][number] = { kind: "member", name: "Ward", value: { kind: "held", value: "East" }, form: { kind: "text" } };
    const onThePage = prepare({ ...get(), url: { parts: [{ kind: "text", text: "/Wards/" }, ward], query: "fixed=a" } }, clock, enUs);
    const elsewhere = prepare({ ...get(), url: { parts: [{ kind: "text", text: "https://pharmacy.example:8443/Stock/" }, ward], query: "" } }, clock, enUs);
    const bare = prepare({ ...get(), url: { parts: [{ kind: "text", text: "https://pharmacy.example" }], query: "" } }, clock, enUs);
    expect([onThePage.template, elsewhere.template, bare.template]).toStrictEqual(["/Wards/{Ward}", "/Stock/{Ward}", "/"]);
  });

  test("NothingBindsAsDeclared: leave omits a field, empty writes an empty field, and held null follows the same policy", () => {
    const missing: Field = { name: "Beds", path: ["Beds"], value: { kind: "nothing" }, form: { kind: "number" }, whenNothing: "leave" };
    const send = get([
      missing,
      { ...missing, name: "Note", path: ["Note"], whenNothing: "empty", form: { kind: "text" } },
      field("Phone", null, { kind: "text" }, "empty"),
      field("Rooms", null, { kind: "list", item: { kind: "number" }, itemWhenNothing: "stop" }),
    ]);
    const prepared = prepare(send, clock, enUs);
    expect(prepared.url).toBe("/Residents?Note=&Phone=");
    expect(prepared.request).toStrictEqual({ Note: null, Phone: null });
  });

  test("ScalarWireForms: sends finite numbers invariantly, booleans and written text or numbers as held", () => {
    const prepared = prepare(get([
      field("Ratio", 0.00001, { kind: "number" }),
      field("Open", true, { kind: "boolean" }),
      field("Code", "007", { kind: "written" }),
      field("Count", 19, { kind: "written" }),
    ]), clock, enUs);
    expect(prepared.url).toBe("/Residents?Ratio=0.00001&Open=true&Code=007&Count=19");
  });

  test("EnumWireNames: maps the held form to its C# name, including a boolean held form", () => {
    const prepared = prepare(get([
      field("Choice", "high", { kind: "enum", values: [{ held: "low", name: "Minimum" }, { held: "high", name: "Maximum" }] }),
      field("Enabled", true, { kind: "enum", values: [{ held: false, name: "No" }, { held: true, name: "Yes" }] }),
    ]), clock, enUs);
    expect(prepared.url).toBe("/Residents?Choice=Maximum&Enabled=Yes");
  });

  test("FlagsUseTheExactSplit: splits text and numeric flags largest first and sends names smallest first beyond 32 bits", () => {
    const prepared = prepare(get([
      field("Care", "weekend, mon", care),
      field("Days", 97, { ...care, values: care.values.map(row => ({ ...row, held: row.number })) }),
      field("Large", 4_294_967_297, { kind: "flags", values: [{ held: 4_294_967_296, name: "High", number: 4_294_967_296 }, { held: 1, name: "Low", number: 1 }] }),
      field("Zero", "none", care),
    ]), clock, enUs);
    expect(prepared.url).toBe("/Residents?Care=Mon%2C+Weekend&Days=Mon%2C+Weekend&Large=Low%2C+High&Zero=None");
  });

  test("ItemNothingBindsAsDeclared: repeats a list key and keeps nullable items as empty fields", () => {
    const prepared = prepare(get([
      field("Shifts", ["day", null, undefined, "night"], { kind: "list", item: { kind: "enum", values: [{ held: "day", name: "Day" }, { held: "night", name: "Night" }] }, itemWhenNothing: "empty" }),
    ]), clock, enUs);
    expect(prepared.url).toBe("/Residents?Shifts=Day&Shifts=&Shifts=&Shifts=Night");
    expect(prepared.request).toStrictEqual({ Shifts: ["day", null, null, "night"] });
  });

  test("DateWire: dates keep wall fields, midnight omits time, and instants carry the offset of their own day", () => {
    const prepared = prepare(get([
      field("Day", new Date(2026, 0, 12), { kind: "date" }),
      field("Visit", new Date(2026, 0, 12, 9, 30, 0, 125), { kind: "date" }),
      field("Winter", new Date("2026-01-12T14:30:00Z"), { kind: "instant" }),
      field("Summer", new Date("2026-07-12T13:30:00Z"), { kind: "instant" }),
    ]), clock, enUs);
    expect(prepared.url).toBe("/Residents?Day=2026-01-12&Visit=2026-01-12T09%3A30%3A00.125&Winter=2026-01-12T09%3A30%3A00-05%3A00&Summer=2026-07-12T09%3A30%3A00-04%3A00");
  });

  test("InstantUsesTheViewsZone: a named zone overrides the browser clock and UTC writes Z", () => {
    const send = get([field("At", new Date("2026-01-12T00:00:00Z"), { kind: "instant" }), field("Day", new Date(2026, 0, 12), { kind: "date" })]);
    expect(prepare(send, { kind: "zone", id: "Asia/Kolkata" }, enUs).url).toBe("/Residents?At=2026-01-12T05%3A30%3A00%2B05%3A30&Day=2026-01-12");
    expect(prepare(send, { kind: "zone", id: "UTC" }, enUs).url).toBe("/Residents?At=2026-01-12T00%3A00%3A00Z&Day=2026-01-12");
  });

  test("HistoricalInstantWaitsForAForm: an offset containing seconds has no invented wire form", () => {
    const send = get([field("At", new Date("1900-01-01T00:00:00Z"), { kind: "instant" })]);
    expect(() => prepare(send, { kind: "zone", id: "Europe/Paris" }, enUs))
      .toThrow("This page cannot run an instant with an offset containing seconds: DateWire states minute offsets yet: the framework's code for it is not built.");
  });

  test("SentMembersUseTheirPaths: the request snapshot nests fields, keeps own names and does not share a sent list", () => {
    const beds = [2, 4];
    const prepared = prepare(get([
      { ...field("Lines[0].Beds", beds, { kind: "list", item: { kind: "number" }, itemWhenNothing: "stop" }), path: ["lines", 0, "beds"] },
      { ...field("Contact.Name", "Ada", { kind: "text" }), path: ["contact", "name"] },
      { ...field("Own", "kept", { kind: "text" }), path: ["__proto__", "own"] },
    ]), clock, enUs);
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
    }, clock, enUs);
    expect([...prepared.headers]).toStrictEqual([["x-count", "2.5"], ["x-name", "Ada"]]);
  });

  test.each([undefined, null, "", "a/b"])("MissingPathValueStops: a path with %s stops before the send", value => {
    const send: Send = { ...get(), url: { parts: [{ kind: "text", text: "/Rooms/" }, { kind: "member", name: "Code", value: value === undefined ? { kind: "nothing" } : { kind: "held", value }, form: { kind: "text" } }], query: "" } };
    expect(() => prepare(send, clock, enUs)).toThrow("The request's path member 'Code' must hold nonempty text with no '/'.");
  });

  test.each(["Zoë", "Ada\nHart", "Ada\rHart", "Ada\u007f"])("HeaderGuard: rejects %s outside the HTTP field value", value => {
    expect(() => prepare({ ...get(), headers: [{ name: "X-Applicant", value: { kind: "held", value }, form: { kind: "text" } }] }, clock, enUs))
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
    expect(() => prepare(get([sent]), clock, enUs)).toThrow("The request's member");
  });

  test("UnknownEnumStops: names the member and every accepted held form", () => {
    expect(() => prepare(get([field("Choice", "medium", { kind: "enum", values: [{ held: "low", name: "Minimum" }, { held: "high", name: "Maximum" }] })]), clock, enUs))
      .toThrow('The request\'s member \'Choice\' holds "medium"; its enum forms are "low", "high".');
  });

  test.each(["unknown", 97, "mon, unknown"])("FlagsGuard: refuses %s under text rows", held => {
    expect(() => prepare(get([field("Care", held, care)]), clock, enUs)).toThrow("The request's member 'Care'");
  });

  test.each([129, 0, 1.5, -1, 9_007_199_254_740_992, "1"])("FlagsGuard: refuses %s that numeric rows cannot split", held => {
    expect(() => prepare(get([field("Care", held, { kind: "flags", values: [{ held: 1, name: "Mon", number: 1 }] })]), clock, enUs))
      .toThrow("The request's member 'Care'");
  });

  test("MissingInputsStop: checks the captured absence of a container and an individual input", () => {
    expect(() => prepare({ ...get(), query: { fields: [], inputs: { container: { kind: "missing", id: "admission" }, individual: [] } } }, clock, enUs))
      .toThrow("The request's container 'admission' is missing.");
    expect(() => prepare({ ...get(), query: { fields: [], inputs: { container: { kind: "none" }, individual: [{ kind: "missing", id: "room", name: "Room" }] } } }, clock, enUs))
      .toThrow("The request's input 'room' for 'Room' is missing.");
  });
});

describe("when a request sends a body, D9.7 to D9.10, D9.16, D9.19, D9.21 and D9.22 write it", () => {
  const post = (body: Body, antiforgery: Send["antiforgery"] = { kind: "none" }): Send => ({ ...get(), method: "POST", body, antiforgery });
  const urlEncoded = (fields: readonly Field[]): Body => ({ kind: "urlEncoded", fields, inputs: emptyInputs });
  const multipart = (parts: Extract<Body, { kind: "multipart" }>["parts"]): Body => ({ kind: "multipart", parts, inputs: emptyInputs });
  const json = (held: unknown): Body => ({ kind: "json", value: held, inputs: emptyInputs });
  const sent = async (body: BodyInit | null): Promise<string> => new Response(body).text();

  test.each<Send["method"]>(["POST", "PUT", "DELETE"])("MethodsCarryTheirBodies: %s is sent as written, with no body when the plan carries none", method => {
    const prepared = prepare({ ...get(), method }, clock, enUs);
    expect(prepared.method).toBe(method);
    expect(prepared.body).toBeNull();
  });

  test("JsonBodySetsItsOwnType: one object, written by JSON.stringify under application/json", async () => {
    const prepared = prepare(post(json({ code: "A-7", beds: 2, open: true, ward: { name: "North" } })), clock, enUs);
    expect(prepared.headers.get("Content-Type")).toBe("application/json");
    expect(await sent(prepared.body)).toBe('{"code":"A-7","beds":2,"open":true,"ward":{"name":"North"}}');
  });

  test("NothingBindsAsDeclared: a value type or list that reads nothing is left out of JSON, a nullable is null, and form data leaves or empties it", async () => {
    expect(await sent(prepare(post(json({ beds: undefined, rooms: undefined, phone: null })), clock, enUs).body)).toBe('{"phone":null}');
    const missing: Field = { name: "Beds", path: ["Beds"], value: { kind: "nothing" }, form: { kind: "number" }, whenNothing: "leave" };
    const form = prepare(post(urlEncoded([missing, { ...missing, name: "Phone", path: ["Phone"], form: { kind: "text" }, whenNothing: "empty" }])), clock, enUs);
    expect(await sent(form.body)).toBe("Phone=");
    expect(form.request).toStrictEqual({ Phone: null });
  });

  test("ObjectNothingBindsAsDeclared: an object that reads nothing is null in JSON and left out of form data", async () => {
    expect(await sent(prepare(post(json({ address: null })), clock, enUs).body)).toBe('{"address":null}');
    const address: Field = { name: "Address", path: ["Address"], value: { kind: "nothing" }, form: { kind: "text" }, whenNothing: "leave" };
    expect(await sent(prepare(post(multipart([{ kind: "field", field: address }, { kind: "field", field: field("Ward", "North", { kind: "text" }) }])), clock, enUs).body))
      .not.toContain('name="Address"');
  });

  test("ItemNothingBindsAsDeclared: a nullable item in form data is an empty field, and a value or object item that reads nothing stops the request", async () => {
    const shifts = field("Shifts", ["Day", null], { kind: "list", item: { kind: "text" }, itemWhenNothing: "empty" });
    expect(await sent(prepare(post(urlEncoded([shifts])), clock, enUs).body)).toBe("Shifts=Day&Shifts=");
    expect(() => prepare(post(urlEncoded([field("Beds", [1, null], { kind: "list", item: { kind: "number" }, itemWhenNothing: "stop" })])), clock, enUs))
      .toThrow("The request's member 'Beds' must hold a list whose items all hold values.");
    expect(() => prepare(post(multipart([{ kind: "file", name: "Documents", value: { kind: "held", value: [null] } }])), clock, enUs))
      .toThrow("The request's file part 'Documents' must hold a file in each item.");
  });

  test("FormDataUsesTheViewsCulture: a number carries the culture's separator and a date its time, while the query stays invariant", async () => {
    const prepared = prepare({
      ...post(urlEncoded([field("Rate", 12.5, { kind: "number" }), field("Day", new Date(2026, 0, 12), { kind: "date" }), field("Visit", new Date(2026, 0, 12, 9, 30), { kind: "date" })])),
      query: { fields: [field("Rate", 12.5, { kind: "number" })], inputs: emptyInputs },
    }, clock, deDe);
    expect(prepared.url).toBe("/Residents?Rate=12.5");
    expect(await sent(prepared.body)).toBe("Rate=12%2C5&Day=2026-01-12T00%3A00%3A00&Visit=2026-01-12T09%3A30%3A00");
    expect(prepared.request).toStrictEqual({ Rate: 12.5, Day: new Date(2026, 0, 12), Visit: new Date(2026, 0, 12, 9, 30) });
  });

  test("MultipartSendsFieldsAndFiles: each field by its binding name and each file under its part's name with its own name", () => {
    const care = new File(["care plan"], "care plan \"v2\".pdf", { type: "application/pdf" });
    const scan = new File(["scan"], "Zoë.png", { type: "image/png" });
    const prepared = prepare(post(multipart([
      { kind: "field", field: field("Caption", "Plans", { kind: "text" }) },
      { kind: "file", name: "Documents", value: { kind: "held", value: [care, scan] } },
      { kind: "file", name: "Photo", value: { kind: "nothing" } },
    ])), clock, enUs);
    const form = prepared.body as FormData;
    expect(form.get("Caption")).toBe("Plans");
    expect(form.getAll("Documents").map(file => (file as File).name)).toStrictEqual(['care plan "v2".pdf', "Zoë.png"]);
    expect(form.has("Photo")).toBe(false);
  });

  test("FileOnlyInFormData: a file in a JSON body or a query stops the request, and form data or the whole body sends it", () => {
    const photo = new File(["photo"], "photo.png", { type: "image/png" });
    expect(() => prepare(post(json({ resident: { photo } })), clock, enUs)).toThrow("The request's JSON body holds a file, which is sent only in form data or as the whole body.");
    expect(() => prepare(get([field("Photo", photo, { kind: "text" })]), clock, enUs)).toThrow("The request's member 'Photo' must hold text.");
    const whole = prepare(post({ kind: "file", value: { kind: "held", value: photo }, inputs: emptyInputs }), clock, enUs);
    expect(whole.body).toBe(photo);
    expect(whole.headers.has("Content-Type")).toBe(false);
    expect((prepare(post(multipart([{ kind: "file", name: "Photo", value: { kind: "held", value: [photo] } }])), clock, enUs).body as FormData).get("Photo")).toBe(photo);
  });

  test("TextBodyIsOneValue: a text is the whole body, and a text or file that reads nothing names what waits on a line", async () => {
    expect(await sent(prepare(post({ kind: "text", value: { kind: "held", value: "Night round done" }, inputs: emptyInputs }), clock, enUs).body)).toBe("Night round done");
    expect(() => prepare(post({ kind: "text", value: { kind: "nothing" }, inputs: emptyInputs }), clock, enUs)).toThrow("a text body that reads nothing");
    expect(() => prepare(post({ kind: "file", value: { kind: "nothing" }, inputs: emptyInputs }), clock, enUs)).toThrow("a file body that reads nothing");
  });

  test("AntiforgeryTakesItsPlace: the header place sets the header, and the field place sets the one field of a URL-encoded or multipart body", async () => {
    const header = prepare(post(json({}), { kind: "token", value: "tok-1", place: { kind: "header", name: "RequestVerificationToken" } }), clock, enUs);
    expect(header.headers.get("RequestVerificationToken")).toBe("tok-1");
    const inField = { kind: "token", value: "tok-2", place: { kind: "field", name: "__RequestVerificationToken" } } as const;
    expect(await sent(prepare(post(urlEncoded([]), inField), clock, enUs).body)).toBe("__RequestVerificationToken=tok-2");
    const held = prepare(post(multipart([{ kind: "field", field: field("__RequestVerificationToken", "stale", { kind: "text" }) }]), inField), clock, enUs);
    expect((held.body as FormData).getAll("__RequestVerificationToken")).toStrictEqual(["tok-2"]);
    expect(header.headers.has("__RequestVerificationToken")).toBe(false);
  });
});

type Captured = Extract<Send["query"]["inputs"]["container"], { kind: "captured" }>;
type Binding = Captured["inputs"][number]["entry"]["binding"];
const bound = (name: string, path: Binding["path"], shape: Extract<Binding["value"], { kind: "as" }>["shape"]): Captured["inputs"][number] => ({
  name,
  entry: {
    id: "Intake__" + name,
    binding: {
      name, path, committed: "change", invalidClass: "invalid", message: "",
      value: { kind: "op", op: "coalesce", args: [{ kind: "as", expr: { kind: "lit", value: null }, shape: { kind: "nullable", inner: shape } }, { kind: "lit", value: null }] },
    },
  },
});
const ward = bound("Ward", ["ward"], { kind: "string", date: "none", negativeZero: "0" });
const beds = bound("Beds", ["beds"], { kind: "number", of: "integer", text: "invariant", overflow: "nothing" });
const container = (body: unknown, inputs: Captured["inputs"]): Send["query"]["inputs"] => ({ container: { kind: "captured", id: "intake", body, inputs }, individual: [] });
const post = (body: Body): Send => ({ ...get(), method: "POST", body });

describe("when a request sends a container, a form or an input", () => {
  test("ContainerUnderItsBindingNames: a GET sends each framework input inside the container in its query, in its wire form", () => {
    const prepared = prepare({ ...get([field("Ward", "North", { kind: "text" })]), query: { fields: [field("Ward", "North", { kind: "text" })], inputs: container({ ward: "East", beds: 3 }, [ward, beds]) } }, clock, enUs);
    expect(prepared.url).toBe("/Residents?Ward=East&Beds=3");
    expect(prepared.inputs.map(input => input.name)).toStrictEqual(["Ward", "Beds"]);
  });

  test("SubmissionSetAsTheBrowserBuildsIt: a form's controls are sent, its framework inputs replace their name's entries and the token replaces the form's own", async () => {
    const submitted = new FormData();
    submitted.append("Code", "A7");
    submitted.append("Ward", "typed");
    submitted.append("__RequestVerificationToken", "the form's");
    const body: Body = { kind: "urlEncoded", fields: [], inputs: container(new Submission(submitted, { ward: "East" }), [ward]) };
    const prepared = prepare({ ...post(body), antiforgery: { kind: "token", value: "framework", place: { kind: "field", name: "__RequestVerificationToken" } } }, clock, enUs);
    expect(await new Response(prepared.body).text()).toBe("Code=A7&__RequestVerificationToken=framework&Ward=East");
  });

  test("ContainerInAnyBody: a JSON body holds the container's inputs over the members it sets, and multipart sends them as fields", () => {
    const json = prepare(post({ kind: "json", value: { ward: "North", room: "R4" }, inputs: container({ ward: "East" }, [ward]) }), clock, enUs);
    expect(json.body).toBe("{\"ward\":\"East\",\"room\":\"R4\"}");
    const multipart = prepare(post({ kind: "multipart", parts: [], inputs: container({ beds: 2.5 }, [beds]) }), clock, deDe);
    expect([...(multipart.body as FormData).entries()]).toStrictEqual([["Beds", "2,5"]]);
  });

  test("FileOnlyInFormData: a file a form holds stops a GET or URL-encoded request that would send it", () => {
    const submitted = new FormData();
    submitted.append("Photo", new File(["x"], "photo.png"));
    expect(() => prepare({ ...get(), query: { fields: [], inputs: container(new Submission(submitted, {}), []) } }, clock, enUs))
      .toThrow(/^The request's form control 'Photo' holds a file, which is sent only in multipart form data\.$/);
  });

  test("MissingStopsAtTheGuard: a container or an input the page lacks stops the request where it is prepared", () => {
    expect(() => prepare({ ...get(), query: { fields: [], inputs: { container: { kind: "missing", id: "intake" }, individual: [] } } }, clock, enUs))
      .toThrow(/^The request's container 'intake' is missing\.$/);
    expect(() => prepare({ ...get(), query: { fields: [], inputs: { container: { kind: "none" }, individual: [{ kind: "missing", id: "Intake__Ward", name: "Ward" }] } } }, clock, enUs))
      .toThrow(/^The request's input 'Intake__Ward' for 'Ward' is missing\.$/);
  });
});
