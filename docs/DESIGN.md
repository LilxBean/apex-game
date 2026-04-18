# APEX — Design Brief

## Elevator

APEX is a Vampire Survivors–like where you play as a consuming organism rising through the food chain of all existence. Every era you enter is ruled by a previous apex trying to hold its throne; you eat, grow, and move on.

## Fantasy

You are not evil. You are the sharpest edge of evolution. Visual tone, pickup feedback, and narration should celebrate escalation, not cruelty.

## Eras

Nine stages from Primordial to Post-Singularity. Each era has escalating tech and enemy sophistication.

- Early game: biological flood chaos — swarms, visceral, organic.
- Late game: laser siege geometry — precise, architectural, abstract.

The "magic" enemy archetype recontextualizes per era:

| Era feel       | Magic archetype example   |
|----------------|---------------------------|
| Primordial     | Chemical reactant         |
| Tribal         | Shaman                    |
| Pre-industrial | Alchemist                 |
| Industrial     | Tesla-coil operator       |
| Information    | EMP drone                 |
| Spacefaring    | Reality-field projector   |
| Post-Singularity | Physics rewriter        |

(Exact era list and names are TBD — nine slots is the contract.)

## Enemy structure

Each era ships a swarm built from five archetypes. All five are present across every era; only the skin and specific mechanic changes.

- **Melee** — closes distance, contact damage.
- **Ranged** — maintains distance, projectiles.
- **Magic** — status effects, debuffs, zone control.
- **Elite** — modifies swarm behavior (buffs adjacent enemies, alters spawn rate, shifts formation) rather than simply being tankier.
- **Boss** — either interrupts the run or emerges from the existing swarm (no "wave clear then boss spawns" cliché unless the design explicitly wants it).

## Player kit

Four attacks. They are the same four across the entire game. They reskin per era; they do not change verb.

1. **Lunge / Consume** — `[Melee]` — short dash that bites.
2. **Spore / Acid Spit** — `[Projectile]` — forward projectile.
3. **Radial Pulse** — `[Nova]` — 360° burst from player.
4. **Corrosive Field** — `[Area][Persistent]` — dropped persistent zone.

Plus a **passive melee stream** that triggers automatically when enemies are within reach — the "you are always biting something" feel.

## Controls

- All four attacks auto-fire on timers.
- Each attack can be toggled manual or auto individually.
- Player input focuses on movement, menu navigation, and pickup routing.

## Tag system

Every ability carries a tag list, e.g. `[Fire][Burn][Melee]`. Passives, Hammers, and global synergies filter on tags. The tag vocabulary is global — prefer extending the set over forking it per era.

Examples of tag axes:
- Damage type: `[Physical][Fire][Acid][Electric][Void]`
- Effect: `[Burn][Bleed][Poison][Shock][Freeze]`
- Delivery: `[Melee][Projectile][Nova][Area][Persistent][Beam]`
- Origin: `[OnHit][OnKill][OnPickup][OnLevel]`

## Hammers

Mid-run modifiers. A Hammer mutates a core attack's behavior, stats, or tags.

Examples:
- "Lunge gains `[Fire]`; +30% damage; leaves a 1s burn trail."
- "Pulse becomes two pulses, smaller radius."
- "Field becomes an expanding ring instead of a static zone."

Hammers are rarer than passives and have stronger identity.

## Progression

Between runs: a lightweight "den."

Two currencies:

- **Biomass** — common. Spent on permanent upgrades and on expanding the passive pool.
- **Milestone currency** (name TBD) — rare. Spent on unlocking the next era.

In-run:
- Enemies drop currency.
- A store appears on wave intervals or on boss kill.
- Leveling triggers a 3-option passive pick (standard VS-like).

## Modes

- **Campaign** — progression-gated, nine eras.
- **Endless** — infinite scaling in a chosen era.
- **Challenge** — curated constraints (single-attack, tag lock, speedrun, etc.).

## Open questions

- **Player evolution.** How the organism *looks* and *feels* across eras — does the sprite literally evolve, or does the camera framing and world change around a consistent silhouette? What mechanical evolution happens (stat scaling, new base-kit verbs in later eras, etc.)?
- **Elite modifiers.** What are the concrete "swarm modifier" primitives for Elites?
- **Hammer acquisition.** Level-up only, store only, or both? How many per run?
- **Era pacing.** Do eras gate behind boss kills, total biomass, or a specific milestone?
