import sys
counts = {}
path = sys.argv[1]
lines = open(path, encoding="utf-8").read().split("\n")
for kind, add in counts.items():
    hits = [i for i, line in enumerate(lines) if line.startswith("| " + kind)]
    if len(hits) != 1:
        sys.exit("counts: " + kind + " matches " + str(len(hits)) + " rows of " + path)
    cells = lines[hits[0]].split(" | ")
    before = int(cells[1])
    cells[1] = str(before + add)
    lines[hits[0]] = " | ".join(cells)
    print("counts: " + kind[:40] + " " + str(before) + " -> " + cells[1])
open(path, "w", encoding="utf-8").write("\n".join(lines))
