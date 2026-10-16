# Changelog

**English** · [Русский](CHANGELOG-RU.md)

The version is set in one place — `ShadowTrimPlugin.Version` in `src/ShadowTrimPlugin.cs`.

## 0.1.0

- First release. Shadow distance by layer on the game's sun: building pieces 60 m, pieces you
  walk through 40 m, small objects 30 m, dropped items 25 m (the game: 120 m); any other layer
  through `OtherLayers`.
- Flat pieces lying on the ground (floors, path stones, rugs) cast no shadow; each piece is
  looked at once after it loads, within 1 ms per frame.
- Console: `shadowtrim` (on / off), `shadowtrim status`, `shadowtrim bench`.
