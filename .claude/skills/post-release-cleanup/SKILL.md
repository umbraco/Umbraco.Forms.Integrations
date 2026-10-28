---
name: post-release-cleanup
description: Closes out a release - verifies the packages are on NuGet, creates the release tags and GitHub releases, merges the release branch into main-v<N> and back into v<N>/dev, bumps the patch of each released package on dev so preview builds sort above the release, and optionally deletes the release branch. Use once a v18 release has been promoted to NuGet.
---

# Post-Release Cleanup

You close out a release for **Umbraco.Forms.Integrations**.

## Where this sits in the flow

```
1. /release-management        - cuts v18/release/YYYY.MM.N, writes
                               release-manifest.json, bumps version.json
                               + CHANGELOG there, pushes the branch
2. CI builds the release branch -> clean 9.1.0 artifacts
3. Promote to NuGet           (manual - the only manual step left)
4. THIS SKILL                 - verify on NuGet, tag, GitHub releases,
                               delete the manifest, merge back, bump dev
                               patches, tidy up
```

This process applies to the **v18 line only**. The v17 line still uses one
pipeline per package and a `<Version>` in each csproj; if the user asks to clean up
a v17 release, say this skill does not cover it.

## When to run

Once the packages have been **promoted to NuGet**. That is the only thing this
skill cannot do for you.

Tagging and the GitHub releases used to be manual steps after publishing, and they
are the steps that get forgotten: Zapier `8.0.0` went to NuGet with no tag, and
none of the v18 `.0.0` releases has a GitHub release. A step owned by no skill is
a step nobody does, so this skill owns it.

They cannot move into `/release-management` instead. That runs before CI has built
anything, so there is nothing published for a tag to point at, and a tag created
then becomes a lie if the artifact is never promoted.

## What identifies the release

`release-manifest.json`, on the release branch. Its `include` list is what
`/release-management` declared and what CI actually built and packed, so it is the
same answer CI used - no inference, no slug guessing.

This skill reads it in Phase 1 and deletes it in Phase 5, so it is available at
exactly the moment it is needed. **Do not** work out what shipped by reading tags:
this skill creates those, so at Phase 1 they do not exist yet.

## Why the version bump matters

NBGV plus `Umbraco.GitVersioning.Extensions` produces preview versions like
`9.1.0--preview.4.gabc1234` on `v18/dev`. That sorts **below** the stable `9.1.0`
in SemVer, so without bumping the patch on dev after a release, dev builds look
older than what just shipped and are useless for testing.

## What this repo does NOT need

Adapted from `Umbraco.AI`'s skill of the same name (via Umbraco.Cms.Integrations),
minus:

- **Major-version cutover.** A new Umbraco major gets a deliberately created
  `main-v<N>` / `v<N>/dev` pair as a planning decision. Never create those branches
  or change the repo's default branch from this skill.
- **Git hooks.** `Umbraco.AI` deletes `release-manifest.json` with a `.githooks`
  post-merge hook. This repo has no `.githooks` directory, and adding one would
  mean every contributor has to set `core.hooksPath` or the cleanup silently stops
  happening. Phase 5 does it explicitly instead.

---

## Phase 1: Identify the release, and verify it shipped

```bash
git branch --show-current
git fetch origin --tags
```

Expect to be on `v18/release/*` or `v18/hotfix/*`. If not, ask the user which
release branch to process. Derive the line from the branch name
(`v18/release/2026.11.1` → line `v18`, released branch `main-v18`, dev `v18/dev`).

Read what shipped from the manifest:

```bash
release_branch=$(git branch --show-current)
cat release-manifest.json
```

Every name in `include` is a released package. For each one, read the version from
`src/<PackageName>/version.json` on this branch. That pairing - name plus version -
is what everything below is built from.

**If `release-manifest.json` is missing**, stop and offer choices. Do not guess:

```
No release-manifest.json on v18/release/2026.11.1.

It is required on a release branch, so either this cleanup has already run
(Phase 5 deletes it), or the branch was cut by hand. On a hotfix branch it
is optional.

Options:
  - Tell me which packages and versions were released, and I will use those
  - Check whether the tags already exist, and skip to the merges
  - Cancel
```

**Verify each package is actually on NuGet.** Make the check against the registry
rather than against memory:

```bash
pkg=Umbraco.Forms.Integrations.Crm.Hubspot
version=9.1.0

curl -s "https://api.nuget.org/v3-flatcontainer/$(echo "$pkg" | tr 'A-Z' 'a-z')/index.json" \
  | grep -q "\"$version\"" && echo "on NuGet" || echo "NOT VISIBLE"
```

The package id is the folder name, lowercased - the NuGet API needs lower case.

A hit means it is published. A miss means one of two things, and they need
different responses:

- **Not promoted yet.** Stop. Tagging and merging now would leave `main-v18`
  claiming a release that does not exist.
- **Promoted, but NuGet has not indexed it yet.** Indexing lags behind an upload
  by minutes, and all three of NuGet's indexes (flat container, registration,
  search) can lag together.

So report the miss, say plainly that absence here is not proof it was never
pushed, and let the user decide:

```
Not visible on NuGet:
  - Umbraco.Forms.Integrations.Crm.Hubspot 9.1.0

NuGet indexing lags an upload by a few minutes, so this may be timing rather
than a missing promotion.

Options:
  - Wait and re-check
  - I have confirmed it is pushed, proceed anyway
  - Cancel
```

---

## Phase 2: Confirm the whole plan

**One gate, covering everything outward-facing.** Show exactly what will be
created and changed, then use **AskUserQuestion**. Nothing below this point runs
until the user says yes.

```
Release branch: v18/release/2026.11.1
On NuGet:       Crm.Hubspot 9.1.0, Automation.Zapier 8.0.1

Will create and push these tags:
  release/crm-hubspot-9.1.0
  release/automation-zapier-8.0.1
  2026.11.1                      (release event, repo-wide)

Will create these GitHub releases:
  release/crm-hubspot-9.1.0        body from Crm.Hubspot CHANGELOG [9.1.0]
  release/automation-zapier-8.0.1  body from Automation.Zapier CHANGELOG [8.0.1]
  (the date tag gets no GitHub release - it is bookkeeping)

Then:
  1. delete release-manifest.json on the release branch
  2. merge v18/release/2026.11.1 -> main-v18
  3. merge v18/release/2026.11.1 -> v18/dev
  4. bump on v18/dev: 9.1.0 -> 9.1.1, 8.0.1 -> 8.0.2

Proceed?
```

Tags and GitHub releases are public and awkward to retract, which is why they get
a gate. It is one gate rather than several: the decision is the same decision, and
splitting it just interrupts a flow the user has already agreed to.

---

## Phase 3: Create and push the tags

Two shapes, both annotated, both on the release branch.

**One per released package**, `release/<slug>-<version>`. The slug is the package
name minus the `Umbraco.Forms.Integrations.` prefix, lowercased, dots to hyphens:

| Package | Slug |
|---|---|
| `Umbraco.Forms.Integrations.Automation.Zapier` | `automation-zapier` |
| `Umbraco.Forms.Integrations.Crm.ActiveCampaign` | `crm-activecampaign` |
| `Umbraco.Forms.Integrations.Crm.Hubspot` | `crm-hubspot` |

> Tags from before the `release/` namespace use `release-<name>-<version>` (e.g.
> `release-crm-hubspot-2.0.0`, `release-oauthproxy-1.1.0`). Never create that
> shape; when you need to find an *existing* tag, use `git tag --list` rather than
> deriving it.

**One for the release event**, `YYYY.MM.N`, matching the release branch name. It is
repo-wide and carries no line prefix. It is how the *next* release works out its
number, so skipping it breaks the sequence for whoever releases next.

```bash
git checkout "$release_branch"

git tag -a release/crm-hubspot-9.1.0       -m "Crm.Hubspot 9.1.0"
git tag -a release/automation-zapier-8.0.1 -m "Automation.Zapier 8.0.1"
git tag -a 2026.11.1                       -m "Release 2026.11.1"

git push origin release/crm-hubspot-9.1.0 release/automation-zapier-8.0.1 2026.11.1
```

Then confirm each tag points at the release branch tip:

```bash
for t in release/crm-hubspot-9.1.0 release/automation-zapier-8.0.1 2026.11.1; do
  echo "$t -> $(git rev-parse --short "$t^{commit}")"
done
```

The two tag shapes answer different questions and cannot collide: one starts with
`release/`, the other with a digit.

---

## Phase 4: Create the GitHub releases

One per **package** tag. The date tag gets none - it is bookkeeping, not a thing
anyone downloads.

Title is the tag name. Body is that version's changelog section, so the release
notes and the changelog cannot drift apart:

```bash
pkg=src/Umbraco.Forms.Integrations.Crm.Hubspot
version=9.1.0
tag=release/crm-hubspot-9.1.0

# Everything between this version's heading and the next one. index(...) == 1
# anchors to the start of the line without needing regex escaping for the
# brackets, which is what makes this survive a version like 9.1.0.
awk -v v="$version" '
  index($0, "## [" v "]") == 1 { found=1; next }
  found && index($0, "## [") == 1 { exit }
  found { print }
' "$pkg/CHANGELOG.md" > /tmp/notes.md

test -s /tmp/notes.md || { echo "No changelog body for $version - stopping"; exit 1; }

gh release create "$tag" \
  --repo umbraco/Umbraco.Forms.Integrations \
  --title "$tag" \
  --notes-file /tmp/notes.md \
  --verify-tag
```

Two guards worth keeping:

- `test -s` catches an empty body. That means the changelog heading and
  `version.json` disagree, which is a real problem - publishing an empty release
  note hides it. Stop and show the user the changelog.
- `--verify-tag` makes `gh` fail rather than create the tag itself, so a typo
  cannot produce a release pointing at a tag nobody reviewed.

Add PR links to the body where they help. Do not try to derive them from commit
messages; ask the user if it is not obvious.

---

## Phase 5: Delete the manifest, then merge into main-v&lt;N&gt;

`release-manifest.json` belongs only to the release branch. It is **required** on
`v<N>/release/*`, so leaving it in place would carry it onto `main-v<N>` and
`v<N>/dev`, where it is meaningless and where the next release branch would
inherit a stale copy naming last release's packages.

Delete it on the release branch first, so both merges are clean:

```bash
git checkout "$release_branch"
git rm release-manifest.json
git commit -m "chore(release): Remove release manifest after publishing"
git push origin "$release_branch"
```

If the file is already absent, say so and move on - a hotfix release may not have
had one.

Then merge. Confirm before the first push - this updates a shared branch.

```bash
git checkout main-v18
git pull origin main-v18
git merge "$release_branch" --no-ff -m "Merge $release_branch into main-v18"
git push origin main-v18
```

`main-v18` now matches what shipped.

**The first release after the switch conflicts on every `version.json`, by design.**
`main-v18` was seeded with its own `version.json` files, which share no history with
the ones on `v18/dev`, so git reports `CONFLICT (add/add)` on each package's
`version.json` and nothing else (checked by simulating it). Resolve each one from
the `release-manifest.json` lists read in Phase 1:

- Package in `include` - take the **release branch's** side: it holds the version
  that just shipped. Keeping `main-v18`'s side would record the previous release.
- Package in `exclude` - keep **`main-v18`'s** side. The release branch carries
  dev's unreleased version for it.

Any other conflict here is unexpected - stop and ask.

> If `main-v18` has no `version.json` files before this merge, it was never seeded
> with the released state (see `CLAUDE.md`). The merge brings `version.json` in, but
> it also brings the whole v18 history at once. Stop and tell the user rather than
> merging - seeding `main-v18` is a decision for them.

---

## Phase 6: Merge into v&lt;N&gt;/dev

The release branch carries the bumped `version.json` and changelog entries, so dev
needs them too or the next release starts from stale versions.

```bash
git checkout v18/dev
git pull origin v18/dev
git merge "$release_branch" --no-ff -m "Merge $release_branch into v18/dev"
```

Resolve conflicts as:

- `version.json` - keep the **higher** version (Phase 7 overwrites it anyway).
- `CHANGELOG.md` - keep **both** sets of entries, newest version first.
- `release-manifest.json` - delete it. It must not exist on dev. Phase 5 should
  have removed it already, so seeing it here means that step was skipped.
- Anything else - ask the user.

Never force-push. If a push is rejected, pull and retry the merge.

---

## Phase 7: Bump the patch on dev

For each released package, increment the patch in `src/<PackageName>/version.json`:

- `9.1.0` → `9.1.1`
- `8.0.1` → `8.0.2`
- Dotted pre-release `10.0.0-beta.2` → `10.0.0-beta.3`
- Pre-release with no number `10.0.0-alpha` → `10.0.0-alpha.1`

Always bump the **patch**, whatever kind of bump the release itself was. This is
mechanical - it only exists to keep preview builds sorting above the release.

**Do not touch `Client/public/umbraco-package.json`.** It is a fixed `1.0.0`
placeholder; the real version is stamped into the `wwwroot` copy by
`UpdatePackageManifestVersion` on CI builds. Bumping it would make the package look
changed to release detection.

> **Changelog gate.** CI fails a version bump with no matching changelog entry. A
> bare patch bump has no user-facing content, so add a short entry:
> ```markdown
> ## [9.1.1] - Unreleased
>
> ### Internal
>
> * Development version bump after the 9.1.0 release.
> ```
> Keep the literal word `Unreleased` and the `### Internal` bullet.
> `/release-management` finalises this same entry at the next release, replacing
> the word with a date and dropping the bullet - it does not add a second heading.

Commit, check, then push:

```bash
git add src/*/version.json src/*/CHANGELOG.md
git commit -m "chore(release): Bump dev versions after release 2026.11.1

- Umbraco.Forms.Integrations.Crm.Hubspot: 9.1.0 -> 9.1.1
- Umbraco.Forms.Integrations.Automation.Zapier: 8.0.1 -> 8.0.2"

pwsh -File .azure-pipelines/scripts/validate-changelogs.ps1 -SourceBranch refs/heads/v18/dev

git push origin v18/dev
```

The check runs after the commit because it diffs committed history: with
`-SourceBranch refs/heads/v18/dev` it compares `HEAD~1..HEAD`, which is exactly the
bump commit, as CI does on a `v18/dev` push. Run before the commit, it would not see
the bump.

---

## Phase 8: Delete the release branch (optional)

Ask the user:

```
Delete v18/release/2026.11.1?
  - Local and remote
  - Local only
  - Keep it
```

The tags are what preserve the released history, so deleting the branch loses
nothing. Only delete it once both merges are pushed.

```bash
git branch -d v18/release/2026.11.1
git push origin --delete v18/release/2026.11.1
```

Use `-d`, not `-D`: if git refuses because the branch is unmerged, that is a real
signal one of the merges did not land - investigate rather than forcing it.

---

## Phase 9: Summary

```
Post-release cleanup complete.

Tagged:
  release/crm-hubspot-9.1.0
  release/automation-zapier-8.0.1
  2026.11.1

GitHub releases:
  release/crm-hubspot-9.1.0
  release/automation-zapier-8.0.1

Merged:
  v18/release/2026.11.1 -> main-v18
  v18/release/2026.11.1 -> v18/dev

Dev version bumps on v18/dev:
  - Crm.Hubspot:       9.1.0 -> 9.1.1
  - Automation.Zapier: 8.0.1 -> 8.0.2

Release branch: deleted / kept

Preview builds on v18/dev now sort above the released versions.
```

Verify rather than assert. Before reporting done:

```bash
git ls-remote --tags origin | grep -E "2026.11.1|crm-hubspot-9.1.0"
gh release list --repo umbraco/Umbraco.Forms.Integrations --limit 5

for b in origin/main-v18 origin/v18/dev; do
  git cat-file -e "$b:release-manifest.json" 2>/dev/null \
    && echo "LEAKED on $b" || echo "clean: $b"
done
```

Then flag anything still outstanding.

## Error handling

- **`release-manifest.json` missing** - stop and offer the Phase 1 choices. It is
  the only thing that says what shipped.
- **A package is not visible on NuGet** - offer the Phase 1 choices. Indexing lag
  and a missing promotion look identical from here, so do not decide for the user.
- **A tag already exists** - the cleanup may have partly run. Check what it points
  at. If it is the release branch tip, skip creating it and carry on; if not, stop.
  Never move an existing tag.
- **`gh release create` fails on `--verify-tag`** - the tag was not pushed. Push
  Phase 3's tags first, then retry.
- **The changelog body comes out empty** - the heading and `version.json` disagree.
  Stop and show the user; do not publish an empty release note.
- **`main-v18` has no `version.json`** - it was never seeded. Stop before merging.
- **Merge conflicts** - resolve per the rules in Phase 6, ask about the rest.
- **Push rejected** - the branch is protected, or someone else pushed. Pull and
  retry. Never force-push.
- **Malformed `version.json`** - show the user and ask; do not guess.
