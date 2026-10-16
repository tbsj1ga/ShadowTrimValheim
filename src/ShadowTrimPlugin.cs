using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace ShadowTrim
{
    // Fewer shadows where nobody sees them, for more frames in big bases.
    //
    // In a built-up base the game draws the sun's shadow map from thousands of objects every
    // frame - 6 000 to 12 000 shadow casters at a 7 000-piece base, against 1 500 in the wild -
    // and it is the largest single cost of the frame that is left there. Two things cut it:
    //
    //   * Distance by layer. Unity can stop an object casting the sun's shadow beyond a
    //     distance of its own, per layer (Light.layerShadowCullDistances), inside the engine's
    //     own culling: no per-object work at all. Buildings, small decor, small objects and
    //     dropped items get shorter distances than the game's shadow distance; terrain, trees,
    //     rocks and creatures keep it. The object itself is still drawn, and still receives
    //     shadows - only its own shadow ends earlier.
    //   * Flat pieces lying on the ground. A floor, a path stone or a rug on the ground casts
    //     its shadow into the ground right under itself, where it can never be seen; it still
    //     costs a place in the shadow map. Such pieces are found once, when they load, and
    //     stop casting.
    [BepInPlugin(Guid, Name, Version)]
    public partial class ShadowTrimPlugin : BaseUnityPlugin
    {
        public const string Guid = "j1ga.shadowtrim";
        public const string Name = "ShadowTrim";
        public const string Version = "0.1.0";

        internal static ShadowTrimPlugin Instance;

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            BindConfig();
            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(ShadowTrimPlugin).Assembly);
            RegisterCommands();
            Logger.LogInfo(Name + " " + Version + " loaded.");
        }

        private void OnDestroy()
        {
            try
            {
                RestoreLayers();
                RestoreFlat();
                if (_harmony != null) _harmony.UnpatchSelf();
            }
            catch (Exception e) { Logger.LogWarning("OnDestroy: " + e.Message); }
        }

        private void Update()
        {
            try { UpdateLayers(); }
            catch (Exception e) { Fail("layers", e); }
            try { UpdateFlat(); }
            catch (Exception e) { Fail("flat pieces", e); }
        }

        private readonly HashSet<string> _failed = new HashSet<string>();

        private void Fail(string where, Exception e)
        {
            if (_failed.Add(where)) Logger.LogError(where + ": " + e);
        }

        internal void Log(string s) { Logger.LogInfo(s); }
        internal void Warn(string s) { Logger.LogWarning(s); }
    }
}
