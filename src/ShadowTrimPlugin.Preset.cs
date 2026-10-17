using System;
using BepInEx.Configuration;

namespace ShadowTrim
{
    // Presets: Normal (the defaults) and Low (for weak PCs) write their distances into the
    // four layer settings and OtherLayers; editing any of those by hand turns the preset into
    // Custom, so a hand-made value is never overwritten. A preset whose values no longer match
    // at startup (the file was edited with the game closed) is read as Custom too.
    public partial class ShadowTrimPlugin
    {
        private const string PresetNormal = "Normal", PresetLow = "Low", PresetCustom = "Custom";

        private sealed class PresetValues
        {
            public float Buildings, Decor, Small, Items;
            public string Other;
        }

        private static readonly PresetValues s_normal = new PresetValues { Buildings = 60f, Decor = 40f, Small = 30f, Items = 25f, Other = "" };
        private static readonly PresetValues s_low = new PresetValues { Buildings = 35f, Decor = 25f, Small = 20f, Items = 15f, Other = "static_solid=60,Default=60" };

        private ConfigEntry<string> _cfgPreset;
        private bool _applyingPreset;

        private void BindPreset()
        {
            _cfgPreset = Config.Bind(SecGeneral, "Preset", PresetNormal,
                new ConfigDescription(
                    "Normal: the defaults (buildings 60 m, decor 40, small objects 30, items 25). " +
                    "Low: for weak PCs (buildings 35 m, decor 25, small objects 20, items 15, rocks and other objects 60) - " +
                    "shadows appear closer, there are fewer of them. Custom: the distances below as you set them; " +
                    "changing any of them switches to Custom by itself.",
                    new AcceptableValueList<string>(PresetNormal, PresetLow, PresetCustom)));

            PresetValues p = PresetFor(_cfgPreset.Value);
            if (p != null && !Matches(p)) _cfgPreset.Value = PresetCustom;

            _cfgPreset.SettingChanged += delegate { ApplyPreset(); };
            EventHandler toCustom = delegate
            {
                if (_applyingPreset) return;
                PresetValues cur = PresetFor(_cfgPreset.Value);
                if (cur != null && !Matches(cur)) _cfgPreset.Value = PresetCustom;
            };
            _cfgBuildings.SettingChanged += toCustom;
            _cfgDecor.SettingChanged += toCustom;
            _cfgSmall.SettingChanged += toCustom;
            _cfgItems.SettingChanged += toCustom;
            _cfgOther.SettingChanged += toCustom;
        }

        private static PresetValues PresetFor(string name)
        {
            if (string.Equals(name, PresetNormal, StringComparison.OrdinalIgnoreCase)) return s_normal;
            if (string.Equals(name, PresetLow, StringComparison.OrdinalIgnoreCase)) return s_low;
            return null;
        }

        private bool Matches(PresetValues p)
        {
            return _cfgBuildings.Value == p.Buildings && _cfgDecor.Value == p.Decor && _cfgSmall.Value == p.Small &&
                   _cfgItems.Value == p.Items && string.Equals((_cfgOther.Value ?? "").Replace(" ", ""), p.Other, StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyPreset()
        {
            PresetValues p = PresetFor(_cfgPreset.Value);
            if (p == null || Matches(p)) return;
            _applyingPreset = true;
            try
            {
                _cfgBuildings.Value = p.Buildings;
                _cfgDecor.Value = p.Decor;
                _cfgSmall.Value = p.Small;
                _cfgItems.Value = p.Items;
                _cfgOther.Value = p.Other;
            }
            finally { _applyingPreset = false; }
            _layersDirty = true;
        }
    }
}
