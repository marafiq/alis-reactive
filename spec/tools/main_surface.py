#!/usr/bin/env python3
import os
import re
import shutil
import subprocess
import sys
from pathlib import Path

SPEC = Path(__file__).resolve().parent.parent
OUT = SPEC / "inventory" / "main"
ANALYZERS = """    <ItemGroup>
        <PackageReference Include="Microsoft.CodeAnalysis.PublicApiAnalyzers" Version="5.6.0" PrivateAssets="all" />
        <AdditionalFiles Include="PublicAPI/$(TargetFramework)/PublicAPI.Shipped.txt" />
        <AdditionalFiles Include="PublicAPI/$(TargetFramework)/PublicAPI.Unshipped.txt" />
    </ItemGroup>
</Project>"""
PACKAGES = {
    "Alis.Reactive": ["net48", "net10.0"],
    "Alis.Reactive.Fusion": ["net48", "net10.0"],
    "Alis.Reactive.Native": ["net48", "net10.0"],
    "Alis.Reactive.FluentValidator": ["net48", "net10.0"],
    "Alis.Reactive.NativeTagHelpers": ["net10.0"],
    "Alis.Reactive.DesignSystem": ["net48", "net10.0"],
}


def doors(path):
    return [line for line in path.read_text().splitlines() if line.strip() and line.strip() != "#nullable enable"]


def single_target(text, tfm):
    replaced, count = re.subn(r"<TargetFrameworks>[^<]*</TargetFrameworks>", f"<TargetFrameworks>{tfm}</TargetFrameworks>", text)
    if count == 0:
        replaced, count = re.subn(r"<TargetFramework>[^<]*</TargetFramework>", f"<TargetFramework>{tfm}</TargetFramework>", text)
    if count != 1:
        raise SystemExit(f"expected one target framework element, found {count}")
    return replaced


def wire(project, tfm):
    original = project.read_text()
    if "PublicApiAnalyzers" in original:
        raise SystemExit(f"{project} already references PublicApiAnalyzers")
    wired = single_target(original, tfm)
    if wired.count("</Project>") != 1:
        raise SystemExit(f"{project} has {wired.count('</Project>')} closing Project tags")
    project.write_text(wired.replace("</Project>", ANALYZERS))
    folder = project.parent / "PublicAPI" / tfm
    folder.mkdir(parents=True, exist_ok=True)
    for name in ["PublicAPI.Shipped.txt", "PublicAPI.Unshipped.txt"]:
        (folder / name).write_text("#nullable enable\n")
    return original


def fix(project):
    environment = {**os.environ, "BuildReactiveBrowserAssets": "false"}
    completed = subprocess.run(
        ["dotnet", "format", "analyzers", str(project), "--diagnostics", "RS0016", "--severity", "info"],
        capture_output=True, text=True, env=environment,
    )
    if completed.returncode != 0:
        raise SystemExit(f"dotnet format failed for {project}:\n{completed.stdout}\n{completed.stderr}")


def main():
    main_read = Path(sys.argv[1]).resolve()
    head = subprocess.run(["git", "-C", str(main_read), "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
    print(f"main-read at {head}")
    for package, tfms in PACKAGES.items():
        project = main_read / package / f"{package}.csproj"
        for tfm in tfms:
            original = wire(project, tfm)
            try:
                unshipped = project.parent / "PublicAPI" / tfm / "PublicAPI.Unshipped.txt"
                fix(project)
                first = len(doors(unshipped))
                fix(project)
                second = len(doors(unshipped))
                target = OUT / package / tfm
                target.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(unshipped, target / "PublicAPI.Unshipped.txt")
                print(f"{package}\t{tfm}\t{first} lines\tsecond pass {second}")
                if second != first:
                    raise SystemExit(f"{package} {tfm}: the second pass changed the count from {first} to {second}")
            finally:
                project.write_text(original)
                shutil.rmtree(project.parent / "PublicAPI")


if __name__ == "__main__":
    main()
