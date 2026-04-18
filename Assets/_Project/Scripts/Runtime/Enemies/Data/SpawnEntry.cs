using System;

namespace APEX.Enemies.Data
{
    [Serializable]
    public struct SpawnEntry
    {
        public EnemyDefinition definition;
        public float weight;
    }
}
