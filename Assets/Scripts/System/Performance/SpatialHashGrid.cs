using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    public sealed class SpatialHashGrid
    {
        private readonly Dictionary<long, List<Transform>> _buckets = new Dictionary<long, List<Transform>>(4096);
        private readonly float _cellSize;

        public SpatialHashGrid(float cellSize)
        {
            _cellSize = Mathf.Max(0.01f, cellSize);
        }

        public void Clear()
        {
            foreach (var kv in _buckets) kv.Value.Clear();
        }

        public void Add(Transform t)
        {
            if (!t) return;
            var key = KeyFromPosition(t.position);
            if (!_buckets.TryGetValue(key, out var list))
            {
                list = new List<Transform>(64);
                _buckets.Add(key, list);
            }
            list.Add(t);
        }

        public void Query(Vector2 center, float radius, List<Transform> results)
        {
            var r = Mathf.Max(0f, radius);
            if (r <= 0f) return;

            var min = center - Vector2.one * r;
            var max = center + Vector2.one * r;

            var minX = Mathf.FloorToInt(min.x / _cellSize);
            var maxX = Mathf.FloorToInt(max.x / _cellSize);
            var minY = Mathf.FloorToInt(min.y / _cellSize);
            var maxY = Mathf.FloorToInt(max.y / _cellSize);

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var key = PackKey(x, y);
                    if (!_buckets.TryGetValue(key, out var list)) continue;
                    for (var i = 0; i < list.Count; i++)
                    {
                        var t = list[i];
                        if (t) results.Add(t);
                    }
                }
            }
        }

        private long KeyFromPosition(Vector3 position)
        {
            var x = Mathf.FloorToInt(position.x / _cellSize);
            var y = Mathf.FloorToInt(position.y / _cellSize);
            return PackKey(x, y);
        }

        private static long PackKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }
    }
}

