import glob
import json
import os
import re
import subprocess
from pathlib import Path

os.chdir(Path(__file__).resolve().parent.parent)
src = Path("tests/Alis.Reactive.Conformance.World/Promises.g.cs").read_text()
feature = {}
for path in glob.glob("spec/*/features.json"):
    for entry in json.loads(Path(path).read_text())["features"]:
        feature[entry["id"].replace(".", "_")] = (path.split("/")[1], entry["summary"].split(":")[0][:60])
tests = subprocess.run(["git", "ls-files", "tests/Alis.Reactive.Conformance.PlaywrightTests"], stdin=subprocess.DEVNULL, capture_output=True, text=True, check=True).stdout.splitlines()
named = set()
for path in tests:
    if path.endswith(".cs"):
        named.update(re.findall(r"Promises\.(M\d+_F\d+)\.(\w+)", Path(path).read_text()))
holder, placed, waiting = None, 0, []
for line in src.split("\n"):
    found = re.search(r"class (\w+)", line)
    if found:
        holder = found.group(1)
    constant = re.search(r"const string (\w+)\s*=", line)
    if constant and holder in feature:
        placed += 1
        if (holder, constant.group(1)) not in named:
            waiting.append((feature[holder][0], feature[holder][1], holder + "." + constant.group(1)))


def hits(pattern, paths):
    result = subprocess.run(["git", "grep", "-n", "-F", pattern, "--", *paths], stdin=subprocess.DEVNULL, capture_output=True, text=True)
    if result.returncode not in (0, 1):
        raise SystemExit(result.stderr)
    return [line.split(":", 2) for line in result.stdout.splitlines()]


runtime = [f"{path}:{number}" for path, number, text in hits("notBuilt(", ["Alis.Reactive.Assets/runtime/*.ts"])
           if not path.endswith(".test.ts") and "throw notBuilt(" in text]
doors = [f"{path}:{number}" for path, number, text in hits("NotImplementedException", ["Alis.Reactive/*.cs", "Alis.Reactive.Native/*.cs", "Alis.Reactive.Fusion/*.cs"])
         if "throw new " in text]
print(f"open: {len(waiting)} promises with no browser test, {len(runtime)} runtime cases not built, {len(doors)} doors not implemented")
print(f"feature promises: {placed}; named by a browser test: {placed - len(waiting)}; with no browser test: {len(waiting)}")
for module, summary, constant in sorted(waiting):
    print(f"  {module:22} {summary:44} {constant}")
print("runtime cases not built:")
for location in runtime:
    print(location)
print("doors not implemented:")
for location in doors:
    print(location)
