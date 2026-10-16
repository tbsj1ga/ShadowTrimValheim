using System;
using System.Collections;
using Unity.Profiling;
using UnityEngine;

namespace ShadowTrim
{
    // 'shadowtrim bench': the honest measurement. A profiler minute mixes where the camera
    // looks, the weather and what loads; here the player stands still and the mod switches
    // itself on and off every few seconds, the frame time and the engine's own count of shadow
    // casters measured in each state. Switching is instant: the flat pieces already found are
    // flipped back and forth, not looked at again.
    public partial class ShadowTrimPlugin
    {
        private bool _benching;

        private const float BenchPhase = 5f;     // seconds per state
        private const float BenchSettle = 1f;    // not measured after each switch
        private const int BenchPhases = 8;

        private sealed class BenchSide
        {
            public double Frames, Seconds, Casters, CasterFrames;
        }

        private void StartBench(Terminal.ConsoleEventArgs args)
        {
            if (_benching) { Say(args, "A benchmark is already running."); return; }
            if (Sun == null) { Say(args, "No sun here (indoors or in a dungeon); nothing to measure."); return; }
            Terminal term = args != null ? args.Context : null;
            Say(args, "ShadowTrim benchmark: " + (BenchPhases * BenchPhase).ToString("0") + " s, switching every " +
                      BenchPhase.ToString("0") + " s. Stand still and keep the camera where it is.");
            StartCoroutine(Bench(term));
        }

        private IEnumerator Bench(Terminal term)
        {
            _benching = true;
            BenchSide on = new BenchSide(), off = new BenchSide();
            ProfilerRecorder casters = new ProfilerRecorder();
            try { casters = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count"); }
            catch (Exception) { }
            float[] trimmed = Distances();
            try
            {
                for (int phase = 0; phase < BenchPhases; phase++)
                {
                    bool trim = phase % 2 == 1;            // starts with the game's own shadows
                    ApplyBench(trim, trimmed);
                    BenchSide side = trim ? on : off;
                    float t0 = Time.unscaledTime;
                    while (Time.unscaledTime - t0 < BenchPhase)
                    {
                        yield return null;
                        if (Time.unscaledTime - t0 < BenchSettle) continue;
                        side.Frames++;
                        side.Seconds += Time.unscaledDeltaTime;
                        if (casters.Valid) { side.Casters += casters.LastValue; side.CasterFrames++; }
                    }
                }
            }
            finally
            {
                if (casters.Valid) casters.Dispose();
                // back to what the settings say
                FlipFlat(_flatOn);
                _layersDirty = true;
                _sun = null;
                _benching = false;
            }

            string r = "ShadowTrim benchmark:\n" + BenchLine("game's shadows", off) + "\n" + BenchLine("ShadowTrim", on);
            if (off.Frames > 0 && on.Frames > 0)
            {
                double a = off.Seconds / off.Frames * 1000.0, b = on.Seconds / on.Frames * 1000.0;
                r += "\nFrame time " + (a - b >= 0 ? "-" : "+") + Math.Abs(a - b).ToString("0.00") + " ms (" +
                     ((a - b) / a * 100.0).ToString("0.0") + "% faster)";
            }
            Log(r.Replace("\n", " | "));
            if (term != null) term.AddString(r);
        }

        private static string BenchLine(string name, BenchSide s)
        {
            if (s.Frames <= 0) return "  " + name + ": no frames";
            double ms = s.Seconds / s.Frames * 1000.0;
            string line = "  " + name + ": " + ms.ToString("0.00") + " ms (" + (1000.0 / ms).ToString("0") + " fps)";
            if (s.CasterFrames > 0) line += ", " + (s.Casters / s.CasterFrames).ToString("0") + " shadow casters";
            return line;
        }

        private void ApplyBench(bool trim, float[] trimmed)
        {
            Light sun = Sun;
            if (sun != null)
            {
                if (trim && trimmed != null) sun.layerShadowCullDistances = trimmed;
                else Clear(sun);
            }
            _sun = sun;
            _applied = trim ? trimmed : null;
            FlipFlat(trim);
        }
    }
}
