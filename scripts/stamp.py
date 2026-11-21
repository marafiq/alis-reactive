import sys
from pathlib import Path

message, browser_log, compared_log, stamped = map(Path, sys.argv[1:5])
results = [line for line in browser_log.read_text().splitlines() if line.startswith("[playwright:result] all ")]
if not results:
    sys.exit("the browser log holds no result line")
run = results[-1].split(": ", 1)[1].split(", slowest")[0]
compared = compared_log.read_text().splitlines()[0]
text = message.read_text()
for token, value in (("{run}", run), ("{compared}", compared)):
    found = text.count(token)
    if found != 1:
        sys.exit(f"expected one {token} in the message, found {found}")
    text = text.replace(token, value)
stamped.write_text(text)
print(run)
