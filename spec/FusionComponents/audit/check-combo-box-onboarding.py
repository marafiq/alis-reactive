import json
from pathlib import Path
import sys

graph_path, case = sys.argv[1:]
graph = json.loads(Path(graph_path).read_text())
combo = graph["slices"]["FusionComboBox"]

def vendor(path, name):
    context, = (c for c in combo["contexts"] if c["path"] == path)
    result, = (row for row in graph["objects"][context["id"]]["members"] if row["name"] == name)
    return result

def declaration(holder, name, operator, source_contains=None):
    cls, = (shape for shape in combo["declarations"] if shape["symbol"] == holder)
    location = f"{holder}.{name}"
    named = [row for row in cls["members"] if row["js"] == name]
    assert named, f"{location} is missing"
    matches = [row for row in named if row["operator"] == operator]
    kinds = ", ".join(row["operator"] for row in named)
    assert len(matches) == 1, f"{location} expects one {operator}; found {kinds}"
    row, = matches
    assert set(row["exposed"]) == {"net10.0", "net48"}, f"{location} must match both host manifests"
    if source_contains is not None:
        assert source_contains in row["source"], f"{location} is missing {source_contains}"
    print(f"{location} -> {operator} on both hosts")

root = "FusionComboBox<TValue>"

if case == "payload":
    change = root + ".ChangeEventArgs"
    declaration(change, "itemData", "ReadOnly", "ReadOnly<TItem?>")
    declaration(change, "previousItemData", "ReadOnly", "ReadOnly<TItem?>")
    declaration(change, "item", "LiveObject", "nullable: true")
    assert vendor("event.change", "itemData")["owner"] == "SelectEventArgs"
    assert vendor("event.change", "previousItemData")["owner"] == "ChangeEventArgs"
elif case == "search":
    for name in ("showSpinner", "hideSpinner"):
        assert vendor("$", name)["signatures"][0]["result"]["type"] == "void"
        declaration(root, name, "Method")
    declaration(root, "filtering", "Event")
    filtering = root + ".FilteringEventArgs"
    declaration(filtering, "text", "ReadOnly", "ReadOnly<string>")
    declaration(filtering, "preventDefaultAction", "Property", "Property<bool>")
    declaration(filtering, "updateData", "Method", "Arg<IReadOnlyList<TItem>>")
    sig, = vendor("event.filtering", "updateData")["signatures"]
    assert sig["result"]["type"] == "void"
    assert [(p["name"], p["optional"]) for p in sig["parameters"]] == [
        ("dataSource", False), ("query", True), ("fields", True)
    ]
elif case == "catalogue":
    assert vendor("$", "dataSource")["owner"] == "DropDownBase"
    assert "DataManager" in vendor("$", "dataSource")["value"]["type"]
    declaration(root, "dataSource", "WriteOnly", "IReadOnlyList<TItem>")
    declaration(root, "fields", "LiveObject")
    for name in ("text", "groupBy"):
        assert vendor("class.FieldSettings", name)["optional"]
        declaration(root + ".FieldSettings", name, "Property", "optional: true")
elif case == "promise":
    assert vendor("$", "query")["defaultNull"]
    declaration(root, "query", "LiveObject", "nullable: true")
    assert vendor("class.DataQuery", "execute")["signatures"][0]["result"]["type"].startswith("Promise<")
    declaration(root + ".DataQuery", "execute", "MethodAsync", "QueryResult<TRow>")
    declaration(root + ".QueryResult<TRow>", "result", "JSON", "IReadOnlyList<TRow>")
elif case == "manager":
    assert "DataManager" in vendor("$", "dataSource")["value"]["type"]
    declaration(root, "dataSource", "LiveObject", 'LiveObject<DataManager>(name: "dataSource")')
    sig, = vendor("class.DataManager", "executeQuery")["signatures"]
    assert sig["result"]["type"] == "Promise<Response>"
    assert sig["parameters"][0]["name"] == "query"
    assert not sig["parameters"][0]["optional"]
    declaration(root + ".DataManager", "executeQuery", "MethodAsync", "TypedSource<DataQuery>")
else:
    raise ValueError(f"Unknown calibration case: {case}")
