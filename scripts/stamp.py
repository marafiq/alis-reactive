import sys
from pathlib import Path

USAGE = "usage: stamp.py <message> <stamped> full <browser log> <compared log>, or stamp.py <message> <stamped> markdown <gate log>"
args = sys.argv[1:]
if len(args) < 3 or len(args) != {"full": 5, "markdown": 4}.get(args[2]):
    sys.exit(USAGE)
message, stamped, path, logs = Path(args[0]), Path(args[1]), args[2], [Path(arg) for arg in args[3:]]


def full(browser_log, compared_log):
    results = [line for line in browser_log.read_text().splitlines() if line.startswith("[playwright:result] all ")]
    if not results:
        sys.exit("the browser log holds no result line")
    run = results[-1].split(": ", 1)[1].split(", slowest")[0]
    compared = compared_log.read_text().splitlines()[0]
    here = (f'scripts/check.sh --no-spec, "[check] every check passed."; scripts/playwright.sh --all, "{run}", compared test by\n'
            f'test with the run before, "{compared}".')
    return here, run


def markdown(gate_log):
    lines = gate_log.read_text().splitlines()
    ran = [line for line in lines if not line.startswith("[gate] ")]
    if not ran or len(ran) == len(lines):
        sys.exit("the gate log holds no line of what ran, or no line of the gate's own")
    here = f"the Markdown path of scripts/land.sh, every staged path a .md file: {'; '.join(ran)}. No build, no browser run and no compare with the run before."
    return here, lines[-1]


here, shown = full(*logs) if path == "full" else markdown(*logs)
text = message.read_text()
found = text.count("{here}")
if found != 1:
    sys.exit(f"expected one {{here}} in the message, found {found}")
stamped.write_text(text.replace("{here}", here))
print(shown)
