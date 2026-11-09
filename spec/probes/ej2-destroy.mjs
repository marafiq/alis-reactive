import { createRequire } from "node:module";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";

const require = createRequire(import.meta.url);
const root = dirname(require.resolve("@syncfusion/ej2-base/package.json"));
const meta = JSON.parse(readFileSync(join(root, "package.json"), "utf8"));
const { Base, Event } = require(join(root, "dist", "ej2-base.umd.min.js"));

function element() {
  const classes = new Set();
  return {
    classList: {
      add: (...names) => names.forEach(name => classes.add(name)),
      remove: (...names) => names.forEach(name => classes.delete(name)),
      contains: name => classes.has(name),
    },
  };
}

class Probe extends Base {
  getModuleName() { return "probe"; }
  onPropertyChanged() {}
  getPersistData() { return ""; }
}

class Alarmed extends Probe {}
Event()(Alarmed.prototype, "alarm");

function putBackAfterDestroy() {
  const alarmed = new Alarmed({}, element());
  const previous = alarmed.alarm;
  alarmed.alarm = () => {};
  alarmed.destroy();
  try {
    alarmed.alarm = previous;
    return "ran";
  } catch (error) {
    return error.constructor.name;
  }
}

const host = element();
const first = new Probe({}, host);
first.addInstance();
const subscribed = host.ej2_instances[0];
first.destroy();

console.log(`ej2-base: ${meta.version}`);
console.log(`ej2-destroy-removes-instance: ${host.ej2_instances[0] === subscribed ? "kept" : "removed"}`);
console.log(`ej2-destroy-sets-is-destroyed: ${first.isDestroyed}`);
console.log(`ej2-event-put-back-after-destroy: ${putBackAfterDestroy()}`);
