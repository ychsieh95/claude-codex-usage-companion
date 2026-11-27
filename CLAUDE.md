# Claude Code instructions

## Git commits

- Do not add a `Co-Authored-By:` trailer or any other AI attribution line to commit messages or pull request descriptions, including when you only suggest a message for the user to commit.
- When the user says "git commit", ask whether to also run a git release before committing.

## Releases

- When the user says "release to next version", ask which version number to release before making any changes.
- When releasing to the next version, also update `CHANGELOG.md` with an entry for that version.

## Screenshots

- When the user says "git commit" or "release to next version" and the changes affect anything displayed (the desktop panel, Plasma widget, Settings window, or other UI), ask whether to also update the screenshots in `assets/screenshots/` before proceeding.

## HoleyCC site

- When the user says "git commit" or "release to next version" and the changes affect anything described on the project's HoleyCC pages (features, UI, screenshots, install or download steps, versions, privacy behavior, or the changelog), ask whether to also update the Claude Codex Usage Companion info in HoleyCC (`~/Programmings/HoleyCC/`) before proceeding. If yes, review and update all of these:
  - `Pages/Tools/ClaudeCodexUsageCompanion/Index.cshtml` (overview: features, screenshots, version and download commands)
  - `Pages/Tools/ClaudeCodexUsageCompanion/History.cshtml` and `History.cshtml.cs` (update history page)
  - `Pages/Tools/ClaudeCodexUsageCompanion/Privacy.cshtml` (privacy policy)
  - `Pages/Tools/ClaudeCodexUsageCompanion/Contact.cshtml` and `Contact.cshtml.cs` (contact page)
  - `Pages/Projects.cshtml` (project card description)
  - `files/applications/claude-codex-usage-companion/update-history.en-us.md` (copy of `CHANGELOG.md`) and `update-history.zh-tw.md` (Traditional Chinese translation, keeping every `## [VERSION] - YYYY-MM-DD` heading unchanged)
  - `wwwroot/images/applications/claude-codex-usage-companion/` (app icon and screenshots copied from `assets/screenshots/`)
  - `wwwroot/css/claude-codex-usage-companion.css` (page styles, if layout changes)
  - `README.md`, `README.zh-TW.md`, `docs/PRIVACY.md`, and `docs/update-history/app/README.en-US.md` / `README.zh-TW.md` (HoleyCC's own docs and update history entry for the change)
- Follow HoleyCC's own `CLAUDE.md` when editing that repository.
