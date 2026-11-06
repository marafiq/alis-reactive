import { createRequire } from "node:module";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";

const require = createRequire(import.meta.url);
const root = dirname(require.resolve("@syncfusion/ej2-base/package.json"));
const meta = JSON.parse(readFileSync(join(root, "package.json"), "utf8"));
const { Base } = require(join(root, "dist", "ej2-base.umd.min.js"));

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

const host = element();
const first = new Probe({}, host);
first.addInstance();
const subscribed = host.ej2_instances[0];
first.destroy();

console.log(`ej2-base: ${meta.version}`);
console.log(`ej2-destroy-removes-instance: ${host.ej2_instances[0] === subscribed ? "kept" : "removed"}`);
console.log(`ej2-destroy-sets-is-destroyed: ${first.isDestroyed}`);
