# ShadowTrim

[English](README.md) · **Русский**

Мод для Valheim: меньше теней от солнца там, где их никто не видит, — больше кадров на больших
базах. Только клиент: другим игрокам и серверу ничего не нужно.

История версий — в `CHANGELOG-RU.md`.

## Как это работает

На застроенной базе карта теней от солнца каждый кадр рисуется из тысяч объектов —
10–15 тысяч отбрасывающих тень на базе в 7000 деталей против примерно 1500 в диком лесу, — и
когда расходы на логику убраны (см. Valheim Community Patch), это самая большая часть кадра,
что там остаётся. ShadowTrim сокращает её двумя способами.

**Дальность тени по слоям.** Unity умеет обрывать тень объекта от солнца на дальности его слоя
(`Light.layerShadowCullDistances` у солнца игры) внутри собственного отсечения движка — без
какой-либо работы на каждый объект. По умолчанию, при 120 м в игре:

| Слой | Что | Тень до |
|---|---|---|
| `piece` | детали построек: стены, полы, крыши, балки, станции | 60 м |
| `piece_nonsolid` | детали, сквозь которые можно пройти: баннеры, ковры, подставки | 40 м |
| `Default_small` | кусты, собираемое, мелкие камни и ветки | 30 м |
| `item` | выпавшие предметы, плавающие брёвна | 25 м |

Земля, деревья, скалы и существа сохраняют дальность игры. Сам объект по-прежнему рисуется и
по-прежнему затеняется чужими тенями; раньше кончается только его собственная тень.

**Плоские детали на земле.** Пол, плита дорожки или ковёр на земле отбрасывают тень в землю
прямо под собой, где её никогда не видно, — но место в карте теней она занимает. Каждая деталь
осматривается один раз, вскоре после загрузки, не больше 1 мс за кадр: если всё, что от неё
рисуется, лежит между самой низкой точкой земли под ней (минус `MaxGapBelow`) и самой высокой
(плюс `MaxHeightAbove`), её рендереры перестают отбрасывать тень. Пол на сваях, над ямой или
над водой не трогается. Выключение функции или мода сразу возвращает каждому рендереру его
режим.

**Замер** (`shadowtrim bench`, база в 7000 деталей, i5-12400F / RTX 5070, 3440×1440):

| Где | Тени игры | ShadowTrim |
|---|---|---|
| снаружи, лицом к базе | 16,53 мс, 10 083 тени | 15,53 мс (−1,00 мс, 6,1%), 5116 теней |
| в доме | 15,21 мс, 15 079 теней | 14,63 мс (−0,58 мс, 3,8%), 12 926 теней |

## Консоль

| Команда | Что |
|---|---|
| `shadowtrim` | вкл / выкл, чтобы сравнить на месте (`shadowtrim on`, `shadowtrim off`) |
| `shadowtrim status` | объекты с тенью вокруг по слоям, сколько из них дальше дальности своего слоя и сколько плоских деталей без тени |
| `shadowtrim bench` | 40 с: на месте сам переключается каждые 5 с и выводит время кадра и число теней по счётчику движка в обоих состояниях |

## Совместимость

Собран под **Valheim 1.0.20** (сетевая версия 41), **BepInEx 5.4.23.5**
(BepInExPack_Valheim 5.4.2351). Работает вместе с Valheim Community Patch. Другой мод, который
задаёт солнцу дальности теней по слоям, перезапишет те же значения.

## Кому что нужно

| Кто | Что |
|---|---|
| Игрок, которому нужно больше кадров | ставит мод |
| Другие игроки, хост, выделенный сервер | ничего |

## Ошибки и пожелания

https://github.com/tbsj1ga/ShadowTrimValheim/issues

## Установка

Через r2modman / Thunderstore или положите `build/ShadowTrim.dll` в

```
%AppData%\r2modmanPlus-local\Valheim\profiles\Valheim\BepInEx\plugins\ShadowTrim\
```

(или `build.ps1 -Install`).

## Настройки

`BepInEx/config/j1ga.shadowtrim.cfg` или в игре через ConfigurationManager (F1). Изменения
применяются сразу. Дальности — в метрах от камеры; 0 — дальность теней игры.

| Раздел | Ключ | По умолчанию | Что |
|---|---|---|---|
| 01 General | `Enabled` | `true` | выкл: все тени как в игре |
| 01 General | `Preset` | `Normal` | `Normal` (значения ниже), `Low` для слабых ПК (постройки 35 м, декор 25, мелкие объекты 20, предметы 15, `static_solid=60,Default=60`), `Custom` — ставится сам, когда меняете дальность |
| 02 Shadow distance by layer | `Buildings` | `60` | слой `piece` |
| 02 Shadow distance by layer | `Decor` | `40` | слой `piece_nonsolid` |
| 02 Shadow distance by layer | `SmallObjects` | `30` | слой `Default_small` |
| 02 Shadow distance by layer | `Items` | `25` | слой `item` |
| 02 Shadow distance by layer | `OtherLayers` | пусто | любой другой слой как `слой=метры`, напр. `static_solid=80,character=50` |
| 03 Flat on the ground | `Enabled` | `true` | плоские детали на земле без тени |
| 03 Flat on the ground | `MaxGapBelow` | `0.3` | насколько низ детали может быть выше самой низкой земли под ней, м |
| 03 Flat on the ground | `MaxHeightAbove` | `0.6` | насколько верх детали может быть выше самой высокой земли под ней, м |

## Сборка

```
powershell -ExecutionPolicy Bypass -File .\build.ps1            # собрать и проверить ссылки
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Install   # ... и скопировать в plugins
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Package   # ... и собрать архив для Thunderstore
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Nexus     # ... архив и набор для Nexus Mods
```

Компилятор — `csc.exe` из .NET Framework (C# 5); ссылки берутся прямо из папки игры и из
`BepInEx\core` профиля r2modman. После сборки `check-refs.ps1` сверяет каждую ссылку и цель
патча Harmony с игрой.

## Репозиторий

Ветка `main` на GitHub: https://github.com/tbsj1ga/ShadowTrimValheim.

## Другие моды j1gA

| | Мод |
|---|---|
| [![LivingMap](https://raw.githubusercontent.com/tbsj1ga/LivingMapValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/LivingMap/) | **[LivingMap](https://thunderstore.io/c/valheim/p/j1gA/LivingMap/)** — ваши постройки, дороги и вырубки на карте и миникарте, и подробная карта при приближении. |
| [![StationSpeed](https://raw.githubusercontent.com/tbsj1ga/StationSpeedValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/StationSpeed/) | **[StationSpeed](https://thunderstore.io/c/valheim/p/j1gA/StationSpeed/)** — быстрее плавильни, печи, ферментеры и посадки, одинаково и для игроков без мода. |
| [![WeaponArts](https://raw.githubusercontent.com/tbsj1ga/WeaponArtsValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/WeaponArts/) | **[WeaponArts](https://thunderstore.io/c/valheim/p/j1gA/WeaponArts/)** — одна клавиша, одна активная способность на оружие: оглушение, агро, лечение, берсерк, криты. |
| [![ExtendedBosses](https://raw.githubusercontent.com/tbsj1ga/ExtendedBossesValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/ExtendedBosses/) | **[ExtendedBosses](https://thunderstore.io/c/valheim/p/j1gA/ExtendedBosses/)** — рейдовые бои с боссами: фазы, прислужники, гнёзда, щиты, метки — из ванильных частей. |
| [![HostOwner](https://raw.githubusercontent.com/tbsj1ga/HostOwnerValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/HostOwner/) | **[HostOwner](https://thunderstore.io/c/valheim/p/j1gA/HostOwner/)** — хост забирает себе станции и боссов рядом, чтобы его моды работали для всех. |
| [![HudLayout](https://raw.githubusercontent.com/tbsj1ga/HudLayoutValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/HudLayout/) | **[HudLayout](https://thunderstore.io/c/valheim/p/j1gA/HudLayout/)** — двигайте, масштабируйте и перекрашивайте HUD мышью: полосы, еда, хотбар, миникарта, даже HUD других модов. |
| [![HealthPulse](https://raw.githubusercontent.com/tbsj1ga/HealthPulseValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/HealthPulse/) | **[HealthPulse](https://thunderstore.io/c/valheim/p/j1gA/HealthPulse/)** — при низком здоровье края экрана пульсируют красным, как сердце. |
| [![ChestReach](https://raw.githubusercontent.com/tbsj1ga/ChestReachValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/ChestReach/) | **[ChestReach](https://thunderstore.io/c/valheim/p/j1gA/ChestReach/)** — крафт, стройка, заправка и готовка из сундуков вокруг — без тормозов, игроки без мода могут зайти. |
| [![SwimGear](https://raw.githubusercontent.com/tbsj1ga/SwimGearValheim/main/docs/media/icon-128.png)](https://thunderstore.io/c/valheim/p/j1gA/SwimGear/) | **[SwimGear](https://thunderstore.io/c/valheim/p/j1gA/SwimGear/)** — оружие и инструменты остаются в руках, пока плывёшь, и FPS не проседает. |

## Помощь ИИ

Мод разработан с помощью ИИ-ассистента (Claude от Anthropic); решения по устройству,
проверка в игре и выпуски — за автором.
