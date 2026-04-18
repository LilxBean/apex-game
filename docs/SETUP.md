# APEX — First-Time Setup

This is the cold-start guide for a new machine cloning the repo.

## 1. Prerequisites

- **Git** with **Git LFS**. Check: `git --version` and `git lfs version`. Install LFS from <https://git-lfs.com> if missing.
- **Unity Hub**. Download: <https://unity.com/download>.
- **Unity 6.3 (6000.3 LTS stream)**, installed through Unity Hub. Include the *Windows Build Support (IL2CPP)* module if you want standalone builds.

## 2. Clone

```bash
git lfs install                        # once per machine, global
git clone git@github.com:LilxBean/apex-game.git
cd apex-game
```

If you cloned before installing LFS, run `git lfs pull` afterwards to hydrate binary assets.

## 3. Open in Unity

1. Unity Hub → **Open** → select the `apex-game` folder.
2. Unity regenerates `Library/`. This can take several minutes on first open — this is normal.
3. When the project is open, go to **Edit → Project Settings** and verify:
   - **Player → Company Name** and **Product Name** are set.
   - **Graphics** uses the URP asset under `Assets/_Project/Settings/Rendering/`.
   - **Input System Package** is the active input handler (not Input Manager).

## 4. Unity Smart Merge (for `.unity` / `.prefab` merges)

Set up Unity's YAML merge tool once per machine. Edit your global `.gitconfig`:

```ini
[merge "unityyamlmerge"]
    name = Unity SmartMerge
    driver = '<UNITY_INSTALL>\Editor\Data\Tools\UnityYAMLMerge.exe' merge -p --force --fallback none %O %B %A %A
    recursive = binary
```

Replace `<UNITY_INSTALL>` with the actual path Unity Hub used (typically `C:\Program Files\Unity\Hub\Editor\6000.3.x\`).

This is what makes the `merge=unityyamlmerge` attributes in `.gitattributes` actually do something useful.

## 5. Claude Code

```bash
cd apex-game
claude
```

Claude Code loads `CLAUDE.md` automatically from the repo root. If you haven't installed Claude Code yet: <https://docs.claude.com/en/docs/claude-code/overview>.

## 6. Sanity check

- Open **Window → Package Manager** and confirm URP and Input System are installed.
- Press Play on the default scene — it should open without console errors.
- Commit a no-op change (e.g., touch `README.md`) to confirm LFS and line-ending rules round-trip cleanly.
