# Claude Code instructions

## Git commits

- Do not add a `Co-Authored-By:` trailer or any other AI attribution line to commit messages or pull request descriptions, including when you only suggest a message for the user to commit.
- When the user says "git commit", ask whether to also run a git release before committing.

## Releases

- When the user says "release to next version", ask which version number to release before making any changes.
- When releasing to the next version, also update `CHANGELOG.md` with an entry for that version.
