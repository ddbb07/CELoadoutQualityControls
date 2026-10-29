// SPDX-FileCopyrightText: 2026 ddbb07
//
// SPDX-License-Identifier: MIT

using HarmonyLib;
using Verse;

namespace CELoadoutQualityControls;

[StaticConstructorOnStartup]
internal static class HarmonyPatches
{
    static HarmonyPatches() => new Harmony("ddbb07.CELoadoutQualityControls").PatchAll();
}
