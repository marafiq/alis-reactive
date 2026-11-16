import type { Owner } from "../OwnershipAndState/index";
import { assertNever, type ElementLocator, type ListenStatement, type LocatedRelease, type Release } from "../Schema/index";
import { got, Stop, value } from "../ValuesAndSources/index";
import { subscribe, type Listener, type Unsubscribe } from "../Vendors/index";
import { bind, isAnObject, type Context } from "./context";
import { evaluate } from "./expressions";
import { Lane } from "./lane";
import { elementOf, heldBy, type ElementFor, type StillHeld } from "./objects";

const listened = new WeakMap<Owner, WeakMap<ListenStatement, WeakSet<object>>>();

type Told = (stopped: Stop, outcome: string) => Stop;

export function listenTo(statement: ListenStatement, context: Context, elementFor: ElementFor): void {
  const told = toldOf(statement, context);
  try {
    listens(statement, context, elementFor, told);
  } catch (stopped) {
    if (!(stopped instanceof Stop)) throw stopped;
    throw told(stopped, "did not start");
  }
}

function listens(statement: ListenStatement, context: Context, elementFor: ElementFor, told: Told): void {
  const read = evaluate(statement.object, context);
  if (read.kind === "nothing" || !isAnObject(read.value)) throw new Stop("The object that raises '" + statement.event + "' is not on the page.");
  const object = read.value;
  const objects = objectsOf(statement, context.owner);
  if (objects.has(object)) return;
  const stillHeld = releaseOf(statement.release, object, context, elementFor);
  const hearing = new Hearing(statement, context, object);
  objects.add(object);
  const weak = new WeakRef(hearing);
  context.owner.runs(ends(weak));
  context.owner.listens(releases(weak, new WeakRef(object), stillHeld, telling(remover(hearing, statement.listen.kind === "returned"), told)));
}

function toldOf(statement: ListenStatement, context: Context): Told {
  const release = statement.release;
  switch (release.kind) {
    case "living":
    case "held":
      return (stopped) => stopped;
    case "located": {
      const locator = locatorOf(release, context);
      return (stopped, outcome) => new Stop("The listen to '" + statement.event + "' on the element '" + locator.id + "' of the vendor '" + locator.vendor + "' " + outcome + ": " + stopped.reason);
    }
    default:
      return assertNever(release);
  }
}

function telling(remove: Unsubscribe, told: Told): Unsubscribe {
  return () => {
    try {
      remove();
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      throw told(stopped, "was not removed");
    }
  };
}

function objectsOf(statement: ListenStatement, owner: Owner): WeakSet<object> {
  let statements = listened.get(owner);
  if (statements === undefined) {
    statements = new WeakMap();
    listened.set(owner, statements);
  }
  let objects = statements.get(statement);
  if (objects === undefined) {
    objects = new WeakSet();
    statements.set(statement, objects);
  }
  return objects;
}

class Hearing {
  readonly lane: Lane;
  readonly remove: Unsubscribe;
  released = false;

  constructor(statement: ListenStatement, context: Context, object: object) {
    this.lane = new Lane(statement.policy, statement.do, context.owner);
    const receive: Listener = (...args) => {
      if (this.released) return;
      try {
        const payload = got(value(args), statement.payload, statement.members);
        this.lane.raise(bind(bind(context, statement.payloadName, payload), statement.argumentsName, value(args)));
      } catch (stopped) {
        if (!(stopped instanceof Stop)) throw stopped;
        console.error(stopped.reason);
      }
    };
    this.remove = subscribe(object, statement.listen, statement.event, receive);
  }
}

function ends(weak: WeakRef<Hearing>): () => void {
  return () => {
    weak.deref()?.lane.dispose();
  };
}

function remover(hearing: Hearing, returned: boolean): Unsubscribe {
  if (returned) return hearing.remove;
  return removesWeakly(new WeakRef(hearing));
}

function removesWeakly(weak: WeakRef<Hearing>): Unsubscribe {
  return () => {
    weak.deref()?.remove();
  };
}

function releases(weak: WeakRef<Hearing>, object: WeakRef<object>, stillHeld: StillHeld, remove: Unsubscribe): Unsubscribe {
  return () => {
    const hearing = weak.deref();
    if (hearing === undefined || hearing.released) return;
    hearing.released = true;
    const live = object.deref();
    if (live !== undefined && stillHeld(live)) remove();
  };
}

function releaseOf(release: Release, object: object, context: Context, elementFor: ElementFor): StillHeld {
  switch (release.kind) {
    case "living":
      return () => true;
    case "held": {
      const element = got(value(object), release.element, "required");
      if (element.kind === "nothing" || !isAnObject(element.value)) throw new Stop("The listened component holds no element.");
      return heldBy(element.value, release.vendor, context);
    }
    case "located": {
      const locator = locatorOf(release, context);
      return heldBy(elementOf(locator.id, elementFor), locator.vendor, context);
    }
    default:
      return assertNever(release);
  }
}

function locatorOf(release: LocatedRelease, context: Context): ElementLocator {
  return context.plan.objects[release.id] as ElementLocator;
}
