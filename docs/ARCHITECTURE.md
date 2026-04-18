# APEX — Architecture & Folder Layout

This document is the canonical source for how code and content are organized inside the Unity project. If Claude (or anyone) needs to add a new system, it goes here first.

## Top-level

Unity creates and owns `Assets/`, `Packages/`, `ProjectSettings/` on first open. Everything project-specific lives under `Assets/_Project/` so it never collides with imported asset-store or package content.

```
Assets/
├── _Project/                  # everything we author
│   ├── Art/
│   │   ├── Sprites/
│   │   ├── Shaders/
│   │   ├── VFX/
│   │   └── Animations/
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/
│   ├── Data/                  # all ScriptableObject assets
│   │   ├── Attacks/
│   │   ├── Enemies/
│   │   ├── Passives/
│   │   ├── Hammers/
│   │   ├── Eras/
│   │   └── Tags/
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Projectiles/
│   │   ├── VFX/
│   │   └── UI/
│   ├── Scenes/
│   │   ├── Runtime/
│   │   │   ├── Bootstrap.unity
│   │   │   ├── Den.unity
│   │   │   └── Run.unity
│   │   └── Test/
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   ├── Core/          # Bootstrap, GameLoop, Services, EventBus
│   │   │   │   ├── Events/    # EventBus (static C# events)
│   │   │   │   └── Pooling/   # PrefabPool wrapper
│   │   │   ├── Player/        # Organism, Movement, Stats
│   │   │   ├── Combat/        # Attacks, Hit, Damage, Tags
│   │   │   │   └── Tags/      # TagDefinition, TagSet
│   │   │   ├── Enemies/       # AI, Archetypes, Spawner, Director
│   │   │   │   ├── AI/        # EnemyAIBehaviour + concrete AIs
│   │   │   │   ├── Data/      # EnemyDefinition, EraDefinition, SpawnEntry
│   │   │   │   └── Spawning/  # EnemySpawner, EraDirector
│   │   │   ├── Progression/   # XP, LevelUp, Passives, Hammers
│   │   │   ├── Eras/          # (reserved for future era-specific code)
│   │   │   ├── Den/           # Between-run UI and persistence
│   │   │   ├── Testing/       # Temp stubs: TestPlayerStub, DebugDamageDealer, SandboxHUD
│   │   │   ├── UI/
│   │   │   └── Utils/
│   │   │   APEX.Runtime.asmdef
│   │   ├── Editor/            # Custom inspectors, tooling
│   │   │   APEX.Editor.asmdef
│   │   └── Tests/
│   │       ├── EditMode/
│   │       └── PlayMode/
│   └── Settings/
│       ├── Input/             # InputActions asset
│       ├── Rendering/         # URP asset, 2D renderer
│       └── Physics/
└── Plugins/                   # third-party drop-ins (rare)
```

## Assembly definitions

Start with a single runtime asmdef once the `Runtime/` folder has more than a couple of systems:

- `APEX.Runtime.asmdef` — everything under `Scripts/Runtime/`
- `APEX.Editor.asmdef` — `Scripts/Editor/`, references `APEX.Runtime`
- `APEX.Tests.EditMode.asmdef` — `Scripts/Tests/EditMode/`, references `APEX.Runtime`
- `APEX.Tests.PlayMode.asmdef` — `Scripts/Tests/PlayMode/`, references `APEX.Runtime`

Split further when a subsystem stabilizes and compile-time starts to feel sluggish.

## Core runtime shape

A rough sketch of how the main systems relate. Concrete APIs will evolve.

- **Bootstrap** (scene or `[RuntimeInitializeOnLoadMethod]`) constructs singletons/services once: `EventBus`, `ObjectPool`, `TagRegistry`, `ContentCatalog`.
- **EraDirector** owns the active era: selects enemy archetypes, art swaps, spawn tables, music.
- **Spawner** asks the EraDirector "what should I spawn right now?" on a fixed interval, pulls from `ObjectPool`.
- **Player.Organism** holds stats; `Player.Movement` reads input; `Player.Combat` runs the four attack timers.
- **Attacks** are data (ScriptableObjects) + one `IAttack` runtime that reads the data, emits hits, applies tags.
- **Hits** flow through a small `Damage` record (source, tags, amount, crit) → target's `Health` → `EventBus.OnHit` / `OnKill`.
- **Passives** subscribe to `EventBus` events and filter by tag.
- **Hammers** mutate an attack's runtime config at pickup time; implementations live in `Progression/Hammers/`.

## Data vs. code

- **Data** — everything designer-tweakable: stat numbers, spawn weights, attack cooldowns, era palettes. Lives in `_Project/Data/` as ScriptableObjects.
- **Code** — behavior, not numbers. If you find yourself typing a literal number into a `.cs` file that a designer would want to change, move it to a SO.

## Naming

- Namespaces mirror folders: `APEX.Combat.Attacks`, `APEX.Enemies.AI`, etc.
- Prefabs: `P_Enemy_Primordial_Melee`, `P_Projectile_AcidSpit`, etc. (`P_` prefix so they sort together in Project view).
- ScriptableObject assets: `SO_Attack_Lunge`, `SO_Enemy_Primordial_Melee`, `SO_Passive_TagSynergy_FireBurn`.
- Scenes: `PascalCase`, no prefix.
