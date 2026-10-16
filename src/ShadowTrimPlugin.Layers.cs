using System.Collections.Generic;
using UnityEngine;

namespace ShadowTrim
{
    // Distance by layer: the sun's Light.layerShadowCullDistances, one float per layer, 0 for
    // "the shadow distance". Set when the sun or a setting changes; the engine does the rest.
    public partial class ShadowTrimPlugin
    {
        private Light _sun;                     // the light the distances were given to
        private float[] _applied;               // what it was given
        private bool _layersDirty = true;
        private float _nextSunCheck;

        private bool Active { get { return _cfgEnabled != null && _cfgEnabled.Value; } }

        private Light Sun
        {
            get
            {
                EnvMan env = EnvMan.instance;
                return env != null ? env.m_dirLight : null;
            }
        }

        private void UpdateLayers()
        {
            // the sun is checked twice a second, not every frame: it only changes with the scene
            if (!_layersDirty && Time.unscaledTime < _nextSunCheck) return;
            _nextSunCheck = Time.unscaledTime + 0.5f;
            Light sun = Sun;
            if (!_layersDirty && sun == _sun) return;
            _layersDirty = false;

            if (_sun != null && _sun != sun) Clear(_sun);
            _sun = sun;
            if (sun == null) { _applied = null; return; }

            float[] d = Active ? Distances() : null;
            if (d == null) { Clear(sun); _applied = null; return; }
            sun.layerShadowCullDistances = d;
            _applied = d;
        }

        private float[] Distances()
        {
            float[] d = new float[32];
            bool any = false;
            foreach (KeyValuePair<string, float> kv in LayerSettings())
            {
                int layer = LayerMask.NameToLayer(kv.Key);
                if (layer < 0)
                {
                    WarnOnce("layer:" + kv.Key, "There is no layer '" + kv.Key + "' in this game; its distance is ignored.");
                    continue;
                }
                float v = kv.Value;
                if (v < 0f || float.IsNaN(v)) v = 0f;
                d[layer] = v;
                if (v > 0f) any = true;
            }
            return any ? d : null;
        }

        private static void Clear(Light l)
        {
            if (l != null) l.layerShadowCullDistances = new float[32];
        }

        private void RestoreLayers()
        {
            if (_sun != null) Clear(_sun);
            _sun = null;
            _applied = null;
        }

        // the distance a layer's shadows end at now (the game's shadow distance when not trimmed)
        private float EffectiveDistance(int layer)
        {
            float game = QualitySettings.shadowDistance;
            if (_applied == null || layer < 0 || layer >= _applied.Length || _applied[layer] <= 0f) return game;
            return Mathf.Min(game, _applied[layer]);
        }
    }
}
