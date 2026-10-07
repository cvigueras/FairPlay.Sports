# Shortcut MCP (stories)

`.mcp.json` registers the official hosted `shortcut` MCP server
(`https://mcp.shortcut.com/mcp`, HTTP transport) so Claude can read a Shortcut story
and work on it. It authenticates with OAuth, so there is no token in the repo or in
an environment variable; each person authorizes their own Shortcut account:

1. Start Claude Code and approve the `shortcut` server when prompted.
2. Run `/mcp`, pick `shortcut` and choose *Authenticate*; sign in to Shortcut in the
   browser tab that opens.
3. Check it with `/mcp` (status should be connected). The old self-hosted
   `@shortcut/mcp` package is deprecated and must not be used.

Usage: give Claude the story id/link; it reads the story, implements it following
this file, and verifies with build/tests. Keep Shortcut write tools (create, update,
comment) on "ask every time". The no-commit rule in `CLAUDE.md` (Commits & PRs) still applies.

Before developing a story, check the current branch (`git branch --show-current`). If it
belongs to a different story, or has uncommitted work from one, stop and suggest running
`/next-branch` first (back to an updated `main`, then the next numbered branch). Never
create or switch branches on your own, and never mix two stories in one branch.

When asked to work on a story that is in "To Do", move it to "In Progress" with
`stories-update` (workflow "Standard": To Do = 500000007, In Progress = 500000008).
Only that one transition: never move it to In Review/Done unless explicitly asked.

When a PR is created for a story (`/pr`), link the PR URL to that story as an external
link (`stories-add-external-link`). The story id comes from the work in progress, never
from the branch number; skip it if the story is unknown.

When a story or bug is solved, post a comment on it (`stories-create-comment`) listing
the tests that **should be run** to verify it (manual and automated, derived from the
acceptance criteria), regardless of which ones Claude already ran or skipped.
