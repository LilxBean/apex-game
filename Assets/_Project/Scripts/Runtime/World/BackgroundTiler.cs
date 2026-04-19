using UnityEngine;

namespace APEX.World
{
    /// <summary>
    /// Repeating background built from a procedural grid tile. Uses SpriteRenderer
    /// in tiled draw mode so the tile repeats across the arena rect without extra
    /// geometry. Runs entirely at runtime so no texture asset is checked in.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundTiler : MonoBehaviour
    {
        private const int TexSize = 64;
        private const int PixelsPerUnit = 32;
        private static readonly Color BaseColor = new(0.047f, 0.098f, 0.063f, 1f); // #0C1910
        private static readonly Color GridColor = new(0.101f, 0.172f, 0.133f, 1f); // #1A2C22

        [SerializeField] private Sprite _overrideSprite;
        [SerializeField] private float _margin = 4f;

        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sprite = _overrideSprite != null ? _overrideSprite : BuildTileSprite();
            _renderer.drawMode = SpriteDrawMode.Tiled;
            _renderer.tileMode = SpriteTileMode.Continuous;
            _renderer.sortingOrder = -100;
        }

        public void Build(Rect worldRect)
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            transform.position = new Vector3(worldRect.center.x, worldRect.center.y, 0f);
            _renderer.size = worldRect.size + Vector2.one * _margin * 2f;
        }

        private static Sprite BuildTileSprite()
        {
            var tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                name = "BackgroundTile_Runtime"
            };

            var pixels = new Color[TexSize * TexSize];
            for (int y = 0; y < TexSize; y++)
            {
                for (int x = 0; x < TexSize; x++)
                {
                    bool gridLine = (x % 32 == 0) || (y % 32 == 0);
                    pixels[y * TexSize + x] = gridLine ? GridColor : BaseColor;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(false, true);

            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, TexSize, TexSize),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "BackgroundTile_Runtime";
            return sprite;
        }
    }
}
