# Valheim Floating Items

BepInEx mod for [Valheim](https://www.valheimgame.com/) that makes dropped
items float on water instead of sinking to the bottom - lose your ore or your
sword over the side of the boat and you can still fish it out.

## What it does

- Every dropped item floats on water (and on tar), bobbing on the surface the
  same way wood does in the vanilla game.
- Items that already float in the game keep their own behaviour, and fish are
  left alone - they still swim away when released.
- Works for items added by other mods too.

## Built on the game's own buoyancy

The mod has no physics of its own. Valheim already has a buoyancy component
(`Floating`) - it is what keeps wood on the surface. The mod adds that
component to every item as it appears in the world and copies its settings
(float height, force, damping, splash effect) from Wood, so everything floats
exactly like wood does.

Items that are already lying on the bottom come up after the world is loaded
again, because the game creates them anew.

## Configuration

`BepInEx\config\com.michal.valheim.floatingitems.cfg`:

| Setting | Default | Description |
|---|---|---|
| `ExcludedItems` | *(empty)* | Comma-separated prefab names of items that should keep sinking, e.g. `Coins, IronScrap`. Applies to items dropped after the change. |

## Requirements

- Valheim (tested on 1.0.15)
- [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) 5.4.x

## Installation (players)

1. Install BepInEx for Valheim if you haven't already (see link above, or
   use [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/)).
2. Download `FloatingItems.dll` from the
   [latest release](../../releases/latest).
3. Drop it into `<Valheim install folder>\BepInEx\plugins\FloatingItems\`.
4. Launch the game and throw something into the sea.

In multiplayer, buoyancy is applied by whoever owns the item (usually the
player closest to it), so everyone who wants floating items should have the
mod installed.

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer)
and a local Valheim install with BepInEx installed.

```bash
git clone https://github.com/Ab5oluteZer0/valheim-floating-items.git
cd valheim-floating-items
dotnet build -c Release -p:ValheimPath="C:\Path\To\Valheim"
```

If you don't pass `-p:ValheimPath`, the build looks for a `VALHEIM_PATH`
environment variable, then falls back to the default Steam location
(`C:\Program Files (x86)\Steam\steamapps\common\Valheim`).

The build automatically copies the built DLL into
`<Valheim>\BepInEx\plugins\FloatingItems\` for quick in-game testing.

## Notes on how it works (and a few gotchas found along the way)

- The component is added in a prefix to `ItemDrop.Awake`: the game caches it
  there (it is also used to tell whether an item is stuck in tar), so it has
  to exist before `Awake` runs. Patching instances instead of prefabs also
  covers items registered by other mods at any time.
- Water finds the object to push through its rigidbody and takes a single
  `IWaterInteractable` from it. Objects that already have one (floating items,
  fish) are skipped - adding a second would break them.
- The template's surface ripple (`m_surfaceEffects`) is not copied: it is a
  child object of the Wood prefab, shared by all copies, and switching it on
  and off from many items would toggle the template itself. The splash effect
  is just a list of prefabs to spawn, so it is shared safely.

## License

MIT - see [LICENSE](LICENSE).
