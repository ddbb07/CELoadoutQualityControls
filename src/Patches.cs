// SPDX-FileCopyrightText: 2026 ddbb07
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CELoadoutQualityControls;

[HarmonyPatch(typeof(Loadout), nameof(Loadout.ExposeData))]
internal static class Loadout_ExposeData
{
    private static void Postfix(Loadout __instance)
    {
        LoadoutQualitySettings settings = __instance.QualitySettings();
        Scribe_Values.Look(ref settings.qualityRange, "qualityRange", QualityRange.All);
        Scribe_Values.Look(ref settings.hitPointsRange, "hitPointsRange", FloatRange.ZeroToOne);
    }
}

[HarmonyPatch(typeof(Loadout), "Copy", typeof(Loadout))]
internal static class Loadout_Copy
{
    private static void Postfix(Loadout source, Loadout __result)
    {
        LoadoutQualitySettings from = source.QualitySettings();
        LoadoutQualitySettings to = __result.QualitySettings();
        to.qualityRange = from.qualityRange;
        to.hitPointsRange = from.hitPointsRange;
    }
}

[HarmonyPatch]
internal static class JobGiver_UpdateLoadout_FindPickup
{
    private static readonly MethodBase[] FindItem =
    [
        .. typeof(JobGiver_UpdateLoadout)
            .GetNestedTypes(AccessTools.all)
            .SelectMany(AccessTools.GetDeclaredMethods)
            .Where(m =>
                m.Name.Contains("<FindPickup>")
                && PatchProcessor
                    .ReadMethodBody(m)
                    .Any(i => i.Value is MethodInfo { Name: "GetInnerIfMinified" })
            ),
    ];

    private static bool Prepare()
    {
        if (FindItem.Length != 2)
        {
            Log.Error(
                "[CE Loadout Quality Controls] Could not find findItem in JobGiver_UpdateLoadout.FindPickup"
            );
        }
        return FindItem.Length == 2;
    }

    private static IEnumerable<MethodBase> TargetMethods() => FindItem;

    private static void Postfix(Thing __0, Pawn ___pawn, ref bool __result)
    {
        if (!__result)
        {
            return;
        }

        Loadout loadout = ___pawn.GetLoadout();
        Thing thing = __0.GetInnerIfMinified();
        __result = loadout.AllowsQuality(thing) && loadout.AllowsHitPoints(thing);
    }
}

[HarmonyPatch]
internal static class Utility_HoldTracker_GetExcessEquipment
{
    private static readonly MethodBase Target = AccessTools.Method(
        "CombatExtended.Utility_HoldTracker:GetExcessEquipment"
    );

    private static bool Prepare()
    {
        if (Target == null)
        {
            Log.Error(
                "[CE Loadout Quality Controls] Could not find Utility_HoldTracker.GetExcessEquipment"
            );
        }
        return Target != null;
    }

    private static MethodBase TargetMethod() => Target;

    private static void Postfix(Pawn pawn, ref ThingWithComps dropEquipment, ref bool __result)
    {
        Loadout loadout = pawn.GetLoadout();
        if (
            __result
            || loadout == null
            || loadout.defaultLoadout
            || !loadout.dropUndefined
            || loadout.adHoc
            || pawn.equipment?.Primary is not { } primary
        )
        {
            return;
        }

        if (
            !pawn.IsItemQuestLocked(primary)
            && !pawn.IsForced(primary)
            && (!loadout.AllowsQuality(primary) || !loadout.AllowsHitPoints(primary))
        )
        {
            dropEquipment = primary;
            __result = true;
        }
    }
}

[HarmonyPatch]
internal static class Utility_HoldTracker_GetExcessThing
{
    private static readonly MethodBase Target = AccessTools.Method(
        "CombatExtended.Utility_HoldTracker:GetExcessThing"
    );

    private static bool Prepare()
    {
        if (Target == null)
        {
            Log.Error(
                "[CE Loadout Quality Controls] Could not find Utility_HoldTracker.GetExcessThing"
            );
        }
        return Target != null;
    }

    private static MethodBase TargetMethod() => Target;

    private static void Postfix(
        Pawn pawn,
        ref Thing dropThing,
        ref int dropCount,
        ref bool __result
    )
    {
        Loadout loadout = pawn.GetLoadout();
        if (
            __result
            || loadout == null
            || loadout.defaultLoadout
            || !loadout.dropUndefined
            || pawn.inventory?.innerContainer is not { } container
        )
        {
            return;
        }

        foreach (Thing thing in container)
        {
            Thing inner = thing.GetInnerIfMinified();
            if (
                !pawn.IsItemQuestLocked(thing)
                && !pawn.IsForced(thing)
                && (!loadout.AllowsQuality(inner) || !loadout.AllowsHitPoints(inner))
            )
            {
                dropThing = thing;
                dropCount = thing.stackCount;
                __result = true;
                return;
            }
        }
    }
}

[HarmonyPatch(typeof(Dialog_ManageLoadouts), nameof(Dialog_ManageLoadouts.DoWindowContents))]
internal static class Dialog_ManageLoadouts_DoWindowContents
{
    private const float ColumnMargin = 6f;
    private const float RowMargin = 3f;
    private const float RowHeight = 32f;
    private const int HitPointsRangeId = 491_736_583;
    private const int QualityRangeId = 491_736_582;

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions
    )
    {
        bool patched = false;
        foreach (CodeInstruction instruction in instructions)
        {
            if (
                !patched
                && instruction.opcode == OpCodes.Ldc_R4
                && (float)instruction.operand == 48f
            )
            {
                instruction.operand = 48f + (RowMargin + RowHeight) * 2f;
                patched = true;
            }
            yield return instruction;
        }

        if (!patched)
        {
            Log.Error(
                "[CE Loadout Quality Controls] Could not find the slot list height in Dialog_ManageLoadouts.DoWindowContents"
            );
        }
    }

    private static void Postfix(Dialog_ManageLoadouts __instance, Rect canvas)
    {
        if (__instance.CurrentLoadout == null)
        {
            return;
        }

        LoadoutQualitySettings settings = __instance.CurrentLoadout.QualitySettings();
        float width = (canvas.width - ColumnMargin) / 2f;

        Rect hitPointsRect = new(0f, canvas.height - RowHeight * 2f - RowMargin, width, RowHeight);
        Widgets.FloatRange(
            hitPointsRect,
            HitPointsRangeId,
            ref settings.hitPointsRange,
            0f,
            1f,
            "HitPoints",
            ToStringStyle.PercentZero
        );

        Rect qualityRect = new(0f, canvas.height - RowHeight, width, RowHeight);
        Widgets.QualityRange(qualityRect, QualityRangeId, ref settings.qualityRange);
    }
}
