# ShadowTrim

**English** · [Русский](README-RU.md)

A Valheim mod: fewer sun shadows where nobody can see them, for more frames in big bases.
Client-side only: other players and the server need nothing.

Version history is in `CHANGELOG.md`.

## How it works

In a built-up base the sun's shadow map is drawn from thousands of objects every frame —
10,000–15,000 shadow casters at a 7,000-piece base, against about 1,500 in the wild — and with
the logic costs fixed (see Valheim Community Patch) it is the largest cost of the frame left
there. ShadowTrim cuts it in two ways.

**Shadow distance by layer.** Unity can stop an object casting the sun's shadow beyond a
distance of its own layer (`Light.layerShadowCullDistances` on the game's sun), inside the
engine's own culling — no per-object work at all. Defaults, against the game's 120 m:

| Layer | What | Shadow up to |
|---|---|---|
| `piece` | building pieces: walls, floors, roofs, beams, stations | 60 m |
| `piece_nonsolid` | pieces you walk through: banners, rugs, item stands | 40 m |
| `Default_small` | bushes, pickables, small stones and branches | 30 m |
| `item` | dropped items, floating logs | 25 m |

Terrain, trees, rocks and creatures keep the game's distance. The object itself is still drawn
and still darkened by other shadows; only its own shadow ends earlier.

**Flat pieces lying on the ground.** A floor, a path stone or a rug on the ground casts its
shadow into the ground right under itself, where it can never be seen — but it still takes its
place in the shadow map. Each piece is looked at once, shortly after it loads, within 1 ms per
frame: if everything drawn of it lies between the lowest ground under it (less `MaxGapBelow`)
and the highest (plus `MaxHeightAbove`), its renderers stop casting. A floor on posts, over a
dip or on water is left alone. Turning the feature or the mod off gives every renderer its mode
back at once.

**Measured** (`shadowtrim bench`, a 7,000-piece base, i5-12400F / RTX 5070, 3440×1440):

| Where | The game's shadows | ShadowTrim |
|---|---|---|
| outside, facing the base | 16.53 ms, 10,083 casters | 15.53 ms (−1.00 ms, 6.1%), 5,116 casters |
| inside a house | 15.21 ms, 15,079 casters | 14.63 ms (−0.58 ms, 3.8%), 12,926 casters |

## Console

| Command | What |
|---|---|
| `shadowtrim` | on / off, to compare on the spot (`shadowtrim on`, `shadowtrim off`) |
| `shadowtrim status` | the shadow casters around you by layer, how many are past their layer's distance, and how many flat pieces cast nothing |
| `shadowtrim bench` | 40 s: switches itself on and off every 5 s where you stand and prints the frame time and the engine's shadow caster count of both |

## Compatibility

Built against **Valheim 1.0.20** (network version 41), **BepInEx 5.4.23.5**
(BepInExPack_Valheim 5.4.2351). Works alongside Valheim Community Patch. Another mod that sets
the sun's per-layer shadow distances would overwrite the same values.

## Who needs it

| Who | What |
|---|---|
| A player who wants more frames | installs it |
| Other players, the host, a dedicated server | nothing |

## Bugs and feedback

https://github.com/tbsj1ga/ShadowTrimValheim/issues

## Installation

Through r2modman / Thunderstore, or put `build/ShadowTrim.dll` into

```
%AppData%\r2modmanPlus-local\Valheim\profiles\Valheim\BepInEx\plugins\ShadowTrim\
```

(or `build.ps1 -Install`).

## Settings

`BepInEx/config/j1ga.shadowtrim.cfg`, or in-game with ConfigurationManager (F1). Changes apply at
once. Distances are metres from the camera; 0 is the game's shadow distance.

| Section | Key | Default | What |
|---|---|---|---|
| 01 General | `Enabled` | `true` | off: every shadow as in the game |
| 02 Shadow distance by layer | `Buildings` | `60` | layer `piece` |
| 02 Shadow distance by layer | `Decor` | `40` | layer `piece_nonsolid` |
| 02 Shadow distance by layer | `SmallObjects` | `30` | layer `Default_small` |
| 02 Shadow distance by layer | `Items` | `25` | layer `item` |
| 02 Shadow distance by layer | `OtherLayers` | empty | any other layer as `layer=metres`, e.g. `static_solid=80,character=50` |
| 03 Flat on the ground | `Enabled` | `true` | flat pieces on the ground cast no shadow |
| 03 Flat on the ground | `MaxGapBelow` | `0.3` | how far above the lowest ground under it the underside may be, m |
| 03 Flat on the ground | `MaxHeightAbove` | `0.6` | how far above the highest ground under it the top may reach, m |

## Building

```
powershell -ExecutionPolicy Bypass -File .\build.ps1            # build and check references
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Install   # ... and copy into plugins
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Package   # ... and make the Thunderstore zip
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Nexus     # ... zip, and the Nexus Mods kit
```

The compiler is `csc.exe` from the .NET Framework (C# 5); references come straight from the
game folder and the r2modman profile's `BepInEx\core`. After the build, `check-refs.ps1` checks
every reference and Harmony patch target against the game.

## Repository

Branch `main` on GitHub: https://github.com/tbsj1ga/ShadowTrimValheim.

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

## AI assistance

This mod was developed with the help of an AI assistant (Claude by Anthropic); the design
decisions, in-game testing and releases are the author's.
