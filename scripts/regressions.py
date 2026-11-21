import sys
import xml.etree.ElementTree as ET

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


before_folder, after_folder, before_sha = sys.argv[1], sys.argv[2], sys.argv[3]
before, after = outcomes(before_folder), outcomes(after_folder)
passed_before = sorted(key for key, outcome in before.items() if outcome == "Passed")
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
newly = sorted(key for key, outcome in after.items() if outcome == "Passed" and before.get(key) != "Passed")
print(f"{len(passed_before)} passed at {before_sha}; {len(newly)} pass now that did not")
if failing or gone:
    print(f"{len(failing)} that passed at {before_sha} fail now and {len(gone)} are gone")
    sys.exit(1)
print(f"no test that passed at {before_sha} fails or is gone")
