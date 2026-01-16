#!/usr/bin/env bash
set -euo pipefail

CONNECTION=""
OUTPUT="docs/AIS_System_Catalog.md"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --connection)
      CONNECTION="$2"
      shift 2
      ;;
    --out)
      OUTPUT="$2"
      shift 2
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

if [[ -z "$CONNECTION" ]]; then
  echo "Missing --connection argument." >&2
  echo "Usage: $0 --connection \"<connection string>\" [--out docs/AIS_System_Catalog.md]" >&2
  exit 1
fi

dotnet run --project tools/AIS.DocGen -- --connection "$CONNECTION" --out "$OUTPUT"
