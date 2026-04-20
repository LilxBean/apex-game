# APEX — Architecture & Folder Layout

This document is the canonical source for how code and content are organized inside the Unity project. If Claude (or anyone) needs to add a new system, it goes here first.

## Top-level

Unity creates and owns `Assets/`, `Packages/`, `ProjectSettings/` on first open. Everything project-specific lives under `Assets/_Project/` so it never collides with imported asset-store or package content.

```
Assets/
├── _Project/                    # everything we author
│   ├── Art/
│   │   ├── Sprites/
│   │   ├── Shaders/
│   │   ├── VFX/
│   │   └── Animations/
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/
│   ├── Data/                    # all ScriptableObject assets
│   │   ├── Attacks/             # 4 AttackDefinition defaults
│   │   ├── Enemies/             # EnemyDefinition + AI behaviour SOs
│   │   ├── Eras/
│   │   ├── Hammers/             # 8 seeded attack mutators
│   │   ├── Passives/            # 12 seeded passives
│   │   ├── Player/              # PlayerStats defaults
│   │   ├── Progression/         # XPCurve, PickTable, PickPool
│   │   └── Tags/                # ~32 tag SOs
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Projectiles/
│   │   ├── Progression/         # P_XPOrb
│   │   ├── VFX/
│   │   └── UI/
│   ├── Scenes/
│   │   ├── Runtime/
│   │   │   └── Sandbox.unity    # current playable scene
│   │   │   # Planned: MainMenu.unity, Run.unity (see docs/ROADMAP.md §1)
│   │   └── Test/
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   ├── Camera/          # CameraFollow2D
│   │   │   ├── Combat/          # Damage, Health, IDamageable, IPlayerTarget
│   │   │   │   ├── Attacks/     # AttackDefinition + 4 attacks + instances + mutations
│   │   │   │   │   └── Passive/ # PassiveMeleeStream
│   │   │   │   └── Tags/        # TagDefinition, TagSet
│   │   │   ├── Core/
│   │   │   │   ├── Events/      # static C# EventBus
│   │   │   │   └── Pooling/     # PrefabPool wrapper
│   │   │   ├── Den/             # between-run shell (unbuilt)
│   │   │   ├── Enemies/         # EnemyController + EnemyProjectile
│   │   │   │   ├── AI/          # EnemyAIBehaviour + archetype AIs + ISwarmModifier
│   │   │   │   ├── Data/        # EnemyDefinition, EraDefinition, SpawnEntry
│   │   │   │   └── Spawning/    # EnemySpawner, EraDirector
│   │   │   ├── Eras/            # (reserved for era-specific code)
│   │   │   ├── Input/           # APEXControls (generated input class)
│   │   │   ├── Player/          # PlayerController, PlayerCombat, PlayerStats
│   │   │   ├── Progression/     # XP, PickPool, Passive/Hammer systems, RunManager, RunBootstrap
│   │   │   ├── Testing/         # DebugDamageDealer, SandboxHUD
│   │   │   ├── UI/              # ProgressionHUD, LevelUpScreen, RunEndScreen, PauseController, UIFactory, FloatingDamageNumbers
│   │   │   ├── Utils/
│   │   │   ├── World/           # ArenaBounds, BackgroundTiler
│   │   │   └── APEX.Runtime.asmdef
│   │   ├── Editor/              # SeedDataGenerator + custom inspectors
│   │   │   └── APEX.Editor.asmdef
│   │   └── Tests/
│   │       ├── EditMode/
│   │       └── PlayMode/
│   └── Settings/
│       ├── Input/               # InputActions asset
│       ├── Rendering/           # URP asset, 2D renderer
│       └── Physics/
└── Plugins/                     # third-party drop-ins (rare)
```

## Assembly definitions

- `APEX.Runtime.asmdef` — everything under `Scripts/Runtime/`
- `APEX.Editor.asmdef` — `Scripts/Editor/`, references `APEX.Runtime`
- `APEX.Tests.EditMode.asmdef` — reserved (tests not yet written)
- `APEX.Tests.PlayMode.asmdef` — reserved

Split further when a subsystem stabilizes and compile-time starts to feel sluggish.

## Core runtime shape

A rough sketch of how the main systems relate. Concrete APIs evolve with each session.

- **RunBootstrap** (scene object) constructs the run-scoped state: instantiates the `RunManager`, HUD, pick screens, and pools for the current scene. `EventBus` handles are already static.
- **RunManager** owns the run lifecycle: tracks elapsed time, triggers run-end on death or timer, aggregates stats, broadcasts `EventBus.OnRunEnded`.
- **EraDirector** owns the active era: selects enemy archetypes, art swaps, spawn tables, music.
- **EnemySpawner** asks the `EraDirector` "what should I spawn right now?" on a fixed interval, pulls from `PrefabPool`.
- **PlayerController / PlayerCombat / PlayerStats** hold stats, read input, and run the four attack timers.
- **Attacks** are ScriptableObjects (`AttackDefinition`) + an `IAttackInstance` runtime that reads the data, emits hits, applies tags. `AttackMutations` is the shared mutation API Hammers plug into.
- **Hits** flow through a `Damage` struct (source, tags, amount, crit) → target's `Health` → `EventBus.OnHit` / `OnKill`.
- **Progression**: `EnemyDrop` spawns `XPOrb`s on kill → `PlayerXP` accrues → `OnLevelUp` triggers `LevelUpScreen` → `PickRoller` pulls from `PickPool` using `PickTable` bands → pick is applied to `PlayerBuild` (via `PassiveApplier` or `HammerApplier`).
- **Passives** subscribe to `EventBus` events and filter by tag.
- **Hammers** mutate an `AttackDefinition`'s runtime config when applied.
- **UI**: `ProgressionHUD` shows the two-tier level+XP bar and run timer; `LevelUpScreen` and `RunEndScreen` are Canvas overlays toggled by events. `UIFactory` builds screens programmatically (no prefab dependency); `FloatingDamageNumbers` pools floating hit numbers.

## Data vs. code

- **Data** — everything designer-tweakable: stat numbers, spawn weights, attack cooldowns, era palettes. Lives in `_Project/Data/` as ScriptableObjects.
- **Code** — behavior, not numbers. If you find yourself typing a literal number into a `.cs` file that a designer would want to change, move it to a SO.

Balance targets are maintained in `docs/balance.xlsx`; ScriptableObjects should mirror its values and be re-tuned from it.

## Naming

- Namespaces mirror folders: `APEX.Combat.Attacks`, `APEX.Enemies.AI`, `APEX.Progression`, etc.
- Prefabs: `P_Enemy_Primordial_Melee`, `P_Projectile_AcidSpit`, `P_XPOrb` (`P_` prefix so they sort together in Project view).
- ScriptableObject assets: `SO_Attack_Lunge_Default`, `SO_Enemy_Primordial_Melee`, `SO_Passive_Ravenous`, `SO_Hammer_IgniteLunge`, `SO_Tag_Fire`.
- Scenes: `PascalCase`, no prefix.
