import json
import os
import subprocess
import sys
from pathlib import Path

root = Path(__file__).resolve().parent.parent
os.chdir(root)
if sys.argv[1:]:
    raise SystemExit("usage: ci.py")


def run(*args):
    done = subprocess.run(args, stdin=subprocess.DEVNULL, capture_output=True, text=True)
    if done.returncode != 0:
        raise SystemExit(f"{' '.join(args[:3])} failed: {done.stderr.strip()[:200]}")
    return done.stdout


branch = run("git", "branch", "--show-current").strip()
head = run("git", "ls-remote", "origin", f"refs/heads/{branch}").split("\t")[0]
if not head:
    raise SystemExit(f"origin holds no branch {branch}")
runs = json.loads(run("gh", "run", "list", "--branch", branch, "--limit", "30", "--json", "databaseId,headSha,workflowName"))
mine = [entry for entry in runs if entry["headSha"] == head]
if not mine:
    raise SystemExit(f"no run of {head[:9]} yet")
lines = []
for entry in mine:
    for job in json.loads(run("gh", "run", "view", str(entry["databaseId"]), "--json", "jobs"))["jobs"]:
        result = job["conclusion"] or ("running" if job["status"] == "in_progress" else "queued")
        lines.append(f"{entry['workflowName']} / {job['name']} ({job['databaseId']}): {result}")
passed = sum(line.endswith(": success") for line in lines)
summary = f"ci at {head[:9]}: {passed} of {len(lines)} jobs passed"
Path(".work").mkdir(exist_ok=True)
(Path(".work") / f"ci-{head[:9]}.txt").write_text("\n".join([summary, *lines]) + "\n")
print(summary)
for line in lines:
    if not line.endswith(": success"):
        print(line)
