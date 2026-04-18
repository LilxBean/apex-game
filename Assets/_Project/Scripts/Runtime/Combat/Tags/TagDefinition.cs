using UnityEngine;

namespace APEX.Tags
{
    /// <summary>
    /// A single tag identity. Tags are the shared language across combat, enemies, and passives.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Tag_New", menuName = "APEX/Tags/Tag Definition")]
    public class TagDefinition : ScriptableObject
    {
        [SerializeField] private string _id;

        public string Id => _id;
    }
}
