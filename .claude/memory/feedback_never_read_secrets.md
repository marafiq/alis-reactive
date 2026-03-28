---
name: Never read or list user secrets
description: Never run dotnet user-secrets list or read secret values — when user says they linked a secret, trust them
type: feedback
---

Never run `dotnet user-secrets list` or any command that reads/displays secret values.

**Why:** User considers this a privacy violation. When they say "I linked the secret" they mean it's done — don't verify by reading it.

**How to apply:** If user says a secret is set up, acknowledge and move on. If something fails due to a missing secret, mention the possibility without reading the secret store. Never display secret values in conversation.
