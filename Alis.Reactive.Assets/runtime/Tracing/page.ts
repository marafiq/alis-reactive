import { plugin, provides } from "../Plugins/index";
import { randomIds, wallClock } from "./ids";
import { Port } from "./port";
import type { SinkLookup, TraceSink } from "./spans";

const sinkName = "trace";

function isASink(held: unknown): held is TraceSink {
  return typeof held === "object" && held !== null && "ended" in held && typeof held.ended === "function";
}

function theSinkOfThePage(): SinkLookup {
  if (!provides(sinkName)) return { kind: "none" };
  const held = plugin(sinkName, "");
  if (!isASink(held)) throw new Error("The plugin '" + sinkName + "' has no ended(span) method.");
  return { kind: "registered", sink: held };
}

export const tracing = new Port(theSinkOfThePage, randomIds, wallClock, console);
