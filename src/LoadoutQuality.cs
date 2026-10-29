// SPDX-FileCopyrightText: 2026 ddbb07
//
// SPDX-License-Identifier: MIT

using System.Linq;
using System.Runtime.CompilerServices;
using CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;

namespace CELoadoutQualityControls;

public class LoadoutQualitySettings
{
    public QualityRange qualityRange = QualityRange.All;
    public FloatRange hitPointsRange = FloatRange.ZeroToOne;
}

public static class LoadoutQuality
{
    private static readonly ConditionalWeakTable<Loadout, LoadoutQualitySettings> Settings = [];

    public static LoadoutQualitySettings QualitySettings(this Loadout loadout) =>
        Settings.GetOrCreateValue(loadout);

    public static bool AllowsQuality(this Loadout loadout, Thing thing) =>
        !thing.def.IsWeapon
        || !thing.TryGetQuality(out QualityCategory quality)
        || loadout.QualitySettings().qualityRange.Includes(quality);

    // Same rounding as vanilla's ThingFilter
    public static bool AllowsHitPoints(this Loadout loadout, Thing thing) =>
        !thing.def.IsWeapon
        || !thing.def.useHitPoints
        || loadout
            .QualitySettings()
            .hitPointsRange.IncludesEpsilon(
                Mathf.Clamp01(GenMath.RoundedHundredth(thing.HitPoints / (float)thing.MaxHitPoints))
            );

    public static bool IsForced(this Pawn pawn, Thing thing) =>
        LoadoutManager.GetHoldRecords(pawn)?.Any(r => r.count >= 1 && r.thingDef == thing.def)
        ?? false;
}
