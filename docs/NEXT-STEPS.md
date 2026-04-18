# APEX — What to do next

This is the handoff checklist for finishing the setup after the initial scaffolding.

## Automated so far

- Repo folder created at `C:\Users\brenn\Repo\apex-game`
- `.gitignore` (Unity-tuned) and `.gitattributes` (LFS + YAML merge) committed
- `.editorconfig`
- `README.md`, `CLAUDE.md` (Claude Code brief), `docs/DESIGN.md`, `docs/ARCHITECTURE.md`, `docs/SETUP.md`
- `scripts/bootstrap.ps1` (runs git init + LFS + initial commit + remote wiring)
- `.claude/settings.json` (placeholder for Claude Code project config)
- Unity Hub opened; **Unity 6.3 LTS (6000.3.13f1) is installing in the background**

## You need to do

### 1. Let Unity 6.3 finish installing

It's already downloading in Unity Hub → Installs. When prompted about modules, include:
- *Microsoft Visual Studio Community* (or Rider if you prefer) — for IntelliSense
- *Windows Build Support (IL2CPP)* — only if you plan to ship standalone .exe builds

### 2. Create the empty GitHub repo

Go to <https://github.com/new> and create:
- **Owner:** LilxBean
- **Repo name:** `apex-game`
- **Visibility:** Private
- **Do NOT** check "Add a README", "Add .gitignore", or "Add license" — we already have those locally. You want a truly empty repo.

### 3. Initialize git locally + push

Open **Git Bash** or **PowerShell** (not the sandbox terminal), then:

```powershell
cd C:\Users\brenn\Repo\apex-game

# Run the bootstrap script (removes broken .git, re-inits, sets up LFS, makes initial commit, adds remote)
.\scripts\bootstrap.ps1

# Push
git push -u origin main
```

If PowerShell blocks the script with an ExecutionPolicy error, run once:
`Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`

If you'd rather do it manually, here's the same thing without the script:

```bash
cd C:/Users/brenn/Repo/apex-game
rm -rf .git                             # there's a broken .git left over from the sandbox
git init -b main
git lfs install
git add -A
git commit -m "Initial scaffold: gitignore, gitattributes, docs, CLAUDE.md"
git remote add origin https://github.com/LilxBean/apex-game.git
git push -u origin main
```

### 4. Create the Unity project in this same folder

Once Unity 6.3 finishes installing:

1. Unity Hub → **New project**
2. Template: **Universal 2D** (URP 2D)
3. Editor version: **Unity 6.3 LTS (6000.3.13f1)**
4. Project name: `apex-game`
5. **Location: `C:\Users\brenn\Repo`** — NOT inside `apex-game`. Unity will put everything into `C:\Users\brenn\Repo\apex-game` and merge with what's already there.
6. Uncheck "Connect to Unity Cloud" if you don't want that.
7. Click **Create project**.

Unity will add `Assets/`, `Packages/`, `ProjectSettings/`, `Library/` into the folder. `.gitignore` already keeps `Library/` out of git.

### 5. First-commit-after-Unity

After Unity finishes generating the project:

```bash
cd C:/Users/brenn/Repo/apex-game
git add -A
git status                              # sanity check — Library/, Temp/, Logs/ should NOT appear
git commit -m "Unity 6.3 project initialized with URP 2D"
git push
```

### 6. Install Claude Code

If you don't already have it:

- <https://docs.claude.com/en/docs/claude-code/overview>

Then from the repo root:

```bash
cd C:/Users/brenn/Repo/apex-game
claude
```

Claude Code will auto-load `CLAUDE.md` as context every session.

### 7. Configure Unity's Smart Merge

See `docs/SETUP.md` section "Unity Smart Merge" — one-time global `.gitconfig` edit so scene/prefab merges are sane.

### 8. Create `Assets/_Project/` skeleton

Once the Unity project is open, make the folder tree described in `docs/ARCHITECTURE.md` inside `Assets/_Project/`. Tell Claude Code "scaffold the folder tree from ARCHITECTURE.md" and it will do it.

## Open design questions to resolve before systems work

These are also tracked at the bottom of `docs/DESIGN.md`:

- Player organism visual + mechanical evolution across eras.
- Elite modifier primitives (what exactly does "modifies swarm" mean mechanically).
- Hammer acquisition rules (level-up, store, or both; and how many per run).
- Era pacing (boss kill vs. milestone currency vs. total biomass).
