# APEX — Claude Code brief

This file is loaded automatically by Claude Code when working in this repo. Keep it short, actionable, and truthful. Update it whenever conventions change.

## Project in one sentence

APEX is a Vampire Survivors–like where the player is a consuming organism ascending through nine eras of existence, from primordial biology to post-singularity geometry.

## Pillars (treat as design constraints, not nice-to-haves)

- **Fantasy of predation, not villainy.** You are not evil — you are the sharpest edge of evolution. Visual and tonal framing should support "apex organism," not "monster."
- **Four verbs, many skins.** The player has exactly four attacks across the whole game — Lunge/Consume `[Melee]`, Spore/Acid Spit `[Projectile]`, Radial Pulse `[Nova]`, Corrosive Field `[Area/Persistent]` — plus a passive melee stream when enemies are close. Eras reskin these four verbs; they are mechanically stable.
- **Era recontextualization.** Each enemy archetype (Melee / Ranged / Magic / Elite / Boss) persists across all nine eras but is reinterpreted per era (shaman → EMP drone → physics-rewriter).
- **Tags are the language.** Abilities carry multi-tag lists like `[Fire][Burn][Melee]`. Passives, Hammers, and synergies filter on tags. The tag system is global and shared across content.
- **Auto-combat, player focuses on movement.** All four attacks auto-fire on timers by default. Players decide where to be, what to pick, and what to build.

See `docs/DESIGN.md` for the full brief.

## Tech choices

- **Engine**: Unity 6.3 (6000.3 LTS stream), 2D URP.
- **Input**: Unity Input System package (not legacy Input).
- **Rendering pipeline**: URP 2D Renderer.
- **Physics**: Physics2D. Enemies and player are non-kinematic rigidbodies with circle/capsule colliders.
- **Data**: ScriptableObjects for all tunable content (attacks, enemies, passives, hammers, eras). Avoid hard-coded numbers in MonoBehaviours.
- **Object lifecycle**: pool everything that spawns more than ~10 times per second (projectiles, enemies, VFX, damage numbers).
- **Events**: a lightweight event bus (C# `event`s or a signal broker) for cross-system hooks (on-kill, on-hit, on-pickup). No `FindObjectOfType` in hot paths.
- **Assembly definitions**: split runtime / editor / tests into their own asmdefs as modules grow. Start with a single `APEX.Runtime.asmdef` when there is enough code to justify it.

## Folder convention inside `Assets/`

All project-specific content lives under `Assets/_Project/` so it never collides with imported packages. See `docs/ARCHITECTURE.md` for the full tree — Claude should not invent new top-level folders without updating that doc.

## Coding conventions

- Namespaces mirror folders: `APEX.Combat`, `APEX.Enemies.AI`, `APEX.Progression.Passives`, etc.
- `PascalCase` for types and methods, `camelCase` for locals, `_camelCase` for private fields, `UPPER_SNAKE` only for genuine constants.
- Public APIs get XML doc comments. Private ones only get comments when intent is non-obvious.
- Favor composition over inheritance. Prefer small MonoBehaviours wired together to one god-script.
- All combat/enemy/passive content is authored as a ScriptableObject asset and referenced by ID — never by scene-object reference.
- Tests live under `Assets/_Project/Scripts/Tests/{EditMode,PlayMode}/`.

## Working agreements for Claude

- **Design doc is source of truth.** If a request conflicts with `docs/DESIGN.md`, surface the conflict instead of silently choosing.
- **Don't commit the `Library/` folder** or anything Unity regenerates. Check `.gitignore` before adding files.
- **Never modify `*.meta` files by hand.** Let Unity own them.
- **Prefer small PRs.** One system or one feature per branch.
- **When adding a new attack, enemy, or passive:**
  1. Author a ScriptableObject in the appropriate `Assets/_Project/Data/` subfolder.
  2. Assign tags from the existing tag set before inventing new ones.
  3. Wire it via a catalog / registry, not direct references.

## Open design questions (track in `docs/DESIGN.md`)

- How the player organism visually and mechanically evolves across eras.
- Whether Hammers are picked on level-up, at stores, or both.
- Elite archetype "modifies swarm" — what the concrete modification primitives are.

## Commands

Use normal Unity workflows. There is no external build script yet. When a `scripts/` folder and a `Makefile` or PowerShell equivalent exist, document them here.
