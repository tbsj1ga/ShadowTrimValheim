using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowTrim
{
    public partial class ShadowTrimPlugin
    {
        // ------------------------------------------------------------------
        // console command: shadowtrim
        // ------------------------------------------------------------------
        private void RegisterCommands()
        {
            new Terminal.ConsoleCommand("shadowtrim",
                "ShadowTrim. 'shadowtrim' switches it on and off to compare; 'shadowtrim status' counts the shadows around you",
                delegate(Terminal.ConsoleEventArgs args) { RunCommand(args); });
        }

        private static void Say(Terminal.ConsoleEventArgs args, string text)
        {
            if (args != null && args.Context != null) args.Context.AddString(text);
        }

        private void RunCommand(Terminal.ConsoleEventArgs args)
        {
            try
            {
                string sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
                if (sub == "status") { Say(args, Status()); return; }
                if (sub == "on" || sub == "off" || sub == "")
                {
                    bool on = sub == "" ? !_cfgEnabled.Value : sub == "on";
                    _cfgEnabled.Value = on;
                    Say(args, "ShadowTrim " + (on ? "on" : "off") + ". 'shadowtrim status' counts the shadows around you.");
                    return;
                }
                Say(args, "shadowtrim [on|off|status]");
            }
            catch (Exception e) { Say(args, "shadowtrim failed: " + e.Message); }
        }

        private sealed class LayerCount
        {
            public int Casting, Trimmed;
        }

        // Every renderer that casts a shadow within the game's shadow distance of the camera,
        // by layer, and how many of them are past their layer's distance now (cast nothing).
        // A walk over the whole scene: fine for a command, not for every frame.
        private string Status()
        {
            StringBuilder sb = new StringBuilder();
            Light sun = Sun;
            sb.Append("ShadowTrim ").Append(Version).Append(Active ? ": on" : ": off")
              .Append(", the game's shadow distance ").Append(QualitySettings.shadowDistance.ToString("0")).Append(" m")
              .Append(sun == null ? ", no sun here" : "").Append('\n');

            Camera cam = Camera.main;
            if (cam == null) { sb.Append("No camera."); return sb.ToString(); }
            Vector3 eye = cam.transform.position;
            float game = QualitySettings.shadowDistance;

            Dictionary<int, LayerCount> counts = new Dictionary<int, LayerCount>();
            int total = 0, trimmed = 0;
            foreach (Renderer r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
                if (r.shadowCastingMode == ShadowCastingMode.Off) continue;
                float dist = Vector3.Distance(eye, r.bounds.center);
                if (dist > game) continue;
                int layer = r.gameObject.layer;
                LayerCount c;
                if (!counts.TryGetValue(layer, out c)) { c = new LayerCount(); counts[layer] = c; }
                c.Casting++;
                total++;
                if (Active && dist > EffectiveDistance(layer)) { c.Trimmed++; trimmed++; }
            }

            sb.Append("Shadow casters within ").Append(game.ToString("0")).Append(" m: ").Append(total);
            if (Active) sb.Append(", of them past their layer's distance (no shadow): ").Append(trimmed);
            sb.Append('\n');
            List<KeyValuePair<int, LayerCount>> list = new List<KeyValuePair<int, LayerCount>>(counts);
            list.Sort(delegate(KeyValuePair<int, LayerCount> a, KeyValuePair<int, LayerCount> b) { return b.Value.Casting.CompareTo(a.Value.Casting); });
            foreach (KeyValuePair<int, LayerCount> kv in list)
            {
                string name = LayerMask.LayerToName(kv.Key);
                if (string.IsNullOrEmpty(name)) name = "layer " + kv.Key;
                sb.Append("  ").Append(name).Append(": ").Append(kv.Value.Casting);
                float eff = EffectiveDistance(kv.Key);
                if (Active && eff < game) sb.Append(", shadow to ").Append(eff.ToString("0")).Append(" m, ").Append(kv.Value.Trimmed).Append(" past it");
                sb.Append('\n');
            }

            int fp, fr;
            FlatCounts(out fp, out fr);
            sb.Append("Flat on the ground: ").Append(_flatOn ? fp + " pieces (" + fr + " renderers) cast no shadow" : "off");
            if (_flatOn && (_queue.Count - _queueCursor) + _later.Count > 0)
                sb.Append(", ").Append((_queue.Count - _queueCursor) + _later.Count).Append(" still to look at");
            return sb.ToString();
        }
    }
}
