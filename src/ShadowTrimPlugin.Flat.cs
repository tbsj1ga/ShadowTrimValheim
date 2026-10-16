using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowTrim
{
    // Flat pieces lying on the ground stop casting the sun's shadow.
    //
    // Every piece is looked at once, a little after it loads (the ground under it must be
    // loaded too), within a time budget per frame. It counts as lying on the ground when the
    // whole of what is drawn of it sits between the lowest ground under it (less a small gap)
    // and the highest (plus a small height): its shadow then lands on the ground it covers.
    // The renderers it turns off are remembered with their mode, and given it back when the
    // feature or the mod is switched off.
    public partial class ShadowTrimPlugin
    {
        private sealed class Changed
        {
            public Renderer R;
            public ShadowCastingMode Mode;
        }

        private readonly List<Piece> _queue = new List<Piece>();
        private readonly List<Piece> _later = new List<Piece>();      // the ground was not loaded yet
        private readonly List<Changed> _changed = new List<Changed>();
        private readonly HashSet<int> _flatPieces = new HashSet<int>();
        private int _queueCursor;
        private float _nextLater;
        private bool _flatDirty = true;
        private bool _flatOn;
        private readonly Stopwatch _sw = new Stopwatch();
        private static readonly List<MeshRenderer> s_renderers = new List<MeshRenderer>();

        private const double FlatBudgetMs = 1.0;

        private bool FlatWanted { get { return Active && _cfgFlat.Value; } }

        [HarmonyPatch(typeof(Piece), "Awake")]
        private static class Piece_Awake_Patch
        {
            private static void Postfix(Piece __instance)
            {
                ShadowTrimPlugin p = Instance;
                if (p != null && p._flatOn && __instance != null) p._queue.Add(__instance);
            }
        }

        [HarmonyPatch(typeof(Piece), "OnDestroy")]
        private static class Piece_OnDestroy_Patch
        {
            private static void Postfix(Piece __instance)
            {
                ShadowTrimPlugin p = Instance;
                if (p != null && __instance != null) p._flatPieces.Remove(__instance.GetInstanceID());
            }
        }

        private float _nextPrune;

        private void UpdateFlat()
        {
            // renderers of unloaded pieces, once in a while
            if (Time.unscaledTime >= _nextPrune)
            {
                _nextPrune = Time.unscaledTime + 30f;
                _changed.RemoveAll(delegate(Changed ch) { return ch.R == null; });
            }
            if (_flatDirty)
            {
                _flatDirty = false;
                RestoreFlat();
                _flatOn = FlatWanted;
                if (_flatOn) QueueAll();
            }
            if (!_flatOn) return;

            if (_later.Count > 0 && Time.unscaledTime >= _nextLater)
            {
                _nextLater = Time.unscaledTime + 2f;
                _queue.AddRange(_later);
                _later.Clear();
            }
            if (_queueCursor >= _queue.Count)
            {
                if (_queue.Count > 0) { _queue.Clear(); _queueCursor = 0; }
                return;
            }

            _sw.Reset();
            _sw.Start();
            while (_queueCursor < _queue.Count && _sw.Elapsed.TotalMilliseconds < FlatBudgetMs)
                Look(_queue[_queueCursor++]);
            if (_queueCursor >= _queue.Count) { _queue.Clear(); _queueCursor = 0; }
        }

        private void QueueAll()
        {
            FieldInfo fi = AccessTools.Field(typeof(Piece), "s_allPieces");
            List<Piece> all = fi != null ? fi.GetValue(null) as List<Piece> : null;
            if (all != null) _queue.AddRange(all);
        }

        private void Look(Piece piece)
        {
            if (piece == null) return;
            // the placement ghost is a piece too, without a world object behind it
            ZNetView nv = piece.GetComponent<ZNetView>();
            if (nv == null || !nv.IsValid()) return;
            if (_flatPieces.Contains(piece.GetInstanceID())) return;

            s_renderers.Clear();
            piece.GetComponentsInChildren<MeshRenderer>(true, s_renderers);
            bool have = false;
            Bounds b = new Bounds();
            for (int i = 0; i < s_renderers.Count; i++)
            {
                MeshRenderer r = s_renderers[i];
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy || r.shadowCastingMode == ShadowCastingMode.Off) continue;
                if (!have) { b = r.bounds; have = true; }
                else b.Encapsulate(r.bounds);
            }
            if (!have) return;

            // the ground under the middle and the four corners
            float gMin = float.MaxValue, gMax = float.MinValue;
            Vector3 c = b.center, e = b.extents;
            if (!Ground(c.x, c.z, ref gMin, ref gMax) ||
                !Ground(c.x - e.x, c.z - e.z, ref gMin, ref gMax) || !Ground(c.x + e.x, c.z - e.z, ref gMin, ref gMax) ||
                !Ground(c.x - e.x, c.z + e.z, ref gMin, ref gMax) || !Ground(c.x + e.x, c.z + e.z, ref gMin, ref gMax))
            {
                _later.Add(piece);
                return;
            }
            if (b.min.y - gMin > _cfgFlatGap.Value || b.max.y - gMax > _cfgFlatTop.Value) return;

            for (int i = 0; i < s_renderers.Count; i++)
            {
                MeshRenderer r = s_renderers[i];
                if (r == null || r.shadowCastingMode == ShadowCastingMode.Off || r.shadowCastingMode == ShadowCastingMode.ShadowsOnly) continue;
                _changed.Add(new Changed { R = r, Mode = r.shadowCastingMode });
                r.shadowCastingMode = ShadowCastingMode.Off;
            }
            _flatPieces.Add(piece.GetInstanceID());
        }

        private static bool Ground(float x, float z, ref float gMin, ref float gMax)
        {
            float h;
            if (!Heightmap.GetHeight(new Vector3(x, 0f, z), out h)) return false;
            if (h < gMin) gMin = h;
            if (h > gMax) gMax = h;
            return true;
        }

        private void RestoreFlat()
        {
            for (int i = 0; i < _changed.Count; i++)
            {
                Changed ch = _changed[i];
                if (ch.R != null && ch.R.shadowCastingMode == ShadowCastingMode.Off) ch.R.shadowCastingMode = ch.Mode;
            }
            _changed.Clear();
            _flatPieces.Clear();
            _queue.Clear();
            _later.Clear();
            _queueCursor = 0;
        }

        // For the benchmark: the renderers already found, shadows off or back on, without
        // looking at the pieces again.
        private void FlipFlat(bool off)
        {
            for (int i = 0; i < _changed.Count; i++)
            {
                Changed ch = _changed[i];
                if (ch.R != null) ch.R.shadowCastingMode = off ? ShadowCastingMode.Off : ch.Mode;
            }
        }

        // pieces now flat and renderers turned off, dropping the ones the game has unloaded
        private void FlatCounts(out int pieces, out int renderers)
        {
            _changed.RemoveAll(delegate(Changed ch) { return ch.R == null; });
            renderers = _changed.Count;
            HashSet<int> owners = new HashSet<int>();
            for (int i = 0; i < _changed.Count; i++)
            {
                Piece pc = _changed[i].R.GetComponentInParent<Piece>();
                if (pc != null) owners.Add(pc.GetInstanceID());
            }
            pieces = owners.Count;
        }
    }
}
