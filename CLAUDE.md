# CLAUDE.md

Repo-level guide for **Umbraco.Forms.Integrations** — a multi-package repository of official Umbraco Forms integration packages ("best of breed" / DXP initiative). Each package ships to NuGet as `Umbraco.Forms.Integrations.<Area>.<Name>` and installs into an Umbraco site running Umbraco Forms.

This file is for **navigation, structure, and release workflow**. Package-specific detail lives in each package's `readme.md` / `docs/`.

The build, versioning and release setup is ported from [Umbraco.Cms.Integrations](https://github.com/umbraco/Umbraco.Cms.Integrations), and the scripts are the same apart from the package prefix.

---

## 1. Overview

- **What it is:** 3 independent integration packages (CRM, automation), each versioned and released separately to NuGet.
- **Stack:** .NET (net10.0 on the v18 line), Umbraco CMS + Umbraco Forms as the host, a **TypeScript + Vite** backoffice client compiled into `wwwroot` for the two CRM packages, built/packed via **Azure Pipelines** (Azure DevOps project "Umbraco Forms Integrations").
- **Not a single product:** there is no shared runtime library across packages — they are sibling projects that share only the repo, tooling, and release conventions. No package references another.
  - `Automation.Zapier` depends on the **NuGet** package `Umbraco.Cms.Integrations.Automation.Zapier` (from the Umbraco.Cms.Integrations repo). That is an ordinary external dependency, declared in the root `Directory.Packages.props`, not a sibling reference.

> **v18 only, for now.** Only the v18 line (`main-v18` / `v18/dev`) uses central package management, per-package `version.json` (NBGV) and a single pipeline — described below. **The v17 line (`main-v17` / `v17/dev`) still uses the old process**: hand-edited `<Version>` in the csproj, one pipeline per package, cdxgen for SBOMs. Porting it is a later follow-up. Older lines (`main-v13`, `main-v10`, legacy `main`) are untouched. Sections 3 and 4 flag where the lines differ.

---

## 2. Repository Structure

```
/src              - The integration packages (one .NET project per package),
                    plus the OAuth proxy web app (not a package)
/tests            - Test projects, `<PackageName>.Tests` (only Crm.Hubspot so far)
/examples         - Test site (Umbraco.Forms.Integrations.Testsite.V18)
/.azure-pipelines - Pipeline scripts + templates (v18 line)
/scripts          - Release tooling (generate-release-manifest.ps1)
azure-pipelines.yml       - ONE pipeline for every package (v18 line)
Directory.Packages.props  - Central dependency versions (v18 line)
Directory.Build.targets   - Manifest version stamping + the pack guard
Umbraco.Forms.Integrations.slnx
package.json      - Root npm workspace over every package's Client/ (v18 line)
package-lock.json - The ONE lockfile for all clients
CONTRIBUTING.md   - Contributor-facing version of much of this file
```

Each package under `src/` follows the same shape:
```
src/Umbraco.Forms.Integrations.<Area>.<Name>/
  <project>.csproj                     - .NET class library; NO <Version>, NO PackageReference versions
  version.json                         - NBGV config; the ONLY hand-edited version (v18 line)
  Directory.Build.props                - pins GitVersionBaseDirectory to this folder
  CHANGELOG.md                         - Keep a Changelog format; required when bumping
  Client/                              - TypeScript + Vite source (CRM packages only); an npm
                                         workspace of the root package.json (no per-package lockfile)
    src/, public/umbraco-package.json  - backoffice manifest; SOURCE copy, version is a
                                         fixed 1.0.0 placeholder - never hand-edit
  wwwroot/                             - COMPILED client assets - GITIGNORED, never committed
                                         (+ umbraco-package.json, copied by Vite, CI-stamped)
  readme.md, docs/, umbraco-marketplace-readme.md, umbraco-marketplace.json
```

> A `src/` folder counts as a package to CI **only if it has both a `.csproj` and a `version.json`**. That is what keeps the OAuth proxy out of the build, and it means adding a package needs no pipeline change.

### Packages (v17/v18 lines)

| Project | Area | Client workspace | v17 major | v18 major |
|---|---|---|---|---|
| `Automation.Zapier` | Automation | — (no `Client/`) | 7.x | 8.x |
| `Crm.ActiveCampaign` | CRM | `activecampaign-crm` | 6.x | 7.x |
| `Crm.Hubspot` | CRM | `hubspot-crm` | 8.x | 9.x |

Released on the v18 line (25-06-2026): Zapier `8.0.0`, ActiveCampaign `7.0.0`, Hubspot `9.0.0`. The `version.json` values on `v18/dev` (`8.0.1`, `7.0.1`, `9.0.1`) are the post-release patch bump.

### Not packages

- **`src/Umbraco.Cms.Integrations.OAuthProxy`** — an Azure web app (net5.0), deployed as the HubSpot auth service. It has its own deploy pipeline, `azure-pipeline - Cms.OAuthProxy.yml`, and its own `Directory.Packages.props` that opts it out of central package management and the global versioning/analyzer references. It has no `version.json` and is not in the solution, so the package pipeline ignores it. Its `wwwroot` is source and **is** committed (the `.gitignore` excepts it).
- **`tests/Umbraco.Forms.Integrations.Crm.Hubspot.Tests`** — NUnit + Moq. Its package versions come from the root `Directory.Packages.props`.
- **`examples/Umbraco.Forms.Integrations.Testsite.V18`** — a test site that references all three packages by project. It has its own scoped `Directory.Packages.props`.

### Client assets (v18 line)

**`wwwroot` is build output and is not in git.** Every package's `Client/` is a workspace of the root `package.json`, so there is one `package-lock.json` for the whole repo:

```bash
npm ci                                       # installs every client, once
npm run build                                # builds every client into its own wwwroot
npm run build --workspace hubspot-crm        # or just one (activecampaign-crm, hubspot-crm)
```

`dotnet build` does **not** build clients — the `Microsoft.AspNetCore.ClientAssets.targets` import, the per-client lockfiles and the generated `Client/config.outputPath.js` are gone (Vite writes to a literal `../wwwroot`).

Consequences worth knowing:

- **A fresh clone has no `wwwroot`.** Run `npm ci && npm run build` before running the test site, or the backoffice loads no extensions.
- **Packing without building fails loudly.** An `EnsureClientAssetsBuilt` target in the root `Directory.Build.targets` errors if a package has a `Client/` but no built `wwwroot`, so a package can never ship with its assets missing.
- **CI builds only the workspace it needs.** `detect-changes.ps1` reads each `Client/package.json` name into the matrix as `clientName`, and the pack template installs and builds just that workspace (`npm ci --workspace <clientName> --include-workspace-root`). Zapier has no client, so its job skips Node entirely. npm's download cache is restored per client via `Cache@2`, keyed on `package-lock.json` — per client because Azure cache entries are immutable and a shared key would only ever serve whichever job finished first.
- Adding a package with a client means adding its `Client` folder to the root `package.json` `workspaces` array — use the casing **git** tracks, not the disk casing.

---

## 3. Branching & Release Model

**One branch pair per Umbraco major.** For each supported Umbraco version there is a `main-v<N>` and a `v<N>/dev`:

- **`main-v<N>`** — reflects the **last released** state for that major.
- **`v<N>/dev`** — the **working branch**; always ahead of `main-v<N>` with unreleased work.
- **`v<N>/release/YYYY.MM.N`** — cut from dev per release; **this is what builds publishable artifacts** (v18 line).
- **`v<N>/hotfix/YYYY.MM.N`** — cut from `main-v<N>` for an urgent fix on the released state (v18 line).
- **`v<N>/feature/*`** — feature branches, cut from and merged back to `v<N>/dev`.
- **`main`** (no suffix) — legacy, untouched.

**Flow:** work lands on `v<N>/dev` → a **release branch** is cut from dev → the pipeline builds *that branch* to get clean versions → published to NuGet and tagged → the release branch is merged back into **both** `main-v<N>` (so it matches what shipped) and `v<N>/dev`. Do **not** commit release/working changes directly to `main-v<N>` — it re-diverges it from dev.

### Before the first v18 release: seed `main-v18`

**`main-v18` is not yet the released state.** Today it is identical to `main-v17` and behind `v18/dev`, and it has no `version.json` files. Every v18 release tool compares against `main-v18`:

- `detect-changes.ps1` diffs `merge-base(main-v18, HEAD)..HEAD`, so every package would look changed by the whole v17→v18 upgrade, not by what changed since `8.0.0`/`7.0.0`/`9.0.0`.
- `validate-changelogs.ps1` on a release branch compares against `main-v18`, so every `version.json` would count as a new bump.
- `main-v18` is a public release ref, so once the pipeline runs on it, it builds clean versions from whatever `version.json` it holds.

So before the first release under this process, bring `main-v18` up to the released state and seed it with the `version.json` files (the equivalent of [Umbraco.Cms.Integrations#317](https://github.com/umbraco/Umbraco.Cms.Integrations/pull/317)). **Seed the released versions — Zapier `8.0.0`, ActiveCampaign `7.0.0`, Hubspot `9.0.0` — not the dev ones** (`8.0.1`/`7.0.1`/`9.0.1`). Seeding dev versions would make a `main-v18` build produce clean `8.0.1`-style packages that were never released. `/release-management` checks for this and stops if it has not been done.

### Versioning — v18 line (NBGV)

Each package has its own `version.json` ([Nerdbank.GitVersioning][nbgv]) holding the full `major.minor.patch`. **That is the only place a version is written by hand.** A sibling `Directory.Build.props` pins `GitVersionBaseDirectory` to the package folder, so each package's version is computed from its own commits — activity in one package never moves another's.

Bump rule, from the commits affecting that package ([Conventional Commits][cc]):

| Commit signal | Bump |
|---|---|
| `BREAKING CHANGE:` in body, or `!` after type/scope | Major |
| `feat:` | Minor |
| `fix:` / `perf:` | Patch |
| Only `docs:` / `chore:` / `refactor:` | Usually none |

**Do not hand-edit:** the csproj (`<Version>` is gone) or `wwwroot/umbraco-package.json` — an `UpdatePackageManifestVersion` target in the root `Directory.Build.targets` stamps the latter on CI builds, so the committed value is always overwritten in the published package.

**Also do not hand-edit:** `Client/public/umbraco-package.json`. It is a fixed `1.0.0` placeholder in both packages that have one (Zapier has no client). The Vite build copies it into `wwwroot`, and `UpdatePackageManifestVersion` stamps the real version into that copy on CI builds, so the placeholder never reaches a published package.

> The placeholder is load-bearing, not just tidiness: bumping it by hand on every release makes the package look changed to release detection. One consequence: a **local** `dotnet pack` has `ContinuousIntegrationBuild` false, so the stamp does not run and a locally built `.nupkg` carries `1.0.0`. Local packs are not published.

On `main-v18` a build produces a clean version (`9.0.2`). On any other branch except a release or hotfix branch, NBGV appends a preview suffix (`9.0.2--preview.4.gabc1234`), which sorts **below** the release — hence the post-release patch bump on dev.

Each package keeps **one major per Umbraco major** (see the table in section 2). Umbraco Forms shares the CMS major, so a new Umbraco major means a new major for every package.

### Versioning — v17 line (manual, not migrated yet)

Still the old way: the per-package pipelines pack with `versioningScheme: off` and the version comes from the csproj `<Version>`. Bumping means editing:

1. `<project>.csproj` → `<Version>`
2. `Client/public/umbraco-package.json` → `"version"`, for the packages that have a client

None of the v18 skills or scripts apply to v17 until it is ported.

### Dependency versions — v18 line

Managed centrally in the root `Directory.Packages.props`. A `PackageReference` **must not** carry a `Version` attribute (`NU1008` otherwise) — declare it once centrally and reference by name.

Umbraco CMS and Umbraco Forms (`Umbraco.Forms.Core`) both take their range from one property, `UmbracoCmsPackageVersion` (`[18.0.0, 19.0.0)`), so moving the Umbraco floor is one line.

`examples/Umbraco.Forms.Integrations.Testsite.V18/` and `src/Umbraco.Cms.Integrations.OAuthProxy/` each have their own `Directory.Packages.props` and sit outside the root one. `tests/` uses the root one.

### Releasing (per package)

The pipeline **only builds + packs the `.nupkg` as artifacts** (plus an `SBOM` artifact on release/hotfix branches) — it does **not** push to NuGet or create tags.

> **A release artifact is built from a release branch.** `publicReleaseRefSpec` lists `main-v18`, `v18/release/*` and `v18/hotfix/*`, so a build on any of those drops the preview suffix (`9.1.0`); a `v18/dev` or feature-branch build gives `9.1.0--preview.N.gSHA`. Publishing a dev artifact would put a preview version on NuGet.
>
> This is a real behavioural difference from the old manual process, where the csproj `<Version>` was clean on every branch and any build was publishable.

**Release branches:** `v<N>/release/YYYY.MM.N` (calendar-based, cut from `v<N>/dev`), and `v<N>/hotfix/YYYY.MM.N` for an urgent fix cut from `main-v<N>`. The name is independent of package versions — one branch can carry several packages at different versions.

> **`N` counts release events across the whole repo, not per line.** It is derived from the `YYYY.MM.N` date tags, which carry no line prefix, so once v17 is ported both lines share one sequence — `v18/release/2026.11.1` would be followed by `v17/release/2026.11.2`, not a second `.1`. No date tags exist yet, so the first release in a month is `.1`. Two tag shapes are in play and cannot collide: `release/<slug>-<version>` identifies a published package, `YYYY.MM.N` identifies the release event and is what the next release counts from.

**`release-manifest.json` declares what ships.** Two layers, because "what changed" and "what are we publishing" are different questions:

1. **Detection proposes.** For each package, `detect-changes.ps1` diffs `merge-base(main-v<N>, HEAD)..HEAD` scoped to the package folder, ignoring `version.json` and `CHANGELOG.md` as release bookkeeping.
2. **The manifest decides.** Required on `v<N>/release/*`, optional on `v<N>/hotfix/*`. Its `include` list replaces the build set outright, so a package with no changes still ships if listed. Anything detection found changed must appear in `include` or `exclude`, or the build fails — that guard is how you cannot silently forget a package.

Written by `scripts/generate-release-manifest.ps1`, which validates names against the real package list. Deleted by `/post-release-cleanup` before the merge back, so it never lands on `main-v<N>` or `v<N>/dev`.

> **Do not select on "`version.json` differs from `main-v<N>`".** It was tried first and does not work: `/post-release-cleanup` bumps each released package's patch on dev, so that test reports every previously-released package as shipping on every later release branch, and a package whose version never moved can never be selected.

Three skills cover the repeatable parts:

- **`/release-management`** — detect changed packages, recommend the bump, **cut the release branch**, write `release-manifest.json`, update `version.json`, finalise the `CHANGELOG.md` entry, dry-run the selection, push. It does **not** tag.
- **`/post-release-cleanup`** — verify each package is on NuGet, create the **tags and GitHub releases**, delete `release-manifest.json`, merge the release branch into `main-v<N>` **and** back into `v<N>/dev`, bump each released package's patch on dev, optionally delete the branch.
- **`/changelog-management`** — changelog work on its own: preview what a package would release, backfill a missing entry, or fix a `validate-changelogs.ps1` failure. There is no generation script behind it; entries come from reading `git log`.

Full sequence:

1. `/release-management` on `v<N>/dev` — cuts `v<N>/release/YYYY.MM.N`, writes the manifest, bumps, changelogs, dry-runs, pushes.
2. Pipeline builds the release branch. **Verify the artifact is clean-versioned** before continuing.
3. **Promote** it to NuGet. **This is the only manual step.** (Who promotes, and how, is not written down in this repo yet — to confirm.)
4. `/post-release-cleanup` on the release branch — verifies NuGet, tags, GitHub releases, merges back into both branches, bumps dev.

> **Tagging and GitHub releases belong to `/post-release-cleanup`.** When they were manual steps after publishing, they were the steps that got skipped. On this repo, Zapier `8.0.0` was published without a tag, and none of the v18 `.0.0` releases has a GitHub release.
>
> They cannot live in `/release-management` instead: that runs before CI has built anything, so nothing is published for a tag to point at, and a tag created then is a lie if the artifact is never promoted. `/post-release-cleanup` already requires the packages to be on NuGet, which is exactly when tagging becomes true.
>
> `release-manifest.json` is what tells the cleanup what shipped — the same declaration CI built from. The cleanup reads it, then deletes it.
>
> Tag slugs are the package name minus `Umbraco.Forms.Integrations.`, lowercased, dots to hyphens: `automation-zapier`, `crm-activecampaign`, `crm-hubspot`. Tags before the `release/` namespace used `release-<name>-<version>` (e.g. `release-crm-hubspot-2.0.0`); use `git tag --list` to find an *existing* tag.

---

## 4. CI (Azure Pipelines)

### v18 line — one pipeline

`azure-pipelines.yml` covers the whole repo in four stages:

1. **DetectChanges** — `.azure-pipelines/scripts/detect-changes.ps1` discovers packages (a `src/` folder with both a `.csproj` and a `version.json`), then selects what to build based on the branch:
   - **`v18/release/*` / `v18/hotfix/*`** — diffs each package against `merge-base(main-v18, HEAD)`, ignoring `version.json` and `CHANGELOG.md`, then `release-manifest.json` decides. See the two-layer model in section 3. The manifest is required on `release/*` and optional on `hotfix/*`.
   - **PRs** — diffs against the merge-base with the target branch.
   - **`main-v18` / `v18/dev`** — diffs against `HEAD~1`.
   - **feature branches** — diffs against the merge-base with `v18/dev`.

   A `Directory.Packages.props` change is traced to only the packages referencing the packages whose versions moved (so bumping `Umbraco.Cms.Integrations.Automation.Zapier` rebuilds Zapier alone). Then `validate-changelogs.ps1` runs (on a release branch it compares against `main-v18`, so every bump the release carries is checked).

   > Package paths are resolved to the casing **git** tracks, not the casing on disk. `git show <ref>:<path>` and `git diff -- <path>` are case-sensitive even on Windows. Manifest name lookup is deliberately case-insensitive for the same reason, resolving to the git-tracked name.

   > **There are no tests for this logic.** The safety net is a dry-run, which `/release-management` runs before pushing a release branch:
   > ```bash
   > pwsh -File .azure-pipelines/scripts/detect-changes.ps1 \
   >   -SourceBranch "refs/heads/v18/release/2026.11.1" \
   >   -ReleaseCompareRef "origin/main-v18"
   > ```
   > `-ReleaseCompareRef` exists for this. It is load-bearing, not a debug flag.

2. **Test** — `dotnet test` over `tests/**/*Tests.csproj`, publishing `.trx` results and Cobertura coverage. **`Pack` depends on this stage**, so a failing test blocks packing and therefore blocks any promotion to NuGet.

   > A custom `condition:` **replaces** the implicit `succeeded()` check. `Test` wraps its condition in `and(succeeded(), ...)`. `Pack` can't, because `succeeded()` is false when a dependency was skipped and `GenerateSBOM` is skipped on most branches, so it checks each dependency's `result` explicitly instead. Drop that and `Pack` runs even after `Test` failed.
3. **GenerateSBOM** — release/hotfix pushes only, over the same packages `Pack` builds (`ChangedProducts` from detection). `.azure-pipelines/scripts/sbom/generate-sbom.ps1` scans each package's `.csproj` and, for the CRM packages, its npm workspace (a runtime-only slice of the root lockfile, from `prune-npm-lockfile.js`) with `umbraco-sbom` ([Umbraco.Internal.Sbom](https://github.com/umbraco/Umbraco.Internal.Sbom)), merges them, and uploads to Dependency-Track as `<PackageId> v<major>` (e.g. `Umbraco.Forms.Integrations.Crm.Hubspot v9`) at the full version, setting `isLatest` from NuGet.org. **A license policy violation fails the stage and `Pack` waits for it**, so a violation stops a release. An upload failure only gives `SucceededWithIssues`. The upload needs the `dtApiKey` and `dtBaseUrl` pipeline variables. The `generateSbom` queue-time parameter runs it on any branch over every package (`AllProducts`), with no upload.
4. **Pack** — a matrix over only the changed packages, one job each, via `.azure-pipelines/templates/pack-product.yml`. `nbgv cloud` runs with `workingDirectory` set to the package folder so it picks up that package's `version.json`.

Adding a package needs **no pipeline change** — give it a `version.json`.

> **The pipeline definition.** `azure-pipelines.yml` needs its own pipeline definition in the Azure DevOps project, with the `dtApiKey`/`dtBaseUrl` variables. The old per-package definitions (Hubspot 353, Zapier 392, ActiveCampaign 435, eMerchantPay 404) build the older lines' per-package YAML files, which are gone from v18.

### v17 line

Still **one pipeline per package** (`azure-pipeline - <Area>.<Name>.yml`), path-filtered to the package folder, with cdxgen for SBOMs. When v17 is ported, keep its `detect-changes.ps1` and `validate-changelogs.ps1` in step with v18's: the two lines' copies should differ only in the fallback line default (`v17` versus `v18`), so a fix to one ports mechanically to the other.

### Gotchas

- **Changelog required on a bump.** CI fails when a `version.json` changed with no matching `## [<version>]` entry in that package's `CHANGELOG.md`. Only bumped packages are checked. (v18 line.)
- **SBOM (umbraco-sbom):** pass the package's `.csproj`, never its folder. The folder also holds `Client/`, and `umbraco-sbom` rejects a path with both `.csproj` and `package.json` as mixed-ecosystem (exit 41). Exit 10 (unknown licenses) is a pass, so the script ends with an explicit `exit 0`, or `PowerShell@2` would fail on the leaked `$LASTEXITCODE`. License exceptions go in `.sbom-policy.json` at the root (none needed today, so the file does not exist).
- **`wwwroot` is gitignored** on the v18 line (except the OAuth proxy's) — see [Client assets](#client-assets-v18-line). Rebuild it with `npm run build`; there is nothing to commit.
- **A pack needs the client built first.** `npm ci && npm run build`, or the `EnsureClientAssetsBuilt` target fails the pack.
- **`Directory.Build.targets` is not inherited past a nearer one.** MSBuild imports only the nearest `Directory.Build.targets`. A package that adds its own must import the root one explicitly, or it loses the stamping and the pack guard.
- **`NU1507`** is expected on the v18 line: central package management plus the two feeds in `NuGet.config`. A warning, not an error; silencing it needs package source mapping.
- **`dotnet test` arguments.** Do not pass `--logger`/`--results-directory` alongside `publishTestResults: true` — the `DotNetCoreCLI@2` task appends its own pair and `dotnet test` rejects two values for `--results-directory`.
- **NBGV needs full history.** Every job that builds a package must `checkout` with `fetchDepth: 0`, including Test (the test project references Crm.Hubspot, which imports NBGV); the agent default is a depth-1 fetch and NBGV throws "Shallow clone lacks the objects required to calculate version height".
- **The build number is set by `name:`, not by NBGV.** `nbgv cloud` sets the cloud build number by default — the `"cloudBuild": { "buildNumber": { "enabled": false } }` in each `version.json` does **not** stop it, as that setting only governs the MSBuild integration. Every Pack matrix job runs the CLI, so without `--skip-cloud-build-number` the last job to finish names the whole build after its own package. The flag is passed in `pack-product.yml` and `name:` in `azure-pipelines.yml` supplies `branch-date-buildid` instead.
- **No `UseDotNet@2`, no `script:`.** On this pool `UseDotNet@2` fails before it starts (no `powershell` on the Windows images, no `bash` on Ubuntu), so `.azure-pipelines/templates/setup-dotnet.yml` uses the agent's preinstalled SDK, selected by `global.json`. Steps use `pwsh`, since `script:` needs bash or cmd.exe.

---

## Quick Reference

**Key directories:** `/src` (packages + OAuth proxy) · `/tests` · `/examples` (test site) · `/.azure-pipelines` (CI scripts + templates, v18) · `/scripts` (release tooling — kept out of `.azure-pipelines/` because a change there rebuilds every package)

**Branches:** `main-v18` / `v18/dev` (Umbraco 18, this setup) · `main-v17` / `v17/dev` (Umbraco 17, old per-package process) · `main-v13`, `main-v10`, `main` (older, untouched)

**Publishable builds come from** `main-v18`, `v18/release/*` or `v18/hotfix/*` only — everything else is `--preview` suffixed

**Release tag convention:** `release/<slug>-<version>` (annotated), e.g. `release/crm-hubspot-9.0.1`, plus one `YYYY.MM.N` date tag per release

**Bump a version:**
- **v18 line** → edit `src/<package>/version.json` only, and add a `CHANGELOG.md` entry. Never `Client/public/umbraco-package.json` — it is a fixed `1.0.0` placeholder
- **v17 line** → edit the csproj `<Version>` and, for a package with a client, `Client/public/umbraco-package.json`

**Declare a release** (v18 line, on the release branch):
```bash
pwsh -File scripts/generate-release-manifest.ps1 -Include Umbraco.Forms.Integrations.Crm.Hubspot
```

**First run after a clone (v18 line) — `wwwroot` is not in git:**
```bash
npm ci
npm run build
```

**Build the .NET side** (never builds clients, so it is always the fast path):
```bash
dotnet build Umbraco.Forms.Integrations.slnx
```

**Run the CI checks locally (v18):**
```bash
pwsh -File .azure-pipelines/scripts/detect-changes.ps1
pwsh -File .azure-pipelines/scripts/validate-changelogs.ps1
```

[nbgv]: https://github.com/dotnet/Nerdbank.GitVersioning
[cc]: https://www.conventionalcommits.org/
