using APEX.Enemies.AI;
using APEX.Tags;
using UnityEngine;

namespace APEX.Enemies.Data
{
    /// <summary>
    /// All tunable data for a single enemy type. One SO per enemy variant.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Enemy_New", menuName = "APEX/Enemies/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public EnemyArchetype archetype;

        [Header("Prefab")]
        public GameObject prefab;

        [Header("Stats")]
        public float maxHp = 30f;
        public float moveSpeed = 3f;
        public float contactDamage = 10f;

        [Header("Tags")]
        public TagSet resistances;
        public TagSet weaknesses;

        [Header("AI")]
        public EnemyAIBehaviour aiBehaviour;

        [Header("Debug")]
        public Color debugColor = Color.red;
    }
}
