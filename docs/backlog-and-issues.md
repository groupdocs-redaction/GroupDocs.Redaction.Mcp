# Backlog & Known Issues

Running list of ideas, planned work, and known limitations for the
GroupDocs.Redaction MCP server. Grouped by topic. Terse on purpose — each line is
a ticket, not an essay. `[ ]` = open, `[x]` = shipped (kept for context).

**Current surface (26.9.0):** `redact_text`, `redact_image_area`, `redact_annotations`,
`erase_metadata`, `get_document_info`.

---

## Confirmed defects — external audit, 2026-08-16

Source: black-box test round against `ghcr.io/groupdocs-redaction/redaction-net-mcp:latest`
(26.7.2, licensed), 46 family-wide defects reported and all 46 independently reproduced with
control calls. A later validation round found **zero false positives**.

`S#` = shared core (`GroupDocs.Mcp.Core`) · `M#` = this repo · `P#` = GroupDocs.Redaction library

**Verdict: the most concerning product in the family.** Two of five tools are dead on PDF — the
flagship format — and the truthfulness defects matter more here than anywhere else: this is a
*redaction* product, and silence about what was removed is a trust issue.

### Product library — upstream (highest priority overall)

- [ ] **P1** `erase_metadata` fails on every PDF — **High**.
- [ ] **P2** `redact_image_area` fails on every PDF — **High**.
      *Proof for both:* GDI+ `A resolver is already set for the assembly`. DOCX and PNG controls
      succeed — it is PDF-specific and packaging-related. `GdiPlusResolver.cs` is **still
      registered in this repo**.
      *Fix:* have the library team resolve the duplicate resolver registration (or align the
      Aspose.PDF / System.Drawing dependency versions in the image). Then add a per-tool Linux
      smoke test — one call per tool in the built container would have caught both before release.
      **P1 — highest priority overall; one shared root cause kills 2 of 5 tools.**

> **Note:** the Annotation repo hit this exact bug, documented it in **changelog 004**, and
> reverted its `GdiPlusResolver`. **Redaction never applied that revert.** Start there.

### MCP wrapper — this repo

- [ ] **M1** No match count, and a zero-match redaction still reports success — **Med**.
      *Proof:* a pattern matching nothing returns "Applied" and writes an output file —
      indistinguishable from a real redaction.
      *Impact:* a caller cannot distinguish *"redacted 47 matches"* from *"redacted nothing"*. On a
      redaction product this invites false confidence that sensitive data is gone.
      *Fix:* surface the engine's change count in the response text
      (`Redacted 47 match(es)` / `Redacted 0 match(es)`). **P1**
- [ ] **M2** Repeated operations overwrite each other and always restart from the original —
      **Med**. Fixed output name (`_redacted`) with `rewrite:true`, and each run re-reads the
      original source.
      *Impact:* multi-step redaction workflows produce documents the caller believes are fully
      redacted **and that are not**.
      *Fix:* dedup output names as Merger/Watermark/Total do, and/or let callers chain by passing
      the previous output explicitly — which requires the Annotation file-lock issue (M1 there) not
      to bite. **P1**

### Shared core — fixed once in `GroupDocs.Mcp.Core`, lands here on the next bump

- [ ] **S1** Passing `fileName` crashes any tool — **High**.
- [ ] **S2** Missing files return an opaque error — **High**; listing capped at 20 entries.
- [ ] **S3** `isError` is set on crashes but not on real failures — **Med**.

Nothing to do in this repo for S1–S3 beyond re-testing after the Core bump.

---

## Known issues & limitations

- Regex text redaction itself works correctly — the defects are in reporting and in the PDF
  packaging path, not in the redaction algorithm.
- Output naming currently overwrites silently; the family convention elsewhere is `' (N)'` dedup.
  This product should converge on it (M2).

---

## Tools & functionality

- [ ] **M1** report match counts truthfully. **P1**
- [ ] **M2** stop restarting from the original; dedup or accept an explicit input. **P1**
- [ ] `redact_text` — return the matched regions (page, count) so a caller can audit. **P2**
- [ ] Expose an output `fileName` parameter. **P2**

## Testing & CI

- [ ] **Per-tool Linux smoke test in image CI** — one call per tool in the built container. Would
      have caught P1 and P2 before release. **P1**
- [ ] Add a `channel: [dnx, docker]` axis — P1/P2 live in the Linux image and are invisible to the
      current dnx/Windows-only matrix. **P1**
- [ ] Zero-match honesty test: redact a pattern that matches nothing, assert the response says so.
      The suite has no such assertion today. **P1**
- [ ] Chained-redaction test: redact twice, assert the second run builds on the first. **P1**
- [ ] Add the two mandatory probes: the **`fileName`-only form**, and a **missing file** asserting
      the promised `Available files:` text. **P1**
- [ ] macOS integration leg hangs (family-wide) — `timeout-minutes: 20` is committed locally but
      unpushed here. Push it, and stream the `dnx` child's stderr to an uploaded file. **P1**

## Documentation & discoverability

- [ ] Document the output-naming policy once M2 lands. **P1**
- [ ] Document PDF support status honestly until P1/P2 are fixed — today the docs imply the tools
      work on PDF. **P1**
- [ ] Licensing section covering the metered option once it ships. **P1**

## Platform & infra (longer-term)

- [ ] Metered licensing (`GROUPDOCS_METERED_PUBLIC_KEY` / `_PRIVATE_KEY`) via
      `GroupDocs.Mcp.Core`, plus the `get_license_status` tool. **P1**
- [ ] Remove `GdiPlusResolver.cs` in line with Annotation's changelog 004. **P1**
- [ ] HTTP/SSE transport for shared/team deploys (stdio stays default). **P2**

---

*Evidence: `TEMP_ThirdPartyAnalysis/redaction.md` (per-product findings),
`ALL-PRODUCTS-REPORT.md` (10-product sweep), `VALIDATION-REPORT.md` (why the green suites miss
these). Conventions: any behaviour change ships with a `changelog/NNN-*.md` entry and a CalVer
bump. Integration tests target the published NuGet via `dnx`, so new-tool tests only pass once the
matching version is live.*
