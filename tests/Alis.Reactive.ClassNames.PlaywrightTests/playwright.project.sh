# The class-names app's Playwright project, as scripts/playwright.sh reads it: one shard, the whole
# project, no browser bundle of its own (the app serves the core's runtime and its own stylesheet), and
# the sources its binaries are built from beside the core.
# shellcheck shell=bash

playwright_project=class-names

playwright_shards="class-names"

playwright_shard_filter() {
  case "$1" in
    class-names) printf '' ;;
    *) return 2 ;;
  esac
}

playwright_check_assets() {
  :
}

playwright_sources() {
  printf '%s\n' Alis.Reactive.Native tests/Alis.Reactive.ClassNames.App
}
