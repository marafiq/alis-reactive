# Notes

- 2026-11-05 owner, step 0: yes, with one correction, "not rebuild - rewrite". The brief says rewrite and spec/REWRITE.md (BRIEF.md lines 19, 113, 116, 131, 403, 833, 869, 956).
- 2026-11-05 owner: "No module and feature must be lost in the process. what so ever, actually this session will help improve in all aspects". BRIEF.md section 1 adds "features lost 0"; a refused door names the doors that carry its capability.
- 2026-11-05 fact: the spec branch starts at 65fdf118d1498a8285db84a43c4da81dfaa0ea3e; main reads at 623dd4a272f8134fb692985fd04ae7151954c9f0 (git rev-parse).
- 2026-11-05 fact: spec/CLAUDE.md is audited by WhenARuleNamesTheCode: each code span names a tracked path, a name the code uses, or text the source holds (tests/Alis.Reactive.ArchitectureTests/WhenARuleNamesTheCode.cs:107-110).
- 2026-11-05 decision: the compact hook cds to the worktree by absolute path; CLAUDE_PROJECT_DIR is empty in the Bash tool, and the session's directory moved into the worktree. It is in the root's and the worktree's .claude/settings.local.json; jq -e printed the command for both.
- 2026-11-05 fact: the hook's command ran by hand while 23 checks failed and printed the brief report, the notes and the re-read line (exit 0). Its firing on compact is observed at the first compaction.
