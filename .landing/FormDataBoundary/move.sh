#!/bin/bash
set -euo pipefail
[ "$#" -eq 1 ] || { echo "usage: bash .landing/FormDataBoundary/move.sh <the lane's head>"; exit 2; }
cd "$(dirname "$0")/../.."
git checkout HEAD -- spec/MISTAKES.md
git checkout --detach "$1"
mkdir -p .landing/FormDataBoundary/scratch
npm ci --no-audit --no-fund < /dev/null > .landing/FormDataBoundary/scratch/npm-ci.log 2>&1 || { echo "npm ci failed: read .landing/FormDataBoundary/scratch/npm-ci.log"; exit 1; }
python3 -I .landing/FormDataBoundary/counts.py spec/MISTAKES.md
echo "moved: FormDataBoundary at $(git rev-parse --short=9 HEAD)"
git status --short
