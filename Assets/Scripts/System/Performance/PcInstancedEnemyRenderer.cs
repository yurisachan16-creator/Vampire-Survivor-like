using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PcInstancedEnemyRenderer : MonoBehaviour
    {
        public static bool Enabled;

        private static PcInstancedEnemyRenderer _instance;

        private readonly List<Enemy> _enemies = new List<Enemy>(8192);
        private readonly Dictionary<Texture, List<Enemy>> _perTexture = new Dictionary<Texture, List<Enemy>>(64);
        private readonly Stack<List<Enemy>> _listPool = new Stack<List<Enemy>>(64);
        private readonly Dictionary<Texture, Material> _materialsByTexture = new Dictionary<Texture, Material>(64);
        private readonly HashSet<SpriteRenderer> _disabledSprites = new HashSet<SpriteRenderer>();

        private Mesh _quad;
        private MaterialPropertyBlock _mpb;

        private readonly Matrix4x4[] _matrices = new Matrix4x4[1023];
        private readonly List<Vector4> _uvRects = new List<Vector4>(1023);
        private readonly List<Vector4> _colors = new List<Vector4>(1023);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance) return;
            var go = new GameObject("PcInstancedEnemyRenderer");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PcInstancedEnemyRenderer>();
        }

        public static void ApplyStartup()
        {
            Enabled = GameSettings.EnablePcInstancedEnemyRenderer &&
                      (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor);
        }

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _quad = BuildQuad();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;

            if (_quad) Destroy(_quad);

            foreach (var kv in _materialsByTexture)
            {
                if (kv.Value) Destroy(kv.Value);
            }
            _materialsByTexture.Clear();
        }

        private void LateUpdate()
        {
            if (!Enabled || (Application.platform != RuntimePlatform.WindowsPlayer && !Application.isEditor) || !Camera.main)
            {
                RestoreDisabledSpritesIfNeeded();
                return;
            }

            EnemyRegistry.AddAllSmallEnemiesTo(_enemies);
            if (_enemies.Count == 0) return;

            var cam = Camera.main;
            var camPos = (Vector2)cam.transform.position;
            var halfH = cam.orthographic ? cam.orthographicSize : 0f;
            var halfW = cam.orthographic ? cam.orthographicSize * cam.aspect : 0f;

            foreach (var kv in _perTexture)
            {
                kv.Value.Clear();
                _listPool.Push(kv.Value);
            }
            _perTexture.Clear();

            for (var i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (!e || e.IsDeadOrIgnoringHurt) continue;
                if (!e.Sprite || !e.Sprite.sprite) continue;

                var pos = (Vector2)e.transform.position;
                if (cam.orthographic)
                {
                    var inView = Mathf.Abs(pos.x - camPos.x) <= halfW + 1f && Mathf.Abs(pos.y - camPos.y) <= halfH + 1f;
                    if (!inView) continue;
                }

                var tex = e.Sprite.sprite.texture;
                if (!tex) continue;

                if (!_perTexture.TryGetValue(tex, out var list))
                {
                    list = _listPool.Count > 0 ? _listPool.Pop() : new List<Enemy>(1024);
                    _perTexture.Add(tex, list);
                }
                list.Add(e);
            }

            foreach (var kv in _perTexture)
            {
                DrawGroup(kv.Key, kv.Value);
            }
        }

        private void DrawGroup(Texture texture, List<Enemy> enemies)
        {
            if (!_materialsByTexture.TryGetValue(texture, out var material) || !material)
            {
                var shader = Shader.Find("VampireSurvivorLike/InstancedSprite2D");
                if (!shader) return;
                material = new Material(shader);
                material.mainTexture = texture;
                material.enableInstancing = true;
                _materialsByTexture[texture] = material;
            }

            _uvRects.Clear();
            _colors.Clear();

            var index = 0;
            for (var i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (!e || !e.Sprite || !e.Sprite.sprite) continue;

                var sr = e.Sprite;
                var sprite = sr.sprite;
                var tex = sprite.texture;
                if (!tex) continue;

                var tr = sprite.textureRect;
                _uvRects.Add(new Vector4(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height));

                var c = sr.color;
                _colors.Add(new Vector4(c.r, c.g, c.b, c.a));

                var size = (Vector3)sprite.bounds.size;
                var lossy = e.transform.lossyScale;
                var sx = size.x * lossy.x * (sr.flipX ? -1f : 1f);
                var sy = size.y * lossy.y * (sr.flipY ? -1f : 1f);

                var pos = e.transform.position;
                var pivotOffset = (sprite.pivot - (Vector2)sprite.rect.size * 0.5f) / Mathf.Max(0.0001f, sprite.pixelsPerUnit);
                pos += new Vector3(pivotOffset.x * lossy.x * (sr.flipX ? -1f : 1f), pivotOffset.y * lossy.y * (sr.flipY ? -1f : 1f), 0f);

                var sortingLayerValue = SortingLayer.GetLayerValueFromID(sr.sortingLayerID);
                pos.z += -(sortingLayerValue * 0.00000001f + sr.sortingOrder * 0.000001f);

                var scale = new Vector3(sx, sy, 1f);
                _matrices[index] = Matrix4x4.TRS(pos, e.transform.rotation, scale);

                if (sr.enabled)
                {
                    sr.enabled = false;
                    _disabledSprites.Add(sr);
                }

                index++;
                if (index >= 1023)
                {
                    Flush(material, index);
                    index = 0;
                    _uvRects.Clear();
                    _colors.Clear();
                }
            }

            if (index > 0) Flush(material, index);
        }

        private void Flush(Material material, int count)
        {
            _mpb.Clear();
            _mpb.SetVectorArray("_UvRect", _uvRects);
            _mpb.SetVectorArray("_Color", _colors);

            Graphics.DrawMeshInstanced(_quad, 0, material, _matrices, count, _mpb, ShadowCastingMode.Off, false, 0, Camera.main);
        }

        private void RestoreDisabledSpritesIfNeeded()
        {
            if (_disabledSprites.Count == 0) return;

            foreach (var sr in _disabledSprites)
            {
                if (!sr) continue;
                if (!sr.gameObject.activeInHierarchy) continue;
                sr.enabled = true;
            }
            _disabledSprites.Clear();
        }

        private static Mesh BuildQuad()
        {
            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
