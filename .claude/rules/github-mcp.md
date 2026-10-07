# GitHub MCP (PR review)

`.mcp.json` also registers the official `github` MCP server
(`https://api.githubcopilot.com/mcp/`, HTTP transport), limited to the `context` and
`pull_requests` toolsets, so Claude can read a PR (details, diff, files, comments) and
review it. The token is **never** in the repo; each person sets their own:

1. In GitHub: *Settings → Developer settings → Fine-grained tokens*, create
   `claude-code-fairplay-sports` (30-90 days), repository access **only
   `FairPlay.Sports`**, permissions *Pull requests: Read and write*,
   *Contents: Read-only* (never write: that is what stops it from merging).
2. Store it in the `GITHUB_PERSONAL_ACCESS_TOKEN` environment variable and restart
   Claude Code. PowerShell:
   `[Environment]::SetEnvironmentVariable("GITHUB_PERSONAL_ACCESS_TOKEN", "<token>", "User")`.
3. Approve the `github` server when prompted and check it with `/mcp`.

Usage: give Claude the PR number/link; it reads the PR and reports the review in the
chat. Posting review comments is a write: keep it on "ask every time" and only do it
when asked. Merging is denied in `.claude/settings.json` (`merge_pull_request`) and the
no-merge rule in `CLAUDE.md` (Commits & PRs) still applies.
