import sys
import xml.etree.ElementTree as ET
from pathlib import Path

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}


def outcomes(run_folder):
    root = ET.parse(f"{run_folder}/run.trx").getroot()
    names = {}
    for test in root.iterfind(".//t:TestDefinitions/t:UnitTest", NS):
        method = test.find("t:TestMethod", NS)
        names[test.get("id")] = f"{method.get('className')}.{method.get('name')}"
    result = {}
    for run in root.iterfind(".//t:Results/t:UnitTestResult", NS):
        key = names[run.get("testId")]
        if key in result:
            raise SystemExit(f"two results for {key} in {run_folder}")
        result[key] = run.get("outcome")
    if not result:
        raise SystemExit(f"no result in {run_folder}")
    return result


def retired_lines(retired_file):
    retired = {}
    for line in Path(retired_file).read_text().splitlines():
        if not line.strip():
            continue
        parts = [part.strip() for part in line.split("|")]
        if len(parts) != 3 or not all(parts):
            raise SystemExit(f"a line of {retired_file} is not '<class.method> | <promise constant> | <class.method that now carries it>': {line}")
        retired[parts[0]] = parts[2]
    return retired


before_folder, after_folder, before_sha, retired_file = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
before, after = outcomes(before_folder), outcomes(after_folder)
retired = retired_lines(retired_file)
refused = []
for key, carrier in retired.items():
    if key in after:
        refused.append(f"retired and still run: {key}")
    elif before.get(key) != "Passed":
        refused.append(f"retired and not passed at {before_sha}: {key}")
    elif after.get(carrier) != "Passed":
        refused.append(f"retired, and its carrier did not pass: {key} -> {carrier}")
    elif carrier in before:
        refused.append(f"retired, and its carrier ran at {before_sha}: {key} -> {carrier}")
retired_now = {key: carrier for key, carrier in retired.items() if key not in after and before.get(key) == "Passed" and after.get(carrier) == "Passed" and carrier not in before}
passed_before = sorted(key for key, outcome in before.items() if outcome == "Passed" and key not in retired_now)
failing = [key for key in passed_before if key in after and after[key] != "Passed"]


def unhosted(key):
    opens = key.find("(")
    closes = key.find(")", opens)
    return key if opens < 0 else key[:opens] + key[closes + 1:]


hosts_before = {}
for key in before:
    hosts_before.setdefault(unhosted(key), set()).add(key)
moved = {}
for key in passed_before:
    if key in after:
        continue
    landed = [other for other, outcome in after.items() if outcome == "Passed" and other not in before and unhosted(other) == unhosted(key)]
    if landed:
        moved[key] = landed
gone = [key for key in passed_before if key not in after and key not in moved]
for key, landed in moved.items():
    print(f"passed at {before_sha}, moved to another host and passes there: {key} -> {', '.join(landed)}")
for key in failing:
    print(f"passed at {before_sha}, {after[key].lower()} now: {key}")
for key in gone:
    print(f"passed at {before_sha}, gone now: {key}")
for key, carrier in retired_now.items():
    print(f"retired: {key}, its promise carried by {carrier}")
for line in refused:
    print(line)
newly = sorted(key for key, outcome in after.items() if outcome == "Passed" and before.get(key) != "Passed")
print(f"{len(passed_before)} passed at {before_sha}; {len(newly)} pass now that did not")
if failing or gone or refused:
    print(f"{len(failing)} that passed at {before_sha} fail now, {len(gone)} are gone and {len(refused)} retired lines are refused")
    sys.exit(1)
print(f"no test that passed at {before_sha} fails or is gone")
