# Contributing to Umbraco.Forms.Integrations

This repository holds the official Umbraco Forms integration packages. Each package
is independent: it has its own version, ships its own NuGet package, and is
released on its own schedule. There is no shared runtime library between them.

> **Which branch?** This guide describes the **v18 line** (`main-v18` / `v18/dev`).
> The v17 line (`main-v17` / `v17/dev`) still uses the older manual process — a
> `<Version>` in each csproj and one pipeline per package — and will be brought onto
> this setup later. Older lines are unchanged.

---

## Branches

| Branch | Purpose |
|---|---|
| `v18/dev` | Working branch. All changes land here first. |
| `main-v18` | Reflects the **last released** state. Never commit working changes here. |
| `v18/feature/*` | Feature branches, cut from and merged back to `v18/dev`. |
| `v18/release/*` | Cut from `v18/dev` per release; builds the publishable artifacts. |
| `v18/hotfix/*` | Cut from `main-v18` for an urgent fix on the released state. |

The flow is: work on a feature branch → PR into `v18/dev` → cut a release branch
from `v18/dev` → publish from there → merge the release branch back into both
`main-v18` and `v18/dev`.

---

## Getting set up

```bash
git clone https://github.com/umbraco/Umbraco.Forms.Integrations.git
cd Umbraco.Forms.Integrations
git checkout v18/dev
npm ci                                  # installs every package's client, once
npm run build                           # builds them into each package's wwwroot
dotnet build Umbraco.Forms.Integrations.slnx
```

Requires the **.NET 10 SDK** and **Node 24+ / npm 11+**.

**The npm steps are not optional on a fresh clone.** `wwwroot` holds compiled client
assets and is *not* committed, so until you have run `npm run build` the packages have
no backoffice assets. `dotnet build` does not run npm for you; `dotnet pack` will fail
with a clear error rather than produce a package with nothing in it.

Every package's `Client/` folder is a workspace of the root `package.json`, so there is
a single `package-lock.json` for the whole repo. To rebuild just one:

```bash
npm run build --workspace hubspot-crm
```

Workspace names are `activecampaign-crm` and `hubspot-crm`. `Automation.Zapier` has no
client. `npm run watch` works the same way.

Use `examples/Umbraco.Forms.Integrations.Testsite.V18` to run the packages in a real
Umbraco instance. It references all three packages by project.

`src/Umbraco.Cms.Integrations.OAuthProxy` is not a package: it is an Azure web app
with its own deploy pipeline (`azure-pipeline - Cms.OAuthProxy.yml`), and it is not
in the solution.

---

## Tests

```bash
dotnet test Umbraco.Forms.Integrations.slnx
```

Test projects live under `tests/`, one per package that has them, named
`<PackageName>.Tests`. Coverage is currently partial — only `Crm.Hubspot` has tests,
covering `HubspotContactService`.

They use **NUnit + Moq**. HTTP calls are faked by mocking `HttpMessageHandler`
through `Moq.Protected`; see `tests/Umbraco.Forms.Integrations.Crm.Hubspot.Tests/Services/HubspotContactServiceTests.cs`.

CI runs the tests in their own stage, and **`Pack` depends on it** — a failing test
blocks packing, so nothing can be promoted to NuGet from a red build. Any project
added under `tests/` matching `*Tests.csproj` is picked up with no pipeline change.

---

## Dependency versions

Dependency versions are **centrally managed**. Do not put a `Version` attribute on
a `PackageReference` — it will fail the build with `NU1008`.

- Declare the version once in the root `Directory.Packages.props`.
- Reference it by name only in the `.csproj`:

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Umbraco.Cms.Integrations.Automation.Zapier" Version="6.0.0" />

<!-- the package's .csproj -->
<PackageReference Include="Umbraco.Cms.Integrations.Automation.Zapier" />
```

Umbraco CMS and Umbraco Forms take their version range from one property,
`UmbracoCmsPackageVersion`, so moving the Umbraco version is a one-line change that
applies to every package.

> `examples/Umbraco.Forms.Integrations.Testsite.V18/` and
> `src/Umbraco.Cms.Integrations.OAuthProxy/` each have their own
> `Directory.Packages.props` and are deliberately outside the root one.

---

## Versioning

Each package folder has a `version.json` ([Nerdbank.GitVersioning][nbgv]) holding
the full `major.minor.patch`. **That file is the only place a version is written by
hand.**

```jsonc
// src/Umbraco.Forms.Integrations.Crm.Hubspot/version.json
{
    "version": "9.0.1",
    "publicReleaseRefSpec": ["^refs/heads/main-v18$", "^refs/heads/v18/release/", "^refs/heads/v18/hotfix/"]
    // ...
}
```

Alongside it, `Directory.Build.props` pins `GitVersionBaseDirectory` to that
folder, so each package's version is computed from its own commits only — activity
in one package never moves another package's version.

### Which number to bump

Based on the commits affecting that package, using [Conventional Commits][cc]:

| Commits since the last release | Bump | Example |
|---|---|---|
| A breaking change (`feat!:`, or `BREAKING CHANGE:` in the body) | **Major** | `9.0.1` → `10.0.0` |
| `feat:` | **Minor** | `9.0.1` → `9.1.0` |
| `fix:` or `perf:` | **Patch** | `9.0.1` → `9.0.2` |
| Only `docs:` / `chore:` / `refactor:` | Usually none | — |

Each package keeps **one major per Umbraco major** (e.g. Crm.Hubspot `8.x` = v17,
`9.x` = v18). A new Umbraco major means a new major for every package, so a breaking
change within a line needs discussing first.

### The other version-carrying files

- **`<Version>` in the `.csproj`** — removed. The version comes from `version.json`.
  Do not add it back.
- **`wwwroot/umbraco-package.json`** — not in git at all. The Vite build copies it out
  of `Client/public`, and CI then stamps the computed version into it
  (`ContinuousIntegrationBuild=true`). Nothing to hand-edit.
- **`Client/public/umbraco-package.json`** — the *source* manifest, and the only one
  in git. Its version is a fixed `1.0.0` placeholder. **Do not bump it**: the real
  version is stamped into the `wwwroot` copy on CI builds, and editing the source
  copy makes the package look changed to release detection.

On `main-v18` and on release/hotfix branches a build produces a clean version
(`9.0.2`). On any other branch NBGV appends a preview suffix
(`9.0.2--preview.4.gabc1234`), so dev builds always sort below the release.

---

## Changelogs

Every package has a `CHANGELOG.md` in the [Keep a Changelog][kac] format. **If you
bump a `version.json`, add a matching entry** headed with the new version:

```markdown
## [9.1.0] - 2026-11-12

### feat

* **hubspot:** Map form fields to custom contact properties
```

CI fails the build if a `version.json` changed without a matching
`## [<version>]` entry. Only bumped packages are checked. After a release, dev
carries an `## [<version>] - Unreleased` entry for the next patch; add your bullets
under it rather than adding a second heading for the same version.

---

## CI

One pipeline, `azure-pipelines.yml`, covers every package:

1. **Detect changed packages** — `.azure-pipelines/scripts/detect-changes.ps1`
   discovers packages (any `src/` folder with both a `.csproj` and a
   `version.json`) and diffs against the right base for the build context.
2. **Validate changelogs** — `.azure-pipelines/scripts/validate-changelogs.ps1`.
3. **Test** — runs every `tests/**/*Tests.csproj`.
4. **Generate SBOMs** — release and hotfix branches only. Scans the packages being
   released with `umbraco-sbom`, fails on a license policy violation, and uploads to
   Dependency-Track. Tick **Generate SBOMs and check licenses** when queueing a run to
   check every package on any other branch, without uploading.
5. **Build & pack** — fans out over only the changed packages.

The pipeline builds and packs artifacts. It does **not** push to NuGet or create
tags; those are separate steps (see below).

### Adding a new package

No pipeline change is needed. Create `src/Umbraco.Forms.Integrations.<Area>.<Name>/`
containing:

- `<name>.csproj` (no `<Version>`, no `PackageReference` versions)
- `version.json` — this is what marks the folder as a package to build
- `Directory.Build.props` with `GitVersionBaseDirectory`
- `CHANGELOG.md`
- `readme.md`, `docs/readme.md`, `umbraco-marketplace-readme.md`

Then add it to `Umbraco.Forms.Integrations.slnx`. If it has a `Client/` folder, also
add that folder to the `workspaces` array in the root `package.json` — using the
casing **git** tracks, not the casing on your disk.

### Gotchas

- **Never commit `wwwroot`.** It is gitignored build output. Rebuild it with
  `npm run build` whenever you change `Client/src`; there is nothing to commit.
- **A license policy violation blocks a release.** The SBOM stage fails and `Pack`
  waits for it. If a dependency is genuinely allowed, add a per-package exception to
  `.sbom-policy.json` at the repo root (see the
  [Umbraco.Internal.Sbom](https://github.com/umbraco/Umbraco.Internal.Sbom) readme).
- **Every CI job that builds a package needs `fetchDepth: 0`.** NBGV cannot compute a
  version from a shallow clone.

---

## Releasing

**A release is built from a release branch.** `version.json` lists `main-v18`,
`v18/release/*` and `v18/hotfix/*` as public release refs, and NBGV only drops the
preview suffix on a branch matching one of them:

| Branch built | Version produced |
|---|---|
| `v18/release/2026.11.1` | `9.1.0` ← publishable |
| `main-v18` | `9.1.0` |
| `v18/dev`, feature branches | `9.1.0--preview.4.gabc1234` |

So the release branch is what produces the artifact you publish, and `main-v18` /
`v18/dev` stay untouched until the release is proven.

Release branches are named `v<N>/release/YYYY.MM.N` (calendar-based, e.g.
`v18/release/2026.11.1`). That is independent of the package versions — one branch
can carry several packages at different versions. Urgent fixes on top of an
already-released state use `v<N>/hotfix/YYYY.MM.N`, cut from `main-v<N>`.

**`release-manifest.json` declares what ships.** It sits at the repo root on the
release branch, written by `scripts/generate-release-manifest.ps1`. Its `include`
list is exactly what CI packs. CI also checks every package that changed since
`main-v18`: each must be named in `include` or `exclude`, so a changed package cannot
be left out by accident. The manifest is deleted before the release branch is merged
back.

Claude Code skills guide the process:

- **`/release-management`** — detects changed packages, recommends the bump per the
  table above, cuts the release branch, writes the manifest, updates `version.json`,
  finalises the changelog entry, dry-runs the selection, and pushes.
- **`/post-release-cleanup`** — checks the packages are on NuGet, creates the tags
  and GitHub releases, deletes the manifest, merges the release branch into
  `main-v18` and back into `v18/dev`, bumps the patch on dev so the next dev build
  sorts above the release, and offers to delete the branch.
- **`/changelog-management`** — changelog work outside a release: preview, backfill,
  or fix a `validate-changelogs.ps1` failure.

Full sequence:

1. `/release-management` on `v18/dev` — cuts `v18/release/YYYY.MM.N`, writes the
   manifest, bumps, writes changelogs, pushes the branch.
2. CI builds the release branch. **Check the artifact is clean-versioned**
   (`9.1.0`, not `9.1.0--preview.N`) before continuing.
3. Promote the artifact to NuGet.
4. `/post-release-cleanup` on the release branch — tags, GitHub releases, merges
   back and bumps the dev versions.

Tags are annotated: one `release/<slug>-<version>` per package (e.g.
`release/crm-hubspot-9.1.0`) and one `YYYY.MM.N` per release. The branch is
`v18/release/<date>` while tags are `release/<slug>-<version>`; they live in
different ref namespaces and do not collide.

> **Before the first release on this process**, `main-v18` has to be brought up to
> the released state and seeded with each package's `version.json` at its released
> version (Zapier `8.0.0`, ActiveCampaign `7.0.0`, Hubspot `9.0.0`). See `CLAUDE.md`.

---

## Reporting security issues

**Do not open a public GitHub issue for a security vulnerability.** Report it
privately via
[How to report a vulnerability in Umbraco](https://umbraco.com/trust-center/security-and-umbraco/how-to-report-a-vulnerability-in-umbraco/),
and do not disclose it publicly until a fix has shipped.

[nbgv]: https://github.com/dotnet/Nerdbank.GitVersioning
[cc]: https://www.conventionalcommits.org/
[kac]: https://keepachangelog.com/en/1.0.0/
