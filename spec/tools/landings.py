import json
import sys
from collections import Counter
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))

import check


def spec_tests(app):
    constants, taken = check.promise_constants(), Counter()

    def promise(group):
        taken[group] += 1
        name, value, _ = constants[group][taken[group] - 1]
        return f"{group}.{name}", value

    for entry in app["placements"]:
        yield promise(check.csharp_name(entry["feature"])), entry
    for entry in app["cells"]:
        yield promise("Cells" if "cell" in entry else "OwnCells"), entry
    for entry in app["harnessTests"]:
        yield promise("Harness"), entry


def landing(app, units, entry, home, module_unit, brought):
    if "unit" in entry:
        return entry["unit"]
    start = home[entry["feature"]] if "feature" in entry else module_unit["Vendors" if "cell" in entry else "FusionComponents"]
    return units[check.rewrite_landing(app, module_unit, start, entry, brought)]["id"]


def main():
    units = check.rewrite_units((check.SPEC / "REWRITE.md").read_text())
    modules = {feature["id"]: path.parent.name for path in check.feature_files() for feature in json.loads(path.read_text()).get("features", [])}
    app = json.loads((check.SPEC / "Reference" / "app.json").read_text())
    home = check.unit_homes(units)
    module_unit, brought = check.module_units(home, modules), check.door_units(units)
    fresh, twin = check.fresh_app_hosts(app["screens"]), {host["id"] for host in app["hosts"] if host["framework"] == "net48"}
    for (constant, value), entry in spec_tests(app):
        unit = landing(app, units, entry, home, module_unit, brought)
        for host in entry["hosts"]:
            served = "fresh" if host in fresh else "twin" if host in twin else "pool"
            print("\t".join([value, constant, unit, entry.get("screen", ""), host, served]))
    return 0


if __name__ == "__main__":
    sys.exit(main())
