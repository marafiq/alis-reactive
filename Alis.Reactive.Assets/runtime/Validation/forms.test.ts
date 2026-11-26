import { describe, expect, test } from "vitest";
import { PageForms } from "./index";

type Plan = Parameters<PageForms["join"]>[0];
type Owner = Parameters<PageForms["join"]>[1];
type Fn = Parameters<Parameters<PageForms["join"]>[2]>[0];
type Check = Parameters<PageForms["validate"]>[0];
type Sent = Check["inputs"][number];
type Entry = Sent["entry"];
type Place = Check["place"];
type Form = Extract<Extract<Plan["objects"][string], { kind: "plugin" }>["data"], { kind: "form" }>;
type Registration = Form["registration"];
type Group = Registration["rules"];
type Format = Check["body"]["format"];
type Table = ConstructorParameters<typeof PageForms>[0];
type Places = ConstructorParameters<typeof PageForms>[1];
type LiveCheck = ReturnType<Table["liveChecksOf"]>[number];

class InputsTable implements Table {
  readonly #typed: Record<string, string>;
  readonly #held = new Map<string, Entry>();
  readonly #summaries: Record<string, string>;
  readonly #live = new Map<string, readonly LiveCheck[]>();

  constructor(typed: Record<string, string> = {}, summaries: Record<string, string> = { intake: "intake_summary" }) {
    this.#typed = typed;
    this.#summaries = summaries;
  }

  typedText(input: Sent): string | null { return this.#typed[input.name] ?? null; }
  holds(entry: Entry): boolean { return this.#held.get(entry.id)?.binding === entry.binding; }
  hold(...inputs: Sent[]): void { for (const input of inputs) this.#held.set(input.entry.id, input.entry); }
  liveChecksOf(inputId: string): readonly LiveCheck[] { return this.#live.get(inputId) ?? []; }
  checksLive(inputId: string, ...checks: LiveCheck[]): void { this.#live.set(inputId, checks); }
  summaryOf(containerId: string): string {
    const summary = this.#summaries[containerId];
    if (summary === undefined) throw new Error("No live plan renders container '" + containerId + "'.");
    return summary;
  }
}

class Page implements Places {
  readonly messages = new Map<string, string>();
  readonly summaries = new Map<string, readonly string[]>();
  readonly notices: [unknown, unknown][] = [];

  showMessage(entry: Entry, message: string): void { this.messages.set(entry.binding.message, message); }
  clearMessage(entry: Entry): void { this.messages.delete(entry.binding.message); }
  showSummary(summaryId: string, messages: readonly string[]): void { this.summaries.set(summaryId, messages); }
  notice(): unknown { return { show: (text: unknown, kind: unknown) => { this.notices.push([text, kind]); } }; }
}

const inTheSummary: Place = { destination: { kind: "summary", containerId: "intake" } };
const atTheNotice: Place = { destination: { kind: "notice", separator: " " } };

class Visit implements Owner {
  readonly drops: (() => void)[] = [];
  runs(): void { return undefined; }
  mounts(): void { return undefined; }
  listens(): void { return undefined; }
  holds(): void { return undefined; }
  registers(drop: () => void): void { this.drops.push(drop); }
  release(): void { for (const drop of this.drops.splice(0)) drop(); }
}

const tests = new Map<Fn, (...values: unknown[]) => unknown>();

function fn(test: (...values: unknown[]) => unknown): Fn {
  const written: Fn = { kind: "fn", params: ["value", "instance"], body: { kind: "lit", value: true } };
  tests.set(written, test);
  return written;
}

const read = (written: Fn, args: readonly unknown[]): unknown => {
  const test = tests.get(written);
  if (test === undefined) throw new Error("This registration's tests are the fns this file writes.");
  return test(...args);
};
const always = fn(() => true);
const holdsAValue = fn(value => value !== null && value !== undefined);
const notBlank = fn(value => typeof value === "string" && value.trim() !== "");
const field = (path: Group["path"], message: string, check = notBlank, when = always): Group["fields"][number] => ({ path, tests: [{ check, when, message }] });
const group = (fields: Group["fields"], groups: Group["groups"] = [], path: Group["path"] = [], when = always): Group => ({ path, when, fields, groups });
const counted = fn(value => typeof value === "number" && value > 0);
const notNull = fn(value => value !== null);

const wardCard: Registration = {
  validator: "WardCard", ruleSet: "default", start: { ward: null },
  scalars: [], objects: [], lists: [],
  rules: group([field(["ward"], "'Ward' must not be empty.", notNull)]),
};

const supplies: Registration = {
  validator: "Supplies", ruleSet: "default", start: { count: 0 },
  scalars: [{ path: ["count"], shape: { kind: "number", of: "integer", text: "view", overflow: "nothing" }, before: "The value '", after: "' is not valid for Count." }],
  objects: [], lists: [],
  rules: group([field(["count"], "'Count' must be greater than '0'.", counted)]),
};

const deputyCard = (deputyStart: string): Registration => ({
  validator: "DeputyCard", ruleSet: "default", start: { deputy: null },
  scalars: [], objects: [{ path: ["deputy"], start: { name: deputyStart, phone: null }, json: "replace" }], lists: [],
  rules: group([], [group([field(["name"], "Name the deputy.")], [], ["deputy"], holdsAValue)]),
});

const intakeCard = (lists: "append" | "replace"): Registration => ({
  validator: "IntakeCard", ruleSet: "default", start: { name: "Unnamed", status: null, urgent: false, carers: [{ name: "Ruth" }] },
  scalars: [], objects: [], lists: [{ path: ["carers"], item: { name: null }, json: lists }],
  rules: group([], [
    group([field(["status"], "'Status' must not be empty.", notBlank, fn(instance => Reflect.get(instance as object, "urgent") === true))]),
    group([field(["name"], "Name the resident.")]),
    group([], [group([field(["name"], "Name the carer.")], [], ["carers", { kind: "each" }], holdsAValue)]),
  ]),
});

const bedNote: Registration = {
  validator: "BedNote", ruleSet: "default", start: { ward: null, shift: "Night" },
  scalars: [], objects: [], lists: [],
  rules: group([
    field(["ward"], "A ward's name holds 5 letters at most.", fn(value => value === null || (typeof value === "string" && value.length <= 5))),
    field(["shift"], "Name the shift.", notNull),
  ]),
};

const home = (objects: "fill" | "replace"): Registration => ({
  validator: "Home", ruleSet: "default", start: { lead: { name: "Ruth", phone: null } },
  scalars: [], objects: [{ path: ["lead"], start: { name: "", phone: null }, json: objects }], lists: [],
  rules: group([field(["lead", "name"], "Lead name is required.")]),
});

function plan(registrations: Registration[]): Plan {
  return {
    version: 7, planId: "Page",
    culture: { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } },
    timeZone: { kind: "clock" }, trace: { kind: "off" }, vendors: {}, handlers: [],
    objects: Object.fromEntries(registrations.map(registration => ["$form:" + registration.validator + ":default", {
      kind: "plugin", name: "alis.form", key: registration.validator + ":default",
      listen: { kind: "method", subscribe: "addEventListener", unsubscribe: "removeEventListener" },
      data: { kind: "form", registration },
    }])),
  };
}

function sent(name: string, path: Entry["binding"]["path"], message = ""): Sent {
  const converted: Entry["binding"]["converted"] = { kind: "as", expr: { kind: "lit", value: null }, shape: { kind: "nullable", inner: { kind: "any" } } };
  const blank: Entry["binding"]["blank"] = { kind: "lit", value: null };
  const textField = { kind: "field", form: { kind: "text" }, whenNothing: "empty", whenBlank: "send" } as const;
  return { name, path, entry: { id: "input_" + name, binding: { name, path, converted, blank, sent: { kind: "lit", value: textField }, committed: "change", invalidClass: "invalid", message } }, sent: textField };
}

function shown(name: string, path: Entry["binding"]["path"]): Sent {
  return sent(name, path, "input_" + name + "_message");
}

function check(validator: string, value: unknown, inputs: Sent[] = [], format: Format = "urlEncoded", place: Place = inTheSummary): Check {
  return { registration: validator + ":default", body: { format, value }, inputs, place };
}

function forms(registrations: Registration[], typed: Record<string, string> = {}): PageForms {
  const page = new PageForms(new InputsTable(typed), new Page());
  page.join(plan(registrations), new Visit(), read);
  return page;
}

function placed(registrations: Registration[], table: InputsTable, page: Page): PageForms {
  const placing = new PageForms(table, page);
  placing.join(plan(registrations), new Visit(), read);
  return placing;
}

describe("when alis.form checks the body a container sends", () => {
  test("NoTypedValueLost: a text its member cannot hold fails it with the binder's message, and its rules are skipped", () => {
    const page = forms([supplies], { Count: "12a" });
    expect(page.validate(check("Supplies", { count: 0 }, [sent("Count", ["count"])]))).toStrictEqual([{ name: "Count", message: "The value '12a' is not valid for Count." }]);
  });

  test("NoTypedValueLost: a text that converts is checked by the member's rules", () => {
    const page = forms([supplies]);
    expect(page.validate(check("Supplies", { count: 0 }, [sent("Count", ["count"])]))).toStrictEqual([{ name: "Count", message: "'Count' must be greater than '0'." }]);
    expect(page.validate(check("Supplies", { count: 12 }, [sent("Count", ["count"])]))).toStrictEqual([]);
  });

  test("EmptyTextAgrees: form data binds an empty text as null", () => {
    const page = forms([wardCard, intakeCard("replace")]);
    expect(page.validate(check("WardCard", { ward: "" }, [sent("Ward", ["ward"])]))).toStrictEqual([{ name: "Ward", message: "'Ward' must not be empty." }]);
    expect(page.validate(check("IntakeCard", { name: "" }, [sent("Name", ["name"])]))).toStrictEqual([{ name: "Name", message: "Name the resident." }]);
  });

  test("EmptyTextAgrees: JSON keeps an empty text as text", () => {
    const page = forms([wardCard]);
    expect(page.validate(check("WardCard", { ward: "" }, [sent("Ward", ["ward"])], "json"))).toStrictEqual([]);
    expect(page.validate(check("WardCard", {}, [], "json"))).toStrictEqual([{ name: "", message: "'Ward' must not be empty." }]);
  });

  test("ConditionAgrees: a rule's condition reads the checked body, the start of what the body lacks included", () => {
    const page = forms([intakeCard("replace")]);
    expect(page.validate(check("IntakeCard", { status: "" }, [sent("Status", ["status"])]))).toStrictEqual([]);
    expect(page.validate(check("IntakeCard", { status: "", urgent: true }, [sent("Status", ["status"]), sent("Urgent", ["urgent"])])))
      .toStrictEqual([{ name: "Status", message: "'Status' must not be empty." }]);
  });

  test("ListStartAgrees: form data replaces the list the model starts with, each item a new one", () => {
    const page = forms([intakeCard("replace")]);
    expect(page.validate(check("IntakeCard", {}))).toStrictEqual([]);
    expect(page.validate(check("IntakeCard", { carers: [{ name: "" }] }, [sent("Carers[0].Name", ["carers", 0, "name"])])))
      .toStrictEqual([{ name: "Carers[0].Name", message: "Name the carer." }]);
    expect(page.validate(check("IntakeCard", { carers: [{}] }))).toStrictEqual([{ name: "", message: "Name the carer." }]);
  });

  test("ListStartAgrees: a populated JSON list keeps its starting items and appends the sent ones after them", () => {
    const appended = forms([intakeCard("append")]);
    expect(appended.validate(check("IntakeCard", { carers: [{ name: "" }] }, [sent("Carers[0].Name", ["carers", 0, "name"])], "json")))
      .toStrictEqual([{ name: "", message: "Name the carer." }]);
    const replaced = forms([intakeCard("replace")]);
    expect(replaced.validate(check("IntakeCard", { carers: [{ name: "" }] }, [sent("Carers[0].Name", ["carers", 0, "name"])], "json")))
      .toStrictEqual([{ name: "Carers[0].Name", message: "Name the carer." }]);
  });

  test("ObjectStartAgrees: JSON on net10 replaces the object a member starts with by a new one", () => {
    const page = forms([home("replace")]);
    expect(page.validate(check("Home", { lead: { phone: "555-0100" } }, [sent("Lead.Phone", ["lead", "phone"])], "json"))).toStrictEqual([{ name: "", message: "Lead name is required." }]);
  });

  test("ObjectStartAgrees: form data, and a populated member, fill the object the member starts with", () => {
    expect(forms([home("replace")]).validate(check("Home", { lead: { phone: "555-0100" } }, [sent("Lead.Phone", ["lead", "phone"])]))).toStrictEqual([]);
    expect(forms([home("fill")]).validate(check("Home", { lead: { phone: "555-0100" } }, [sent("Lead.Phone", ["lead", "phone"])], "json"))).toStrictEqual([]);
  });

  test("NewHolderAgrees: a holder that starts null is a new object of its type, filled with what the body sends", () => {
    expect(forms([deputyCard("Unnamed")]).validate(check("DeputyCard", { deputy: { phone: "555-0101" } }, [sent("Deputy.Phone", ["deputy", "phone"])]))).toStrictEqual([]);
    expect(forms([deputyCard("")]).validate(check("DeputyCard", { deputy: { phone: "555-0101" } }, [sent("Deputy.Phone", ["deputy", "phone"])])))
      .toStrictEqual([{ name: "", message: "Name the deputy." }]);
  });

  test("ChildSkipAgrees: a child validator's rules hold only while its member holds an object", () => {
    expect(forms([deputyCard("")]).validate(check("DeputyCard", {}))).toStrictEqual([]);
  });

  test("OnlyTheNamedValidator: a check runs the registration it names and no other", () => {
    const page = forms([supplies, intakeCard("replace")]);
    expect(page.validate(check("Supplies", { count: 3 }))).toStrictEqual([]);
    expect(() => page.validate(check("Deputy", {}))).toThrow(/^No live plan carries the rules of 'Deputy:default'\.$/);
  });

  test("OnlyTheNamedValidator: a registration leaves with the plan that carried it", () => {
    const page = new PageForms(new InputsTable(), new Page());
    const owner = new Visit();
    page.join(plan([supplies]), owner, read);
    owner.release();
    expect(() => page.validate(check("Supplies", {}))).toThrow(/^No live plan carries the rules of 'Supplies:default'\.$/);
  });

  test("Agreement: a date and a file the body sends are checked as the values they hold, never as holders of members", () => {
    const visit: Registration = {
      validator: "Visit", ruleSet: "default", start: { at: "0001-01-01T00:00:00", letter: null },
      scalars: [], objects: [], lists: [],
      rules: group([
        field(["at"], "Book a visit after November 1.", fn(value => value instanceof Date && value.getTime() > new Date(2026, 10, 1).getTime())),
        field(["letter"], "Attach the letter.", fn(value => value instanceof File)),
      ]),
    };
    const letter = new File(["Dear Ann"], "letter.txt", { type: "text/plain" });
    for (const format of ["multipart", "json"] as const) {
      const page = forms([visit]);
      expect(page.validate(check("Visit", { at: new Date(2026, 10, 2), letter }, [], format))).toStrictEqual([]);
      expect(page.validate(check("Visit", { at: new Date(2026, 9, 27), letter }, [], format))).toStrictEqual([{ name: "", message: "Book a visit after November 1." }]);
    }
  });

  test("A body with no members is checked on the model's start", () => {
    expect(forms([supplies]).validate(check("Supplies", {}, [], "none"))).toStrictEqual([{ name: "", message: "'Count' must be greater than '0'." }]);
  });

  test("SentOutsideAgrees: a body that binds no member is checked on the members its path and its query send", () => {
    for (const format of ["none", "text", "file"] as const) {
      const page = forms([bedNote]);
      expect(page.validate(check("BedNote", { ward: "Northeast" }, [], format))).toStrictEqual([{ name: "", message: "A ward's name holds 5 letters at most." }]);
      expect(page.validate(check("BedNote", { ward: "North" }, [], format))).toStrictEqual([]);
    }
  });

  test("OutsideEmptyStaysText: an empty text the query sends where the body binds no member reads null, and a member it sends nowhere reads its start", () => {
    const page = forms([bedNote]);
    expect(page.validate(check("BedNote", { shift: "" }, [], "none"))).toStrictEqual([{ name: "", message: "Name the shift." }]);
    expect(page.validate(check("BedNote", {}, [], "none"))).toStrictEqual([]);
  });
});

describe("when alis.form shows a check's failures where the plan places them", () => {
  const name = (): Sent => shown("Name", ["name"]);
  const carer = (): Sent => shown("Carers[0].Name", ["carers", 0, "name"]);
  const room = (): Sent => shown("Room", ["room"]);
  const refused = { type: "https://tools.ietf.org/html/rfc9110#section-15.5.1", title: "One or more validation errors occurred.", status: 400, errors: { Name: ["Name is taken", "Pick another name"], Room: ["Room is full"], Bed: ["Choose a bed."] } };

  test("FailureReachesSummary: every failure shows in the summary of the container the request sends, one no input sent among them", () => {
    const table = new InputsTable();
    const page = new Page();
    const nameInput = name();
    table.hold(nameInput);
    placed([intakeCard("replace")], table, page).validate(check("IntakeCard", { name: "", carers: [{}] }, [nameInput]));
    expect(Object.fromEntries(page.summaries)).toStrictEqual({ intake_summary: ["Name the resident.", "Name the carer."] });
    expect(page.notices).toStrictEqual([]);
  });

  test("FailureReachesSummary: a request that sends no container shows its failures in one notice of kind error, its messages joined by the plan's separator", () => {
    const table = new InputsTable();
    const page = new Page();
    const nameInput = name();
    table.hold(nameInput);
    placed([intakeCard("replace")], table, page).validate(check("IntakeCard", { name: "", carers: [{}] }, [nameInput], "urlEncoded", atTheNotice));
    expect(page.notices).toStrictEqual([["Name the resident. Name the carer.", "error"]]);
    expect(Object.fromEntries(page.summaries)).toStrictEqual({});
  });

  test("InlineOnlyAtTheSentInput: a failure shows at the message element of the input sent under its binding name, and beside no other", () => {
    const table = new InputsTable();
    const page = new Page();
    const [nameInput, carerInput, status] = [name(), carer(), shown("Status", ["status"])];
    table.hold(nameInput, carerInput, status);
    placed([intakeCard("replace")], table, page).validate(check("IntakeCard", { name: "", status: "", carers: [{ name: "" }] }, [nameInput, status, carerInput]));
    expect(Object.fromEntries(page.messages)).toStrictEqual({ input_Name_message: "Name the resident.", "input_Carers[0].Name_message": "Name the carer." });
  });

  test("InlineOnlyAtTheSentInput: an input whose binding names no message element shows its failure in the summary only", () => {
    const table = new InputsTable();
    const page = new Page();
    const ward = sent("Ward", ["ward"]);
    table.hold(ward);
    placed([wardCard], table, page).validate(check("WardCard", { ward: "" }, [ward]));
    expect(Object.fromEntries(page.messages)).toStrictEqual({});
    expect(Object.fromEntries(page.summaries)).toStrictEqual({ intake_summary: ["'Ward' must not be empty."] });
  });

  test("A check that passes clears the messages of the inputs it sent, empties its summary and shows no notice", () => {
    const table = new InputsTable();
    const page = new Page();
    const nameInput = name();
    table.hold(nameInput);
    const forms = placed([intakeCard("replace")], table, page);
    forms.validate(check("IntakeCard", { name: "" }, [nameInput]));
    forms.validate(check("IntakeCard", { name: "" }, [nameInput], "urlEncoded", atTheNotice));
    forms.validate(check("IntakeCard", { name: "Ann" }, [nameInput]));
    forms.validate(check("IntakeCard", { name: "Ann" }, [nameInput], "urlEncoded", atTheNotice));
    expect(Object.fromEntries(page.messages)).toStrictEqual({});
    expect(Object.fromEntries(page.summaries)).toStrictEqual({ intake_summary: [] });
    expect(page.notices).toStrictEqual([["Name the resident.", "error"]]);
  });

  test("ShowsFailures: a server's 400 shows as the browser's verdict, every message in the summary and the first of each key at the input sent under it", () => {
    const table = new InputsTable();
    const page = new Page();
    const [nameInput, roomInput] = [name(), room()];
    table.hold(nameInput, roomInput);
    new PageForms(table, page).showErrors(refused, [nameInput, roomInput], inTheSummary);
    expect(Object.fromEntries(page.messages)).toStrictEqual({ input_Name_message: "Name is taken", input_Room_message: "Room is full" });
    expect(Object.fromEntries(page.summaries)).toStrictEqual({ intake_summary: ["Name is taken", "Pick another name", "Room is full", "Choose a bed."] });
  });

  test("ShowsFailures: a server's 400 for a request that sends no container shows in one notice of kind error", () => {
    const table = new InputsTable();
    const page = new Page();
    const roomInput = room();
    table.hold(roomInput);
    new PageForms(table, page).showErrors(refused, [roomInput], atTheNotice);
    expect(page.notices).toStrictEqual([["Name is taken Pick another name Room is full Choose a bed.", "error"]]);
    expect(Object.fromEntries(page.messages)).toStrictEqual({ input_Room_message: "Room is full" });
  });

  test("InlineOnlyAtTheSentInput: an input the table no longer holds as it was sent, left or loaded again, shows nothing inline, and the summary shows every message", () => {
    const table = new InputsTable();
    const page = new Page();
    const [nameInput, roomInput] = [name(), room()];
    table.hold(room());
    new PageForms(table, page).showErrors(refused, [nameInput, roomInput], inTheSummary);
    expect(Object.fromEntries(page.messages)).toStrictEqual({});
    expect(Object.fromEntries(page.summaries)).toStrictEqual({ intake_summary: ["Name is taken", "Pick another name", "Room is full", "Choose a bed."] });
  });

  test("A body that is no validation problem stops the route that shows it", () => {
    const page = new PageForms(new InputsTable(), new Page());
    expect(() => { page.showErrors({ title: "Bad Request" }, [], inTheSummary); })
      .toThrow(/^The body ShowValidationErrors shows is no validation problem, \{"errors": \{"<binding name>": \["message"\]\}\}\.$/);
    expect(() => { page.showErrors({ errors: { Name: "Name is taken" } }, [], inTheSummary); })
      .toThrow(/^The body ShowValidationErrors shows is no validation problem, \{"errors": \{"<binding name>": \["message"\]\}\}\.$/);
  });
});

describe("when an input is checked live", () => {
  const admission: Registration = {
    validator: "Admission", ruleSet: "default", start: { name: null, room: null },
    scalars: [], objects: [], lists: [],
    rules: group([field(["name"], "Name is required."), field(["room"], "Pick a room.")]),
  };
  const ward: Registration = {
    validator: "Ward", ruleSet: "default", start: { name: null },
    scalars: [], objects: [], lists: [],
    rules: group([field(["name"], "Name the resident on the ward.")]),
  };
  const nameInput = shown("Name", ["name"]);
  const roomInput = shown("Room", ["room"]);
  const live = (validator: string, value: object, inputs: Sent[] = [nameInput, roomInput]): LiveCheck => ({ registration: validator + ":default", body: { format: "urlEncoded", value }, inputs });

  test("a live check shows the first failure of the input's own member at its input, once when two containers give it, and leaves the other members and the summary", () => {
    const table = new InputsTable();
    const page = new Page();
    page.showMessage(roomInput.entry, "Earlier");
    table.checksLive(nameInput.entry.id, live("Admission", { name: "", room: "" }), live("Admission", { name: "", room: "" }));
    placed([admission], table, page).checkLive(nameInput.entry);
    expect(Object.fromEntries(page.messages)).toStrictEqual({ input_Room_message: "Earlier", input_Name_message: "Name is required." });
    expect(Object.fromEntries(page.summaries)).toStrictEqual({});
  });

  test("two containers that give the member two messages show the first container's at its input", () => {
    const table = new InputsTable();
    const page = new Page();
    table.checksLive(nameInput.entry.id, live("Ward", { name: "" }, [nameInput]), live("Admission", { name: "", room: "Oak" }));
    placed([admission, ward], table, page).checkLive(nameInput.entry);
    expect(Object.fromEntries(page.messages)).toStrictEqual({ input_Name_message: "Name the resident on the ward." });
  });

  test("a live check the member passes clears its message", () => {
    const table = new InputsTable();
    const page = new Page();
    page.showMessage(nameInput.entry, "Name is required.");
    table.checksLive(nameInput.entry.id, live("Admission", { name: "Ann", room: "" }));
    placed([admission], table, page).checkLive(nameInput.entry);
    expect(Object.fromEntries(page.messages)).toStrictEqual({});
  });

  test("an input no live container holds keeps the message a request showed", () => {
    const page = new Page();
    page.showMessage(nameInput.entry, "Name is required.");
    placed([admission], new InputsTable(), page).checkLive(nameInput.entry);
    expect(Object.fromEntries(page.messages)).toStrictEqual({ input_Name_message: "Name is required." });
  });

  test("typing clears the input's message and no other", () => {
    const page = new Page();
    page.showMessage(nameInput.entry, "Name is required.");
    page.showMessage(roomInput.entry, "Pick a room.");
    placed([admission], new InputsTable(), page).clear(nameInput.entry);
    expect(Object.fromEntries(page.messages)).toStrictEqual({ input_Room_message: "Pick a room." });
  });

  test("a live check whose rules no live plan carries stops", () => {
    const table = new InputsTable();
    table.checksLive(nameInput.entry.id, live("Admission", { name: "" }));
    expect(() => { placed([], table, new Page()).checkLive(nameInput.entry); }).toThrow(/^No live plan carries the rules of 'Admission:default'\.$/);
  });
});
