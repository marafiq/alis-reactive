#!/usr/bin/env node
const GIT_OPTIONS_WITH_VALUE = new Set(["-C", "-c", "--git-dir", "--work-tree", "--namespace", "--exec-path", "--config-env"]);
const USER_SECRETS_FOLDER = /microsoft[\\/]usersecrets/i;

function wordsOf(segment) {
  return (segment.match(/"[^"]*"|'[^']*'|\S+/g) ?? []).map((word) => word.replace(/^(["'])([\s\S]*)\1$/, "$2"));
}

function gitSubcommand(words, at) {
  for (let i = at + 1; i < words.length; i++) {
    const word = words[i];
    if (!word.startsWith("-")) return word;
    if (GIT_OPTIONS_WITH_VALUE.has(word)) i++;
  }
  return undefined;
}

function usesGitStash(words) {
  return words.some((word, at) => /(^|\/)git$/.test(word) && gitSubcommand(words, at) === "stash");
}

function listsUserSecrets(words) {
  const tool = words.findIndex((word) => word === "user-secrets" || /(^|\/)dotnet-user-secrets$/.test(word));
  return tool >= 0 && words.slice(tool + 1).includes("list");
}

function readsUserSecretsFolder(words) {
  return words.some((word) => USER_SECRETS_FOLDER.test(word));
}

function refusalFor(input) {
  if (input.tool_name === "Read") {
    return USER_SECRETS_FOLDER.test(input.tool_input?.file_path ?? "") ? SECRETS : undefined;
  }
  const segments = (input.tool_input?.command ?? "").split(/&&|\|\||[;|\n()`]/).map(wordsOf);
  if (segments.some(usesGitStash)) return STASH;
  if (segments.some((words) => listsUserSecrets(words) || readsUserSecretsFolder(words))) return SECRETS;
  return undefined;
}

const STASH =
  "git stash is never run here: stash@{0} holds work that must never be popped, applied or dropped. " +
  "Commit on a branch in a worktree instead.";
const SECRETS =
  "User secrets are never read: no dotnet user-secrets list and nothing under the user-secrets folder. " +
  "When a secret may be missing, say so without reading the store.";

let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  const reason = refusalFor(JSON.parse(raw));
  if (reason) {
    console.log(
      JSON.stringify({
        hookSpecificOutput: { hookEventName: "PreToolUse", permissionDecision: "deny", permissionDecisionReason: reason },
      }),
    );
  }
  process.exit(0);
});
