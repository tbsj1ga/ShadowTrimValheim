# ShadowTrim

**More frames in big bases, without a visible change.** In a built-up base the sun's shadows
are drawn from 10,000–15,000 objects every frame. ShadowTrim stops the ones nobody can see:

- **Shadow distance by layer.** Building pieces cast the sun's shadow up to 60 m instead of
  120 m, small decor 40 m, bushes and small objects 30 m, dropped items 25 m — done inside the
  engine's own culling, no per-object work. The objects are still drawn; only their own shadow
  ends earlier. Terrain, trees, rocks and creatures keep the game's distance.
- **Flat pieces lying on the ground** — floors, path stones, rugs — stop casting a shadow: it
  falls into the ground right under them and can never be seen. Floors on posts, over a dip or
  on water are left alone.

**Measured** at a 7,000-piece base (i5-12400F / RTX 5070): outside, facing the base,
**−1.00 ms per frame (6.1%)** with half the shadow casters; inside a house −0.58 ms (3.8%).
Measure your own: stand still and type `shadowtrim bench` in the console (F5).

- `shadowtrim` — on / off, to compare on the spot. `shadowtrim status` — the shadows around
  you by layer.
- A `Low` preset for weak PCs; every distance is configurable, for any layer.
- **Client-side.** Install it if you want it; other players, the host and the server need
  nothing.
- Pairs well with Valheim Community Patch, which fixes the logic costs of big bases; ShadowTrim
  takes the rendering side.

## Settings

`BepInEx/config/j1ga.shadowtrim.cfg`, or in-game with ConfigurationManager (F1):

- `Enabled` — on / off.
- `Preset` — `Normal`, `Low` for weak PCs (shorter distances, rocks and other objects too), or
  `Custom`, which it becomes by itself when you change a distance.
- `Buildings` (60), `Decor` (40), `SmallObjects` (30), `Items` (25) — shadow distance in metres;
  0 = the game's own.
- `OtherLayers` — any other layer as `layer=metres`, e.g. `static_solid=80`.
- `Flat on the ground`: `Enabled`, `MaxGapBelow` (0.3 m), `MaxHeightAbove` (0.6 m).

## Compatibility

Built against Valheim 1.0.20, BepInEx 5.4.23.5 (BepInExPack_Valheim 5.4.2351).

## Bugs and feedback

[GitHub issues](https://github.com/tbsj1ga/ShadowTrimValheim/issues) - please include
`BepInEx/LogOutput.log` and the output of `shadowtrim status`.

## More mods by j1gA

| | Mod |
|---|---|
| [![LivingMap](https://raw.githubusercontent.com/tbsj1ga/LivingMapValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/LivingMap/) | **[LivingMap](https://thunderstore.io/c/valheim/p/j1gA/LivingMap/)** — Your buildings, roads and cleared forest on the map and the minimap — and a detailed map when you zoom in. |
| [![StationSpeed](https://raw.githubusercontent.com/tbsj1ga/StationSpeedValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/StationSpeed/) | **[StationSpeed](https://thunderstore.io/c/valheim/p/j1gA/StationSpeed/)** — Faster smelters, kilns, fermenters and crops — consistent even for players without the mod. |
| [![WeaponArts](https://raw.githubusercontent.com/tbsj1ga/WeaponArtsValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/WeaponArts/) | **[WeaponArts](https://thunderstore.io/c/valheim/p/j1gA/WeaponArts/)** — One key, one active ability per weapon: stagger, taunt, heals, berserk, crits. |
| [![ExtendedBosses](https://raw.githubusercontent.com/tbsj1ga/ExtendedBossesValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/ExtendedBosses/) | **[ExtendedBosses](https://thunderstore.io/c/valheim/p/j1gA/ExtendedBosses/)** — Raid-style boss fights: phases, adds, nests, shields, marks — built from vanilla parts. |
| [![HostOwner](https://raw.githubusercontent.com/tbsj1ga/HostOwnerValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/HostOwner/) | **[HostOwner](https://thunderstore.io/c/valheim/p/j1gA/HostOwner/)** — The host takes ownership of stations and bosses near it, so its mods work for everyone. |
| [![HudLayout](https://raw.githubusercontent.com/tbsj1ga/HudLayoutValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/HudLayout/) | **[HudLayout](https://thunderstore.io/c/valheim/p/j1gA/HudLayout/)** — Move, resize and restyle your HUD with the mouse: bars, food, hotbar, minimap, even other mods' HUD. |
| [![HealthPulse](https://raw.githubusercontent.com/tbsj1ga/HealthPulseValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/HealthPulse/) | **[HealthPulse](https://thunderstore.io/c/valheim/p/j1gA/HealthPulse/)** — Low health warning: the screen edges pulse red like a heartbeat, faster as your health drops. |
| [![ChestReach](https://raw.githubusercontent.com/tbsj1ga/ChestReachValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/ChestReach/) | **[ChestReach](https://thunderstore.io/c/valheim/p/j1gA/ChestReach/)** — Craft, build, refuel and cook from the chests around you — no lag, players without the mod can join. |
| [![SwimGear](https://raw.githubusercontent.com/tbsj1ga/SwimGearValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/SwimGear/) | **[SwimGear](https://thunderstore.io/c/valheim/p/j1gA/SwimGear/)** — Keep your weapons and tools in your hands while swimming — no FPS cost. |

Source, full documentation and the changelog: https://github.com/tbsj1ga/ShadowTrimValheim

*Developed with the help of an AI assistant (Claude by Anthropic); the design decisions,
verification against the game code and in-game testing are the author's.*
