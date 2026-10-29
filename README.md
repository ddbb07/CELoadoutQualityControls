# CE Loadout Quality Controls

## Overview

Adds quality and hp controls to Combat Extended's loadouts. The controls apply to the entire loadout, like vanilla's apparel policies. You can't set them for items individually.

Both controls only apply to weapons, so any other items like ammo, medicine and food remain unfiltered.

## Installation

Currently supports 1.6, requires Harmony and Combat Extended.

Available on [Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=3829328081) and as releases in this repo.

It's safe to install and remove mid-game.

## Compatibility

Currently no known compatibility issues. Please report if you find any.

## Limitations

The mod currently doesn't store the ranges if you save your loadout to a file. I will probably fix it in the future.

## How to build

You need the dotnet 10 sdk to build. It automatically downloads the dependencies, so this can build completely standalone from the game.

Build outputs to `out/`.

```Shell
dotnet build src
```

## Inspiration

Approach was based on [PirateBY's ExtendedLoadout](https://github.com/bananasss00/RW-CombatExtended_ExtendedLoadout), thank you so much!

## License

Mod is licensed under the MIT license.
