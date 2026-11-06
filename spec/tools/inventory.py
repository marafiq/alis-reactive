#!/usr/bin/env python3
import json
import re
import subprocess
from collections import Counter, defaultdict
from pathlib import Path

SPEC = Path(__file__).resolve().parent.parent
REPO = SPEC.parent
OUT = SPEC / "inventory"
BASE = "65fdf118d1498a8285db84a43c4da81dfaa0ea3e"
MAIN = "623dd4a272f8134fb692985fd04ae7151954c9f0"
LINES = {"redesign": BASE, "main": MAIN}
CARRIED = {"Alis.Reactive.NativeTagHelpers", "Alis.Reactive.DesignSystem"}
REDESIGN_PACKAGES = {
    "Alis.Reactive": ["net48", "net10.0"],
    "Alis.Reactive.Fusion": ["net48", "net10.0"],
    "Alis.Reactive.Native": ["net48", "net10.0"],
    "Alis.Reactive.NativeTagHelpers": ["net10.0"],
}
MAIN_PACKAGES = {
    "Alis.Reactive": ["net48", "net10.0"],
    "Alis.Reactive.Fusion": ["net48", "net10.0"],
    "Alis.Reactive.Native": ["net48", "net10.0"],
    "Alis.Reactive.FluentValidator": ["net48", "net10.0"],
    "Alis.Reactive.NativeTagHelpers": ["net10.0"],
    "Alis.Reactive.DesignSystem": ["net48", "net10.0"],
}
MODIFIERS = ("static ", "override ", "virtual ", "abstract ", "const ", "readonly ", "sealed ", "new ", "~")
ACCESSORS = {"get", "set", "init", "add", "remove"}
TEST_ATTRIBUTE = re.compile(r"\[\s*(Test|TestCase|TestCaseSource)(?!\w)")
TEST_METHOD = re.compile(r"^\s*public\s+(?:async\s+)?(?:Task|void)\s+(\w+)\s*\(")
ROUTE_CONSTANT = re.compile(r"(?:const|static\s+readonly)\s+string\s+(\w+)\s*=\s*\"(/[^\"]*)\"")
ROUTE_LITERAL = re.compile(r"\"(/[A-Za-z][^\"\s]*)\"")
CASE_LABEL = re.compile(r"\bcase\s+(?:\"([^\"]*)\"|'([^']*)'|([\w.]+))\s*:")
ANALYZER_ID = re.compile(r"const\s+string\s+(\w+)\s*=\s*\"(ALIS\d{3})\"")
PLUGIN_NAME = re.compile(r"registerPlugin\(\s*\"([^\"]+)\"")
ELEMENT_NAME = re.compile(r"customElements\.define\(\s*\"([^\"]+)\"")
GRAMMAR_KIND = re.compile(r"\bKind\s*=>\s*\"(\w+)\"")
TS_KIND = re.compile(r"\bkind\s*:\s*\"(\w+)\"")


def git(*args):
    completed = subprocess.run(["git", "-C", str(REPO), *args], capture_output=True, text=True)
    if completed.returncode != 0:
        raise SystemExit(f"git {' '.join(args)} failed: {completed.stderr}")
    return completed.stdout


def files(sha, prefix):
    return [path for path in git("ls-tree", "-r", "--name-only", sha, "--", prefix).splitlines()]


def show(sha, path):
    return git("show", f"{sha}:{path}")


def write(name, header, rows):
    rows = sorted(rows)
    (OUT / name).write_text("\t".join(header) + "\n" + "".join("\t".join(str(cell) for cell in row) + "\n" for row in rows))
    return len(rows)


def namespaces():
    found = set()
    for sha in LINES.values():
        for line in git("grep", "-h", "-E", "^[[:space:]]*namespace[[:space:]]+Alis[.]", sha, "--", "*.cs").splitlines():
            match = re.search(r"namespace\s+([\w.]+)", line)
            if match:
                found.add(match.group(1))
    return sorted(found, key=len, reverse=True)


def stripper(spaces):
    pattern = re.compile(r"(?<![\w.])(" + "|".join(re.escape(space) for space in spaces) + r")\.(?=[A-Za-z_])")
    return lambda line: pattern.sub("", line)


def split_dots(text):
    parts, depth, current = [], 0, ""
    for character in text:
        if character in "<[":
            depth += 1
        elif character in ">]":
            depth -= 1
        if character == "." and depth == 0:
            parts.append(current)
            current = ""
        else:
            current += character
    parts.append(current)
    return parts


def declaring_type(line):
    text = line
    while text.startswith(MODIFIERS):
        text = text[1:] if text.startswith("~") else text.split(" ", 1)[1]
    is_type = " -> " not in text and "(" not in text and " = " not in text
    head = text.split(" -> ")[0].split(" = ")[0]
    head = head.split("(")[0]
    if is_type:
        return head
    parts = split_dots(head)
    if parts[-1] in ACCESSORS:
        parts = parts[:-1]
    return ".".join(parts[:-1])


def doors_of(path_text):
    return [line for line in path_text.splitlines() if line.strip() and line.strip() != "#nullable enable"]


def raw_texts():
    texts = []
    for package, tfms in REDESIGN_PACKAGES.items():
        for tfm in tfms:
            for name in ["PublicAPI.Unshipped.txt", "PublicAPI.Shipped.txt"]:
                texts.append(("redesign", package, tfm, show(BASE, f"{package}/PublicAPI/{tfm}/{name}")))
    for package, tfms in MAIN_PACKAGES.items():
        for tfm in tfms:
            texts.append(("main", package, tfm, (SPEC / "inventory" / "main" / package / tfm / "PublicAPI.Unshipped.txt").read_text()))
    return texts


def type_names(texts):
    names = set()
    for _, _, _, text in texts:
        for line in doors_of(text):
            if " -> " not in line and "(" not in line and " = " not in line and not line.startswith(MODIFIERS):
                names.add(line.split("<")[0])
    return names


def read_doors(texts, normalize):
    redesign = []
    main = []
    for line_name, package, tfm, text in texts:
        for raw in doors_of(text):
            (redesign if line_name == "redesign" else main).append((package, tfm, normalize(raw), raw))
    return redesign, main


def match_doors(redesign, main):
    rows = []
    open_main = defaultdict(list)
    for package, tfm, door, raw in main:
        open_main[(package, tfm, door)].append((package, raw))
    unmatched_redesign = []
    for package, tfm, door, raw in redesign:
        bucket = open_main.get((package, tfm, door))
        if bucket:
            main_package, main_raw = bucket.pop()
            rows.append((package, main_package, tfm, door, raw, main_raw))
        else:
            unmatched_redesign.append((package, tfm, door, raw))
    moved = defaultdict(list)
    for (package, tfm, door), bucket in open_main.items():
        for main_package, main_raw in bucket:
            moved[(tfm, door)].append((main_package, main_raw))
    for package, tfm, door, raw in unmatched_redesign:
        bucket = moved.get((tfm, door))
        if bucket:
            main_package, main_raw = bucket.pop()
            rows.append((package, main_package, tfm, door, raw, main_raw))
        else:
            rows.append((package, "", tfm, door, raw, ""))
    for (tfm, door), bucket in moved.items():
        for main_package, main_raw in bucket:
            rows.append(("", main_package, tfm, door, "", main_raw))
    table = []
    for package, main_package, tfm, door, raw, main_raw in rows:
        status = "both" if raw and main_raw else "redesign-only" if raw else "main-only"
        owner = package or main_package
        scope = "carried" if owner in CARRIED else "spec"
        table.append((owner, main_package, scope, tfm, declaring_type(door), status, door, raw, main_raw))
    return table


def test_rows():
    rows = []
    for line, sha in LINES.items():
        for path in files(sha, "tests"):
            if not path.endswith(".cs") or "PlaywrightTests/" not in path:
                continue
            text = show(sha, path)
            if not TEST_ATTRIBUTE.search(text):
                continue
            lines = text.splitlines()
            constants = dict(ROUTE_CONSTANT.findall(text))
            class_match = re.search(r"class\s+(\w+)", text)
            fixture = class_match.group(1) if class_match else ""
            starts = []
            pending = 0
            for number, source in enumerate(lines):
                pending += len(TEST_ATTRIBUTE.findall(source))
                method = TEST_METHOD.match(source)
                if method and pending:
                    starts.append((number, method.group(1), pending))
                    pending = 0
                elif method:
                    pending = 0
            for index, (number, name, cases) in enumerate(starts):
                end = starts[index + 1][0] if index + 1 < len(starts) else len(lines)
                body = "\n".join(lines[number:end])
                named = sorted({value for key, value in constants.items() if re.search(rf"\b{key}\b", body)})
                literal = sorted(set(ROUTE_LITERAL.findall(body)) - set(constants.values()))
                routes, source = (named + literal, "test") if named or literal else (sorted(set(constants.values())), "fixture")
                rows.append((line, path.split("/")[1], path, fixture, name, cases, " ".join(routes), source))
    return rows


def kind_rows():
    rows = []
    schema = json.loads(show(BASE, "Alis.Reactive.Assets/runtime/types/plan.schema.json"))
    for definition, node in schema["$defs"].items():
        for kind in schema_kinds(node):
            rows.append(("redesign", "plan.schema.json", definition, kind))
    for path in files(BASE, "Alis.Reactive/Grammar"):
        for kind in GRAMMAR_KIND.findall(show(BASE, path)):
            rows.append(("redesign", "grammar", path, kind))
    for kind in sorted(set(TS_KIND.findall(show(MAIN, "Alis.Reactive.Assets/runtime/types/plan.ts")))):
        rows.append(("main", "plan.ts", "Alis.Reactive.Assets/runtime/types/plan.ts", kind))
    return rows


def schema_kinds(node):
    found = []
    if isinstance(node, dict):
        kind = node.get("properties", {}).get("kind") if isinstance(node.get("properties"), dict) else None
        if isinstance(kind, dict) and "const" in kind:
            found.append(kind["const"])
        for name, value in node.items():
            if name != "properties":
                found += schema_kinds(value)
            else:
                for prop, child in value.items():
                    if prop != "kind":
                        found += schema_kinds(child)
    elif isinstance(node, list):
        for value in node:
            found += schema_kinds(value)
    return found


def script_files(sha):
    return [path for path in files(sha, "Alis.Reactive.Assets")
            if path.endswith(".ts") and "__tests__" not in path and ".test." not in path and "/types/" not in path and "/dist/" not in path]


def case_rows():
    rows = []
    for line, sha in LINES.items():
        for path in script_files(sha):
            for number, source in enumerate(show(sha, path).splitlines(), 1):
                for match in CASE_LABEL.finditer(source):
                    rows.append((line, path, number, next(group for group in match.groups() if group is not None)))
    return rows


def analyzer_rows():
    found = defaultdict(dict)
    for line, sha in LINES.items():
        for path in files(sha, "Alis.Reactive.Analyzers"):
            if path.endswith(".cs"):
                for constant, identifier in ANALYZER_ID.findall(show(sha, path)):
                    found[identifier][line] = f"{path}#{constant}"
    rows = []
    for identifier, where in found.items():
        status = "both" if len(where) == 2 else f"{next(iter(where))}-only"
        rows.append((identifier, status, where.get("redesign", ""), where.get("main", "")))
    return rows


def plugin_rows():
    rows = []
    for line, sha in LINES.items():
        for path in script_files(sha):
            for name in PLUGIN_NAME.findall(show(sha, path)):
                rows.append((line, name, path))
    return rows


def browser_rows():
    rows = []
    for line, sha in LINES.items():
        for path in script_files(sha):
            if "/components/" not in path:
                continue
            text = show(sha, path)
            rows.append((line, path, " ".join(sorted(set(ELEMENT_NAME.findall(text)))), "mount" if "alis:mount" in text else ""))
    return rows


def runtime_rows():
    rows = []
    for line, sha in LINES.items():
        for path in script_files(sha):
            if "/runtime/" not in path:
                continue
            exported = sorted(set(re.findall(r"^export\s+(?:async\s+)?(?:function|const|class)\s+(\w+)", show(sha, path), re.MULTILINE)))
            rows.append((line, path, " ".join(exported)))
    return rows


def main():
    texts = raw_texts()
    types = type_names(texts)
    spaces = [space for space in namespaces() if space not in types]
    normalize = stripper(spaces)
    redesign, main_doors = read_doors(texts, normalize)
    doors = match_doors(redesign, main_doors)
    counts = {
        "doors.tsv": write("doors.tsv", ["package", "main_package", "scope", "tfm", "declaring_type", "status", "door", "redesign_line", "main_line"], doors),
        "tests.tsv": write("tests.tsv", ["line", "project", "fixture_path", "fixture", "test", "cases", "routes", "route_source"], test_rows()),
        "kinds.tsv": write("kinds.tsv", ["line", "source", "where", "kind"], kind_rows()),
        "cases.tsv": write("cases.tsv", ["line", "file", "line_number", "label"], case_rows()),
        "analyzers.tsv": write("analyzers.tsv", ["id", "status", "redesign", "main"], analyzer_rows()),
        "plugins.tsv": write("plugins.tsv", ["line", "name", "file"], plugin_rows()),
        "browser.tsv": write("browser.tsv", ["line", "file", "elements", "mount"], browser_rows()),
        "runtime.tsv": write("runtime.tsv", ["line", "file", "exports"], runtime_rows()),
    }
    for name, count in counts.items():
        print(f"{name}\t{count} rows")
    tally = Counter((row[0], row[3], row[5]) for row in doors)
    for (package, tfm, status), count in sorted(tally.items()):
        print(f"doors\t{package}\t{tfm}\t{status}\t{count}")


if __name__ == "__main__":
    main()
