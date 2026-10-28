---
name: release-management
description: Prepares a release for one or more packages in this repo - detects which packages changed since their last release, recommends a version bump from the commit history, cuts the release branch, writes release-manifest.json, updates version.json, finalises the changelog entry, and pushes. Use when preparing a release on the v18 line.
---

# Release Manager

You prepare releases for **Umbraco.Forms.Integrations**.

## What makes this repo different

Ported from the Umbraco.Cms.Integrations skill of the same name (itself adapted
from `Umbraco.AI`'s), because this repo now uses the same build and release setup:

- **v18 only.** Only `v18/dev` / `main-v18` use `version.json`, the release
  manifest and the single pipeline. The v17 line still has a `<Version>` in each
  csproj and one pipeline per package, so this skill does not apply to it. If the
  user wants a v17 release, say so and stop.
- **Packages are independent siblings.** None references another. There is no
  dependency cascade, no forced bumps, no `Directory.Packages.props` range
  updates and no npm peer-dependency syncing. If you find yourself reasoning
  about "which other packages does this one affect", stop - the answer is none.
  (`Automation.Zapier` depends on the external NuGet package
  `Umbraco.Cms.Integrations.Automation.Zapier`; that is not a sibling.)
- **`release-manifest.json` decides what ships.** It is required on
  `v<N>/release/*`, optional on `v<N>/hotfix/*`, and its `include` list replaces
  the build set outright. Change detection in `detect-changes.ps1` is only a
  cross-check: anything it finds changed must appear in `include` or `exclude`,
  or CI fails.

  A bumped `version.json` is **not** the declaration, and selecting on it does
  not work. `/post-release-cleanup` bumps each released package's patch on dev,
  which would make every previously-released package look like it was shipping
  on every subsequent release branch, while a package whose version had never
  moved could not be selected at all.
- **The version is usually already bumped.** Because of that same dev patch bump,
  the target version is often already in `version.json` when you cut the branch.
  Check before editing.
- **`Client/public/umbraco-package.json` is a fixed `1.0.0` placeholder.** Do not
  bump it. The real version is stamped into the `wwwroot` copy by CI. Editing it
  would also make the package look changed to release detection. (Zapier has no
  client at all.)
- **Tags are `release/<slug>-<version>`**, e.g. `release/crm-hubspot-9.1.0` -
  not `Product@Version`. Note the branch is `v18/release/<date>` while tags are
  `release/<slug>-<version>`; they live in different ref namespaces and do not
  collide.
- **`main-v<N>`, not `v<N>/main`.** This repo names its released branch
  `main-v18`; `Umbraco.AI`/`Umbraco.Automate` use `v18/main`. The
  `publicReleaseRefSpec` entries reflect that difference.

## Task

0. Check `main-v18` has been seeded with the released state.
1. Detect which packages changed since their last release.
2. Recommend a version bump per package from the commit history.
3. Confirm with the user.
4. **Cut the release branch** - before any file changes.
5. Write `release-manifest.json`.
6. Update each package's `version.json`, where it is not already correct.
7. Finalise each package's `CHANGELOG.md` entry.
8. Review the changelogs for noise and completeness.
9. Commit, validate, dry-run the selection, and push the release branch.
10. Report what remains manual.

---

## Phase 0: Check `main-v18` is the released state

Every release tool compares against `main-v18`: detection diffs from
`merge-base(main-v18, HEAD)`, `validate-changelogs.ps1` checks every bump since
`main-v18`, and `/post-release-cleanup` merges into it. When this process was
introduced, `main-v18` was still identical to `main-v17` and had no `version.json`
files, so all of that would compare against the v17 code.

```bash
git fetch origin --tags
for d in src/*/; do
  [ -f "$d/version.json" ] || continue
  p=$(basename "$d")
  git show "origin/main-v18:src/$p/version.json" 2>/dev/null \
    | grep '"version"' || echo "MISSING on main-v18: $p"
done
```

**If any package is missing**, stop and tell the user: `main-v18` must first be
brought up to the released state and seeded with each package's `version.json` at
its **released** version (Zapier `8.0.0`, ActiveCampaign `7.0.0`, Hubspot `9.0.0`
for the first v18 release), the equivalent of
[Umbraco.Cms.Integrations#317](https://github.com/umbraco/Umbraco.Cms.Integrations/pull/317).
Not the dev versions: `main-v18` is a public release ref, so a build there would
pack whatever version it holds as a clean, unreleased version. Doing that seeding is
a change to a shared branch and is not part of this skill.

If every package is present, check each value is the last released version, not
a dev one. Then continue.

---

## Phase 1: Detect changed packages

Confirm the current branch is the dev branch:

```bash
git branch --show-current    # expect v18/dev
```

If it is `main-v18`, stop and tell the user - releases are prepared on the dev
branch (a hotfix is cut from `main-v18`; confirm which they mean). If it is
`v17/dev` or `main-v17`, stop: that line is not on this process yet.

Discover the packages (any `src/` folder with both a `.csproj` and a
`version.json`):

```bash
ls -d src/*/ | while read d; do
  [ -f "$d/version.json" ] && basename "$d"
done
```

That gives `Umbraco.Forms.Integrations.Automation.Zapier`,
`Umbraco.Forms.Integrations.Crm.ActiveCampaign` and
`Umbraco.Forms.Integrations.Crm.Hubspot`. The OAuth proxy has no `version.json` and
is not a package.

For each package, find its last release tag on this line and the commits since.
The tag slug is the package name minus the `Umbraco.Forms.Integrations.` prefix,
lowercased, with dots replaced by hyphens: `automation-zapier`,
`crm-activecampaign`, `crm-hubspot`. Filter on the line's major (Zapier `8`,
ActiveCampaign `7`, Hubspot `9` on v18), or the highest tag may be from another
line.

```bash
git tag --list "release/<slug>-<major>.*" --sort=-version:refname | head -n1
git log <tag>..HEAD --oneline -- src/<PackageName>/
```

**No tag on this line** - say so explicitly. Use
`$(git merge-base origin/main-v18 HEAD)` as the start of the range instead, which
is what CI's detection uses, and take the last released version from the newest
dated `## [x.y.z] - YYYY-MM-DD` heading in the package's `CHANGELOG.md`. Zapier
`8.0.0` was published without a tag, so this applies to Zapier until
`/post-release-cleanup` (or a backfill) tags a Zapier `8.x` release.

Ignore changes that are only to `CHANGELOG.md` or `version.json` - those are
release-prep artifacts, not substantive changes.

Present what you found:

```
Changes since last release:

| Package           | Last release | Range from          | Commits since             |
|-------------------|--------------|---------------------|---------------------------|
| Crm.Hubspot       | 9.0.0        | release/crm-hubspot-9.0.0 | 3 commits (2 fix, 1 feat) |
| Automation.Zapier | 8.0.0        | main-v18 merge-base (no tag) | 1 commit (1 fix)   |
```

---

## Phase 2: Recommend a version bump

For each changed package, read its commits and apply this rule (highest wins):

| Commit signal | Bump |
|---|---|
| `BREAKING CHANGE:` in the body, or `!` after the type/scope | **Major** |
| `feat:` | **Minor** |
| `fix:` or `perf:` | **Patch** |
| Only `docs:` / `chore:` / `refactor:` / `test:` / `ci:` | **Ask the user** (default: no bump) |

Apply the bump to the **last released version** from Phase 1, not to the value in
`version.json`. On dev, `version.json` already carries the post-release patch bump
(`9.0.1` after `9.0.0`), so bumping it again would skip a version:

- Major: increment X, reset Y and Z to 0
- Minor: increment Y, reset Z to 0
- Patch: increment Z (usually what `version.json` already says)

**Never** change a package's major to a number that does not match its Umbraco
line. Each package keeps one major per Umbraco major (Crm.Hubspot `8.x` = v17,
`9.x` = v18). If the commits genuinely warrant a major bump within a line, raise
it with the user rather than silently jumping the major - it would collide with
the next Umbraco major's numbering.

Present the recommendation:

```
| Package           | Released | version.json | Proposed | Reason        |
|-------------------|----------|--------------|----------|---------------|
| Crm.Hubspot       | 9.0.0    | 9.0.1        | 9.1.0    | 1 feat, 2 fix |
| Automation.Zapier | 8.0.0    | 8.0.1        | 8.0.1    | 1 fix         |
```

---

## Phase 3: Confirm with the user

Use **AskUserQuestion** to confirm. Options:

- **Use the recommended versions** (default)
- **Adjust individual versions** - ask per package, validate the `X.Y.Z` format,
  warn if it breaks the one-major-per-Umbraco-major convention
- **Drop a package from this release**
- **Cancel**

Do not proceed until the user has confirmed.

---

## Phase 4: Cut the release branch

**Do this before touching any file.** The bump belongs on the release branch, not
on `v18/dev`.

Why it matters: `version.json` sets `publicReleaseRefSpec` to `main-v18`,
`v18/release/*` and `v18/hotfix/*`. NBGV only drops the preview suffix on a branch
matching one of those:

| Branch built | Version produced |
|---|---|
| `v18/release/2026.11.1` | `9.1.0` ← publishable |
| `main-v18` | `9.1.0` |
| `v18/dev`, feature branches | `9.1.0--preview.4.gabc1234` |

So the release branch is what produces the artifact you publish. This is the same
arrangement `Umbraco.AI` and `Umbraco.Automate` use.

**Naming:** `v<N>/release/YYYY.MM.N` - calendar-based, matching the sibling repos.
It is independent of the package versions: one release branch can carry several
packages at different versions.

**`N` counts release events across the whole repo, not per line.** It comes from
the date tags, which have no line prefix, so once v17 is on this process a v17
release and a v18 release share one sequence. `Umbraco.Automate` and `Umbraco.AI`
both work this way.

Do **not** derive `N` from branch names. That gives two lines a `.1` in the same
month, and the collision only shows up later when the date tags are created.

Work out the name:

```bash
git branch --show-current            # the line prefix, e.g. v18/dev -> v18
date +%Y.%m                          # e.g. 2026.11

git fetch origin --tags
git tag --list "2026.11.*" | sort -V | tail -1   # highest release event this month
```

`git tag --list "2026.11.*"` matches only date tags, never the
`release/<slug>-<version>` package tags. Take the highest trailing number and add
one; start at `1` if the month has none (no date tags existed when this process was
introduced, so the first release is `.1`). Confirm the name with the user, then:

```bash
git checkout -b v18/release/2026.11.1
```

The matching date tag `2026.11.1` is created at release time by
`/post-release-cleanup`, next to the per-package tags. It is what lets the next
release pick the right number, so skipping it breaks the sequence for whoever
releases next.

For an urgent fix on top of an already-released state, cut
`v18/hotfix/YYYY.MM.N` from `main-v18` instead of from dev.

---

## Phase 5: Write the release manifest

Still on the release branch, before touching versions. **This is what CI treats as
the decision on what ships** - see `Get-ManifestSelection` in
`.azure-pipelines/scripts/detect-changes.ps1`. Change detection there is only a
cross-check.

```bash
pwsh -File scripts/generate-release-manifest.ps1 \
  -Include Umbraco.Forms.Integrations.Crm.Hubspot,Umbraco.Forms.Integrations.Automation.Zapier
```

Full package folder names. The script validates them against the real package
list, so a typo fails here rather than in CI, and it is case-insensitive. With no
arguments it shows a numbered menu.

If any package has substantive changes but is deliberately **not** shipping, name
it in `-Exclude`. CI fails when a changed package appears in neither list - that
guard is the whole point, so do not work around it by dropping the package from
the release without saying so.

`release-manifest.json` is **required** on `v<N>/release/*` and optional on
`v<N>/hotfix/*`. `/post-release-cleanup` deletes it before merging back, so it
never reaches `main-v<N>` or `v<N>/dev`.

---

## Phase 6: Update the version files

For each confirmed package, edit the `version` field of
`src/<PackageName>/version.json`. Leave every other property untouched. This is
the only file that drives the version.

**Usually it is already correct.** `/post-release-cleanup` bumps each released
package's patch on dev after a release, so for a patch release the target version
is already sitting there. Check before editing.

Do **not** touch:

- the `.csproj` (it has no `<Version>` - the version comes from `version.json`)
- `Client/public/umbraco-package.json` - it is a fixed `1.0.0` placeholder and is
  not maintained by hand. The real version is stamped into the `wwwroot` copy by
  `UpdatePackageManifestVersion` in the root `Directory.Build.targets`. Bumping it
  would also make the package look changed to release detection.
- `wwwroot/umbraco-package.json` - build output, gitignored, stamped on CI builds

---

## Phase 7: Write the changelog entry

**Check for an existing entry first.** `/post-release-cleanup` leaves an
`## [<version>] - Unreleased` heading on dev for the version being worked on, so
the entry for the version you are releasing usually already exists. **Finalise it
- do not add a second one**, or the changelog ends up with two headings for the
same version.

If the release is a minor or major bump, the `Unreleased` heading carries the
patch version (`9.0.1`). Rename that heading to the version you are releasing
(`9.1.0`) rather than leaving an orphan `9.0.1` entry behind.

Finalising means:

- Replace `Unreleased` with today's real date. Get it with `date +%Y-%m-%d` - do
  not guess.
- Delete the `### Internal` / "Development version bump after the X release."
  bullet. That is bookkeeping, not a released change.
- Keep any real bullets already there (contributors add them under the
  `Unreleased` heading) and add the rest.

Only write a fresh entry when there is genuinely no heading for that version.
Either way the result is one entry, in [Keep a Changelog][kac] format:

```markdown
## [9.1.0] - 2026-11-12

### feat

* **hubspot:** Map form fields to custom contact properties ([abc1234](https://github.com/umbraco/Umbraco.Forms.Integrations/commit/abc1234))

### fix

* **hubspot:** Handle an expired OAuth token when loading contact properties ([def5678](https://github.com/umbraco/Umbraco.Forms.Integrations/commit/def5678))
```

Rules:

- Group by commit type, using the type as the heading (`feat`, `fix`, `perf`).
- One bullet per user-facing change, taken from the commit subject.
- A breaking change gets its own `### Breaking` section, listed first.

---

## Phase 8: Review the changelogs

Before committing, review each entry you just wrote.

**Strip noise:**

- `refactor:`, `chore:`, `docs:`, `test:`, `ci:`, `build:` entries - these are not
  user-facing and must not appear in a public changelog. (An `### Internal` note
  is acceptable when a release would otherwise have an empty entry.)
- `Co-Authored-By:` lines and other commit-body metadata that leaked in.
- PR merge-commit subjects (`Merge pull request #123 from ...`).
- Test-only or build-only fixes.

**Check completeness** - compare the entry against what actually changed:

```bash
git diff <start>..HEAD --name-only -- src/<PackageName>/ | grep -vE 'CHANGELOG.md|version.json'
```

`<start>` is the tag or merge-base from Phase 1. If a file changed but nothing in
the changelog explains it, either add an entry or satisfy yourself it is genuinely
internal. Commits scoped to another package can still touch this one - those are
the ones most often missed. A `Directory.Packages.props` change (an Umbraco floor,
or the `Umbraco.Cms.Integrations.Automation.Zapier` version) does not show in a
path-scoped log but can affect the package; check the root file too.

**Empty entries:** if a package's only changes are build/tooling, recommend
dropping it from the release and reverting its `version.json`.

Present the review, then apply fixes:

```
Changelog review:

Needs attention
- Crm.Hubspot: "Fix failing tests" is internal-only, removing
- Automation.Zapier: no entry for the Directory.Packages.props bump of
  Umbraco.Cms.Integrations.Automation.Zapier

Clean
- Crm.ActiveCampaign
```

---

## Phase 9: Commit, validate, dry-run and push the release branch

Commit locally first. Both scripts below diff committed history
(`<base>..HEAD`), so they do not see uncommitted edits - run before the commit,
the changelog check passes without having looked at your bumps.

```bash
git add release-manifest.json src/*/version.json src/*/CHANGELOG.md
git commit -m "chore(release): Prepare release 2026.11.1

- Umbraco.Forms.Integrations.Crm.Hubspot: 9.0.0 -> 9.1.0
- Umbraco.Forms.Integrations.Automation.Zapier: 8.0.0 -> 8.0.1"
```

`release-manifest.json` must be in the commit, or CI fails on the release branch.
Do not add `src/*/Client/public/umbraco-package.json` - it is a placeholder and a
release does not touch it.

Use the **release branch name** in the commit subject, not the package versions -
one branch can carry several packages at different versions.

Then run the same changelog check CI runs, so a failure surfaces here rather than
in the pipeline:

```bash
pwsh -File .azure-pipelines/scripts/validate-changelogs.ps1 \
  -SourceBranch "refs/heads/v18/release/2026.11.1"
```

Without `-SourceBranch` it reads `BUILD_SOURCEBRANCH`, which is unset locally, and
falls back to comparing against `v18/dev`; passing the release branch makes it
compare against `main-v18` as CI does. If it fails, fix the changelog and add a
commit before pushing.

**Then dry-run the selection.** There are no automated tests for the selection
logic, so this is the only check that what ships is what you meant:

```bash
pwsh -File .azure-pipelines/scripts/detect-changes.ps1 \
  -SourceBranch "refs/heads/v18/release/2026.11.1" \
  -ReleaseCompareRef "origin/main-v18"
```

Read the `Build plan:` block and confirm the `BUILD` lines are exactly the
packages you intend to publish. Watch for two things:

- A `note: <package> has no substantive changes but is being released` line.
  Legitimate for a deliberate no-op release, suspicious otherwise.
- A throw about a changed package missing from the manifest. Add it to `include`
  or `exclude` - do not delete the guard.

If every package shows as changed with a large file count, `main-v18` is probably
not the released state - go back to Phase 0.

Show the user that block, then push:

```bash
git push -u origin v18/release/2026.11.1
```

Pushing a new release branch is safe: it touches no shared branch, and nothing is
published until a human promotes the artifact. `main-v18` and `v18/dev` are left
untouched until `/post-release-cleanup` runs.

CI will then build this branch and produce **clean-versioned** artifacts, for
exactly the packages named in the manifest's `include` list. That needs the
`azure-pipelines.yml` pipeline definition to exist in the Azure DevOps project
"Umbraco Forms Integrations"; the old per-package definitions do not build this
YAML. If no build starts, check that first.

---

## Phase 10: Report what happens next

```
Release branch v18/release/2026.11.1 pushed, carrying:
  - Crm.Hubspot       9.0.0 -> 9.1.0
  - Automation.Zapier 8.0.0 -> 8.0.1

main-v18 and v18/dev are untouched.

Next:
  1. Wait for CI on v18/release/2026.11.1. Confirm the artifacts are
     clean-versioned (9.1.0, NOT 9.1.0--preview.N) before going further.
  2. Promote each artifact to NuGet. This is the one step no skill does.
  3. Run /post-release-cleanup on this branch. It verifies the packages are
     on NuGet, creates the tags and the GitHub releases, deletes the manifest,
     merges into main-v18 and v18/dev, and bumps the dev patches.
```

**Do not tag, and do not create GitHub releases here.** They belong to
`/post-release-cleanup`, and the reason is timing: at this point CI has not built
anything, so nothing is published for a tag to point at. A tag created now becomes
a lie the moment someone decides not to promote the artifact.

That also means **do not** offer to tag as a convenience if the user asks "what's
left" - point them at `/post-release-cleanup` instead, so the tag, the GitHub
release and the merge-back all happen from one place with one confirmation.

> The two tag shapes answer different questions. `release/<slug>-<version>` is
> "which commit is this published package", one per package. `YYYY.MM.N` is "which
> commits went out together", one per release branch. They share the tag namespace
> but cannot collide: one starts with `release/`, the other with a digit.

## Error handling

- **`main-v18` not seeded** - stop at Phase 0. Seeding it is a separate change.
- **No changes detected for any package** - report it and ask whether to release
  anyway (a deliberate no-op release is occasionally wanted).
- **No release tag for a package on this line** - fall back to the `main-v18`
  merge-base and the changelog's last dated heading, and say so.
- **Malformed `version.json`** - report it and stop; do not guess the format.
- **Started from `main-v18`** - a normal release is cut from `v18/dev`. Only a
  hotfix is cut from `main-v18`; confirm which the user means.
- **Started from the v17 line** - not on this process yet. Stop and say so.
- **A release branch for this month already exists** - it may be an in-flight
  release. Show it and ask whether to add to it or start the next number.
- **The dry-run throws about an unaccounted changed package** - a package has
  substantive changes and is in neither `include` nor `exclude`. Decide which it
  belongs in with the user and regenerate the manifest. Never silence the guard.
- **The dry-run notes a package with no substantive changes** - fine for a
  deliberate no-op release, or when rebuilding an already merged-back release
  branch. Otherwise it is probably in `include` by mistake.

[kac]: https://keepachangelog.com/en/1.0.0/
