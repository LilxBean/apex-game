using System.Collections.Generic;
using APEX.Enemies.AI;
using APEX.Enemies.Data;
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
                "OnHit", "OnKill"
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
                EnemyArchetype.Melee, enemyPrefab, 30f, 3.5f, 10f, chaseAI, Color.red);

            CreateEnemyDef("SO_Enemy_Primordial_Ranged", "Primordial Spitter",
                EnemyArchetype.Ranged, enemyPrefab, 20f, 2.5f, 5f, kiteAI, Color.yellow);

            CreateEnemyDef("SO_Enemy_Primordial_Magic", "Primordial Ooze",
                EnemyArchetype.Magic, enemyPrefab, 25f, 1.5f, 5f, zoneAI,
                new Color(0.6f, 0.2f, 0.8f)); // purple

            CreateEnemyDef("SO_Enemy_Primordial_Elite", "Primordial Alpha",
                EnemyArchetype.Elite, enemyPrefab, 80f, 2f, 15f, modifierAI, Color.cyan);

            CreateEnemyDef("SO_Enemy_Primordial_Boss", "Primordial Leviathan",
                EnemyArchetype.Boss, enemyPrefab, 500f, 1.5f, 25f, bossAI, Color.white);
        }

        private static void CreateEnemyDef(string name, string displayName,
            EnemyArchetype archetype, GameObject prefab, float maxHp, float moveSpeed,
            float contactDamage, EnemyAIBehaviour ai, Color debugColor)
        {
            string path = $"Assets/_Project/Data/Enemies/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path) != null) return;

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
