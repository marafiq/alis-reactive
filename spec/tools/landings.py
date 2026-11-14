import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))

import check


def main():
    units = check.rewrite_units((check.SPEC / "REWRITE.md").read_text())
    modules = {feature["id"]: path.parent.name for path in check.feature_files() for feature in json.loads(path.read_text()).get("features", [])}
    app = json.loads((check.SPEC / "Reference" / "app.json").read_text())
    home = check.unit_homes(units)
    module_unit, brought = check.module_units(home, modules), check.door_units(units)
    fresh, twin = check.fresh_app_hosts(app["screens"]), {host["id"] for host in app["hosts"] if host["framework"] == "net48"}
    doors = check.page_doors()
    for (constant, value), entry in check.spec_tests(app):
        unit = entry["unit"] if "unit" in entry else units[check.test_place(app, entry, check.page_modules(doors, value), home, module_unit, brought)]["id"]
        for host in entry["hosts"]:
            served = "fresh" if host in fresh else "twin" if host in twin else "pool"
            print("\t".join([value, constant, unit, entry.get("screen", ""), host, served]))
    return 0


if __name__ == "__main__":
    sys.exit(main())
