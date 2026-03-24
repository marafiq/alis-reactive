#!/usr/bin/env bash
set -euo pipefail

# ──────────────────────────────────────────────────────────────────────
# sonar-analyze.sh — One-command SonarQube analysis for Alis.Reactive
#
# Analyzes C# (all projects) + TypeScript (Scripts/) using a single
# dotnet-sonarscanner invocation. Polls quality gate and exits 0/1.
#
# Prerequisites:
#   - Docker running with SonarQube (docker-compose.sonarqube.yml)
#   - dotnet-sonarscanner (dotnet tool install --global dotnet-sonarscanner)
#   - SONAR_TOKEN environment variable set
#
# Usage:
#   ./scripts/sonar-analyze.sh
# ──────────────────────────────────────────────────────────────────────

SONAR_HOST="http://localhost:9000"
PROJECT_KEY="alis-reactive"
POLL_INTERVAL=5
POLL_TIMEOUT=120

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log()   { echo -e "${GREEN}[sonar]${NC} $*"; }
warn()  { echo -e "${YELLOW}[sonar]${NC} $*"; }
error() { echo -e "${RED}[sonar]${NC} $*" >&2; }

# ── Prerequisites ──────────────────────────────────────────────────────

check_prereqs() {
    local missing=0

    if ! command -v docker &>/dev/null; then
        error "Docker is not installed. Install Docker Desktop first."
        missing=1
    fi

    if ! command -v dotnet-sonarscanner &>/dev/null && ! dotnet sonarscanner 2>/dev/null; then
        error "dotnet-sonarscanner is not installed."
        error "  Install: dotnet tool install --global dotnet-sonarscanner"
        missing=1
    fi

    if [[ -z "${SONAR_TOKEN:-}" ]]; then
        error "SONAR_TOKEN is not set."
        error "  1. Open ${SONAR_HOST} and log in"
        error "  2. Go to My Account > Security > Generate Token"
        error "  3. Add to ~/.zshrc: export SONAR_TOKEN=squ_xxxxx"
        missing=1
    fi

    if [[ $missing -eq 1 ]]; then
        exit 1
    fi
}

# ── SonarQube Health Check ─────────────────────────────────────────────

check_sonarqube() {
    log "Checking SonarQube at ${SONAR_HOST}..."

    local status_response
    status_response=$(curl -sf "${SONAR_HOST}/api/system/status" 2>/dev/null || echo "UNREACHABLE")

    if echo "$status_response" | grep -q '"status":"UP"'; then
        log "SonarQube is UP."
    else
        error "SonarQube is not reachable or not ready at ${SONAR_HOST}"
        error "  Start it: docker compose -f docker-compose.sonarqube.yml up -d"
        error "  Wait ~60-90s for startup, then retry."
        exit 1
    fi
}

# ── Find Repo Root ─────────────────────────────────────────────────────

find_repo_root() {
    local dir
    dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
    echo "$dir"
}

# ── Run Analysis ───────────────────────────────────────────────────────

run_analysis() {
    local repo_root="$1"
    cd "$repo_root"

    log "Starting SonarQube analysis..."
    log "  Project: ${PROJECT_KEY}"
    log "  C#: all projects (excluding examples/)"
    log "  TS: Alis.Reactive.SandboxApp/Scripts/"

    # Clean previous coverage results
    rm -rf TestResults/coverage 2>/dev/null

    # Begin — the .NET scanner auto-discovers C# and TS sources from the solution.
    # sonar.cs.opencover.reportsPaths tells the scanner where to find coverage data.
    dotnet sonarscanner begin \
        /k:"${PROJECT_KEY}" \
        /d:sonar.host.url="${SONAR_HOST}" \
        /d:sonar.token="${SONAR_TOKEN}" \
        /d:sonar.exclusions="**/node_modules/**,**/.worktrees/**,**/docs-site/**,**/wwwroot/**,**/*.verified.txt,**/obj/**,**/bin/**,**/tools/**,**/examples/**" \
        /d:sonar.test.inclusions="**/*.test.ts,**/Tests/**,**/UnitTests/**" \
        /d:sonar.cs.opencover.reportsPaths="TestResults/coverage/**/coverage.opencover.xml"

    # Build — SonarQube hooks into MSBuild to collect C# analysis data
    dotnet build Alis.Reactive.SandboxApp/Alis.Reactive.SandboxApp.csproj

    # Test with coverage — generates OpenCover XML reports that SonarQube consumes.
    # Each test project writes to its own subfolder under TestResults/coverage/.
    log "Running tests with coverage collection..."
    local test_projects=(
        "tests/Alis.Reactive.UnitTests"
        "tests/Alis.Reactive.Native.UnitTests"
        "tests/Alis.Reactive.Fusion.UnitTests"
        "tests/Alis.Reactive.FluentValidator.UnitTests"
    )
    for proj in "${test_projects[@]}"; do
        local proj_name
        proj_name=$(basename "$proj")
        dotnet test "$proj" \
            --no-build \
            --collect:"XPlat Code Coverage" \
            --results-directory "TestResults/coverage/${proj_name}" \
            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
    done

    # End — uploads all results (C# + TS + coverage) to SonarQube
    dotnet sonarscanner end \
        /d:sonar.token="${SONAR_TOKEN}"

    log "Analysis uploaded. Waiting for server-side processing..."
}

# ── Poll Quality Gate ──────────────────────────────────────────────────

poll_quality_gate() {
    local repo_root="$1"
    local report_task_file

    # Find the report-task.txt generated by the scanner
    report_task_file=$(find "$repo_root/.sonarqube" -name "report-task.txt" -type f 2>/dev/null | head -1)

    if [[ -z "$report_task_file" ]]; then
        error "Could not find .sonarqube/out/.sonar/report-task.txt"
        error "Analysis may not have completed successfully."
        exit 1
    fi

    # Extract CE task URL
    local ce_task_url
    ce_task_url=$(grep "^ceTaskUrl=" "$report_task_file" | cut -d'=' -f2-)

    if [[ -z "$ce_task_url" ]]; then
        error "Could not extract ceTaskUrl from report-task.txt"
        exit 1
    fi

    log "Polling Compute Engine task..."

    local elapsed=0
    local status=""

    while [[ $elapsed -lt $POLL_TIMEOUT ]]; do
        local response
        response=$(curl -sf "$ce_task_url" \
            -H "Authorization: Bearer ${SONAR_TOKEN}" 2>/dev/null || echo "")

        status=$(echo "$response" | grep -o '"status":"[^"]*"' | head -1 | cut -d'"' -f4)

        case "$status" in
            SUCCESS)
                log "Server-side analysis complete."
                break
                ;;
            FAILED|CANCELED)
                error "Server-side analysis ${status}."
                exit 1
                ;;
            *)
                sleep "$POLL_INTERVAL"
                elapsed=$((elapsed + POLL_INTERVAL))
                ;;
        esac
    done

    if [[ "$status" != "SUCCESS" ]]; then
        error "Timed out waiting for analysis (${POLL_TIMEOUT}s). Check ${SONAR_HOST}."
        exit 1
    fi

    # Query quality gate
    local qg_response
    qg_response=$(curl -sf "${SONAR_HOST}/api/qualitygates/project_status?projectKey=${PROJECT_KEY}" \
        -H "Authorization: Bearer ${SONAR_TOKEN}" 2>/dev/null || echo "")

    local qg_status
    qg_status=$(echo "$qg_response" | grep -o '"status":"[^"]*"' | head -1 | cut -d'"' -f4)

    # Print issue summary
    print_summary

    case "$qg_status" in
        OK)
            log "${GREEN}Quality gate PASSED.${NC}"
            return 0
            ;;
        ERROR|WARN)
            error "Quality gate FAILED (${qg_status})."
            error "  Review issues at: ${SONAR_HOST}/dashboard?id=${PROJECT_KEY}"
            return 1
            ;;
        *)
            error "Unexpected quality gate status: ${qg_status}"
            error "  Check manually at: ${SONAR_HOST}/dashboard?id=${PROJECT_KEY}"
            return 1
            ;;
    esac
}

# ── Print Issue Summary ────────────────────────────────────────────────

print_summary() {
    local issues_response
    issues_response=$(curl -sf "${SONAR_HOST}/api/issues/search?projectKeys=${PROJECT_KEY}&resolved=false&ps=1&facets=types" \
        -H "Authorization: Bearer ${SONAR_TOKEN}" 2>/dev/null || echo "")

    local total
    total=$(echo "$issues_response" | grep -o '"total":[0-9]*' | head -1 | cut -d':' -f2)

    if [[ -n "$total" ]]; then
        log "Open issues: ${total}"
        log "  Dashboard: ${SONAR_HOST}/dashboard?id=${PROJECT_KEY}"
    fi
}

# ── Main ───────────────────────────────────────────────────────────────

main() {
    local repo_root
    repo_root=$(find_repo_root)

    log "Alis.Reactive SonarQube Analysis"
    log "================================"

    check_prereqs
    check_sonarqube
    run_analysis "$repo_root"
    poll_quality_gate "$repo_root"
}

main "$@"
