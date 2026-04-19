using System.Collections.Generic;
using APEX.Combat.Attacks;
using APEX.Enemies.AI;
using APEX.Enemies.Data;
using APEX.Player;
using APEX.Progression;
using APEX.Tags;
using UnityEditor;
using UnityEngine;

namespace APEX.Editor
{
    /// <summary>
    /// One-shot menu item to generate all seed ScriptableObject assets.
    /// Run via APEX > Seed Data > Generate All.
    /// </summary>
    public static class SeedDataGenerator
    {
        [MenuItem("APEX/Seed Data/Generate All")]
        public static void GenerateAll()
        {
            GenerateTags();
            GenerateAI();
            GenerateEnemyPrefabs();
            GenerateEnemyDefinitions();
            GenerateEraDefinition();
            GeneratePlayerAndAttacks();
            GenerateProgression();
            WireEnemyDrops();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SeedDataGenerator] All seed data generated.");
        }

        private static void GenerateTags()
        {
            string[] tagIds =
            {
                "Physical", "Fire", "Acid", "Ice", "Spikey", "Burn", "Bleed",
                "Poison", "Melee", "Freeze", "Projectile", "Area", "Persistent",
                "Nova", "OnHit", "OnKill",
                // Progression descriptor tags
                "Heal", "DamageMult", "Stat", "MoveSpeed", "Passive", "Rate",
                "AoE", "Radius", "Duration", "XPMult", "Chain", "Void", "OnLevel",
                "Slow", "Pierce", "Pull", "Spread"
            };

            EnsureFolder("Assets/_Project/Data/Tags");

            foreach (string id in tagIds)
            {
                string path = $"Assets/_Project/Data/Tags/SO_Tag_{id}.asset";
                if (AssetDatabase.LoadAssetAtPath<TagDefinition>(path) != null) continue;

                var tag = ScriptableObject.CreateInstance<TagDefinition>();
                // Use SerializedObject to set the private _id field
                var so = new SerializedObject(tag);
                so.FindProperty("_id").stringValue = id;
                so.ApplyModifiedPropertiesWithoutUndo();

                AssetDatabase.CreateAsset(tag, path);
            }
        }

        private static void GenerateAI()
        {
            EnsureFolder("Assets/_Project/Data/Enemies");

            CreateAIAsset<ChaseMeleeAI>("SO_AI_Primordial_Chase");
            CreateAIAsset<BossAI>("SO_AI_Primordial_Boss");

            var kite = CreateAIAsset<KiteRangedAI>("SO_AI_Primordial_Kite");
            if (kite != null)
            {
                kite.preferredDistance = 6f;
                kite.fireCooldown = 2f;
                kite.projectileSpeed = 8f;
                kite.projectileDamage = 5f;
                kite.projectileLifetime = 4f;
                // projectilePrefab wired after prefab creation
                EditorUtility.SetDirty(kite);
            }

            var zone = CreateAIAsset<ZoneMagicAI>("SO_AI_Primordial_Zone");
            if (zone != null)
            {
                zone.driftSpeedScale = 0.5f;
                EditorUtility.SetDirty(zone);
            }

            var modifier = CreateAIAsset<ModifierEliteAI>("SO_AI_Primordial_Modifier");
            if (modifier != null)
            {
                modifier.auraRadius = 5f;
                modifier.maxAffected = 5;
                modifier.retargetInterval = 1f;
                modifier.speedMultiplier = 1.25f;
                EditorUtility.SetDirty(modifier);
            }
        }

        private static T CreateAIAsset<T>(string name) where T : EnemyAIBehaviour
        {
            string path = $"Assets/_Project/Data/Enemies/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var ai = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(ai, path);
            return ai;
        }

        private static void GenerateEnemyPrefabs()
        {
            EnsureFolder("Assets/_Project/Prefabs/Enemies");
            EnsureFolder("Assets/_Project/Prefabs/Projectiles");

            // Enemy prefab
            string enemyPath = "Assets/_Project/Prefabs/Enemies/P_Enemy_Default.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(enemyPath) == null)
            {
                var go = new GameObject("P_Enemy_Default");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                go.AddComponent<CircleCollider2D>();
                go.AddComponent<APEX.Combat.Health>();
                go.AddComponent<APEX.Enemies.EnemyController>();

                PrefabUtility.SaveAsPrefabAsset(go, enemyPath);
                Object.DestroyImmediate(go);
            }

            // Projectile prefab
            string projPath = "Assets/_Project/Prefabs/Projectiles/P_EnemyProjectile.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(projPath) == null)
            {
                var go = new GameObject("P_EnemyProjectile");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                sr.color = Color.yellow;
                go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;

                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.15f;

                go.AddComponent<APEX.Enemies.EnemyProjectile>();

                PrefabUtility.SaveAsPrefabAsset(go, projPath);
                Object.DestroyImmediate(go);
            }

            // Wire projectile prefab to KiteRangedAI
            var kiteAI = AssetDatabase.LoadAssetAtPath<KiteRangedAI>(
                "Assets/_Project/Data/Enemies/SO_AI_Primordial_Kite.asset");
            var projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (kiteAI != null && projPrefab != null && kiteAI.projectilePrefab == null)
            {
                kiteAI.projectilePrefab = projPrefab;
                EditorUtility.SetDirty(kiteAI);
            }
        }

        private static void GenerateEnemyDefinitions()
        {
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Enemies/P_Enemy_Default.prefab");

            var chaseAI = AssetDatabase.LoadAssetAtPath<ChaseMeleeAI>(
                "Assets/_Project/Data/Enemies/SO_AI_Primordial_Chase.asset");
            var kiteAI = AssetDatabase.LoadAssetAtPath<KiteRangedAI>(
                "Assets/_Project/Data/Enemies/SO_AI_Primordial_Kite.asset");
            var zoneAI = AssetDatabase.LoadAssetAtPath<ZoneMagicAI>(
                "Assets/_Project/Data/Enemies/SO_AI_Primordial_Zone.asset");
            var modifierAI = AssetDatabase.LoadAssetAtPath<ModifierEliteAI>(
                "Assets/_Project/Data/Enemies/SO_AI_Primordial_Modifier.asset");
            var bossAI = AssetDatabase.LoadAssetAtPath<BossAI>(
                "Assets/_Project/Data/Enemies/SO_AI_Primordial_Boss.asset");

            CreateEnemyDef("SO_Enemy_Primordial_Melee", "Primordial Crawler",
                EnemyArchetype.Melee, enemyPrefab, 30f, 3.5f, 10f, chaseAI, Color.red, 1, 1);

            CreateEnemyDef("SO_Enemy_Primordial_Ranged", "Primordial Spitter",
                EnemyArchetype.Ranged, enemyPrefab, 20f, 2.5f, 5f, kiteAI, Color.yellow, 1, 1);

            CreateEnemyDef("SO_Enemy_Primordial_Magic", "Primordial Ooze",
                EnemyArchetype.Magic, enemyPrefab, 25f, 1.5f, 5f, zoneAI,
                new Color(0.6f, 0.2f, 0.8f), 1, 1); // purple

            CreateEnemyDef("SO_Enemy_Primordial_Elite", "Primordial Alpha",
                EnemyArchetype.Elite, enemyPrefab, 80f, 2f, 15f, modifierAI, Color.cyan, 3, 3);

            CreateEnemyDef("SO_Enemy_Primordial_Boss", "Primordial Leviathan",
                EnemyArchetype.Boss, enemyPrefab, 500f, 1.5f, 25f, bossAI, Color.white, 25, 5);
        }

        private static void CreateEnemyDef(string name, string displayName,
            EnemyArchetype archetype, GameObject prefab, float maxHp, float moveSpeed,
            float contactDamage, EnemyAIBehaviour ai, Color debugColor, int xpDrop, int orbCount)
        {
            string path = $"Assets/_Project/Data/Enemies/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (existing != null)
            {
                existing.xpDrop = xpDrop;
                existing.orbCount = orbCount;
                EditorUtility.SetDirty(existing);
                return;
            }

            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.id = name;
            def.displayName = displayName;
            def.archetype = archetype;
            def.prefab = prefab;
            def.maxHp = maxHp;
            def.moveSpeed = moveSpeed;
            def.contactDamage = contactDamage;
            def.aiBehaviour = ai;
            def.debugColor = debugColor;
            def.xpDrop = xpDrop;
            def.orbCount = orbCount;

            AssetDatabase.CreateAsset(def, path);
        }

        private static void GenerateEraDefinition()
        {
            EnsureFolder("Assets/_Project/Data/Eras");

            string path = "Assets/_Project/Data/Eras/SO_Era_Primordial.asset";
            if (AssetDatabase.LoadAssetAtPath<EraDefinition>(path) != null) return;

            var era = ScriptableObject.CreateInstance<EraDefinition>();
            era.id = "primordial";
            era.displayName = "Primordial";
            era.index = 0;

            era.spawnTable = new List<SpawnEntry>();

            string[] enemyNames = {
                "SO_Enemy_Primordial_Melee",
                "SO_Enemy_Primordial_Ranged",
                "SO_Enemy_Primordial_Magic",
                "SO_Enemy_Primordial_Elite",
                "SO_Enemy_Primordial_Boss"
            };
            float[] weights = { 50f, 25f, 15f, 8f, 2f };

            for (int i = 0; i < enemyNames.Length; i++)
            {
                var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
                    $"Assets/_Project/Data/Enemies/{enemyNames[i]}.asset");
                era.spawnTable.Add(new SpawnEntry { definition = def, weight = weights[i] });
            }

            // Spawn curve: 0.5/s at t=0, 2.0/s at t=60, 3.0/s at t=120, 4.0/s at t=180
            era.spawnRateOverTime = new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(60f, 2f),
                new Keyframe(120f, 3f),
                new Keyframe(180f, 4f)
            );

            AssetDatabase.CreateAsset(era, path);
        }

        // --- Player + attacks --------------------------------------------------

        private static void GeneratePlayerAndAttacks()
        {
            EnsureFolder("Assets/_Project/Data/Player");
            EnsureFolder("Assets/_Project/Data/Attacks");
            EnsureFolder("Assets/_Project/Prefabs/Player");
            EnsureFolder("Assets/_Project/Prefabs/VFX");

            var stats = CreatePlayerStats();
            var spore = CreateSporeProjectilePrefab();
            var pulseRing = CreatePulseRingPrefab();
            var fieldPrefab = CreateCorrosiveFieldPrefab();

            var lunge = CreateLungeAttack();
            var spit = CreateSpitAttack(spore);
            var pulse = CreatePulseAttack(pulseRing);
            var field = CreateFieldAttack(fieldPrefab);

            CreatePlayerPrefab(stats, new AttackDefinition[] { lunge, spit, pulse, field });
        }

        private static PlayerStats CreatePlayerStats()
        {
            string path = "Assets/_Project/Data/Player/PlayerStats_Default.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PlayerStats>(path);
            if (existing != null) return existing;

            var stats = ScriptableObject.CreateInstance<PlayerStats>();
            stats.maxHp = 60f;
            stats.moveSpeed = 6f;
            stats.passiveStreamEnabled = true;
            stats.passiveBiteRadius = 1.2f;
            stats.passiveBiteIntervalSeconds = 0.25f;
            stats.passiveBiteDamage = 3f;
            stats.passiveBiteTags = BuildTagSet("Melee", "Physical");

            AssetDatabase.CreateAsset(stats, path);
            return stats;
        }

        private static LungeAttack CreateLungeAttack()
        {
            string path = "Assets/_Project/Data/Attacks/SO_Attack_Lunge_Default.asset";
            var existing = AssetDatabase.LoadAssetAtPath<LungeAttack>(path);
            if (existing != null) return existing;

            var a = ScriptableObject.CreateInstance<LungeAttack>();
            a.id = "attack.lunge.default";
            a.displayName = "Lunge";
            a.baseCooldownSeconds = 1.2f;
            a.baseDamage = 20f;
            a.tags = BuildTagSet("Melee", "Physical");
            a.autoFireByDefault = true;
            a.lungeDistance = 4f;
            a.lungeDurationSeconds = 0.18f;
            a.lungeHitboxSize = new Vector2(2f, 1.2f);
            a.lungeAcquireRadius = 6f;

            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        private static SpitAttack CreateSpitAttack(GameObject projectilePrefab)
        {
            string path = "Assets/_Project/Data/Attacks/SO_Attack_Spit_Default.asset";
            var existing = AssetDatabase.LoadAssetAtPath<SpitAttack>(path);
            if (existing != null)
            {
                if (existing.projectilePrefab == null && projectilePrefab != null)
                {
                    existing.projectilePrefab = projectilePrefab;
                    EditorUtility.SetDirty(existing);
                }
                return existing;
            }

            var a = ScriptableObject.CreateInstance<SpitAttack>();
            a.id = "attack.spit.default";
            a.displayName = "Acid Spit";
            a.baseCooldownSeconds = 0.9f;
            a.baseDamage = 8f;
            a.tags = BuildTagSet("Projectile", "Acid");
            a.autoFireByDefault = true;
            a.projectilePrefab = projectilePrefab;
            a.spitProjectileSpeed = 12f;
            a.spitProjectileLifetime = 2.5f;
            a.spitAcquireRadius = 12f;

            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        private static PulseAttack CreatePulseAttack(GameObject pulseRingPrefab)
        {
            string path = "Assets/_Project/Data/Attacks/SO_Attack_Pulse_Default.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PulseAttack>(path);
            if (existing != null)
            {
                if (existing.vfxPrefab == null && pulseRingPrefab != null)
                {
                    existing.vfxPrefab = pulseRingPrefab;
                    EditorUtility.SetDirty(existing);
                }
                return existing;
            }

            var a = ScriptableObject.CreateInstance<PulseAttack>();
            a.id = "attack.pulse.default";
            a.displayName = "Radial Pulse";
            a.baseCooldownSeconds = 2.5f;
            a.baseDamage = 25f;
            a.tags = BuildTagSet("Nova", "Physical");
            a.autoFireByDefault = true;
            a.vfxPrefab = pulseRingPrefab;
            a.windupSeconds = 0.15f;
            a.pulseRadius = 4.5f;
            a.knockbackImpulse = 8f;
            a.visualDurationSeconds = 0.3f;

            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        private static FieldAttack CreateFieldAttack(GameObject fieldPrefab)
        {
            string path = "Assets/_Project/Data/Attacks/SO_Attack_Field_Default.asset";
            var existing = AssetDatabase.LoadAssetAtPath<FieldAttack>(path);
            if (existing != null)
            {
                if (existing.fieldPrefab == null && fieldPrefab != null)
                {
                    existing.fieldPrefab = fieldPrefab;
                    EditorUtility.SetDirty(existing);
                }
                return existing;
            }

            var a = ScriptableObject.CreateInstance<FieldAttack>();
            a.id = "attack.field.default";
            a.displayName = "Corrosive Field";
            a.baseCooldownSeconds = 5f;
            a.baseDamage = 8f; // DPS
            a.tags = BuildTagSet("Area", "Persistent", "Acid");
            a.autoFireByDefault = true;
            a.fieldPrefab = fieldPrefab;
            a.fieldRadius = 2.5f;
            a.fieldDurationSeconds = 4f;
            a.fieldTickIntervalSeconds = 0.5f;

            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        private static GameObject CreateSporeProjectilePrefab()
        {
            string path = "Assets/_Project/Prefabs/Projectiles/P_SporeProjectile.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("P_SporeProjectile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.9f, 0.85f, 0.2f);
            go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f; // local space — world-radius ~= 0.15 after scale

            go.AddComponent<SporeProjectile>();

            var asset = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return asset;
        }

        private static GameObject CreatePulseRingPrefab()
        {
            string path = "Assets/_Project/Prefabs/VFX/P_PulseRing.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("P_PulseRing");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(1f, 1f, 1f, 0.35f);
            go.transform.localScale = Vector3.zero;

            go.AddComponent<PulseVisual>();

            var asset = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return asset;
        }

        private static GameObject CreateCorrosiveFieldPrefab()
        {
            string path = "Assets/_Project/Prefabs/VFX/P_CorrosiveField.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("P_CorrosiveField");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.2f, 0.8f, 0.3f, 0.4f);
            sr.sortingOrder = -1;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1f;

            go.AddComponent<CorrosiveField>();

            var asset = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return asset;
        }

        private static void CreatePlayerPrefab(PlayerStats stats, AttackDefinition[] attacks)
        {
            string path = "Assets/_Project/Prefabs/Player/P_Player.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var go = new GameObject("P_Player");
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.55f, 0.95f, 0.55f); // light green
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            go.AddComponent<APEX.Combat.Health>();
            var controller = go.AddComponent<PlayerController>();
            var combat = go.AddComponent<PlayerCombat>();
            go.AddComponent<APEX.Combat.Attacks.Passive.PassiveMeleeStream>();
            go.AddComponent<APEX.Testing.DebugDamageDealer>();

            // Wire private serialized fields
            var ctrlSO = new SerializedObject(controller);
            ctrlSO.FindProperty("_stats").objectReferenceValue = stats;
            ctrlSO.ApplyModifiedPropertiesWithoutUndo();

            var combatSO = new SerializedObject(combat);
            var defs = combatSO.FindProperty("_attackDefinitions");
            defs.arraySize = attacks.Length;
            for (int i = 0; i < attacks.Length; i++)
            {
                defs.GetArrayElementAtIndex(i).objectReferenceValue = attacks[i];
            }
            combatSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static TagSet BuildTagSet(params string[] tagIds)
        {
            var list = new List<TagDefinition>();
            foreach (var id in tagIds)
            {
                var tag = AssetDatabase.LoadAssetAtPath<TagDefinition>(
                    $"Assets/_Project/Data/Tags/SO_Tag_{id}.asset");
                if (tag != null) list.Add(tag);
            }
            return new TagSet(list);
        }

        // --- Progression (XP curve, picks, passives, hammers) ----------------

        private static void GenerateProgression()
        {
            EnsureFolder("Assets/_Project/Data/Progression");
            EnsureFolder("Assets/_Project/Data/Passives");
            EnsureFolder("Assets/_Project/Data/Hammers");
            EnsureFolder("Assets/_Project/Prefabs/Progression");

            var curve = CreateXPCurve();
            var pickTable = CreatePickTable();
            var orbPrefab = CreateXPOrbPrefab();

            var passives = CreatePassives();
            var hammers = CreateHammers();
            var pool = CreatePickPool(passives, hammers);

            // Modify P_Player prefab: add PlayerXP + PlayerBuild with the curve wired.
            WirePlayerProgressionComponents(curve);

            // Unused-locals guard — values are created above for their side effects.
            _ = pickTable; _ = orbPrefab; _ = pool;
        }

        private static XPCurve CreateXPCurve()
        {
            string path = "Assets/_Project/Data/Progression/SO_XPCurve_Default.asset";
            var existing = AssetDatabase.LoadAssetAtPath<XPCurve>(path);
            if (existing != null)
            {
                existing.xpBase = 10f;
                existing.growth = 1.15f;
                existing.maxLevel = 40;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var curve = ScriptableObject.CreateInstance<XPCurve>();
            curve.xpBase = 10f;
            curve.growth = 1.15f;
            curve.maxLevel = 40;
            AssetDatabase.CreateAsset(curve, path);
            return curve;
        }

        private static PickTable CreatePickTable()
        {
            string path = "Assets/_Project/Data/Progression/SO_PickTable_Default.asset";
            var bands = new List<PickTable.LevelBand>
            {
                new() { name = "Early", minLevel = 1,  commonPct = 0.70f, rarePct = 0.25f, epicPct = 0.05f, hammerPct = 0.25f },
                new() { name = "Mid",   minLevel = 9,  commonPct = 0.45f, rarePct = 0.40f, epicPct = 0.15f, hammerPct = 0.35f },
                new() { name = "Late",  minLevel = 18, commonPct = 0.20f, rarePct = 0.50f, epicPct = 0.30f, hammerPct = 0.40f },
            };

            var existing = AssetDatabase.LoadAssetAtPath<PickTable>(path);
            if (existing != null)
            {
                existing.SetBands(bands);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var table = ScriptableObject.CreateInstance<PickTable>();
            table.SetBands(bands);
            AssetDatabase.CreateAsset(table, path);
            return table;
        }

        private static List<PassiveDefinition> CreatePassives()
        {
            var list = new List<PassiveDefinition>();

            list.Add(CreatePassive("passive.ravenous", "Ravenous", Rarity.Common,
                PassiveKind.Ravenous, "Heal 1 HP on kill.",
                "OnKill", "Heal"));
            list.Add(CreatePassive("passive.sharper_teeth", "Sharper Teeth", Rarity.Common,
                PassiveKind.SharperTeeth, "+10% damage to [Melee] attacks.",
                "Melee", "DamageMult"));
            list.Add(CreatePassive("passive.swift", "Swift", Rarity.Common,
                PassiveKind.Swift, "+5% move speed.",
                "Stat", "MoveSpeed"));
            list.Add(CreatePassive("passive.quick_biter", "Quick Biter", Rarity.Common,
                PassiveKind.QuickBiter, "Passive melee stream ticks 10% faster.",
                "Passive", "Melee", "Rate"));

            list.Add(CreatePassive("passive.acid_spray", "Acid Spray", Rarity.Rare,
                PassiveKind.AcidSpray, "[Acid] hits splash 20% damage in 1.5m.",
                "Acid", "OnHit", "AoE"));
            list.Add(CreatePassive("passive.corrosion", "Corrosion", Rarity.Rare,
                PassiveKind.Corrosion, "Targets hit by [Acid] take 3 dps for 2s.",
                "Acid", "Burn"));
            list.Add(CreatePassive("passive.pulse_overlap", "Pulse Overlap", Rarity.Rare,
                PassiveKind.PulseOverlap, "[Nova] attacks gain +25% radius.",
                "Nova", "Radius"));
            list.Add(CreatePassive("passive.field_lingering", "Field Lingering", Rarity.Rare,
                PassiveKind.FieldLingering, "[Persistent] zones last +30% longer.",
                "Persistent", "Duration"));

            list.Add(CreatePassive("passive.apex_hunger", "Apex Hunger", Rarity.Epic,
                PassiveKind.ApexHunger, "+20% XP gained.",
                "OnKill", "XPMult"));
            list.Add(CreatePassive("passive.swarm_rend", "Swarm Rend", Rarity.Epic,
                PassiveKind.SwarmRend, "Lunge hits chain to 1 additional target.",
                "OnHit", "Chain"));
            list.Add(CreatePassive("passive.voidburst", "Voidburst", Rarity.Epic,
                PassiveKind.Voidburst, "On level-up, deal 50 [Void] damage to all on-screen enemies.",
                "Void", "OnLevel"));
            list.Add(CreatePassive("passive.consuming_wave", "Consuming Wave", Rarity.Epic,
                PassiveKind.ConsumingWave, "[Nova] hits heal 1 HP per enemy hit.",
                "Nova", "Heal"));

            return list;
        }

        private static PassiveDefinition CreatePassive(string id, string displayName,
            Rarity rarity, PassiveKind kind, string description, params string[] tagIds)
        {
            string safeName = displayName.Replace(" ", "");
            string path = $"Assets/_Project/Data/Passives/SO_Passive_{safeName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PassiveDefinition>(path);
            if (existing != null)
            {
                existing.id = id;
                existing.displayName = displayName;
                existing.rarity = rarity;
                existing.kind = kind;
                existing.description = description;
                existing.tags = BuildTagSet(tagIds);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var p = ScriptableObject.CreateInstance<PassiveDefinition>();
            p.id = id;
            p.displayName = displayName;
            p.rarity = rarity;
            p.kind = kind;
            p.description = description;
            p.tags = BuildTagSet(tagIds);
            p.maxStacks = 1;
            AssetDatabase.CreateAsset(p, path);
            return p;
        }

        private static List<HammerDefinition> CreateHammers()
        {
            var lunge = AssetDatabase.LoadAssetAtPath<AttackDefinition>(
                "Assets/_Project/Data/Attacks/SO_Attack_Lunge_Default.asset");
            var spit = AssetDatabase.LoadAssetAtPath<AttackDefinition>(
                "Assets/_Project/Data/Attacks/SO_Attack_Spit_Default.asset");
            var pulse = AssetDatabase.LoadAssetAtPath<AttackDefinition>(
                "Assets/_Project/Data/Attacks/SO_Attack_Pulse_Default.asset");
            var field = AssetDatabase.LoadAssetAtPath<AttackDefinition>(
                "Assets/_Project/Data/Attacks/SO_Attack_Field_Default.asset");

            var list = new List<HammerDefinition>();

            list.Add(CreateHammer("hammer.ignite_lunge", "Ignite Lunge", Rarity.Common,
                HammerKind.IgniteLunge, lunge,
                "Lunge ignites targets for 5 dps over 3s.",
                "Fire", "Burn"));
            list.Add(CreateHammer("hammer.twin_spit", "Twin Spit", Rarity.Common,
                HammerKind.TwinSpit, spit,
                "Spit fires 2 projectiles in a narrow cone.",
                "Projectile"));
            list.Add(CreateHammer("hammer.ringing_pulse", "Ringing Pulse", Rarity.Common,
                HammerKind.RingingPulse, pulse,
                "Pulse fires a second smaller ring 0.25s later.",
                "Nova"));
            list.Add(CreateHammer("hammer.sticky_field", "Sticky Field", Rarity.Common,
                HammerKind.StickyField, field,
                "Corrosive Field slows enemies inside by 30%.",
                "Slow"));

            list.Add(CreateHammer("hammer.harpoon_lunge", "Harpoon Lunge", Rarity.Rare,
                HammerKind.HarpoonLunge, lunge,
                "Lunge becomes a thrown tendril; hits chain to 2.",
                "Projectile", "Chain"));
            list.Add(CreateHammer("hammer.pressurized_spit", "Pressurized Spit", Rarity.Rare,
                HammerKind.PressurizedSpit, spit,
                "Spit pierces through enemies up to 3 times.",
                "Pierce"));
            list.Add(CreateHammer("hammer.gravity_pulse", "Gravity Pulse", Rarity.Rare,
                HammerKind.GravityPulse, pulse,
                "Pulse pulls enemies toward player before damaging.",
                "Pull"));
            list.Add(CreateHammer("hammer.infecting_field", "Infecting Field", Rarity.Rare,
                HammerKind.InfectingField, field,
                "Enemies leaving Field carry a mini-field for 2s.",
                "Spread"));

            return list;
        }

        private static HammerDefinition CreateHammer(string id, string displayName,
            Rarity rarity, HammerKind kind, AttackDefinition target, string description,
            params string[] tagIds)
        {
            string safeName = displayName.Replace(" ", "");
            string path = $"Assets/_Project/Data/Hammers/SO_Hammer_{safeName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<HammerDefinition>(path);
            if (existing != null)
            {
                existing.id = id;
                existing.displayName = displayName;
                existing.rarity = rarity;
                existing.kind = kind;
                existing.targetAttack = target;
                existing.description = description;
                existing.tagsAdded = BuildTagSet(tagIds);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var h = ScriptableObject.CreateInstance<HammerDefinition>();
            h.id = id;
            h.displayName = displayName;
            h.rarity = rarity;
            h.kind = kind;
            h.targetAttack = target;
            h.description = description;
            h.tagsAdded = BuildTagSet(tagIds);
            AssetDatabase.CreateAsset(h, path);
            return h;
        }

        private static PickPool CreatePickPool(
            List<PassiveDefinition> passives, List<HammerDefinition> hammers)
        {
            string path = "Assets/_Project/Data/Progression/SO_PickPool_Default.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PickPool>(path);
            if (existing != null)
            {
                existing.passives = new List<PassiveDefinition>(passives);
                existing.hammers = new List<HammerDefinition>(hammers);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var pool = ScriptableObject.CreateInstance<PickPool>();
            pool.passives = new List<PassiveDefinition>(passives);
            pool.hammers = new List<HammerDefinition>(hammers);
            AssetDatabase.CreateAsset(pool, path);
            return pool;
        }

        private static GameObject CreateXPOrbPrefab()
        {
            string path = "Assets/_Project/Prefabs/Progression/P_XPOrb.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("P_XPOrb");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.4f, 0.9f, 0.5f, 1f);
            sr.sortingOrder = 3;
            go.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            go.AddComponent<XPOrb>();

            var asset = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return asset;
        }

        private static void WirePlayerProgressionComponents(XPCurve curve)
        {
            string path = "Assets/_Project/Prefabs/Player/P_Player.prefab";
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return;

            using var scope = new PrefabUtility.EditPrefabContentsScope(path);
            var root = scope.prefabContentsRoot;

            var xp = root.GetComponent<PlayerXP>();
            if (xp == null) xp = root.AddComponent<PlayerXP>();
            var xpSO = new SerializedObject(xp);
            xpSO.FindProperty("_curve").objectReferenceValue = curve;
            xpSO.ApplyModifiedPropertiesWithoutUndo();

            if (root.GetComponent<PlayerBuild>() == null) root.AddComponent<PlayerBuild>();
        }

        private static void WireEnemyDrops()
        {
            string path = "Assets/_Project/Prefabs/Enemies/P_Enemy_Default.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;

            using var scope = new PrefabUtility.EditPrefabContentsScope(path);
            var root = scope.prefabContentsRoot;
            if (root.GetComponent<EnemyDrop>() == null) root.AddComponent<EnemyDrop>();
        }

        // --- Misc --------------------------------------------------------------

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
