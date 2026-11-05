import { afterEach, describe, expect, it } from "vitest";
import { requestJsonBody, requestWireText } from "../../../execution/requests/request-wire-format";

const zoneOfTheRun = process.env.TZ;

afterEach(() => {
  process.env.TZ = zoneOfTheRun;
});

function wireTextIn(zone: string, date: Date): string {
  process.env.TZ = zone;
  const text = requestWireText(date);
  if (!text.ok) throw new Error(text.error);
  return text.value;
}

describe("a date on the wire", () => {
  it.each([
    ["UTC", "2026-11-04T17:00:00.000Z"],
    ["America/Los_Angeles", "2026-11-04T09:00:00.000-08:00"],
    ["America/Chicago", "2026-11-04T11:00:00.000-06:00"],
    ["Asia/Kolkata", "2026-11-04T22:30:00.000+05:30"],
    ["Asia/Kathmandu", "2026-11-04T22:45:00.000+05:45"],
    ["Asia/Shanghai", "2026-11-05T01:00:00.000+08:00"],
  ])("in %s is the clock the user saw with that clock's offset", (zone, expected) => {
    const instant = new Date(Date.UTC(2026, 10, 4, 17, 0, 0));

    const text = wireTextIn(zone, instant);

    expect(text).toBe(expected);
    expect(Date.parse(text)).toBe(instant.getTime());
  });

  it("takes the offset its own day had, either side of a daylight-saving change", () => {
    expect(wireTextIn("America/Los_Angeles", new Date(Date.UTC(2027, 0, 13, 17, 0, 0))))
      .toBe("2027-01-13T09:00:00.000-08:00");
    expect(wireTextIn("America/Los_Angeles", new Date(Date.UTC(2027, 6, 14, 16, 0, 0))))
      .toBe("2027-07-14T09:00:00.000-07:00");
  });

  it("keeps a local midnight on its own day east of UTC", () => {
    process.env.TZ = "Asia/Kolkata";
    const pickedDay = new Date(2026, 10, 4);

    expect(requestWireText(pickedDay)).toEqual({ ok: true, value: "2026-11-04T00:00:00.000+05:30" });
  });

  it("writes every date nested in a JSON body the same way, leaving the values it sends as they were", () => {
    process.env.TZ = "America/Los_Angeles";
    const session = { subject: "Dementia care", startTime: new Date(2027, 0, 11, 9, 0), endTime: new Date(2027, 0, 11, 10, 30) };
    const body = { sessions: [session], publishedOn: new Date(2027, 0, 8) };

    expect(JSON.parse(requestJsonBody(body))).toEqual({
      sessions: [{ subject: "Dementia care", startTime: "2027-01-11T09:00:00.000-08:00", endTime: "2027-01-11T10:30:00.000-08:00" }],
      publishedOn: "2027-01-08T00:00:00.000-08:00",
    });
    expect(session.startTime).toBeInstanceOf(Date);
  });

  it("writes an invalid date in a JSON body as null, as JSON does", () => {
    expect(requestJsonBody({ when: new Date(Number.NaN) })).toBe('{"when":null}');
  });

  it("leaves text, numbers and booleans as they read", () => {
    expect(requestWireText("Ana Reyes")).toEqual({ ok: true, value: "Ana Reyes" });
    expect(requestWireText(42)).toEqual({ ok: true, value: "42" });
    expect(requestJsonBody({ count: 2, done: false, note: "2026-11-04" })).toBe('{"count":2,"done":false,"note":"2026-11-04"}');
  });
});
