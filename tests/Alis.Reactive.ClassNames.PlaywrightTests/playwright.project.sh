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
