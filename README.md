# APEX

A Vampire Survivors–like where you play as a consuming organism rising through the food chain of all existence. Eat, grow, and move on — from the primordial ooze to post-singularity geometry.

## Status

Developing Alpha. Core systems and UX

## Stack

- Unity 6.3 (6000.3 LTS stream)
- C#, Unity Input System, Universal Render Pipeline (URP) — 2D
- Git + Git LFS for binary assets
- [Claude Code](https://docs.claude.com/en/docs/claude-code/overview) for AI-assisted development (see `CLAUDE.md`)

## Repo layout

```
apex-game/
├── Assets/                 # Unity-managed — project code, art, scenes, prefabs
├── Packages/               # Unity-managed — package manifest
├── ProjectSettings/        # Unity-managed — project settings
├── docs/                   # Design + architecture docs
├── .claude/                # Claude Code config (project-level, committed)
├── .editorconfig
├── .gitattributes          # LFS + Unity YAML merge drivers
├── .gitignore
├── CLAUDE.md               # Project brief for Claude Code
└── README.md
```

See `docs/DESIGN.md` for the game-design brief and `docs/ARCHITECTURE.md` for the intended code layout inside `Assets/`.

## Getting started

1. Install Unity Hub and Unity 6.3 (latest stable 6000.3.x).
2. Clone this repo: `git clone git@github.com:LilxBean/apex-game.git`
3. In Unity Hub → *Open* → point at the cloned folder.
4. Let Unity regenerate `Library/` on first open (can take a few minutes).
5. Open the default scene under `Assets/_Project/Scenes/`.

See `docs/SETUP.md` for the full first-time setup, including Git LFS and Unity's Smart Merge tool.
