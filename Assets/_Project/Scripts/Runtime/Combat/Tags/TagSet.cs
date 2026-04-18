using System;
using System.Collections.Generic;
using UnityEngine;

namespace APEX.Tags
{
    /// <summary>
    /// A lightweight set of TagDefinition references. Serializable for inspector use.
    /// </summary>
    [Serializable]
    public struct TagSet
    {
        [SerializeField] private List<TagDefinition> _tags;

        public TagSet(List<TagDefinition> tags)
        {
            _tags = tags ?? new List<TagDefinition>();
        }

        public IReadOnlyList<TagDefinition> Tags => _tags ??= new List<TagDefinition>();

        public bool Contains(TagDefinition tag)
        {
            if (_tags == null) return false;
            for (int i = 0; i < _tags.Count; i++)
            {
                if (_tags[i] == tag) return true;
            }
            return false;
        }

        /// <summary>Returns true if any tag in other is also in this set.</summary>
        public bool Any(TagSet other)
        {
            return IntersectionCount(other) > 0;
        }

        /// <summary>Returns true if every tag in other is also in this set.</summary>
        public bool All(TagSet other)
        {
            if (other._tags == null || other._tags.Count == 0) return true;
            for (int i = 0; i < other._tags.Count; i++)
            {
                if (!Contains(other._tags[i])) return false;
            }
            return true;
        }

        /// <summary>Counts how many tags are shared between this set and other.</summary>
        public int IntersectionCount(TagSet other)
        {
            if (_tags == null || other._tags == null) return 0;
            int count = 0;
            for (int i = 0; i < other._tags.Count; i++)
            {
                if (Contains(other._tags[i])) count++;
            }
            return count;
        }
    }
}
