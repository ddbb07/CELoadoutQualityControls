# CE Loadout Quality Controls

## Overview

Adds quality and HP controls to Combat Extended's loadouts. The controls apply to the entire loadout, like vanilla's apparel policies. You can't set them for items individually.

Both controls only apply to weapons. Any other items like ammo, medicine and food remain unfiltered.

## Installation

Currently supports 1.6, requires Harmony and Combat Extended.

Available on [Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=3829328081) and as releases in this repo.

It's safe to install and remove mid-game.

## Compatibility

This mod will cause desync when used with Multiplayer. To what extent the desync will cause issues is untested.

Currently no other known compatibility issues. Please report any you find.

## Limitations

The mod currently doesn't store the ranges if you save your loadout to a file. I will probably fix it in the future.

## How to build

You need the .NET 10.0 SDK to build. It automatically downloads the dependencies, so this can build completely standalone from the game.

Building the preview image requires rsvg-convert and the Inter Display font to be installed. The regular build works fine without them and just skips the image, but the packaging to zip explicitly exits when they're missing.

Build outputs to `out/`.

```Shell
dotnet build src
```

Package the output into a versioned zip file. It also ends up in `out/`.

```Shell
dotnet build src -t:Zip
```

## Inspiration

Implementation was inspired by [PirateBY's ExtendedLoadout](https://github.com/bananasss00/RW-CombatExtended_ExtendedLoadout). Thank you to PirateBY for creating it!

## License

Mod is licensed under the MIT license.
