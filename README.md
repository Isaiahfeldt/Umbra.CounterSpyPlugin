# Counter Spy plugin for Umbra

Adds world markers to other players that are currently targeting you.

## How to Install

1. Open Umbra Settings
2. Navigate to Plugins
3. Enter `una-xiv` as repo owner and `Umbra.CounterSpyPlugin` as repo name and click the install button.

## Enable the World Markers

Once the plugin has been enabled in Umbra, a world marker type named **Counter Spy Markers** should show up in your World Markers list. Click on it and tick the "Show markers of this type in the Game World" checkbox.

## Targeting filters (this fork)

Open `/counterspy`, or choose **Targeting filters...** from the Counter Spy toolbar popup.

- **Ignore party members** excludes players in your current party, including cross-world party members. Alliance-only players are not excluded. Leaving the party makes a player eligible again unless their name is blacklisted.
- Enter full character names in **Ignored character names**, one per line. Commas and semicolons also work. Matching ignores capitalization and surrounding spaces, but never matches partial names. Names apply across all worlds.
- Both options apply to world markers, existing glow effects, and the toolbar list, including preview mode. NPCs are unaffected. Defaults are an empty blacklist and party filtering off.
- Settings save automatically to `CounterSpy.settings.json` in Umbra's configuration directory. They are shared across Umbra profiles and survive reloads. Changes apply on the next targeting update.

This is an Umbra extension, not a standalone Dalamud plugin. The new code must be built and released in this fork before Umbra can download it using repository owner `Isaiahfeldt` and repository name `Umbra.CounterSpyPlugin`. Do not load the upstream and fork versions together.

## Development

Requires .NET 10, Dalamud API 15 and Umbra 3.1.18 libraries. Set `DALAMUD_HOME` to the Dalamud library directory and pass an explicit Umbra library directory when building outside a standard Windows installation:

```sh
dotnet build Umbra.CounterSpyPlugin/Umbra.CounterSpyPlugin.csproj -c Release -p:EnableWindowsTargeting=true -p:UmbraLibPath=/path/to/umbra/
dotnet run --project CounterSpy.Checks
```

In-game checks before release:

1. Have a player target you; add their full name and confirm both their marker and existing glow disappear, and the toolbar entry/count updates (including the last remaining target).
2. Remove the name; confirm they return while still targeting you. Check case differences, multiple names, and that partial names do not hide someone else.
3. Enable the party filter; verify party members disappear while non-party players remain. Join/leave a party and test a cross-world party member.
4. Confirm NPC markers are unaffected, preview respects filters, and settings survive an Umbra restart. Log out or change areas and check for stale markers/glows.
