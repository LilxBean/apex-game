using UnityEngine;

namespace APEX.World
{
    /// <summary>
    /// Runtime arena frame: four thick BoxCollider2D walls forming a rectangle
    /// around the playfield. Holds the world rect so cameras, spawners, etc. can
    /// clamp to it.
    /// </summary>
    public class ArenaBounds : MonoBehaviour
    {
        private const float WallThickness = 2f;

        private Rect _worldRect;
        private bool _built;

        public Rect WorldRect => _worldRect;
        public bool IsBuilt => _built;

        public void Build(Vector2 center, Vector2 size)
        {
            // Clear any previous walls if rebuilt.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }

            transform.position = center;
            _worldRect = new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);

            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;
            float t = WallThickness;

            CreateWall("Wall_Top",    new Vector2(0f,  halfH + t * 0.5f), new Vector2(size.x + t * 2f, t));
            CreateWall("Wall_Bottom", new Vector2(0f, -halfH - t * 0.5f), new Vector2(size.x + t * 2f, t));
            CreateWall("Wall_Left",   new Vector2(-halfW - t * 0.5f, 0f), new Vector2(t, size.y));
            CreateWall("Wall_Right",  new Vector2( halfW + t * 0.5f, 0f), new Vector2(t, size.y));

            _built = true;
        }

        private void CreateWall(string name, Vector2 localPos, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = localPos;
            var box = wall.AddComponent<BoxCollider2D>();
            box.size = size;
        }
    }
}
