#!/usr/bin/env bash
set -euo pipefail

dotnet tool restore 2>/dev/null || true
dotnet cake "$@"
