---
name: changelog-management
description: Generates and previews per-package CHANGELOG.md entries from conventional commit history. Use when previewing what a package would release, backfilling a missing entry, or fixing a changelog CI failure - outside of a full release.
allowed-tools: Bash, Read, Edit, AskUserQuestion
---

# Changelog Manager

Generates and previews `CHANGELOG.md` entries for the packages in this repo, from
conventional commit history.

`/release-management` already writes changelog entries as part of cutting a
release. Use this skill when you want the changelog work on its own:

- previewing what a package *would* release, before deciding to release it
- backfilling an entry for a version that shipped without one
- fixing a `validate-changelogs.ps1` CI failure
- rewriting an entry that reads badly

## What makes this repo different

- **v18 only.** Per-package changelogs and the CI check exist on the v18 line
  (`v18/dev`, `main-v18`). The v17 line has not been moved onto this setup yet.
- **No generation script.** `Umbraco.Automate` and `Umbraco.AI` have
  `scripts/generate-changelog.js` behind `npm run changelog`. This repo has no
  such script, so entries come from reading `git log` directly. Do not invent a
  script or an npm target - neither exists.
- **One changelog per package**, at `src/<PackageName>/CHANGELOG.md`. There is no
  repo-level changelog. The OAuth proxy (`src/Umbraco.Cms.Integrations.OAuthProxy`)
  is not a package and has none.
- **Packages are independent.** A commit only belongs in a package's changelog if
  it touched that package's folder. No cascade, no shared entries.

## Format

[Keep a Changelog][kac], matching what every package already has:

```markdown
## [9.1.0] - 2026-11-12

### feat

* **hubspot:** Map form fields to custom contact properties ([abc1234](https://github.com/umbraco/Umbraco.Forms.Integrations/commit/abc1234))

### fix

* **hubspot:** Handle an expired OAuth token when loading contact properties ([def5678](https://github.com/umbraco/Umbraco.Forms.Integrations/commit/def5678))
```

Rules:

- New entries go at the top, directly below the header block.
- Get the date with `date +%Y-%m-%d`. Never guess it.
- Group by commit type, using the type as the heading: `feat`, `fix`, `perf`.
- A breaking change gets its own `### Breaking` section, listed first.
- One bullet per user-facing change, from the commit subject.
- `docs:`, `chore:`, `refactor:`, `test:` and `ci:` are normally left out. If a
  version contains nothing else, use `### Internal` and say so in one line.
- On dev the newest heading is usually `## [<version>] - Unreleased`, left by
  `/post-release-cleanup`. Add bullets under it; do not add a second heading for
  the same version.

## Phase 1: Pick the package and the range

```bash
ls -d src/Umbraco.Forms.Integrations.*/        # the packages
cat src/<PackageName>/version.json             # current version
git tag --list 'release/*' | sort -V | tail    # confirm the real tag
```

Tag slugs are the package name minus `Umbraco.Forms.Integrations.`, lowercased,
dots to hyphens: `automation-zapier`, `crm-activecampaign`, `crm-hubspot`. Older
tags use `release-<name>-<version>`. Confirm the tag exists with `git tag --list`
before using it, and pick the newest one **on this line's major** (Zapier `8`,
ActiveCampaign `7`, Hubspot `9` on v18) - the highest tag overall may belong to
another line.

Range is that package's last release tag on this line to `HEAD`. With no tag
(Zapier `8.0.0` was published without one), fall back to
`$(git merge-base origin/main-v18 HEAD)` once `main-v18` holds the released state,
or else to the last commit that changed the package's `CHANGELOG.md`. Say which you
used.

## Phase 2: Read the commits

```bash
git log <start>..HEAD --format='%h %s' -- src/<PackageName>/
```

Path-scoped, so only commits touching that package appear. Note that a
`Directory.Packages.props` change will not show up here even though it can affect
the package - an Umbraco CMS/Forms range change, or for Zapier a bump of
`Umbraco.Cms.Integrations.Automation.Zapier`. Mention it separately if the range
contains one:

```bash
git log <start>..HEAD --format='%h %s' -- Directory.Packages.props
```

## Phase 3: Write or preview

State clearly which you are doing.

**Preview** - print the proposed entry, change nothing.

**Write** - insert the entry below the header block (or under the existing
`Unreleased` heading) with `Edit`. Never reorder or reword existing entries; only
add.

If the version already has a dated entry, ask before touching it. An entry for a
shipped version is a historical record.

## Phase 4: Validate

```bash
pwsh -File .azure-pipelines/scripts/validate-changelogs.ps1 \
  -SourceBranch "refs/heads/$(git branch --show-current)"
```

This is the same check CI runs. It fails when a `version.json` moved with no
matching `## [<version>]` heading, so it catches a bumped-but-undocumented
package - which is exactly the CI failure this skill is usually invoked to fix.

Pass `-SourceBranch`: locally `BUILD_SOURCEBRANCH` is unset, and the script then
compares against `v18/dev` whatever branch you are on. It finds bumped packages from
**committed** history and reads the changelog from disk, so it picks up an
uncommitted changelog fix for a committed bump, but not an uncommitted
`version.json` bump.

## Do not

- Commit or push unless asked. `/release-management` owns the release commit.
- Bump `version.json`. That is a release decision, not a changelog one.
- Write entries for packages the range did not touch.

## Error handling

- **No commits in range** - say so. Do not invent an entry.
- **Commits are not conventional** - group by reading the subjects, and say the
  grouping was inferred rather than parsed.
- **No tag for the package** - state the fallback you used for the range.
- **Version already documented** - stop and ask.

[kac]: https://keepachangelog.com/en/1.0.0/
