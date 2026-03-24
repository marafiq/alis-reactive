# SonarQube Community Integration — Design Spec

**Date:** 2026-03-24
**Branch:** `feature/sonarqube-integration`
**Status:** Approved

## Goal

Integrate SonarQube Community Edition into the Alis.Reactive development workflow as a
local Docker-based static analysis tool covering both C# and TypeScript. Add the quality
gate to the pre-commit verification section in CLAUDE.md.

## Scope

- **In scope:** C# static analysis (all source + test projects), TypeScript static analysis (`Alis.Reactive.SandboxApp/Scripts/`), Docker Compose setup, one-command analysis script, CLAUDE.md pre-commit gate
- **Out of scope:** Code coverage reporting, GitHub Actions CI, SonarQube authentication/hardening, CSS analysis

## Architecture

```
Repo Root (.worktrees/sonarqube-integration/)
├── docker-compose.sonarqube.yml    ← SonarQube + PostgreSQL
├── scripts/
│   └── sonar-analyze.sh            ← One-command full scan (single scanner)
└── CLAUDE.md                       ← Updated pre-commit gate
```

### Docker Services

| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| `sonarqube` | `sonarqube:community` | `9000` | SonarQube web UI + analyzer |
| `sonarqube-db` | `postgres:15` | internal | Persistent analysis database |

Data persisted via named Docker volumes (`sonarqube_data`, `sonarqube_extensions`, `sonarqube_logs`, `sonarqube_db`).

**Note:** On Apple Silicon Macs, `sonarqube:community` runs under Rosetta emulation (amd64 image). Works correctly but ~2-3x slower than native.

### Analysis Pipeline — Single Scanner

The `dotnet-sonarscanner` can forward non-C# files (TypeScript, JS) to SonarQube's built-in
analyzers. This eliminates the need for a standalone `sonar-scanner` and avoids the problem
where two separate scanner invocations overwrite each other's results on the same project key.

The shell script `scripts/sonar-analyze.sh` runs:

```
1. dotnet sonarscanner begin \
     /k:"alis-reactive" \
     /d:sonar.host.url=http://localhost:9000 \
     /d:sonar.token=$SONAR_TOKEN \
     /d:sonar.sources=Alis.Reactive.SandboxApp/Scripts \
     /d:sonar.tests=Alis.Reactive.SandboxApp/Scripts/__tests__ \
     /d:sonar.test.inclusions=**/*.test.ts \
     /d:sonar.exclusions=**/node_modules/**,**/.worktrees/**,**/docs-site/**,**/wwwroot/**,**/*.verified.txt,**/obj/**,**/bin/**

2. dotnet build Alis.Reactive.slnx

3. dotnet sonarscanner end /d:sonar.token=$SONAR_TOKEN

4. Poll quality gate via API (see Quality Gate Polling below)
```

One scanner, one scan, one quality gate check.

### Quality Gate Polling

After `dotnet sonarscanner end` completes, the SonarQube server processes results asynchronously
via its Compute Engine. The script must wait for this:

1. Extract the CE task URL from `.sonarqube/out/.sonar/report-task.txt` (`ceTaskUrl` field)
2. Poll `GET {ceTaskUrl}` every 5 seconds until `task.status` is `SUCCESS` or `FAILED` (max 60s timeout)
3. If `SUCCESS`: query `GET api/qualitygates/project_status?projectKey=alis-reactive`
4. Exit 0 if quality gate status is `OK`, exit 1 if `ERROR` or `WARN`
5. Print a summary of new issues (bugs/vulnerabilities/code smells counts)

### CLAUDE.md Change

Add to the Pre-Commit Verification section after step 6:

```bash
# 7. SonarQube quality gate (requires Docker running with SonarQube)
./scripts/sonar-analyze.sh
# If exit code is 1 (quality gate failed), fix reported issues before committing.
# Skip if Docker/SonarQube is not running — but run it at least once per feature branch.
```

The script's exit code is the gate: 0 = pass, 1 = fail. No manual interpretation needed.
Uses SonarQube's default "Sonar way" quality profile.

### Token Flow

| Step | Action |
|------|--------|
| First login | `admin`/`admin` at `localhost:9000`, forced password change |
| Generate token | `localhost:9000/account/security` → generate project analysis token |
| Persist token | Add `export SONAR_TOKEN=squ_xxxxx` to `~/.zshrc` (or `~/.zprofile`) |
| Script usage | `dotnet sonarscanner begin ... /d:sonar.token=$SONAR_TOKEN` |
| No-token fallback | Script checks `$SONAR_TOKEN` is set, prints error with instructions if missing |

### Prerequisites

Tools that must be installed (not managed by this spec):

| Tool | Install | Purpose |
|------|---------|---------|
| Docker + Compose | Already installed | Run SonarQube server |
| `dotnet-sonarscanner` | `dotnet tool install --global dotnet-sonarscanner` | C# + TS analysis |

Only one scanner tool needed. The script checks for prerequisites and prints clear error messages if missing.

### First-Time Setup

1. `docker compose -f docker-compose.sonarqube.yml up -d` — start SonarQube
2. Wait for SonarQube to be healthy (~60-90 seconds on ARM Macs)
3. Open `localhost:9000`, login with `admin`/`admin`, change password
4. Generate a token at `localhost:9000/account/security`
5. `echo 'export SONAR_TOKEN=squ_xxxxx' >> ~/.zshrc && source ~/.zshrc`
6. `./scripts/sonar-analyze.sh` — first analysis run

### File Details

#### docker-compose.sonarqube.yml

Separate compose file (not `docker-compose.yml`) to avoid implying the main app runs in Docker.
- `sonarqube:community` image (free, supports C# + TS + JS)
- `postgres:15` for persistence
- Named volumes for data durability across container restarts
- Health check on SonarQube web UI (`/api/system/health`)

#### scripts/sonar-analyze.sh

- Checks prerequisites (`docker`, `dotnet-sonarscanner`, `$SONAR_TOKEN`)
- Verifies SonarQube is running and healthy via `curl localhost:9000/api/system/health`
- Runs single-scanner analysis (begin → build → end)
- Polls CE task status until complete (5s interval, 60s timeout)
- Queries quality gate status
- Prints issue summary (new bugs, vulnerabilities, code smells)
- Exits 0 (pass) or 1 (fail)

#### .gitignore additions

```
# SonarQube
.sonarqube/
.scannerwork/
```

## Decisions

1. **Separate compose file** (`docker-compose.sonarqube.yml`) — the main app is not Dockerized, so a standalone `docker-compose.yml` would be misleading
2. **Single scanner** (`dotnet-sonarscanner`) — forwards TS sources to SonarQube's built-in TS analyzer, avoiding the two-scanner overwrite problem
3. **No coverage** — deferred to keep initial setup simple; can add coverlet + vitest coverage later
4. **Default "Sonar way" quality profile** — well-maintained, no custom rules needed initially
5. **Token-based auth** — `SONAR_TOKEN` env var in `~/.zshrc`, no hardcoded credentials
6. **Pre-commit gate is conditional** — required when Docker/SonarQube is running; skip with warning otherwise. Must run at least once per feature branch before merge.
