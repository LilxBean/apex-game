# APEX — Forward Roadmap

Planning doc for sessions after prompt5. Everything below is still unbuilt (or
stubbed). Sections are grouped by how load-bearing they are for shipping a
playable, coherent experience. Each section names the files we'd touch and
gives a rough session budget — these are prompts, not stories.

Current playable loop (as of prompt5): spawn into Sandbox → 4 auto-attacks fire
→ enemies drop XP orbs → level-up picks → run ends on death or timer. There is
no game shell around that loop, no era progression, no narrative, no audio,
no persistence.

---

## 1. UI/UX shell — ~2-3 sessions

The biggest gap between "sandbox demo" and "game". Without this the run can't
start, pause, or end cleanly, and the player has no entry point.

### 1a. Main Menu (session: 1)
Scene: `Scenes/Runtime/MainMenu.unity` (new). Screen-space canvas built via
`UIFactory`, same pattern as `ProgressionHUD`.

Buttons:
- **Main Game** — load Sandbox (will become `Run.unity`) with default run
  config.
- **Challenge Mode** — loads Run with a specific modifier set (see 1f).
- **Endless** — loads Run with the "no-timer + cycling eras" modifier.
- **Settings** — opens the shared settings overlay (1e).
- **Stats** — opens a stats panel backed by the save file (5).
- **Credits** — scrolling credit text.
- **Exit Game** — `Application.Quit()`.

New: `APEX.UI.MainMenuScreen`, `APEX.Meta.RunRequest` (struct describing which
mode was chosen), `APEX.Meta.SceneLoader`.

### 1b. Pause menu (session: shared with 1a)
In-run overlay. Listens for `Escape` (Input System). Sets `Time.timeScale = 0`
and raises `_simFrozen` equivalent on the player.

Buttons: **Unpause**, **Settings**, **End Run** (routes to Defeat screen with
current stats), **Restart Run** (reload scene), **Quit to Main Menu**.

New: `APEX.UI.PauseScreen`. `RunBootstrap` instantiates it alongside the HUD.

### 1c. Victory / Defeat (session: 1)
Already have `RunEndScreen` — extend it to:
- Detect win vs. loss (timer expired vs. player died).
- Show a stats block: time survived, level reached, kills, DPS peak, XP gained,
  passives picked, hammers applied.
- Buttons: **Play Again** (reload with same `RunRequest`), **Main Menu**.

Touches: `RunEndScreen`, add stats aggregation on `RunManager`. Subscribe to
`EventBus.OnEnemyKilled` / `OnPlayerHitEnemy` for counters.

### 1d. Settings overlay (session: 1)
Shared between Main Menu and Pause.

Tabs / sections: **Audio** (master / music / SFX sliders), **Graphics**
(fullscreen toggle, resolution, vsync), **Controls** (rebind movement + four
attacks — Input System supports runtime rebind), **Accessibility** (screen
shake toggle, hit-flash intensity, colorblind-friendly palette swap).

New: `APEX.Meta.SettingsStore` (serializes to JSON under
`Application.persistentDataPath`), `APEX.UI.SettingsScreen`.

### 1e. Challenge Mode framing (later — mechanical work, not UI)
Challenge mode is a named run config: fixed seed, forced modifiers (e.g. "no
healing", "double spawn rate", "single attack slot"), leaderboard-friendly.
Defer mechanical detail; Main Menu just needs the button stub for now.

---

## 2. Era Progression — ~1-2 sessions

The pillar. Right now the Sandbox runs one era (Primordial) with a single
spawn table and a single visual tone. The design intent is nine eras that
recontextualize the same five archetypes.

### 2a. Era bar + transition trigger (session: 1)
- Add an **era progress bar** to the HUD beneath the XP row. Fills from 0→1
  based on a mix of: time elapsed, XP earned, and boss defeat. Concrete formula
  to decide during planning — suggested start: `eraProgress = min(1, time /
  eraDurationSeconds)` with a late-era boss gate.
- On fill: trigger `EraDirector.AdvanceEra()`. New event
  `EventBus.OnEraAdvanced(EraDefinition next)`.
- Visual transition: brief slow-mo (timeScale → 0.25 for 0.8s), flash, arena
  tint shift via the BackgroundTiler color, HUD banner ("Primordial → Cambrian").

Touches: `EraDirector`, `ProgressionHUD` (era bar), `BackgroundTiler` (accept
color palette from `EraDefinition`), new `EraDefinition.palette` field.

### 2b. Enemy evolution across eras (session: 1)
The pillar says each archetype persists but gets reinterpreted. Two concrete
mechanisms:

1. **Palette + stat scaling on the shared `P_Enemy_Default` prefab.** Each era
   declares per-archetype HP / speed / damage multipliers and debug colors.
   `EnemyController.Initialize` already reads these — extend `EnemyDefinition`
   to be queried *through the current era* rather than directly.
2. **Swarm modifiers unlock per era.** `ISwarmModifier` only has
   `SpeedAuraModifier` today. Add era-specific modifiers:
   - Primordial: SpeedAura (existing)
   - Cambrian: `ArmorPlatedModifier` (resist Physical)
   - Cretaceous: `PackHunterModifier` (damage scales with nearby allies)
   - Industrial: `EMPBurstModifier` (interrupts passive melee)
   - Post-Singularity: `PhysicsRewriteModifier` (inverts player acceleration briefly)

Touches: `EnemyDefinition`, `EraDefinition`, new modifier SOs, `EnemyController`.

### 2c. Era boss as gate (session: 1, can share with 2b)
`BossAI` is stubbed. Design per-era boss loops:
- Phase tree (healthy / bloodied / enraged) with different attack selections.
- Arena lockdown during the boss fight (existing `ArenaBounds` helps).
- Defeating the boss completes the era regardless of the progress bar.

New: `BossAI` concrete implementations per era, or a single parameterized
`PhaseBossAI` that eras configure.

### 2d. Open design questions (still in `docs/DESIGN.md`)
- **Player organism evolution** — visual change on era advance. Sprite swap?
  Additional limb overlay? Size bump?
- **Carry-over semantics** — do passives / hammers persist across era
  transitions in a single run? (Recommend yes; era advance is not a new run.)
- **Elite archetype "modifies swarm"** — pick 2-3 concrete primitives per era
  rather than inventing ad-hoc.

---

## 3. Narrative structure — ~1 session plus asset work

The fiction is "apex organism ascending through nine eras of existence". We
have framing but no delivery mechanism.

### 3a. Framing layers
- **Cold-open per era.** On `OnEraAdvanced` show a full-screen card with a
  short flavor line ("Primordial: the first hunger.") and a silhouette of the
  era's biome. Auto-dismisses or press-to-continue.
- **Run intro.** Before first spawn, 2-3 seconds of title-card text: the
  player is introduced as *the* apex organism for this run.
- **Run outro.** On Victory, a closing card tied to the final era reached
  ("You became the Geometry. Again."); on Defeat, a wry evolutionary-dead-end
  line ("Selection pressure was not kind.").

### 3b. Meta-narrative across runs (later, depends on 5)
Each run is one attempt at the ascent. Persistent state can track
"furthest era reached", "total kills across all runs", "organisms lost". A
codex fills in as the player first encounters each era / enemy / boss. This is
the long-tail hook; build it after save/load exists.

### 3c. Tonal rules (keep in `DESIGN.md`)
- Player is not evil. Frame as *evolutionary inevitability*, not villainy.
- Enemies are not "bad guys" — they're *competing selection pressures*.
- Tone drifts era to era: primal → biological → civilizational → synthetic →
  post-material. Copy should reflect that, not stay flat.

New: `APEX.Narrative.CardDefinition` (SO), `APEX.UI.NarrativeCardScreen`.

---

## 4. Audio system — ~1 session

No sound currently. Survivors-likes rely heavily on audio feedback for the
auto-combat to feel responsive.

- `APEX.Audio.AudioService` singleton (created by `RunBootstrap`).
- Subscribe to `EventBus`: `OnPlayerHitEnemy` → bite sfx (pitch-varied),
  `OnEnemyKilled` → kill thud, `OnLevelUp` → fanfare, `OnEraAdvanced` →
  transition stinger, pickup → orb chime.
- Music: one track per era, crossfade on transition.
- Volume buses: master / music / sfx, driven by `SettingsStore`.
- Pool `AudioSource` components (we already pool projectiles).

Placeholder assets ok (synth one-shots); production audio is later.

---

## 5. Save / Load + Stats — ~1 session

Currently nothing persists between runs.

- `APEX.Meta.SaveFile` — serialized JSON under `Application.persistentDataPath`.
- Contents: settings, lifetime stats (runs played, total kills, furthest era,
  best time, total damage), unlocked entries (if we do meta-progression),
  codex entries seen.
- Auto-save on run end. Hot-reload into the main menu Stats screen.
- `EventBus` already provides hooks — add a thin `StatsRecorder` that
  subscribes and mutates the in-memory save state.

Do this before meta-progression or leaderboards.

---

## 6. Visual & feedback polish — ~1 session (ongoing)

Small wins that massively improve game feel. Not urgent but compounding.

- **Damage numbers** — floating text on hit (pooled).
- **Hit flash** — `EnemyController` already has a SpriteRenderer; brief white
  tint on damage.
- **Pickup feedback** — orb snap + 1-frame bar pulse when XP lands.
- **Level-up burst** — radial particle ring on the player.
- **Screen shake** — small on hit, big on boss kill / era advance (respect
  accessibility toggle).
- **Muzzle / attack telegraphs** — spore trajectory arc preview, corrosive
  field rim pulse.

New: `APEX.VFX.DamageNumber`, `APEX.VFX.HitFlashController`,
`APEX.VFX.ScreenShake` on Camera.

---

## 7. Known TODOs from earlier sessions

Small but listed in memory; fold into appropriate sessions:

- `EnemyProjectile` uses `Instantiate`/`Destroy` — needs pooling (fold into
  Audio / VFX session since we'll be touching projectiles).
- `ZoneMagicAI` area-effect drop is unimplemented.
- Boss AI per era (covered in 2c).
- Hammer/Passive balance after first real playtest.

---

## Suggested session order

1. **UI shell** (1a + 1b + 1c) — makes the game bootable as a game, not a
   scene. Unlocks testing end-to-end flows.
2. **Settings + Save stub** (1d + 5) — small save file, persist settings
   first, stats later. Needed before anything that touches persistence.
3. **Era progression** (2a + 2b) — the pillar. Biggest single lift.
4. **Audio** (4) — survivor-likes die without hit feedback; do before more
   mechanical work piles on.
5. **Narrative cards** (3a) — cheap, big perceived-polish win once era
   transitions exist.
6. **Era bosses** (2c) — properly gates era advance.
7. **Visual polish** (6) — ongoing; bank wins between big systems.
8. **Meta-narrative + codex** (3b) — long-tail hook.
9. **Challenge / Endless mode mechanics** (1e) — once the base run is
   satisfying.
