using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace ShadowTrim
{
    public partial class ShadowTrimPlugin
    {
        private ConfigEntry<bool> _cfgEnabled;

        private ConfigEntry<float> _cfgBuildings, _cfgDecor, _cfgSmall, _cfgItems;
        private ConfigEntry<string> _cfgOther;

        private ConfigEntry<bool> _cfgFlat;
        private ConfigEntry<float> _cfgFlatGap, _cfgFlatTop;

        private const string SecGeneral = "01 General";
        private const string SecDistance = "02 Shadow distance by layer";
        private const string SecFlat = "03 Flat on the ground";

        private void BindConfig()
        {
            _cfgEnabled = Config.Bind(SecGeneral, "Enabled", true,
                "Off: every shadow as in the game. The console command 'shadowtrim' switches it too, to compare on the spot.");

            const string d = " Metres from the camera; 0 = the game's own shadow distance (Graphics settings). " +
                             "The object is still drawn and still darkened by other shadows - only its own shadow ends there.";
            _cfgBuildings = Config.Bind(SecDistance, "Buildings", 60f,
                "Building pieces (layer 'piece'): walls, floors, roofs, beams, stations." + d);
            _cfgDecor = Config.Bind(SecDistance, "Decor", 40f,
                "Pieces you can walk through (layer 'piece_nonsolid'): banners, rugs, item stands, small decor." + d);
            _cfgSmall = Config.Bind(SecDistance, "SmallObjects", 30f,
                "Small world objects (layer 'Default_small'): bushes, pickables, small stones and branches." + d);
            _cfgItems = Config.Bind(SecDistance, "Items", 25f,
                "Dropped items and floating logs (layer 'item')." + d);
            _cfgOther = Config.Bind(SecDistance, "OtherLayers", "",
                "Any other layer, as layer=metres separated by commas, e.g. static_solid=80,character=50. " +
                "'shadowtrim status' lists the layers that cast shadows around you and how many objects each has.");

            _cfgFlat = Config.Bind(SecFlat, "Enabled", true,
                "Pieces lying flat on the ground (floors, path stones, rugs) stop casting a shadow: it falls into the " +
                "ground right under them and cannot be seen anyway.");
            _cfgFlatGap = Config.Bind(SecFlat, "MaxGapBelow", 0.3f,
                "How far above the lowest ground under it a piece's underside may be and still count as lying on the " +
                "ground, in metres. A floor over a dip or on posts is left alone.");
            _cfgFlatTop = Config.Bind(SecFlat, "MaxHeightAbove", 0.6f,
                "How far above the highest ground under it a piece's top may reach, in metres. Taller pieces cast a " +
                "visible shadow beside themselves and are left alone.");

            _cfgEnabled.SettingChanged += delegate { _layersDirty = true; _flatDirty = true; };
            _cfgBuildings.SettingChanged += delegate { _layersDirty = true; };
            _cfgDecor.SettingChanged += delegate { _layersDirty = true; };
            _cfgSmall.SettingChanged += delegate { _layersDirty = true; };
            _cfgItems.SettingChanged += delegate { _layersDirty = true; };
            _cfgOther.SettingChanged += delegate { _layersDirty = true; };
            _cfgFlat.SettingChanged += delegate { _flatDirty = true; };
            _cfgFlatGap.SettingChanged += delegate { _flatDirty = true; };
            _cfgFlatTop.SettingChanged += delegate { _flatDirty = true; };
            BindPreset();
        }

        // layer name -> metres, from the four named settings and OtherLayers
        private Dictionary<string, float> LayerSettings()
        {
            Dictionary<string, float> r = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
            r["piece"] = _cfgBuildings.Value;
            r["piece_nonsolid"] = _cfgDecor.Value;
            r["Default_small"] = _cfgSmall.Value;
            r["item"] = _cfgItems.Value;
            foreach (string raw in (_cfgOther.Value ?? "").Split(',', ';'))
            {
                string s = raw.Trim();
                if (s.Length == 0) continue;
                int eq = s.IndexOf('=');
                float v;
                if (eq <= 0 || !float.TryParse(s.Substring(eq + 1).Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out v))
                {
                    WarnOnce("other:" + s, "OtherLayers: '" + s + "' is not layer=metres; ignored.");
                    continue;
                }
                r[s.Substring(0, eq).Trim()] = v;
            }
            return r;
        }

        private readonly HashSet<string> _warned = new HashSet<string>();

        private void WarnOnce(string key, string text)
        {
            if (_warned.Add(key)) Warn(text);
        }
    }
}
