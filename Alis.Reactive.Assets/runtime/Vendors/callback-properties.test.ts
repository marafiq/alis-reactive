import { describe, expect, test } from "vitest";
import { subscribe } from "./index";

function ring(bell: { onRaised: unknown }, ...args: unknown[]): unknown {
  const callback = bell.onRaised;
  if (typeof callback !== "function") throw new Error("The bell has no callback.");
  return Reflect.apply(callback, bell, args);
}

function vendorFailure(value: unknown): never {
  throw value;
}

function thrownBy(action: () => unknown): unknown {
  try {
    action();
    return "returned";
  } catch (error) {
    return error;
  }
}

describe("when listeners share a callback property", () => {
  test("EveryHearerHearsOnceInOrder: the own callback precedes every listener, including two uses of one function", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    const repeated = (): void => {
      heard.push("page");
    };
    subscribe(bell, { kind: "assignment" }, "onRaised", repeated);
    subscribe(bell, { kind: "assignment" }, "onRaised", repeated);
    subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("card");
    });

    ring(bell);

    expect(heard).toStrictEqual(["own", "page", "page", "card"]);
  });

  test("EveryHearerHearsOnceInOrder: a middle release leaves its neighbours and a reload joins last", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("page");
    });
    const release = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("card");
    });
    subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("notes");
    });

    release();
    release();
    ring(bell);
    subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("reloaded");
    });
    ring(bell);

    expect(heard).toStrictEqual(["own", "page", "notes", "own", "page", "notes", "reloaded"]);
  });

  test("ALiveHandlerKeepsTheFrameworksFunction: later listeners and a middle release do not assign again", () => {
    const heard: string[] = [];
    const own = (): void => {
      heard.push("own");
    };
    let held: unknown = own;
    let assignments = 0;
    const bell = {
      get onRaised(): unknown { return held; },
      set onRaised(value: unknown) { assignments += 1; held = value; },
    };
    const first = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("first");
    });
    const installed = bell.onRaised;
    const second = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("second");
    });

    first();
    ring(bell);

    expect(assignments).toBe(1);
    expect(bell.onRaised === installed).toBe(true);
    expect(heard).toStrictEqual(["own", "second"]);
    second();
    expect(assignments).toBe(2);
    expect(bell.onRaised === own).toBe(true);
  });

  test("EveryHearerHearsOnceInOrder: the own callback keeps the vendor this, arguments and result", () => {
    const heard: unknown[][] = [];
    const bell = {
      room: "7",
      onRaised(this: { room: string }, ...args: unknown[]): string {
        heard.push(["own", this.room, ...args]);
        return "own answer";
      },
    };
    subscribe(bell, { kind: "assignment" }, "onRaised", (...args) => {
      heard.push(["listener", ...args]);
      return "ignored answer";
    });

    const result = bell.onRaised.call({ room: "12" }, "water", 3, null);

    expect(heard).toStrictEqual([["own", "12", "water", 3, null], ["listener", "water", 3, null]]);
    expect(result).toBe("own answer");
  });

  test("AThrowSkipsNoHandler: the own error reaches the vendor unchanged after every listener", () => {
    const heard: string[] = [];
    const failure = new Error("The bell failed.");
    const bell = {
      onRaised(): never {
        heard.push("own");
        throw failure;
      },
    };
    subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("first");
    });
    subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("second");
    });

    const caught = thrownBy(() => ring(bell));

    expect(caught === failure).toBe(true);
    expect(heard).toStrictEqual(["own", "first", "second"]);
  });

  test("AThrowSkipsNoHandler: even a thrown undefined is rethrown after the listeners", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => vendorFailure(undefined) };
    subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("heard");
    });

    expect(thrownBy(() => ring(bell))).toBeUndefined();
    expect(heard).toStrictEqual(["heard"]);
  });

  test("TheLastReleasePutsBackItsValue: an undefined property answers nothing and is restored", () => {
    const heard: string[] = [];
    const bell = { onRaised: undefined };
    const release = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("heard");
      return "ignored";
    });

    expect(ring(bell)).toBeUndefined();
    release();

    expect(heard).toStrictEqual(["heard"]);
    expect(bell.onRaised).toBeUndefined();
  });

  test("TheLastReleasePutsBackItsValue: a nonfunction property answers nothing and is restored exactly", () => {
    const heard: string[] = [];
    const bell = { onRaised: null };
    const release = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("heard");
    });

    expect(ring(bell)).toBeUndefined();
    release();

    expect(heard).toStrictEqual(["heard"]);
    expect(bell.onRaised).toBeNull();
  });

  test("EveryHearerHearsOnceInOrder: each object and each property owns its own chain", () => {
    const heard: string[] = [];
    const first = {
      onRaised: () => heard.push("first own"),
      onQuiet: () => heard.push("quiet own"),
    };
    const second = { onRaised: () => heard.push("second own") };
    const release = subscribe(first, { kind: "assignment" }, "onRaised", () => {
      heard.push("first listener");
    });
    subscribe(first, { kind: "assignment" }, "onQuiet", () => {
      heard.push("quiet listener");
    });
    subscribe(second, { kind: "assignment" }, "onRaised", () => {
      heard.push("second listener");
    });

    release();
    ring(first);
    first.onQuiet();
    ring(second);

    expect(heard).toStrictEqual(["first own", "quiet own", "quiet listener", "second own", "second listener"]);
  });

  test("TheLastReleasePutsBackItsValue: an old release leaves a script assignment intact", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    const release = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("old");
    });
    ring(bell);
    bell.onRaised = () => heard.push("script");

    release();
    ring(bell);

    expect(heard).toStrictEqual(["own", "old", "script"]);
  });

  test("EveryHearerHearsOnceInOrder: a listener after a script assignment hears only the script and new listeners", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    const old = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("old");
    });
    bell.onRaised = () => heard.push("script");
    const recent = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("recent");
    });

    old();
    ring(bell);
    recent();
    ring(bell);

    expect(heard).toStrictEqual(["script", "recent", "script"]);
  });

  test("TheLastReleasePutsBackItsValue: the next first listener keeps the value held after the prior last release", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    const first = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("first");
    });

    first();
    const second = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("second");
    });
    ring(bell);
    second();
    ring(bell);

    expect(heard).toStrictEqual(["own", "second", "own"]);
  });

  test("ARefusedAssignmentStartsNoHandler: an absent callback property is not created", () => {
    const bell = { room: "7" };

    expect(() => subscribe(bell, { kind: "assignment" }, "onRaised", () => undefined))
      .toThrow(expect.objectContaining({ name: "Stop", message: "The callback property 'onRaised' is not on the object that should hold it." }));
    expect(Object.keys(bell)).toStrictEqual(["room"]);
  });

  test("ARefusedAssignmentStartsNoHandler: a frozen object keeps its own callback", () => {
    const heard: string[] = [];
    const bell = Object.freeze({ onRaised: () => heard.push("own") });

    expect(() => subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("refused");
    })).toThrow(expect.objectContaining({ name: "Stop", message: "The object refused the callback property 'onRaised'." }));
    ring(bell);

    expect(heard).toStrictEqual(["own"]);
  });

  test("ARefusedAssignmentStartsNoHandler: a nonwritable callback keeps its own value", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    Object.defineProperty(bell, "onRaised", { writable: false });

    expect(() => subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("refused");
    })).toThrow(expect.objectContaining({ name: "Stop", message: "The object refused the callback property 'onRaised'." }));
    ring(bell);

    expect(heard).toStrictEqual(["own"]);
  });

  test("ARefusedAssignmentStartsNoHandler: a getter with no setter keeps its own callback", () => {
    const heard: string[] = [];
    const own = (): void => { heard.push("own"); };
    const bell = { get onRaised(): unknown { return own; } };

    expect(() => subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("refused");
    })).toThrow(expect.objectContaining({ name: "Stop", message: "The object refused the callback property 'onRaised'." }));
    ring(bell);

    expect(heard).toStrictEqual(["own"]);
  });

  test("ARefusedAssignmentStartsNoHandler: a throwing setter leaves no listener for a later accepted assignment", () => {
    const heard: string[] = [];
    let held: unknown = (): void => { heard.push("own"); };
    const bell = {
      get onRaised(): unknown { return held; },
      set onRaised(value: unknown) { throw new Error("The bell rejected " + typeof value); },
    };

    expect(() => subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("refused");
    })).toThrow(expect.objectContaining({ name: "Stop", message: "The object refused the callback property 'onRaised'." }));
    Object.defineProperty(bell, "onRaised", { set(value: unknown) { held = value; } });
    const release = subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("accepted");
    });
    ring(bell);
    release();
    ring(bell);

    expect(heard).toStrictEqual(["own", "accepted", "own"]);
  });

  test("ARefusedAssignmentStartsNoHandler: a setter that discards the function is a refusal", () => {
    const heard: string[] = [];
    const own = (): void => { heard.push("own"); };
    const bell = {
      get onRaised(): unknown { return own; },
      set onRaised(value: unknown) { heard.push("discarded " + typeof value); },
    };

    expect(() => subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("refused");
    })).toThrow(expect.objectContaining({ name: "Stop", message: "The object refused the callback property 'onRaised'." }));
    ring(bell);

    expect(heard).toStrictEqual(["discarded function", "own"]);
  });

  test("ALiveHandlerKeepsTheFrameworksFunction: a frozen installed function accepts another listener without assignment", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("first"); });
    Object.freeze(bell);

    subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("second"); });
    ring(bell);

    expect(heard).toStrictEqual(["own", "first", "second"]);
  });

  test("TheLastReleasePutsBackItsValue: a refused put-back leaves the own callback and no released listener", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    const first = subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("first"); });
    const last = subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("last"); });
    Object.freeze(bell);

    first();
    ring(bell);
    expect(last).toThrow(expect.objectContaining({ name: "Stop", message: "The object refused the callback property 'onRaised'." }));
    last();
    ring(bell);
    subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("new"); });
    ring(bell);

    expect(heard).toStrictEqual(["own", "last", "own", "own", "new"]);
  });

  test("TheLastReleasePutsBackItsValue: a setter that throws on put-back leaves no released listener", () => {
    const heard: string[] = [];
    const own = (): void => { heard.push("own"); };
    let held: unknown = own;
    const bell = {
      get onRaised(): unknown { return held; },
      set onRaised(value: unknown) {
        if (value === own) throw new Error("The bell refused put-back.");
        held = value;
      },
    };
    const release = subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("listener"); });
    ring(bell);

    expect(release).toThrow(expect.objectContaining({ name: "Stop", message: "The object refused the callback property 'onRaised'." }));
    ring(bell);

    expect(heard).toStrictEqual(["own", "listener", "own"]);
  });

  test("EveryHearerHearsOnceInOrder: a listener released by the own callback is inactive in that same call", () => {
    const heard: string[] = [];
    const releases: (() => void)[] = [];
    const bell = {
      onRaised(): void {
        heard.push("own");
        releases.forEach((release) => { release(); });
      },
    };
    releases.push(subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("released"); }));
    subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("live"); });

    ring(bell);

    expect(heard).toStrictEqual(["own", "live"]);
  });

  test("EveryHearerHearsOnceInOrder: a listener added while the property's function runs hears the next call alone", () => {
    const heard: string[] = [];
    const bell = { onRaised: () => heard.push("own") };
    let added = false;
    subscribe(bell, { kind: "assignment" }, "onRaised", () => {
      heard.push("first");
      if (added) return;
      added = true;
      subscribe(bell, { kind: "assignment" }, "onRaised", () => {
        heard.push("joined");
      });
    });

    ring(bell);
    ring(bell);

    expect(heard).toStrictEqual(["own", "first", "own", "first", "joined"]);
  });

  test("TheLastReleasePutsBackItsValue: a put-back that calls the framework function hears no released listener", () => {
    const heard: string[] = [];
    const own = (): void => { heard.push("own"); };
    let held: unknown = own;
    const bell = {
      get onRaised(): unknown { return held; },
      set onRaised(value: unknown) {
        if (value === own) ring(bell);
        held = value;
      },
    };
    const release = subscribe(bell, { kind: "assignment" }, "onRaised", () => { heard.push("listener"); });
    ring(bell);

    release();

    expect(heard).toStrictEqual(["own", "listener", "own"]);
  });
});
