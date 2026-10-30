// The plans the C# builders wrote (tests/Alis.Reactive.PlanTests, the goldens), run by this runtime
// against the plain DOM they name: the framework's own proof that a page authored in C# behaves,
// with no component library and no browser between the two.

import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { authored, mount, type } from "./support/authored";
import { server } from "./support/network";

// Each test runs a fresh runtime, as every suite here does.
let Owner: (typeof import("../objects/owner"))["Owner"];
let startOwner: (typeof import("../plans/start"))["startOwner"];

beforeEach(async () => {
  vi.resetModules();
  ({ Owner } = await import("../objects/owner"));
  ({ startOwner } = await import("../plans/start"));
  const { registerPlugin } = await import("../objects/plugins");
  registerPlugin("alis.document", (await import("../plugins/document")).documentPlugin);
  registerPlugin("alis.http", (await import("../plugins/http")).httpPlugin);
  registerPlugin("alis.model", (await import("../plugins/model")).modelPlugin);
  registerPlugin("alis.form", (await import("../plugins/form/form")).formPlugin);
  registerPlugin("alis.slot", (await import("../plugins/slot")).slotPlugin);
});

afterEach(() => {
  vi.unstubAllGlobals();
  document.body.innerHTML = "";
});

const input = (elements: Record<string, HTMLElement>, member: string): HTMLElement =>
  elements[Object.keys(elements).find(id => id.endsWith(`__${member}`))!]!;

describe("triggers", () => {
  it("a ready trigger runs at boot", () => {
    const plan = authored("Triggers", "a_ready_trigger_is_one_handler_of_the_document_run_at_boot");
    const { status } = mount(plan);
    startOwner(Owner.page(), [plan]);
    expect(status!.textContent).toBe("Round open");
  });

  it("a click runs the pipeline written for the element", () => {
    const plan = authored("Triggers", "a_click_on_a_declared_element_runs_the_pipeline_written_for_it");
    const { save, status } = mount(plan);
    startOwner(Owner.page(), [plan]);
    expect(status!.textContent).toBe("");
    save!.click();
    expect(status!.textContent).toBe("Saved");
  });

  it("an event dispatched with a payload is heard with it", () => {
    const plan = authored("Triggers", "an_event_dispatched_with_a_payload_is_heard_with_that_payload");
    const { room } = mount(plan);
    startOwner(Owner.page(), [plan]);
    expect(room!.textContent).toBe("214");
  });
});

describe("conditions", () => {
  it("the branch runs when the input's value meets its condition", () => {
    const plan = authored("Conditions", "a_branch_guards_its_pipeline_with_the_value_it_compares");
    const elements = mount(plan);
    type(input(elements, "Name"), "Helen");
    startOwner(Owner.page(), [plan]);
    expect(elements["greeting"]!.textContent).toBe("Hello Helen");
  });

  it("the branch stays silent when the value does not", () => {
    const plan = authored("Conditions", "a_branch_guards_its_pipeline_with_the_value_it_compares");
    const elements = mount(plan);
    type(input(elements, "Name"), "Tom");
    startOwner(Owner.page(), [plan]);
    expect(elements["greeting"]!.textContent).toBe("");
  });

  it("else-if and else follow in authored order", () => {
    const plan = authored("Conditions", "else_if_and_else_follow_in_authored_order_and_the_first_match_wins");
    const elements = mount(plan);
    type(input(elements, "Name"), "Tom");
    startOwner(Owner.page(), [plan]);
    expect(elements["greeting"]!.textContent).toBe("Hello Tom");
  });

  it("the else arm runs when no arm matched", () => {
    const plan = authored("Conditions", "else_if_and_else_follow_in_authored_order_and_the_first_match_wins");
    const elements = mount(plan);
    startOwner(Owner.page(), [plan]);
    expect(elements["greeting"]!.textContent).toBe("Hello");
  });

  it("a matching synchronous term spares the desk its question", async () => {
    const plan = authored("Conditions", "an_awaited_method_result_is_asked_only_when_its_term_is_reached");
    const elements = mount(plan);
    const isFree = vi.fn(async () => true);
    Object.assign(elements["desk"]!, { isFree });
    type(input(elements, "Name"), "Helen");
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(elements["status"]!.textContent).toBe("Admit"));
    expect(isFree).not.toHaveBeenCalled();
  });

  it("the desk is asked, with its argument, when the name did not match", async () => {
    const plan = authored("Conditions", "an_awaited_method_result_is_asked_only_when_its_term_is_reached");
    const elements = mount(plan);
    const isFree = vi.fn(async () => true);
    Object.assign(elements["desk"]!, { isFree });
    type(input(elements, "Name"), "Tom");
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(elements["status"]!.textContent).toBe("Admit"));
    expect(isFree).toHaveBeenCalledWith("214");
  });
});

describe("requests", () => {
  it("a GET fills the route placeholder and sends the rest as the query", async () => {
    const plan = authored("Requests", "a_get_fills_the_route_placeholder_from_the_member_named_like_it_and_sends_the_rest_as_the_query");
    const sent = server();
    mount(plan);
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(sent).toHaveLength(1));
    expect(sent[0]).toMatchObject({ method: "GET", url: "/residents/1042?Wing=Garden" });
  });

  // The view lays the form out: the inputs inside the element the request's Form(formId) names.
  function formHolding(formId: string, inputs: HTMLElement[]): void {
    const form = document.createElement("form");
    form.id = formId;
    document.body.appendChild(form);
    for (const held of inputs) form.appendChild(held);
  }

  it("a POST of the form sends the input's value and reads the success body", async () => {
    const plan = authored("Requests", "a_post_of_the_form_reads_the_success_body_where_the_route_names_it");
    const sent = server({ "/admit": { json: { confirmation: "ADM-7" } } });
    const elements = mount(plan);
    formHolding("admission", [input(elements, "Name")]);
    type(input(elements, "Name"), "Helen");
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(elements["status"]!.textContent).toBe("ADM-7"));
    expect(sent[0]).toMatchObject({ method: "POST", url: "/admit", body: { Name: "Helen" } });
  });

  it("the form posts each input converted to the shape its member binds with", async () => {
    const plan = authored("Shapes", "an_input_binds_under_the_shape_mvc_binds_its_member_with");
    const sent = server();
    const elements = mount(plan);
    formHolding("vitals", ["Weight", "MeasuredOn", "RecordedAt", "Notes"].map(member => input(elements, member)));
    type(input(elements, "Weight"), "70");
    type(input(elements, "MeasuredOn"), "2026-10-27");
    type(input(elements, "RecordedAt"), "2026-10-27T05:30:00Z");
    type(input(elements, "Notes"), "walker");
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(sent).toHaveLength(1));
    expect(sent[0]!.body).toEqual({ Weight: 70, MeasuredOn: "2026-10-27", RecordedAt: "2026-10-27T05:30:00.000Z", Notes: ["walker"] });
  });
});

describe("validation", () => {
  // The view lays the form out: the input inside the container the plan validates, an error slot
  // beside it, and the form's summary.
  function layOut(elements: Record<string, HTMLElement>): { error: HTMLElement } {
    const container = elements["admission"]!;
    const name = input(elements, "Name");
    container.appendChild(name);
    const error = document.createElement("span");
    error.id = `${name.id}_error`;
    container.appendChild(error);
    const summary = document.createElement("div");
    summary.id = `${container.id}_validation_summary`;
    document.body.append(summary);
    return { error };
  }

  it("a required input left empty keeps the request from being sent and shows the rule's message", async () => {
    const plan = authored("Validation", "a_check_the_browser_can_make_lands_on_the_input_that_renders_the_member");
    const sent = server();
    const elements = mount(plan);
    const { error } = layOut(elements);
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(error.textContent).toBe("Name the resident."));
    expect(input(elements, "Name").classList.contains("alis-has-error")).toBe(true);
    expect(sent).toHaveLength(0);
  });

  it("a required input filled in lets the request go", async () => {
    const plan = authored("Validation", "a_check_the_browser_can_make_lands_on_the_input_that_renders_the_member");
    const sent = server();
    const elements = mount(plan);
    const { error } = layOut(elements);
    type(input(elements, "Name"), "Helen");
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(sent).toHaveLength(1));
    expect(sent[0]).toMatchObject({ method: "POST", url: "/admit", body: { Name: "Helen" } });
    expect(error.textContent).toBe("");
  });
});

describe("partials", () => {
  // The card the server answers with: a partial rendered for the next of kin, its input filled, its plan beside it.
  function card(): string {
    const partial = authored("Partials", "a_partial_rendered_for_a_member_binds_its_input_under_the_member_prefix");
    const inputId = Object.keys(partial.objects).find(id => id.endsWith("__NextOfKin_Name"))!;
    return `<input id="${inputId}" value="Tom"><script type="application/json" data-reactive-plan>${JSON.stringify(partial)}</script>`;
  }

  it("a response loaded into a slot starts the partial's plan, which binds under its prefix", async () => {
    const plan = authored("Partials", "a_response_loads_into_a_slot_and_a_click_unloads_it");
    const sent = server({ "/card": { html: card() } });
    const { drawer } = mount(plan);
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(sent).toHaveLength(2));
    expect(drawer!.querySelector("input")).not.toBeNull();
    expect(sent[1]).toMatchObject({ method: "POST", url: "/contact", body: { NextOfKin: { Name: "Tom" } } });
  });

  it("a click unloads the slot", async () => {
    const plan = authored("Partials", "a_response_loads_into_a_slot_and_a_click_unloads_it");
    const sent = server({ "/card": { html: card() } });
    const { drawer, close } = mount(plan);
    startOwner(Owner.page(), [plan]);
    await vi.waitFor(() => expect(sent).toHaveLength(2));
    close!.click();
    expect(drawer!.innerHTML).toBe("");
  });
});
